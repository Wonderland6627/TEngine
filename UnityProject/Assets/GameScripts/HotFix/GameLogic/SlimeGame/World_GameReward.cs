using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public partial class World
    {
        public SlimeReward slimeReward  { get; private set; } = new();

        public List<RewardAction> GetRewardActions()
        {
            return slimeReward.GetRewardActions(rewardReader.configs);
        }
    }
}
