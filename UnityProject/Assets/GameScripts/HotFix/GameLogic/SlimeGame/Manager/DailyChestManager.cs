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

        public bool CanClaimToday()
        {
            return !IsTodayClaimed();
        }

        public async UniTask<bool> TryClaimToday()
        {
            if (IsTodayClaimed()) return false;

            var chestReward = ConfigSystem.Instance.Tables.TbGlobalConfig.DailyChestEnergyReward;
            if (chestReward == null) return false;

            int amount = UnityEngine.Random.Range(chestReward.Min, chestReward.Max + 1);
            var today = GetServerDate();
            await World.Instance.UpdateResource(ResourceType.Coin, amount, ResourceSource.DAILY_CHECKIN);
            PlayerPrefs.SetString(KEY_LAST_DATE, today);
            PlayerPrefs.Save();
            return true;
        }
    }
}
