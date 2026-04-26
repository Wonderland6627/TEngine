using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TEngine;
using WeChatWASM;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Utility = TEngine.Utility;

namespace GameLogic.Network
{
    /// <summary>
    /// 服务器类型
    /// </summary>
    public enum ServerType
    {
        Local,      // 本地服
        Dev,       // 测试服
        Production  // 正式服
    }

    /// <summary>
    /// Token状态
    /// </summary>
    public enum TokenState
    {
        None,       // 无Token
        Valid,      // 有效
        Expiring,   // 即将过期（剩余时间小于阈值）
        Expired,    // 已过期
        Invalid     // 无效（解析失败）
    }

    /// <summary>
    /// 网络管理器
    /// 统一处理云函数调用和HTTP请求，返回值解析
    /// 通过 BaseLogicSys 接入 TEngine 生命周期，支持请求取消和安全销毁
    /// </summary>
    public class NetManager : BaseLogicSys<NetManager>
    {
        // 配置
        private const string KEY_AUTH_TOKEN = "NetManager_AuthToken";
        private const string KEY_SERVER_TYPE = "NetManager_ServerType";
        
        // 网络配置
        private const int REQUEST_TIMEOUT = 30;     // 请求超时时间（秒）
        private const int MAX_RETRY_COUNT = 3;      // 最大重试次数
        private const int RETRY_DELAY_MS = 1000;    // 重试间隔（毫秒）
        
        // Token配置
        private const int TOKEN_EXPIRING_THRESHOLD_HOURS = 24;  // Token即将过期阈值（小时）
        
        // 时间同步
        private const float SYNC_INTERVAL_SECONDS = 300f;

        private string _authToken = null;
        private long? _cachedTokenExpireTime = null;     // 缓存的Token过期时间（Unix时间戳）
        private ServerType _currentServerType = ServerType.Production;
        private long _serverTimeOffset = 0;
        private bool _hasSynced = false;
        private float _syncTimer = 0f;

        // 生命周期安全：用于取消在途HTTP请求 + 标记WX回调跳过
        private CancellationTokenSource _cts;
        private bool _disposed = false;

        /// <summary>
        /// 服务器地址配置
        /// </summary>
        private static readonly Dictionary<ServerType, string> ServerUrls = new Dictionary<ServerType, string>
        {
            { ServerType.Local, "http://localhost:3000" },
            { ServerType.Dev, "https://piratecat.top/dev" },
            { ServerType.Production, "https://piratecat.top" }
        };

        // API路由字典：接口名 -> HTTP端点路径
        private static readonly Dictionary<string, string> ApiRouteMap = new Dictionary<string, string>
        {
            { "getServerTime", "/api/time" },
            { "getCode2Session", "/api/minigame/getCode2Session" },
            { "getUserWXContext", "/api/minigame/getUserWXContext" },
            { "getUserGameInfoV2", "/api/minigame/getUserGameInfoV2" },
            { "setUserGameInfoV2", "/api/minigame/setUserGameInfoV2" },
            { "getUserRankListV2", "/api/minigame/getUserRankListV2" },
            { "getLevelsConfigV2", "/api/minigame/getLevelsConfigV2" },
            { "updateResource", "/api/minigame/updateResource" },
            { "getResources", "/api/minigame/getResources" },
            { "claimLevelReward", "/api/minigame/claimLevelReward" },
            { "claimDailyCheckin", "/api/minigame/claimDailyCheckin" },
            { "claimLevelChest", "/api/minigame/claimLevelChest" },
            { "claimAdsGiftPack", "/api/minigame/claimAdsGiftPack" },
            { "debugSetUserGameInfo", "/api/minigame/debugSetUserGameInfo" },
        };

        #region 生命周期

        public override bool OnInit()
        {
            base.OnInit();

            _cts = new CancellationTokenSource();
            _disposed = false;

            // 默认选服：Editor→本地服，Release包→正式服
            // PlayerPrefs 保存手动切换值，优先级高于默认值
#if UNITY_EDITOR
            ServerType defaultServerType = ServerType.Local;
#else
            ServerType defaultServerType = ServerType.Production;
#endif
            _currentServerType = (ServerType)PlayerPrefs.GetInt(KEY_SERVER_TYPE, (int)defaultServerType);
            Log.Info($"[NetManager] Initialized with server: {_currentServerType} ({ServerBaseUrl})");

            RegisterDebuggerServerSwitch();
            SyncServerTime().Forget();

            return true;
        }

        public override void OnUpdate()
        {
            _syncTimer += Time.deltaTime;
            if (_syncTimer >= SYNC_INTERVAL_SECONDS)
            {
                _syncTimer = 0f;
                SyncServerTime().Forget();
            }
        }

        public override void OnApplicationPause(bool pause)
        {
            if (pause) return;
            _syncTimer = 0f;
            SyncServerTime().Forget();
        }

        public override void OnDestroy()
        {
            _disposed = true;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            Log.Info("[NetManager] Destroyed, all pending requests cancelled");
        }

        #endregion

        #region 时间同步

        /// <summary>
        /// 是否已至少完成过一次时间同步
        /// </summary>
        public bool HasSynced => _hasSynced;

        /// <summary>
        /// 获取当前服务器时间（UTC时间）
        /// </summary>
        public DateTime ServerTime
        {
            get
            {
                return DateTime.UtcNow.AddMilliseconds(_serverTimeOffset);
            }
        }

        /// <summary>
        /// 获取当前服务器时间（本地时区）
        /// </summary>
        public DateTime ServerTimeLocal
        {
            get
            {
                return ServerTime.ToLocalTime();
            }
        }

        /// <summary>
        /// 同步服务器时间
        /// </summary>
        public async UniTask SyncServerTime()
        {
            try 
            {
                var response = await CallHttpData<ServerTimeData>("getServerTime");
                if (response != null)
                {
                    long serverTime = response.timestamp;
                    long clientTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    _serverTimeOffset = serverTime - clientTime;
                    _hasSynced = true;
                    Log.Info($"[NetManager] Server time synced - UTC: {ServerTime:yyyy/MM/dd HH:mm:ss}, Local: {ServerTimeLocal:yyyy/MM/dd HH:mm:ss} (offset: {_serverTimeOffset}ms)");
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                if (!_hasSynced)
                {
                    Log.Warning($"[NetManager] SyncServerTime failed and never synced before, ServerTime may be inaccurate: {e.Message}");
                }
                else
                {
                    Log.Error($"[NetManager] SyncServerTime failed: {e.Message}");
                }
            }
        }

        #endregion

        #region 服务器配置

        /// <summary>
        /// 当前服务器类型
        /// </summary>
        public ServerType CurrentServerType
        {
            get => _currentServerType;
            set
            {
                _currentServerType = value;
                PlayerPrefs.SetInt(KEY_SERVER_TYPE, (int)value);
                PlayerPrefs.Save();
                Log.Info($"[NetManager] Server switched to {value}: {ServerBaseUrl}");
            }
        }

        /// <summary>
        /// 服务端基础URL（根据当前服务器类型自动获取）
        /// </summary>
        public string ServerBaseUrl => ServerUrls[_currentServerType];

        /// <summary>
        /// 注册调试器服务器切换功能
        /// </summary>
        private void RegisterDebuggerServerSwitch()
        {
            string[] serverNames = Enum.GetNames(typeof(ServerType));
            DebuggerModule.RegisterServerSwitch(
                () => serverNames,
                () => (int)_currentServerType,
                (index) => CurrentServerType = (ServerType)index
            );
        }

        #endregion

        #region Token管理

        /// <summary>
        /// 认证Token（自动缓存到本地）
        /// </summary>
        public string AuthToken
        {
            get
            {
                if (_authToken == null)
                {
                    _authToken = PlayerPrefs.GetString(KEY_AUTH_TOKEN, "");
                }
                return _authToken;
            }
            set
            {
                _authToken = value;
                _cachedTokenExpireTime = null;
                if (string.IsNullOrEmpty(value))
                {
                    PlayerPrefs.DeleteKey(KEY_AUTH_TOKEN);
                }
                else
                {
                    PlayerPrefs.SetString(KEY_AUTH_TOKEN, value);
                }
                PlayerPrefs.Save();
            }
        }
        
        /// <summary>
        /// 清除Token（登出时调用）
        /// </summary>
        public void ClearToken()
        {
            AuthToken = null;
            _cachedTokenExpireTime = null;
            Log.Info("[NetManager] Token cleared");
        }
        
        /// <summary>
        /// 检查是否需要登录（无Token或Token已过期）
        /// </summary>
        public bool NeedLogin
        {
            get
            {
                var state = GetTokenState();
                return state == TokenState.None || state == TokenState.Expired || state == TokenState.Invalid;
            }
        }
        
        /// <summary>
        /// 获取Token状态
        /// </summary>
        public TokenState GetTokenState()
        {
            string token = AuthToken;
            if (string.IsNullOrEmpty(token))
            {
                return TokenState.None;
            }
            
            long? expireTime = GetTokenExpireTime(token);
            if (!expireTime.HasValue)
            {
                return TokenState.Invalid;
            }
            
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long remaining = expireTime.Value - now;
            
            if (remaining <= 0)
            {
                return TokenState.Expired;
            }
            
            long expiringThreshold = TOKEN_EXPIRING_THRESHOLD_HOURS * 3600;
            if (remaining < expiringThreshold)
            {
                return TokenState.Expiring;
            }
            
            return TokenState.Valid;
        }
        
        /// <summary>
        /// 获取Token剩余有效时间（秒），无效时返回0
        /// </summary>
        public long GetTokenRemainingSeconds()
        {
            long? expireTime = GetTokenExpireTime(AuthToken);
            if (!expireTime.HasValue)
            {
                return 0;
            }
            
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long remaining = expireTime.Value - now;
            return remaining > 0 ? remaining : 0;
        }
        
        /// <summary>
        /// 解析JWT Token获取过期时间（Unix时间戳）
        /// </summary>
        /// <param name="token">JWT Token</param>
        /// <returns>过期时间戳，解析失败返回null</returns>
        private long? GetTokenExpireTime(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            
            if (_cachedTokenExpireTime.HasValue)
            {
                return _cachedTokenExpireTime;
            }
            
            try
            {
                // JWT格式: header.payload.signature
                string[] parts = token.Split('.');
                if (parts.Length != 3)
                {
                    Log.Warning("[NetManager] Invalid JWT format");
                    return null;
                }
                
                // Base64Url解码payload
                string payload = parts[1];
                payload = payload.Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }
                
                byte[] bytes = Convert.FromBase64String(payload);
                string json = Encoding.UTF8.GetString(bytes);
                
                var data = Utility.Json.ToObject<Dictionary<string, object>>(json);
                if (data != null && data.TryGetValue("exp", out var expValue))
                {
                    long exp = Convert.ToInt64(expValue);
                    _cachedTokenExpireTime = exp;
                    return exp;
                }
                
                Log.Warning("[NetManager] JWT payload missing 'exp' field");
                return null;
            }
            catch (Exception e)
            {
                Log.Warning($"[NetManager] Failed to parse JWT: {e.Message}");
                return null;
            }
        }
        
        #endregion

        #region WX云函数

        /// <summary>
        /// 调用云函数（通用方法）
        /// 异常时返回失败的 Response，不会抛出异常
        /// </summary>
        /// <typeparam name="T">响应数据类型</typeparam>
        /// <param name="functionName">云函数名称</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>云函数响应，失败时返回 code=-1 的 Response</returns>
        public async UniTask<Response<T>> Call<T>(
            string functionName,
            object parameters = null)
        {
            var tcs = new UniTaskCompletionSource<Response<T>>();

            string requestJson = parameters != null ? (parameters is string ? (string)parameters : parameters.ToJson()) : "null";
            Log.Info($"[NetManager] Request: {functionName}, params: {requestJson}");

            WX.cloud.CallFunction(new CallFunctionParam()
            {
                name = functionName,
                data = parameters,
                success = (res) =>
                {
                    if (_disposed) return;
                    try
                    {
                        string resultJson = res.result is string ? (string)res.result : res.result.ToJson();
                        Log.Info($"[NetManager] Response: {functionName}, result: {resultJson}");
                        
                        var response = Utility.Json.ToObject<Response<T>>(resultJson);
                        tcs.TrySetResult(response);
                    }
                    catch (Exception e)
                    {
                        var msg = $"Parse response failed for {functionName}: {e}";
                        Log.Error($"[NetManager] {msg}");
                        tcs.TrySetResult(new Response<T>
                        {
                            code = ResponseCode.CLIENT_PARSE_ERROR,
                            msg = msg,
                            data = default(T)
                        });
                    }
                },
                fail = (err) =>
                {
                    if (_disposed) return;
                    var msg = $"Network error: {err.ToJson()}";
                    Log.Error($"[NetManager] {msg}");
                    tcs.TrySetResult(new Response<T>
                    {
                        code = ResponseCode.ERROR,
                        msg = msg,
                        data = default(T)
                    });
                }
            });

            return await tcs.Task;
        }

        /// <summary>
        /// 调用云函数并直接返回数据（便捷方法）
        /// 成功时返回 data，失败时返回 null
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="functionName">云函数名称</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>业务数据，失败时返回 null</returns>
        public async UniTask<T> CallData<T>(
            string functionName,
            object parameters = null) where T : class
        {
            var response = await Call<T>(functionName, parameters);
            return response.IsSuccess ? response.data : null;
        }

        #endregion

        #region HTTP接口（用于Express服务端通信）

        /// <summary>
        /// 调用HTTP接口
        /// </summary>
        /// <typeparam name="T">响应数据类型</typeparam>
        /// <param name="apiName">接口名（从路由字典查找）</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>HTTP响应</returns>
        public async UniTask<Response<T>> CallHttp<T>(string apiName, object parameters = null)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Log.Warning("[NetManager] No network connection");
                return new Response<T> { code = ResponseCode.CLIENT_NO_NETWORK, msg = "No network connection", data = default(T) };
            }
            
            if (!ApiRouteMap.TryGetValue(apiName, out string endpoint))
            {
                var msg = $"Unknown API: {apiName}";
                Log.Error($"[NetManager] {msg}");
                return new Response<T> { code = ResponseCode.CLIENT_UNKNOWN_API, msg = msg, data = default(T) };
            }

            string url = ServerBaseUrl.TrimEnd('/') + endpoint;
            string requestJson = parameters != null ? (parameters is string ? (string)parameters : parameters.ToJson()) : "{}";
            
            return await SendHttpRequestWithRetry<T>(url, requestJson, MAX_RETRY_COUNT);
        }
        
        /// <summary>
        /// 发送HTTP请求（带重试机制）
        /// </summary>
        private async UniTask<Response<T>> SendHttpRequestWithRetry<T>(string url, string requestJson, int retryCount)
        {
            string lastError = null;
            var ct = _cts?.Token ?? CancellationToken.None;
            
            for (int attempt = 0; attempt <= retryCount; attempt++)
            {
                if (attempt > 0)
                {
                    Log.Warning($"[NetManager] Retry attempt {attempt}/{retryCount} for {url}");
                    await UniTask.Delay(RETRY_DELAY_MS, cancellationToken: ct);
                }
                
                try
                {
                    var result = await SendHttpRequestInternal<T>(url, requestJson, ct);
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    lastError = e.Message;
                    
                    if (!IsRetryableError(e))
                    {
                        break;
                    }
                }
            }
            
            Log.Error($"[NetManager] HTTP request failed with url: {url} after {retryCount + 1} attempts: {lastError}");
            return new Response<T> { code = ResponseCode.CLIENT_REQUEST_FAILED, msg = lastError ?? "Request failed", data = default(T) };
        }
        
        /// <summary>
        /// 发送HTTP请求（内部实现）
        /// </summary>
        private async UniTask<Response<T>> SendHttpRequestInternal<T>(string url, string requestJson, CancellationToken ct)
        {
            Log.Info($"[NetManager] HTTP POST {url}, params: {requestJson}");
            
            using var request = new UnityWebRequest(url, "POST");
            request.timeout = REQUEST_TIMEOUT;
            
            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestJson);
            using var uploadHandler = new UploadHandlerRaw(bodyRaw);
            using var downloadHandler = new DownloadHandlerBuffer();
            
            request.uploadHandler = uploadHandler;
            request.downloadHandler = downloadHandler;
            request.SetRequestHeader("Content-Type", "application/json");
            
            if (!string.IsNullOrEmpty(AuthToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {AuthToken}");
            }

            await request.SendWebRequest().WithCancellation(ct);

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (IsRetryableResult(request.result))
                {
                    throw new Exception($"HTTP error (retryable): {request.error}");
                }
                
                Log.Error($"[NetManager] HTTP error: {request.error}");
                return new Response<T> { code = ResponseCode.ERROR, msg = request.error, data = default(T) };
            }

            string responseJson = downloadHandler.text;
            Log.Info($"[NetManager] HTTP Response: {responseJson}");
            
            return Utility.Json.ToObject<Response<T>>(responseJson);
        }
        
        /// <summary>
        /// 判断是否为可重试的异常
        /// </summary>
        private static bool IsRetryableError(Exception e)
        {
            // 超时、连接失败等可以重试
            string message = e.Message.ToLower();
            return message.Contains("timeout") || 
                   message.Contains("connection") || 
                   message.Contains("retryable");
        }
        
        /// <summary>
        /// 判断是否为可重试的请求结果
        /// </summary>
        private static bool IsRetryableResult(UnityWebRequest.Result result)
        {
            // 连接错误和数据处理错误可以重试，协议错误（如404、500）不重试
            return result == UnityWebRequest.Result.ConnectionError;
        }

        /// <summary>
        /// 调用HTTP接口并直接返回数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="apiName">接口名（从路由字典查找）</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>业务数据，失败时返回 null</returns>
        public async UniTask<T> CallHttpData<T>(string apiName, object parameters = null) where T : class
        {
            var response = await CallHttp<T>(apiName, parameters);
            return response.IsSuccess ? response.data : null;
        }

        #endregion
    }

}
