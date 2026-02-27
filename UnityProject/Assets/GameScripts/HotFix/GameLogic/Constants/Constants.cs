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
    /// 货币类型枚举（对应服务端 constants.js -> CURRENCY_TYPES）
    /// </summary>
    public static class CurrencyTypes
    {
        /// <summary>
        /// 金币（对应服务端 CURRENCY_TYPES.COIN）
        /// </summary>
        public const string COIN = "coin";

        /// <summary>
        /// 钻石（对应服务端 CURRENCY_TYPES.DIAMOND，未来扩展）
        /// </summary>
        public const string DIAMOND = "diamond";
    }

    /// <summary>
    /// 货币来源枚举（对应服务端 constants.js -> CURRENCY_SOURCE）
    /// </summary>
    public static class CurrencySource
    {
        /// <summary>
        /// 每日签到（对应服务端 CURRENCY_SOURCE.DAILY_CHECKIN）
        /// </summary>
        public const string DAILY_CHECKIN = "daily_checkin";

        /// <summary>
        /// 推关奖励（对应服务端 CURRENCY_SOURCE.LEVEL_REWARD）
        /// </summary>
        public const string LEVEL_REWARD = "level_reward";

        /// <summary>
        /// 首次通关（对应服务端 CURRENCY_SOURCE.FIRST_CLEAR）
        /// </summary>
        public const string FIRST_CLEAR = "first_clear";

        /// <summary>
        /// 星级奖励（对应服务端 CURRENCY_SOURCE.STAR_REWARD）
        /// </summary>
        public const string STAR_REWARD = "star_reward";

        /// <summary>
        /// 每日任务（对应服务端 CURRENCY_SOURCE.DAILY_TASK）
        /// </summary>
        public const string DAILY_TASK = "daily_task";

        /// <summary>
        /// 成就奖励（对应服务端 CURRENCY_SOURCE.ACHIEVEMENT）
        /// </summary>
        public const string ACHIEVEMENT = "achievement";
    }
    
    /// <summary>
    /// 体力值来源枚举（对应服务端 constants.js -> ENERGY_SOURCE）
    /// </summary>
    public static class EnergySource
    {
        /// <summary>
        /// 每日登录赠送（对应服务端 ENERGY_SOURCE.DAILY_LOGIN）
        /// </summary>
        public const string DAILY_LOGIN = "daily_login";
        
        /// <summary>
        /// 看广告（对应服务端 ENERGY_SOURCE.AD_REWARD）
        /// </summary>
        public const string AD_REWARD = "ad_reward";
        
        /// <summary>
        /// 开宝箱（对应服务端 ENERGY_SOURCE.CHEST_REWARD）
        /// </summary>
        public const string CHEST_REWARD = "chest_reward";
        
        /// <summary>
        /// 进入关卡消耗（对应服务端 ENERGY_SOURCE.LEVEL_CONSUME）
        /// </summary>
        public const string LEVEL_CONSUME = "level_consume";
    }
}

