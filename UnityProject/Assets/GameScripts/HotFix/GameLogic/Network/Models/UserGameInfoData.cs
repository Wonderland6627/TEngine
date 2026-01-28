using System;

namespace GameLogic.Network
{
    /// <summary>
    /// 用户游戏信息数据模型
    /// </summary>
    [Serializable]
    public class UserGameInfoData
    {
        public string _id = "";
        public int progressLevelID = 0;
        public string nickName = "";
        public string avatarUrl = "";
        public string openID = "";
        public int coin = 0;  // 金币（懒加载，老用户可能不存在，视为0）
    }
}

