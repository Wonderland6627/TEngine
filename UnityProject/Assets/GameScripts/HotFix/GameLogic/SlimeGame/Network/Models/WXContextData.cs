using System;

namespace GameLogic.Network
{
    /// <summary>
    /// 微信上下文数据模型
    /// </summary>
    [Serializable]
    public class WXContextData
    {
        [Newtonsoft.Json.JsonProperty("event")]
        public object eventData;
        public string openid;
        public string appid;
        public string unionid;
    }
}

