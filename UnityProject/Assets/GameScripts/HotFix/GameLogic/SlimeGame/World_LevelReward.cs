using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameLogic.Network;
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
        /// 计算关卡通关奖励（纯计算，不发放，用于 UI 预览）
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
        /// 领取关卡通关奖励（服务端统一结算，一次 HTTP 请求）
        /// 服务端根据 levelId 和 progressLevelID 自行判断 isFirstClear 并用公式计算奖励
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

            // 刷新本地用户数据（coin/energy/progressLevelID）
            FetchUserGameInfo();

            // 首通时更新微信排行榜
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
