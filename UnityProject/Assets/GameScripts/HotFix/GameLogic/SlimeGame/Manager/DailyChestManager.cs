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
            var dt = NetManager.Instance.ServerTime.ToLocalTime().Date;
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
            await NetManager.Instance.SyncServerTime();
            if (IsTodayClaimed()) return false;

            var response = await NetManager.Instance.CallHttp<ClaimDailyCheckinResponse>("claimDailyCheckin");
            if (!response.IsSuccess || response.data == null)
            {
                Log.Error($"[DailyChest] ClaimDailyCheckin failed: {response.ErrorMessage}");
                return false;
            }

            World.Instance.SyncResources(response.data.resources);
            World.Instance.SyncGoods(response.data.goods);

            PlayerPrefs.SetString(KEY_LAST_DATE, GetServerDate());
            PlayerPrefs.Save();
            return true;
        }
    }
}
