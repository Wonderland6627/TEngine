using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
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
                    Log.Error($"Set user info is null.");
                    return;
                }
                _userInfo = value;
                SaveUserInfo();
                Log.Info($"Set user info, Current: [{UserInfo.ToJson()}]");
            }
        }

        public SlimeGameData()
        {
            LoadUserInfo();
        }

        protected virtual void LoadUserInfo()
        {

        }

        protected virtual void SaveUserInfo()
        {

        }
    }

    public class WXUserData : SlimeGameData
    {
        private const string USER_INFO_KEY = "wx_user_info";

        protected override void SaveUserInfo()
        {
            if (UserInfo == null)
            {
                return;
            }
            string json = UserInfo.ToJson();
            GameModule.Setting.SetString(USER_INFO_KEY, json);
            GameModule.Setting.Save();
        }

        protected override void LoadUserInfo()
        {
            string json = GameModule.Setting.GetString(USER_INFO_KEY, "");
            if (string.IsNullOrEmpty(json))
            {
                Log.Warning($"WX Local User info is empty, Current: [{json}]");
                return;
            }
            var info = json.ToObject<UserInfo>();
            UserInfo = info;
            Log.Info($"WX Load user info, Current: [{json}]");
        }
    }

    public class EditorUserData : SlimeGameData
    {
        protected override void LoadUserInfo()
        {
            Log.Info($"Editor Load user info, Current: [{UserInfo.ToJson()}]");
        }
    }
}
