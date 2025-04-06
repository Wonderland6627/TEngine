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

        public SlimeGameData()
        {
            LoadUserInfo();
        }

        private const string USER_INFO_KEY = "slime_user_info";

        protected virtual void LoadUserInfo()
        {
            string json = PlayerPrefs.GetString(USER_INFO_KEY, "");
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
            PlayerPrefs.SetString(USER_INFO_KEY, json);
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
