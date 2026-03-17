using System;
using System.Collections.Generic;

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
        public Dictionary<string, int> resources;
        public Dictionary<string, int> goods;
        public List<int> claimedLevelChests;
    }
}
