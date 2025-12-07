using System;

namespace GameLogic.Network
{
    /// <summary>
    /// 用户游戏信息数据模型
    /// </summary>
    [Serializable]
    public class UserGameInfoData
    {
        public string _id;
        public string openid;
        public int? progressLevelID;
        public string nickName;
        public string avatarUrl;
        public string openId;
        public DateTime createdAt;
        public DateTime updatedAt;
    }
}

