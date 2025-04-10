using TEngine;
using WeChatWASM;

namespace GameLogic
{
    [System.Serializable]
    public class UserInfo
    {
        public string openId;
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
        private const string Unlocked_Level_ID_Key = "slime_unlocked_level_id";
        private const string Enable_Sound_Key = "slime_enable_sound";
        private const string Enable_Vibration_Key = "slime_enable_vibration";

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
                SaveUserInfo();
                Log.Info($"[SlimeGameData] Update user info, Current: [{UserInfo.ToJson()}]");
            }
        }

        private int _unlockedLevelId = 1;
        public int UnlockedLevelId
        {
            get => _unlockedLevelId;
            set 
            {
                _unlockedLevelId = value;
                PlayerPrefs.SetInt(Unlocked_Level_ID_Key, _unlockedLevelId);
                Log.Info($"[SlimeGameData] set unlock level id: [{_unlockedLevelId}]");
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

        public SlimeGameData()
        {
            _unlockedLevelId = PlayerPrefs.GetInt(Unlocked_Level_ID_Key, 1);
            _enableSound = PlayerPrefs.GetInt(Enable_Sound_Key, 1) == 1;
            _enableVibration = PlayerPrefs.GetInt(Enable_Vibration_Key, 1) == 1;
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

        protected virtual void SaveUserInfo()
        {
            if (UserInfo == null)
            {
                return;
            }
            string json = UserInfo.ToJson();
            PlayerPrefs.SetString(User_Info_Key, json);
            GameEvent.Get<IActorLogicEvent>().OnUserInfoUpdate(UserInfo);
        }
    }

    public class WXUserData : SlimeGameData
    {

    }

    public class EditorUserData : SlimeGameData
    {

    }
}
