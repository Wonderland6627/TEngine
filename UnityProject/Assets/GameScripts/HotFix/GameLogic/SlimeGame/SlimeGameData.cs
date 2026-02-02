using System;
using TEngine;

namespace GameLogic
{
    [System.Serializable]
    public class UserInfo
    {
        public string openID;

        public string nickName;
        public string avatarUrl;
        public double gender; // 0：未知、1：男、2：女
        public string country;
        public string province;
        public string city;
        public string language;
    }

    public class SlimeGameData
    {
        private const string User_Info_Key = "slime_user_info";
        private const string Progress_Level_ID_Key = "slime_progress_level_id";
        private const string Guide_Finish_Key = "slime_guide_finish";
        private const string Enable_Sound_Key = "slime_enable_sound";
        private const string Enable_Vibration_Key = "slime_enable_vibration";
        private const string Request_User_Info_Date_Key = "slime_request_userinfo_date";
        private const string Coin_Key = "slime_coin";
        private const string Energy_Key = "slime_energy";

        private UserInfo _userInfo = new();
        public UserInfo UserInfo
        {
            get => _userInfo;
            set
            {
                if (value == null)
                {
                    Log.Error($"[SlimeGameData] Set user info is null.");
                    return;
                }

                _userInfo = value;
                string json = _userInfo.ToJson();
                PlayerPrefs.SetString(User_Info_Key, json);
                GameEvent.Send(SlimeEvent.OnUserInfoUpdate, _userInfo);
                Log.Info($"[SlimeGameData] Update user info, Current: [{json}]");
            }
        }

        private int _progressLevelID = 0;
        public int ProgressLevelID
        {
            get => _progressLevelID;
            private set 
            {
                _progressLevelID = value;
                PlayerPrefs.SetInt(Progress_Level_ID_Key, _progressLevelID);
                Log.Info($"[SlimeGameData] set progress level id: [{_progressLevelID}]");
            }
        }

        private bool _guideFinish = false;
        public bool GuideFinish
        {
            get => _guideFinish;
            set 
            {
                _guideFinish = value;
                PlayerPrefs.SetInt(Guide_Finish_Key, _guideFinish? 1 : 0);
                Log.Info($"[SlimeGameData] set guide finish: [{_guideFinish}]");  
            } 
        }

        private bool _enableSound = true;
        public bool EnableSound
        {
            get => _enableSound;
            set
            {
                _enableSound = value;
                PlayerPrefs.SetInt(Enable_Sound_Key, _enableSound ? 1 : 0);
                Log.Info($"[SlimeGameData] set enable sound: [{_enableSound}]");
            }
        }

        private bool _enableVibration = true;
        public bool EnableVibration
        {
            get => _enableVibration;
            set
            {
                _enableVibration = value;
                PlayerPrefs.SetInt(Enable_Vibration_Key, _enableVibration ? 1 : 0);
                Log.Info($"[SlimeGameData] set enable vibration: [{_enableVibration}]");
            }
        }

        private int _coin = 0;
        /// <summary>
        /// 当前金币数量
        /// </summary>
        public int Coin => _coin;

        private int _energy = 150; // 默认体力值
        /// <summary>
        /// 当前体力值
        /// </summary>
        public int Energy => _energy;

        public SlimeGameData()
        {
            _progressLevelID = PlayerPrefs.GetInt(Progress_Level_ID_Key, 0);
            _enableSound = PlayerPrefs.GetInt(Enable_Sound_Key, 1) == 1;
            _enableVibration = PlayerPrefs.GetInt(Enable_Vibration_Key, 1) == 1;
            _coin = PlayerPrefs.GetInt(Coin_Key, 0);
            _energy = PlayerPrefs.GetInt(Energy_Key, 150); // 默认150
            LoadUserInfo();
            Log.Info($"[SlimeGameData] init data: {this.ToJson()}");
        }

        protected virtual void LoadUserInfo()
        {
            string json = PlayerPrefs.GetString(User_Info_Key, "");
            if (string.IsNullOrEmpty(json))
            {
                Log.Warning($"[SlimeGameData] Local User info is empty, Current: [{json}]");
                return;
            }
            var info = json.ToObject<UserInfo>();
            Log.Info($"[SlimeGameData] Load user info, Current: [{json}]");
            UserInfo = info;
        }

        public void SetProgressLevelID(int levelID)
        {
            ProgressLevelID = Math.Max(ProgressLevelID, levelID);
        }

        /// <summary>
        /// 是否在今天已经触发过“请求用户信息”（用于控制每天最多触发一次）
        /// </summary>
        public bool HasRequestedUserInfoToday()
        {
            string today = DateTime.Now.ToString("yyyyMMdd");
            string last = PlayerPrefs.GetString(Request_User_Info_Date_Key, "");
            return last == today;
        }

        /// <summary>
        /// 标记今天已经触发过“请求用户信息”
        /// </summary>
        public void MarkRequestedUserInfoToday()
        {
            string today = DateTime.Now.ToString("yyyyMMdd");
            PlayerPrefs.SetString(Request_User_Info_Date_Key, today);
            Log.Info($"[SlimeGameData] MarkRequestedUserInfoToday: [{today}]");
        }

        /// <summary>
        /// 更新金币数量（从服务器数据同步）
        /// </summary>
        /// <param name="coin">金币数量</param>
        public void UpdateCoin(int coin)
        {
            if (_coin == coin) return;
            
            _coin = coin;
            PlayerPrefs.SetInt(Coin_Key, _coin);
            PlayerPrefs.Save();
            GameEvent.Send(SlimeEvent.OnCoinChanged, _coin);
            Log.Info($"[SlimeGameData] Update coin: {_coin}");
        }

        /// <summary>
        /// 更新体力值（从服务器数据同步）
        /// </summary>
        /// <param name="energy">体力值</param>
        public void UpdateEnergy(int energy)
        {
            if (_energy == energy) return;
            
            _energy = energy;
            PlayerPrefs.SetInt(Energy_Key, _energy);
            PlayerPrefs.Save();
            GameEvent.Send(SlimeEvent.OnEnergyChanged, _energy);
            Log.Info($"[SlimeGameData] Update energy: {_energy}");
        }
    }

    public class WXUserData : SlimeGameData
    {

    }

    public class EditorUserData : SlimeGameData
    {

    }
}
