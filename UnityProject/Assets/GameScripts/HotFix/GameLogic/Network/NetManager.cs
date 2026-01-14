using System;
using System.Collections.Generic;
using System.Text;
using TEngine;
using WeChatWASM;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using Utility = TEngine.Utility;

namespace GameLogic.Network
{
    /// <summary>
    /// 网络管理器
    /// 统一处理云函数调用和HTTP请求，返回值解析
    /// </summary>
    public static class NetManager
    {
        // 配置
        private const string KEY_AUTH_TOKEN = "NetManager_AuthToken";
        private static string _authToken = null;

        /// <summary>
        /// 服务端基础URL（例如：http://localhost:3000）
        /// </summary>
        public static string ServerBaseUrl = "http://localhost:3000";

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

        // API路由字典：接口名 -> HTTP端点路径
        private static readonly Dictionary<string, string> ApiRouteMap = new Dictionary<string, string>
        {
            { "getCode2Session", "/api/minigame/getCode2Session" },
            { "getUserWXContext", "/api/minigame/getUserWXContext" },
            { "getUserGameInfoV2", "/api/minigame/getUserGameInfoV2" },
            { "setUserGameInfoV2", "/api/minigame/setUserGameInfoV2" },
            { "getUserRankListV2", "/api/minigame/getUserRankListV2" },
            { "getLevelsConfigV2", "/api/minigame/getLevelsConfigV2" }
        };

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
                            code = -1,
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
                        code = -1,
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
            try
            {
                var response = await Call<T>(functionName, parameters);
                return response.IsSuccess ? response.data : null;
            }
            catch (Exception e)
            {
                Log.Error($"[NetManager] CallData {functionName} exception: {e}");
                return null;
            }
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
            try
            {
                // 从路由字典查找接口路径
                if (!ApiRouteMap.TryGetValue(apiName, out string endpoint))
                {
                    var msg = $"Unknown API: {apiName}";
                    Log.Error($"[NetManager] {msg}");
                    return new Response<T> { code = -1, msg = msg, data = default(T) };
                }

                // 构建完整URL
                string url = ServerBaseUrl.TrimEnd('/') + endpoint;

                // 准备请求数据
                string requestJson = parameters != null ? (parameters is string ? (string)parameters : parameters.ToJson()) : "{}";
                Log.Info($"[NetManager] HTTP POST {url}, params: {requestJson}");

                // 发送HTTP请求
                using UnityWebRequest request = new UnityWebRequest(url, "POST");
                byte[] bodyRaw = Encoding.UTF8.GetBytes(requestJson);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                
                // 添加token
                if (!string.IsNullOrEmpty(AuthToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {AuthToken}");
                }

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Log.Error($"[NetManager] HTTP error: {request.error}");
                    return new Response<T> { code = -1, msg = request.error, data = default(T) };
                }

                string responseJson = request.downloadHandler.text;
                Log.Info($"[NetManager] HTTP Response: {responseJson}");
                
                return Utility.Json.ToObject<Response<T>>(responseJson);
            }
            catch (Exception e)
            {
                Log.Error($"[NetManager] HTTP exception: {e}");
                return new Response<T> { code = -1, msg = e.Message, data = default(T) };
            }
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

