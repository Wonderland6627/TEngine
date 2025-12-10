using System;
using TEngine;
using WeChatWASM;
using Cysharp.Threading.Tasks;

namespace GameLogic.Network
{
    /// <summary>
    /// 云函数网络管理器
    /// 统一处理云函数调用和返回值解析
    /// </summary>
    public static class NetManager
    {
        /// <summary>
        /// 调用云函数（通用方法）
        /// </summary>
        /// <typeparam name="T">响应数据类型</typeparam>
        /// <param name="functionName">云函数名称</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>云函数响应</returns>
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
                        Log.Error($"[NetManager] Parse response failed for {functionName}: {e}");
                        tcs.TrySetException(e);
                    }
                },
                fail = (err) =>
                {
                    // 网络错误处理
                    string errorJson = err.ToJson();
                    Log.Error($"[NetManager] Response: {functionName} failed, error: {errorJson}");
                    tcs.TrySetException(new Exception(errorJson));
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
    }
}

