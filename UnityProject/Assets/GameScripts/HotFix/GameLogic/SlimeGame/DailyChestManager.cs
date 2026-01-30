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

        public async UniTask<bool> TryClaimToday(int amount, object metadata = null)
        {
            await NetManager.SyncServerTime();
            if (IsTodayClaimed()) return false;
            var today = GetServerDate();
            var newCoin = await CurrencyManager.Instance.AddCoin(amount, CurrencySource.DAILY_CHECKIN, metadata);
            PlayerPrefs.SetString(KEY_LAST_DATE, today);
            PlayerPrefs.Save();
            return true;
        }
    }
}