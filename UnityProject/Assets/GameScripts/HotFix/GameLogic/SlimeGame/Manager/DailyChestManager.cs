using System;
using Cysharp.Threading.Tasks;
using GameBase;
using GameLogic.Network;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public class DailyChestManager : Singleton<DailyChestManager>
    {
        private const string KEY_LAST_DATE = "DailyChest_LastClaimDate";

        private string GetServerDate()
        {
            var dt = NetManager.ServerTime.ToLocalTime().Date;
            return dt.ToString("yyyy-MM-dd");
        }

        public bool IsTodayClaimed()
        {
            var last = PlayerPrefs.GetString(KEY_LAST_DATE, "");
            var today = GetServerDate();
            return last == today;
        }

        public async UniTask<bool> CanClaimToday()
        {
            await NetManager.SyncServerTime();
            return !IsTodayClaimed();
        }

        public async UniTask<bool> TryClaimToday()
        {
            await NetManager.SyncServerTime();
            if (IsTodayClaimed()) return false;

            var config = World.Instance.GetEnergyConfig();
            if (config?.chestReward == null) return false;

            int amount = UnityEngine.Random.Range(config.chestReward.min, config.chestReward.max + 1);
            var today = GetServerDate();
            await World.Instance.UpdateResource(ResourceType.Coin, amount, ResourceSource.DAILY_CHECKIN);
            PlayerPrefs.SetString(KEY_LAST_DATE, today);
            PlayerPrefs.Save();
            return true;
        }
    }
}
