using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameLogic.Network;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 统一资源管理（World 的部分类）
    /// 替代原 World_Currency + World_Energy
    /// </summary>
    public partial class World
    {
        private const string Last_Daily_Energy_Reward_Date_Key = "slime_last_daily_energy_reward_date";

        /// <summary>
        /// 通用资源增减（调用服务器接口）
        /// </summary>
        /// <param name="type">资源类型</param>
        /// <param name="change">变化量（正数增加，负数扣除）</param>
        /// <param name="source">来源（ResourceSource 常量）</param>
        /// <returns>更新后的值，失败返回 -1</returns>
        public async UniTask<int> UpdateResource(ResourceType type, int change, string source)
        {
            var request = new
            {
                resourceType = (int)type,
                change = change,
                source = source
            };

            var response = await NetManager.CallHttp<UpdateResourceResponse>("updateResource", request);

            if (!response.IsSuccess || response.data == null)
            {
                Log.Error($"[World] UpdateResource failed: type={type}, change={change}, err={response.ErrorMessage}");
                return -1;
            }

            GameData.UpdateResource(type, response.data.value);
            return response.data.value;
        }

        /// <summary>
        /// 检查并处理每日登录体力奖励
        /// </summary>
        public async UniTask CheckDailyLoginReward()
        {
            var config = GetEnergyConfig();
            if (config == null)
            {
                Log.Warning("[World] EnergyConfig is null, skip daily login reward");
                return;
            }

            await NetManager.SyncServerTime();

            string lastRewardDateStr = PlayerPrefs.GetString(Last_Daily_Energy_Reward_Date_Key, "");
            DateTime? lastRewardDate = null;
            if (!string.IsNullOrEmpty(lastRewardDateStr) && DateTime.TryParse(lastRewardDateStr, out DateTime parsedDate))
            {
                lastRewardDate = parsedDate.Date;
            }

            DateTime today = NetManager.ServerTime.ToLocalTime().Date;

            if (lastRewardDate != null && lastRewardDate.Value >= today)
            {
                Log.Info("[World] Same day reward already claimed, skip daily login reward");
                return;
            }

            int currentEnergy = GameData.Energy;
            int rewardAmount = config.dailyLoginReward;
            int maxEnergy = config.energyMax;
            int actualReward = Math.Min(rewardAmount, maxEnergy - currentEnergy);

            if (actualReward <= 0)
            {
                Log.Info($"[World] Energy already at max ({currentEnergy}/{maxEnergy}), skip daily login reward");
                PlayerPrefs.SetString(Last_Daily_Energy_Reward_Date_Key, today.ToString("yyyy-MM-dd"));
                PlayerPrefs.Save();
                return;
            }

            int result = await UpdateResource(ResourceType.Energy, actualReward, ResourceSource.DAILY_LOGIN);
            if (result >= 0)
            {
                PlayerPrefs.SetString(Last_Daily_Energy_Reward_Date_Key, today.ToString("yyyy-MM-dd"));
                PlayerPrefs.Save();
                Log.Info($"[World] Daily login reward success: +{actualReward}");
            }
        }

        /// <summary>
        /// 尝试消耗体力（进入关卡前调用）
        /// </summary>
        public async UniTask<bool> TryConsumeEnergy()
        {
            var config = GetEnergyConfig();
            if (config == null)
            {
                Log.Error("[World] EnergyConfig is null");
                return false;
            }

            int consumeAmount = config.levelConsume;
            int currentEnergy = GameData.Energy;

            if (currentEnergy < consumeAmount)
            {
                Log.Warning($"[World] Energy not enough: {currentEnergy} < {consumeAmount}");
                GameEvent.Send(SlimeEvent.OnResourceNotEnough, ResourceType.Energy);
                return false;
            }

            int result = await UpdateResource(ResourceType.Energy, -consumeAmount, ResourceSource.LEVEL_CONSUME);
            if (result >= 0)
            {
                Log.Info($"[World] Consume energy success: -{consumeAmount}");
                return true;
            }
            return false;
        }
    }
}
