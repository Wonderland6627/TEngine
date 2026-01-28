using System;

namespace GameLogic.Network
{
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
        public bool IsSuccess => code == Constants.ResponseCode.SUCCESS;

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
        public bool IsUnauthorized => code == Constants.ResponseCode.UNAUTHORIZED;
    }
}

