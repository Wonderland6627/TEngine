using System;

namespace GameLogic.Network
{
    /// <summary>
    /// 响应码定义（与后端 constants.js 保持一致）
    /// </summary>
    public static class ResponseCode
    {
        // ========== 服务端响应码（与后端一致） ==========
        public const int SUCCESS = 0;              // 成功
        public const int ERROR = -1;               // 通用错误
        public const int UNAUTHORIZED = -2;        // 未授权（Token无效/过期）
        public const int NOT_FOUND = -3;           // 资源未找到
        public const int VALIDATION_ERROR = -4;    // 参数验证失败
        
        // ========== 客户端本地错误码（-100 ~ -199） ==========
        public const int CLIENT_NO_NETWORK = -100;       // 无网络连接
        public const int CLIENT_TIMEOUT = -101;          // 请求超时
        public const int CLIENT_PARSE_ERROR = -102;      // 响应解析失败
        public const int CLIENT_UNKNOWN_API = -103;      // 未知API
        public const int CLIENT_REQUEST_FAILED = -104;   // 请求失败（重试后仍失败）
    }
    
    /// <summary>
    /// 云函数标准响应格式
    /// </summary>
    /// <typeparam name="T">业务数据类型</typeparam>
    [Serializable]
    public class Response<T>
    {
        /// <summary>
        /// 响应码：0=成功, 非0=失败
        /// </summary>
        public int code;

        /// <summary>
        /// 业务数据
        /// </summary>
        public T data;

        /// <summary>
        /// 消息描述
        /// </summary>
        public string msg;

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess => code == ResponseCode.SUCCESS;

        /// <summary>
        /// 错误消息（失败时返回）
        /// </summary>
        public string ErrorMessage => IsSuccess ? null : msg;
        
        /// <summary>
        /// 是否为客户端本地错误
        /// </summary>
        public bool IsClientError => code <= -100 && code >= -199;
        
        /// <summary>
        /// 是否为未授权错误（需要重新登录）
        /// </summary>
        public bool IsUnauthorized => code == ResponseCode.UNAUTHORIZED;
    }
}

