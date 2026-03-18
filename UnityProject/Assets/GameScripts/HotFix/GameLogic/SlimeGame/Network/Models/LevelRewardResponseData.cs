using System;
using System.Collections.Generic;
using GameConfig;

namespace GameLogic.Network
{
    [Serializable]
    public class ClaimLevelRewardResponse
    {
        public List<RewardItemData> rewards;
        public bool isFirstClear;
    }

    /// <summary>
    /// 服务端奖励条目：与 ITEM_TYPE(1=Resource,2=Goods) + item_id 一致
    /// </summary>
    [Serializable]
    public class RewardItemData
    {
        public EItemType itemType;
        public int itemId;
        public int amount;
        public string source;
    }
}
