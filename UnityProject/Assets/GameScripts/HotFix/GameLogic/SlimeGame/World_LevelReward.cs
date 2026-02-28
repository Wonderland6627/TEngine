using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 关卡通关奖励（World 的部分类）
    /// 公式驱动：所有奖励基于 levelId 自动计算，无需逐关配置
    /// </summary>
    public partial class World
    {
        /// <summary>
        /// 计算关卡通关奖励（纯计算，不发放）
        /// </summary>
        public LevelRewardResult CalculateLevelReward(int levelId, bool isFirstClear)
        {
            var rewardConfig = GetLevelRewardConfig();
            var energyConfig = GetEnergyConfig();
            if (rewardConfig == null || energyConfig == null)
            {
                Log.Error("[World] CalculateLevelReward failed: config is null");
                return null;
            }

            var result = new LevelRewardResult
            {
                levelId = levelId,
                isFirstClear = isFirstClear,
            };

            // coinReward = floor(baseCoin + coinPerLevel * (levelId - 1))
            int coinReward = (int)Math.Floor(rewardConfig.baseCoin + rewardConfig.coinPerLevel * (levelId - 1.0));
            result.baseRewards.Add(new RewardItem
            {
                type = CurrencyTypes.COIN,
                amount = coinReward,
                source = CurrencySource.LEVEL_REWARD,
            });

            // energyReturn = floor(levelConsume * energyReturnRate)
            int energyReturn = (int)Math.Floor(energyConfig.levelConsume * rewardConfig.energyReturnRate);
            if (energyReturn > 0)
            {
                result.baseRewards.Add(new RewardItem
                {
                    type = "energy",
                    amount = energyReturn,
                    source = EnergySource.LEVEL_REWARD,
                });
            }

            // firstClearCoin = floor(coinReward * firstClearMultiplier)
            if (isFirstClear)
            {
                int firstClearCoin = (int)Math.Floor(coinReward * rewardConfig.firstClearMultiplier);
                result.firstClearRewards.Add(new RewardItem
                {
                    type = CurrencyTypes.COIN,
                    amount = firstClearCoin,
                    source = CurrencySource.FIRST_CLEAR,
                });
            }

            // adBonusCoin = coinReward * (adMultiplier - 1)
            int adBonusCoin = coinReward * (rewardConfig.adMultiplier - 1);
            if (adBonusCoin > 0)
            {
                result.adBonusRewards.Add(new RewardItem
                {
                    type = CurrencyTypes.COIN,
                    amount = adBonusCoin,
                    source = CurrencySource.LEVEL_REWARD,
                });
            }

            Log.Info($"[World] CalculateLevelReward: level={levelId}, isFirstClear={isFirstClear}, " +
                     $"coin={coinReward}, energyReturn={energyReturn}, " +
                     $"firstClearCoin={(isFirstClear ? result.firstClearRewards[0].amount : 0)}, " +
                     $"adBonusCoin={adBonusCoin}");

            return result;
        }

        /// <summary>
        /// 领取关卡通关奖励（实际发放，调用服务端接口）
        /// </summary>
        public async UniTask<bool> ClaimLevelReward(LevelRewardResult result, bool watchedAd)
        {
            if (result == null)
            {
                Log.Error("[World] ClaimLevelReward failed: result is null");
                return false;
            }

            var rewards = result.GetClaimableRewards(watchedAd);
            bool allSuccess = true;

            foreach (var item in rewards)
            {
                bool ok = await DispatchRewardItem(item);
                if (!ok) allSuccess = false;
            }

            if (allSuccess)
            {
                GameEvent.Send(SlimeEvent.OnLevelRewardClaimed, result);
                Log.Info($"[World] ClaimLevelReward success: level={result.levelId}, watchedAd={watchedAd}");
            }

            return allSuccess;
        }

        /// <summary>
        /// 按 type 分发单个奖励项（扩展新货币类型只需加 case）
        /// </summary>
        private async UniTask<bool> DispatchRewardItem(RewardItem item)
        {
            switch (item.type)
            {
                case CurrencyTypes.COIN:
                    await AddCoin(item.amount, item.source);
                    return true;

                case "energy":
                    return await UpdateEnergyOnServer(item.amount, item.source);

                default:
                    Log.Warning($"[World] DispatchRewardItem: unknown reward type '{item.type}'");
                    return false;
            }
        }
    }
}
