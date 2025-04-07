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

        public SlimeGameData()
        {
            _unlockedLevelId = PlayerPrefs.GetInt(Unlocked_Level_ID_Key, 1);
            LoadUserInfo();
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
