namespace GameLogic
{
    /// <summary>
    /// 货币类型枚举（与服务端保持一致）
    /// </summary>
    public static class CurrencyType
    {
        /// <summary>
        /// 金币
        /// </summary>
        public const string COIN = "coin";
        
        /// <summary>
        /// 钻石（未来扩展）
        /// </summary>
        public const string DIAMOND = "diamond";
    }
    
    /// <summary>
    /// 货币来源枚举（与服务端保持一致）
    /// </summary>
    public static class CurrencySource
    {
        /// <summary>
        /// 每日签到
        /// </summary>
        public const string DAILY_CHECKIN = "daily_checkin";
        
        /// <summary>
        /// 推关奖励
        /// </summary>
        public const string LEVEL_REWARD = "level_reward";
        
        /// <summary>
        /// 首次通关
        /// </summary>
        public const string FIRST_CLEAR = "first_clear";
        
        /// <summary>
        /// 星级奖励
        /// </summary>
        public const string STAR_REWARD = "star_reward";
        
        /// <summary>
        /// 每日任务
        /// </summary>
        public const string DAILY_TASK = "daily_task";
        
        /// <summary>
        /// 成就奖励
        /// </summary>
        public const string ACHIEVEMENT = "achievement";
    }
}

