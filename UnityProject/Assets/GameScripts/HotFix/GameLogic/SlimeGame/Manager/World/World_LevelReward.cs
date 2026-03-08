using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameLogic.Network;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 关卡通关奖励（World 的部分类）
    /// </summary>
    public partial class World
    {
        /// <summary>
        /// 计算关卡通关奖励（纯计算，不发放，用于 UI 预览）
        /// </summary>
        public LevelRewardResult CalculateLevelReward(int levelId, bool isFirstClear)
        {
            var gc = GlobalConfig;

            var result = new LevelRewardResult
            {
                levelId = levelId,
                isFirstClear = isFirstClear,
            };

            // coinReward = floor(baseCoin + coinPerLevel * (levelId - 1))
            int baseCoin = ResourceDef.GetDefaultValue(ResourceType.Coin);
            int coinReward = (int)Math.Floor(baseCoin + gc.CoinPerLevel * (levelId - 1.0));
            result.baseRewards.Add(new RewardItem
            {
                resourceType = ResourceType.Coin,
                amount = coinReward,
                source = ResourceSource.LEVEL_REWARD,
            });

            // energyReturn = floor(levelConsume * energyReturnRate)
            int energyReturn = (int)Math.Floor(gc.LevelEnergyConsume * (double)gc.EnergyReturnRate);
            if (energyReturn > 0)
            {
                result.baseRewards.Add(new RewardItem
                {
                    resourceType = ResourceType.Energy,
                    amount = energyReturn,
                    source = ResourceSource.LEVEL_REWARD,
                });
            }

            // firstClearCoin = floor(coinReward * firstClearMultiplier)
            if (isFirstClear)
            {
                int firstClearCoin = (int)Math.Floor(coinReward * (double)gc.FirstClearMultiplier);
                result.firstClearRewards.Add(new RewardItem
                {
                    resourceType = ResourceType.Coin,
                    amount = firstClearCoin,
                    source = ResourceSource.FIRST_CLEAR,
                });
            }

            // adBonusCoin = coinReward * (adMultiplier - 1)
            int adBonusCoin = (int)(coinReward * (gc.AdMultiplier - 1));
            if (adBonusCoin > 0)
            {
                result.adBonusRewards.Add(new RewardItem
                {
                    resourceType = ResourceType.Coin,
                    amount = adBonusCoin,
                    source = ResourceSource.LEVEL_REWARD,
                });
            }

            Log.Info($"[World] CalculateLevelReward: level={levelId}, isFirstClear={isFirstClear}, " +
                     $"coin={coinReward}, energyReturn={energyReturn}, " +
                     $"firstClearCoin={(isFirstClear ? result.firstClearRewards[0].amount : 0)}, " +
                     $"adBonusCoin={adBonusCoin}");

            return result;
        }

        /// <summary>
        /// 领取关卡通关奖励（服务端统一结算）
        /// </summary>
        public async UniTask<bool> ClaimLevelReward(int levelId, bool watchedAd)
        {
            var request = new
            {
                levelId = levelId,
                watchedAd = watchedAd,
            };

            var response = await NetManager.CallHttp<ClaimLevelRewardResponse>("claimLevelReward", request);
            if (!response.IsSuccess || response.data == null)
            {
                Log.Error($"[World] ClaimLevelReward failed: {response.ErrorMessage}");
                return false;
            }

            FetchUserGameInfo();

            if (response.data.isFirstClear)
            {
                UpdateWXLeaderboard(levelId);
            }

            GameEvent.Send(SlimeEvent.OnLevelRewardClaimed, response.data);
            Log.Info($"[World] ClaimLevelReward success: level={levelId}, watchedAd={watchedAd}, " +
                     $"isFirstClear={response.data.isFirstClear}, rewards count={response.data.rewards?.Count ?? 0}");

            return true;
        }
    }
}
