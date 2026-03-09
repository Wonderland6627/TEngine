namespace GameLogic
{
    /// <summary>
    /// 客户端与服务端共享的常量定义
    /// 所有常量必须与服务端 constants.js 保持一致
    /// 服务端定义位置：piratecat_slime_express/config/constants.js
    /// <summary>
    /// 响应码定义（对应服务端 constants.js -> RESPONSE_CODE）
    /// </summary>
    public static class ResponseCode
    {
        // ========== 服务端响应码（与后端 constants.js -> RESPONSE_CODE 一致） ==========
        /// <summary>
        /// 成功
        /// </summary>
        public const int SUCCESS = 0;

        /// <summary>
        /// 通用错误
        /// </summary>
        public const int ERROR = -1;

        /// <summary>
        /// 未授权（Token无效/过期）
        /// </summary>
        public const int UNAUTHORIZED = -2;

        /// <summary>
        /// 资源未找到
        /// </summary>
        public const int NOT_FOUND = -3;

        /// <summary>
        /// 参数验证失败
        /// </summary>
        public const int VALIDATION_ERROR = -4;

        // ========== 客户端本地错误码（-100 ~ -199，仅客户端使用） ==========
        /// <summary>
        /// 无网络连接
        /// </summary>
        public const int CLIENT_NO_NETWORK = -100;

        /// <summary>
        /// 请求超时
        /// </summary>
        public const int CLIENT_TIMEOUT = -101;

        /// <summary>
        /// 响应解析失败
        /// </summary>
        public const int CLIENT_PARSE_ERROR = -102;

        /// <summary>
        /// 未知API
        /// </summary>
        public const int CLIENT_UNKNOWN_API = -103;

        /// <summary>
        /// 请求失败（重试后仍失败）
        /// </summary>
        public const int CLIENT_REQUEST_FAILED = -104;
    }
    
    /// <summary>
    /// 资源类型枚举（与服务端 RESOURCE_TYPE 一致）
    /// </summary>
    public enum ResourceType
    {
        Coin = 1,
        Energy = 2,
        Diamond = 3,
    }

        /// <summary>
    /// 物品类型枚举（用于 RewardEntry.ItemType）
    /// </summary>
    public enum ItemType
    {
        Resource = 1,
        Goods = 2, // 预留
    }

    /// <summary>
    /// 资源来源常量（与服务端 RESOURCE_SOURCE 一致）
    /// </summary>
    public static class ResourceSource
    {
        public const string DAILY_CHECKIN = "daily_checkin";
        public const string LEVEL_REWARD = "level_reward";
        public const string FIRST_CLEAR = "first_clear";
        public const string STAR_REWARD = "star_reward";
        public const string DAILY_TASK = "daily_task";
        public const string ACHIEVEMENT = "achievement";
        public const string DAILY_LOGIN = "daily_login";
        public const string AD_REWARD = "ad_reward";
        public const string CHEST_REWARD = "chest_reward";
        public const string LEVEL_CONSUME = "level_consume";
    }
}
