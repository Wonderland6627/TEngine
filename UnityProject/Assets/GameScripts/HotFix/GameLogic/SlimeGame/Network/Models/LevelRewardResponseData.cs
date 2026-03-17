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

    [Serializable]
    public class RewardItemData
    {
        public EItemType itemType;
        public int itemId;
        public int amount;
        public string source;
    }
}
