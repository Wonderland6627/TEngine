using System;

namespace GameLogic.Network
{
    /// <summary>
    /// 登录会话数据模型（getCode2Session接口返回）
    /// </summary>
    [Serializable]
    public class SessionData
    {
        public string openid;
        public string token;
        public string session_key;
        public string unionid;
        public string appid;
        public string platform;
    }
}

