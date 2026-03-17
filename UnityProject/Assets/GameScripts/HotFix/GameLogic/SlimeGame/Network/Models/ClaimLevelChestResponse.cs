using System;
using System.Collections.Generic;

namespace GameLogic.Network
{
    /// <summary>
    /// 推关激励宝箱领取响应
    /// </summary>
    [Serializable]
    public class ClaimLevelChestResponse
    {
        public List<RewardItemData> rewards;
        public List<int> claimedLevelChests;
        public Dictionary<string, int> resources;
        public Dictionary<string, int> goods;
    }
}
