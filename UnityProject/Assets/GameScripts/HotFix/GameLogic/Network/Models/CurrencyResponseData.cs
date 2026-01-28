using System;

namespace GameLogic.Network
{
    /// <summary>
    /// 增加货币响应数据
    /// </summary>
    [Serializable]
    public class AddCurrencyResponse
    {
        public int coin;
        public int added;
        public string source;
        public object metadata;
    }
    
    /// <summary>
    /// 扣除货币响应数据
    /// </summary>
    [Serializable]
    public class DeductCurrencyResponse
    {
        public int coin;
        public int deducted;
        public string reason;
    }
}

