using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public enum RewardRarity: int
    {
        Rare = 0,       // 稀有
        Epic = 1,       // 史诗
        Legendary = 2,  // 传说
    }

    public class RewardAction
    {
        public RewardConfig config;     // 奖励配置
        public RewardRarity rarity;     // 奖励稀有度

        public string GetDescription()
        {
            string desc = config.rewardDesc;
            desc = desc.Replace("{effectValues}", GetEffectValue().ToString());
            desc = desc.Replace("{durations}", GetDuration().ToString());
            return desc;
        }

        private float GetEffectValue()
        {
            return config.effectValues[(int)rarity];
        }

        private float GetDuration()
        {
            return config.durations[(int)rarity];
        }

        private bool NeedAds()
        {
            return config.needAds[(int)rarity];
        }

        public string toString()
        {
            return $"[[RewardAction] desc: {GetDescription()}, rarity: {rarity}, rewardId: {config.rewardId}, rewardName: {config.rewardName}, rewardType: {config.rewardType}, needAds: {NeedAds()}]";
        }
    }

    public class SlimeReward
    {
        private readonly Dictionary<RewardRarity, float> _rarityProbabilities = new()
        {
            { RewardRarity.Rare, 0.5f },
            { RewardRarity.Epic, 0.4f },
            { RewardRarity.Legendary, 0.1f }
        };

        private RewardRarity GetRandomRarity()
        {
            float randomValue = Random.value;
            float probabilitySum = 0f;

            foreach (var pair in _rarityProbabilities)
            {
                probabilitySum += pair.Value;
                if (randomValue <= probabilitySum)
                {
                    return pair.Key;
                }
            }

            return RewardRarity.Rare; // 默认返回
        }

        public List<RewardAction> GetRewardActions(List<RewardConfig> rewardConfigs)
        {
            //获取随机3个RewardConfig, 再随机一个稀有度(稀有度Rare = 0.5, Epic = 0.4, Legendary = 0.1), 组成3个RewardAction
            List<RewardAction> actions = new List<RewardAction>();
            if (rewardConfigs == null || rewardConfigs.Count < 3)
            {
                return actions;
            }

            var selectedConfigs = new List<RewardConfig>();
            var tempConfigs = new List<RewardConfig>(rewardConfigs);
            
            for (int i = 0; i < 3 && tempConfigs.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, tempConfigs.Count);
                selectedConfigs.Add(tempConfigs[randomIndex]);
                tempConfigs.RemoveAt(randomIndex);
            }

            foreach (var config in selectedConfigs)
            {
                actions.Add(new RewardAction
                {
                    config = config,
                    rarity = GetRandomRarity()
                });
            }

            return actions;
        }

        public void Trigger(RewardAction action)
        {
            GameEvent.Send(SlimeEvent.OnRewardSelect, action);
        }
    }
}
