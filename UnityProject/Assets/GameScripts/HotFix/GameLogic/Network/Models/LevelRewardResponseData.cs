using System;
using System.Collections.Generic;

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
        public string type;
        public int amount;
        public string source;
    }
}
