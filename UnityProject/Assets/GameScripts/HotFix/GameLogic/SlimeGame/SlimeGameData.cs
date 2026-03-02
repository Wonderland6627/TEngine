using System;
using System.Collections.Generic;
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
        private const string Resources_Key_Prefix = "slime_res_";

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
        /// <summary>
        /// 已通关解锁的最高关卡ID
        /// </summary>
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

        // ========== 统一资源存储 ==========
        private readonly Dictionary<ResourceType, int> _resources = new();

        /// <summary>
        /// 获取指定资源的当前值
        /// </summary>
        public int GetResource(ResourceType type)
        {
            return _resources.TryGetValue(type, out int value) ? value : 0;
        }

        /// <summary>
        /// 更新资源值（从服务器数据同步），发送变化事件
        /// </summary>
        public void UpdateResource(ResourceType type, int newValue)
        {
            int oldValue = GetResource(type);
            if (oldValue == newValue) return;
            
            _resources[type] = newValue;
            PlayerPrefs.SetInt(Resources_Key_Prefix + (int)type, newValue);
            PlayerPrefs.Save();
            
            int change = newValue - oldValue;
            GameEvent.Send(SlimeEvent.OnResourceChanged, new ResourceChangedParam(type, change, newValue));
            Log.Info($"[SlimeGameData] Update resource {type}: {oldValue} -> {newValue} (change: {change})");
        }

        /// <summary>
        /// 批量更新所有资源（从服务器拉取后同步）
        /// </summary>
        public void UpdateAllResources(Dictionary<ResourceType, int> resources)
        {
            foreach (var kvp in resources)
            {
                UpdateResource(kvp.Key, kvp.Value);
            }
        }

        // 便捷属性
        public int Coin => GetResource(ResourceType.Coin);
        public int Energy => GetResource(ResourceType.Energy);
        public int Diamond => GetResource(ResourceType.Diamond);

        public SlimeGameData()
        {
            _progressLevelID = PlayerPrefs.GetInt(Progress_Level_ID_Key, 0);
            _enableSound = PlayerPrefs.GetInt(Enable_Sound_Key, 1) == 1;
            _enableVibration = PlayerPrefs.GetInt(Enable_Vibration_Key, 1) == 1;

            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                int defaultValue = type == ResourceType.Energy ? 150 : 0;
                _resources[type] = PlayerPrefs.GetInt(Resources_Key_Prefix + (int)type, defaultValue);
            }

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

        /// <summary>
        /// 更新已通关最高关卡ID（只增不减）
        /// </summary>
        public void SetProgressLevelID(int levelID)
        {
            int newValue = Math.Max(ProgressLevelID, levelID);
            if (newValue == ProgressLevelID) return;
            
            ProgressLevelID = newValue;
            GameEvent.Send(SlimeEvent.OnProgressLevelIDChanged, newValue);
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
    }

    public class WXUserData : SlimeGameData
    {

    }

    public class EditorUserData : SlimeGameData
    {

    }
}
