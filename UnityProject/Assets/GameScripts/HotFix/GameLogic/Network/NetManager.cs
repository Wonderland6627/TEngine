using System;
using System.Collections.Generic;
using System.Text;
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
    /// </summary>
    public static class NetManager
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
        
        private static string _authToken = null;
        private static long? _cachedTokenExpireTime = null;     // 缓存的Token过期时间（Unix时间戳）
        private static ServerType _currentServerType = ServerType.Production;
        private static long _serverTimeOffset = 0;

        /// <summary>
        /// 服务器地址配置
        /// </summary>
        private static readonly Dictionary<ServerType, string> ServerUrls = new Dictionary<ServerType, string>
        {
            { ServerType.Local, "http://localhost:3000" },
            { ServerType.Dev, "https://express-slime-dev-216111-7-1352845565.sh.run.tcloudbase.com" },
            { ServerType.Production, "https://express-slime-216111-7-1352845565.sh.run.tcloudbase.com" }
        };

        /// <summary>
        /// 获取当前服务器时间（UTC时间）
        /// </summary>
        public static DateTime ServerTime
        {
            get
            {
                return DateTime.UtcNow.AddMilliseconds(_serverTimeOffset);
            }
        }

        /// <summary>
        /// 获取当前服务器时间（本地时区）
        /// </summary>
        public static DateTime ServerTimeLocal
        {
            get
            {
                return ServerTime.ToLocalTime();
            }
        }

        /// <summary>
        /// 当前服务器类型
        /// </summary>
        public static ServerType CurrentServerType
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
        public static string ServerBaseUrl => ServerUrls[_currentServerType];

        /// <summary>
        /// 初始化（在游戏启动时调用一次）
        /// </summary>
        public static void Initialize()
        {
            // 默认选服：Editor→测试服，Release包→正式服
            // PlayerPrefs 保存手动切换值，优先级高于默认值
#if UNITY_EDITOR
            ServerType defaultServerType = ServerType.Local;
#else
            ServerType defaultServerType = ServerType.Production;
#endif
            _currentServerType = (ServerType)PlayerPrefs.GetInt(KEY_SERVER_TYPE, (int)defaultServerType);
            Log.Info($"[NetManager] Initialized with server: {_currentServerType} ({ServerBaseUrl})");

            // 注册调试器服务器切换功能
            RegisterDebuggerServerSwitch();
        }

        /// <summary>
        /// 注册调试器服务器切换功能
        /// </summary>
        private static void RegisterDebuggerServerSwitch()
        {
            string[] serverNames = Enum.GetNames(typeof(ServerType));
            DebuggerModule.RegisterServerSwitch(
                () => serverNames,
                () => (int)_currentServerType,
                (index) => CurrentServerType = (ServerType)index
            );
        }

        /// <summary>
        /// 认证Token（自动缓存到本地）
        /// </summary>
        public static string AuthToken
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
                _cachedTokenExpireTime = null; // 清除缓存的过期时间，下次访问时重新解析
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
        public static void ClearToken()
        {
            AuthToken = null;
            _cachedTokenExpireTime = null;
            Log.Info("[NetManager] Token cleared");
        }
        
        #region Token有效性检查
        
        /// <summary>
        /// 检查是否需要登录（无Token或Token已过期）
        /// </summary>
        public static bool NeedLogin
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
        public static TokenState GetTokenState()
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
            
            // 检查是否即将过期
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
        public static long GetTokenRemainingSeconds()
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
        private static long? GetTokenExpireTime(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }
            
            // 使用缓存
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
                // Base64Url -> Base64 转换
                payload = payload.Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }
                
                byte[] bytes = Convert.FromBase64String(payload);
                string json = Encoding.UTF8.GetString(bytes);
                
                // 解析JSON获取exp字段
                var data = Utility.Json.ToObject<Dictionary<string, object>>(json);
                if (data != null && data.TryGetValue("exp", out var expValue))
                {
                    long exp = Convert.ToInt64(expValue);
                    _cachedTokenExpireTime = exp; // 缓存结果
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
            { "addCurrency", "/api/minigame/addCurrency" },
            { "deductCurrency", "/api/minigame/deductCurrency" },
            { "addCoin", "/api/minigame/addCoin" },
            { "deductCoin", "/api/minigame/deductCoin" },
            { "updateEnergy", "/api/minigame/updateEnergy" },
            { "claimLevelReward", "/api/minigame/claimLevelReward" },
        };

        /// <summary>
        /// 同步服务器时间
        /// </summary>
        public static async UniTask SyncServerTime()
        {
            try 
            {
                var response = await CallHttpData<ServerTimeData>("getServerTime");
                if (response != null)
                {
                    long serverTime = response.timestamp;
                    long clientTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    _serverTimeOffset = serverTime - clientTime;
                    Log.Info($"[NetManager] Server time synced - UTC: {ServerTime:yyyy/MM/dd HH:mm:ss}, Local: {ServerTimeLocal:yyyy/MM/dd HH:mm:ss} (offset: {_serverTimeOffset}ms)");
                }
            }
            catch (Exception e)
            {
                Log.Error($"[NetManager] SyncServerTime failed: {e.Message}");
            }
        }

        /// <summary>
        /// 调用云函数（通用方法）
        /// 异常时返回失败的 Response，不会抛出异常
        /// </summary>
        /// <typeparam name="T">响应数据类型</typeparam>
        /// <param name="functionName">云函数名称</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>云函数响应，失败时返回 code=-1 的 Response</returns>
        public static async UniTask<Response<T>> Call<T>(
            string functionName,
            object parameters = null)
        {
            var tcs = new UniTaskCompletionSource<Response<T>>();

            // 记录请求日志
            string requestJson = parameters != null ? (parameters is string ? (string)parameters : parameters.ToJson()) : "null";
            Log.Info($"[NetManager] Request: {functionName}, params: {requestJson}");

            WX.cloud.CallFunction(new CallFunctionParam()
            {
                name = functionName,
                data = parameters,
                success = (res) =>
                {
                    try
                    {
                        // 自动解析响应
                        // res.result 可能是字符串或对象，统一转换为字符串再解析
                        string resultJson = res.result is string ? (string)res.result : res.result.ToJson();
                        
                        // 记录响应日志
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
        public static async UniTask<T> CallData<T>(
            string functionName,
            object parameters = null) where T : class
        {
            var response = await Call<T>(functionName, parameters);
            return response.IsSuccess ? response.data : null;
        }

        #region HTTP接口（用于Express服务端通信）

        /// <summary>
        /// 调用HTTP接口
        /// </summary>
        /// <typeparam name="T">响应数据类型</typeparam>
        /// <param name="apiName">接口名（从路由字典查找）</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>HTTP响应</returns>
        public static async UniTask<Response<T>> CallHttp<T>(string apiName, object parameters = null)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Log.Warning("[NetManager] No network connection");
                return new Response<T> { code = ResponseCode.CLIENT_NO_NETWORK, msg = "No network connection", data = default(T) };
            }
            
            // 从路由字典查找接口路径
            if (!ApiRouteMap.TryGetValue(apiName, out string endpoint))
            {
                var msg = $"Unknown API: {apiName}";
                Log.Error($"[NetManager] {msg}");
                return new Response<T> { code = ResponseCode.CLIENT_UNKNOWN_API, msg = msg, data = default(T) };
            }

            // 构建完整URL
            string url = ServerBaseUrl.TrimEnd('/') + endpoint;

            // 准备请求数据
            string requestJson = parameters != null ? (parameters is string ? (string)parameters : parameters.ToJson()) : "{}";
            
            // 带重试的请求
            return await SendHttpRequestWithRetry<T>(url, requestJson, MAX_RETRY_COUNT);
        }
        
        /// <summary>
        /// 发送HTTP请求（带重试机制）
        /// </summary>
        private static async UniTask<Response<T>> SendHttpRequestWithRetry<T>(string url, string requestJson, int retryCount)
        {
            string lastError = null;
            
            for (int attempt = 0; attempt <= retryCount; attempt++)
            {
                if (attempt > 0)
                {
                    Log.Warning($"[NetManager] Retry attempt {attempt}/{retryCount} for {url}");
                    await UniTask.Delay(RETRY_DELAY_MS);
                }
                
                try
                {
                    var result = await SendHttpRequestInternal<T>(url, requestJson);
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch (Exception e)
                {
                    lastError = e.Message;
                    
                    // 判断是否为可重试的错误（超时、连接失败等）
                    if (!IsRetryableError(e))
                    {
                        break;
                    }
                }
            }
            
            // 所有重试都失败
            Log.Error($"[NetManager] HTTP request failed with url: {url} after {retryCount + 1} attempts: {lastError}");
            return new Response<T> { code = ResponseCode.CLIENT_REQUEST_FAILED, msg = lastError ?? "Request failed", data = default(T) };
        }
        
        /// <summary>
        /// 发送HTTP请求（内部实现）
        /// </summary>
        private static async UniTask<Response<T>> SendHttpRequestInternal<T>(string url, string requestJson)
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
            
            // 添加token
            if (!string.IsNullOrEmpty(AuthToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {AuthToken}");
            }

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                // 区分可重试和不可重试的错误
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
        public static async UniTask<T> CallHttpData<T>(string apiName, object parameters = null) where T : class
        {
            var response = await CallHttp<T>(apiName, parameters);
            return response.IsSuccess ? response.data : null;
        }

        #endregion
    }

}

