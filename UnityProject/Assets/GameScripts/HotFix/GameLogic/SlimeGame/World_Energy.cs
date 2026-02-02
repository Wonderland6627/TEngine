using System;
using Cysharp.Threading.Tasks;
using GameLogic.Network;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 体力值管理（World 的部分类）
    /// 负责每日登录奖励检查、进入关卡前的体力检查
    /// </summary>
    public partial class World
    {
        private const string Last_Daily_Energy_Reward_Date_Key = "slime_last_daily_energy_reward_date";

        /// <summary>
        /// 检查并处理每日登录奖励（登录后调用）
        /// </summary>
        public async UniTask CheckDailyLoginReward()
        {
            var config = GetEnergyConfig();
            if (config == null)
            {
                Log.Warning("[World] EnergyConfig is null, skip daily login reward");
                return;
            }

            // 确保服务器时间已同步
            await NetManager.SyncServerTime();

            // 获取本地存储的上次领取日期
            string lastRewardDateStr = PlayerPrefs.GetString(Last_Daily_Energy_Reward_Date_Key, "");
            DateTime? lastRewardDate = null;
            if (!string.IsNullOrEmpty(lastRewardDateStr) && DateTime.TryParse(lastRewardDateStr, out DateTime parsedDate))
            {
                lastRewardDate = parsedDate.Date;
            }

            DateTime today = NetManager.ServerTime.ToLocalTime().Date;

            // 检查是否需要奖励
            if (lastRewardDate != null && lastRewardDate.Value >= today)
            {
                Log.Info("[World] Same day reward already claimed, skip daily login reward");
                return;
            }

            // 计算奖励数量（赠满上限为止）
            int currentEnergy = GameData.Energy;
            int rewardAmount = config.dailyLoginReward;
            int maxEnergy = config.energyMax;
            int actualReward = Math.Min(rewardAmount, maxEnergy - currentEnergy);

            if (actualReward <= 0)
            {
                Log.Info($"[World] Energy already at max ({currentEnergy}/{maxEnergy}), skip daily login reward");
                // 保存日期，避免同一天重复检查
                PlayerPrefs.SetString(Last_Daily_Energy_Reward_Date_Key, today.ToString("yyyy-MM-dd"));
                PlayerPrefs.Save();
                return;
            }

            // 调用服务端接口校验
            bool success = await UpdateEnergyOnServer(actualReward, EnergySource.DAILY_LOGIN);
            if (success)
            {
                // 保存领取日期
                PlayerPrefs.SetString(Last_Daily_Energy_Reward_Date_Key, today.ToString("yyyy-MM-dd"));
                PlayerPrefs.Save();
                Log.Info($"[World] Daily login reward success: +{actualReward}");
            }
        }

        /// <summary>
        /// 尝试消耗体力（进入关卡前调用）
        /// </summary>
        /// <returns>是否成功消耗</returns>
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

            // 客户端判断体力是否足够
            if (currentEnergy < consumeAmount)
            {
                Log.Warning($"[World] Energy not enough: {currentEnergy} < {consumeAmount}");
                GameEvent.Send(SlimeEvent.OnEnergyNotEnough, consumeAmount);
                return false;
            }

            // 调用服务端接口校验
            bool success = await UpdateEnergyOnServer(-consumeAmount, EnergySource.LEVEL_CONSUME);
            if (success)
            {
                Log.Info($"[World] Consume energy success: -{consumeAmount}");
            }
            return success;
        }

        /// <summary>
        /// 调用服务端接口更新体力值
        /// </summary>
        /// <param name="change">体力值变化量（正数为增加，负数为消耗）</param>
        /// <param name="source">来源（EnergySource 常量）</param>
        /// <returns>是否成功</returns>
        private async UniTask<bool> UpdateEnergyOnServer(int change, string source)
        {
            var request = new { change = change, source = source };
            var response = await NetManager.CallHttp<UpdateEnergyResponse>("updateEnergy", request);

            if (!response.IsSuccess || response.data == null)
            {
                Log.Error($"[World] Update energy failed: {response.ErrorMessage}");
                return false;
            }

            // 更新本地体力值
            GameData.UpdateEnergy(response.data.energy);
            Log.Info($"[World] Update energy success: {change} ({source}), new energy: {response.data.energy}");
            return true;
        }
    }
}


