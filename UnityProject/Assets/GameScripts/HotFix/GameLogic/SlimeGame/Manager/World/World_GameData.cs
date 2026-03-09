using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TEngine;
using WeChatWASM;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
using GameLogic.Network;

namespace GameLogic
{
    partial class World
    {
        public SlimeGameData GameData =
#if UNITY_EDITOR
            new EditorUserData();
#else
            new WXUserData();
#endif
        private static string WXCloudENV = "slimecloudservice-6enxmrfbc5bddc";

        public void InitWX(System.Action<bool> callback)
        {
            Log.Info("[World] InitWX WX.InitSDK");
            WX.InitSDK((code) =>
            {
                Log.Info($"[World] InitWX WX.InitSDK callback code: {code}");
                // if (code != 0)
                // {
                //     Log.Error($"World InitWX WX.InitSDK failed with code: {code}");
                //     callback?.Invoke(false);
                //     return;
                // }

                WX.cloud.Init(new ICloudConfig()
                {
                    env = WXCloudENV,
                    traceUser = true
                });
                callback?.Invoke(true);
            });
        }

        /// <summary>
        /// 请求用户信息（2021年后新版本必须通过用户主动触发获取）
        /// 直接创建用户信息按钮，无需先检查授权状态
        /// </summary>
        public void RequestUserInfo()
        {
            Log.Info("[World] RequestUserInfo - Create user info button");
            // 2021年后，wx.getUserInfo已废弃，必须通过用户主动触发（如点击按钮）获取用户信息
            // 因此直接创建用户信息按钮，无需先检查授权状态
            WX_CreateUserInfoButton();
        }

        [Obsolete("使用WX_CreateUserInfoButton直接获取WXUserInfo")]
        private void WX_GetUserInfo(string log)
        {
            Log.Info($"[World] WX.GetUserInfo: {log}");
            WX.GetUserInfo(new GetUserInfoOption()
            {
                success = (res) =>
                {
                    Log.Info($"[World] WX.GetUserInfo success: {res.ToJson()}");
                    var result = res.userInfo;
                    var currentUserInfo = GameData.UserInfo;
                    currentUserInfo.nickName = result.nickName;
                    currentUserInfo.avatarUrl = result.avatarUrl;
                    currentUserInfo.gender = result.gender;
                    currentUserInfo.province = result.province;
                    currentUserInfo.city = result.city;
                    currentUserInfo.country = result.country;
                    currentUserInfo.language = result.language;
                    GameData.UserInfo = currentUserInfo;
                    
                    // 更新用户信息后，同步到云端 只更新昵称和头像，不更新进度
                    NetManager.Instance.CallHttp<object>("setUserGameInfoV2", new Dictionary<string, object> { { "nickName", result.nickName }, { "avatarUrl", result.avatarUrl } }).Forget();
                },
                fail = (err) =>
                {
                    Log.Error($"[World] WX.GetUserInfo fail: {err.ToJson()}");
                }
            });
        }

        private void WX_CreateUserInfoButton()
        {
            Log.Info("[World] WX.CreateUserInfoButton");
            var button = WX.CreateUserInfoButton(0, 0, Screen.width, Screen.height, "zh_CN", false);
            button.OnTap((tapRes) =>
            {
                Log.Info("[World] WX.CreateUserInfoButton OnTap: " + tapRes.ToJson());
                button.Hide();
                if (tapRes.errCode != 0) return;
                OnGetWXUserInfo(tapRes.userInfo);
            });
        }

        private void OnGetWXUserInfo(WXUserInfo userInfo)
        {
            var currentUserInfo = GameData.UserInfo;
            currentUserInfo.nickName = userInfo.nickName;
            currentUserInfo.avatarUrl = userInfo.avatarUrl;
            currentUserInfo.gender = userInfo.gender;
            currentUserInfo.province = userInfo.province;
            currentUserInfo.city = userInfo.city;
            currentUserInfo.country = userInfo.country;
            currentUserInfo.language = userInfo.language;
            GameData.UserInfo = currentUserInfo;

            NetManager.Instance.CallHttp<object>("setUserGameInfoV2", new Dictionary<string, object> 
            { 
                { "nickName", userInfo.nickName }, 
                { "avatarUrl", userInfo.avatarUrl } 
            }).Forget();
        }

        [Obsolete("云函数可以直接调用getWXContext")]
        private async void GetOpenID()
        {
            Log.Info("[World] GetOpenID");
            Response<WXContextData> response = await NetManager.Instance.CallHttp<WXContextData>("getUserWXContext");
            if (response.IsSuccess && response.data != null)
            {
                var currentUserInfo = GameData.UserInfo;
                currentUserInfo.openID = response.data.openid;
                GameData.UserInfo = currentUserInfo;
                Log.Info($"[World] GetOpenID success: {response.data.openid}");
            }
            else
            {
                string errorMsg = response.ErrorMessage;
                if (string.IsNullOrEmpty(errorMsg))
                {
                    errorMsg = $"code={response.code}, msg={(string.IsNullOrEmpty(response.msg) ? "null" : response.msg)}, data={(response.data == null ? "null" : "not null")}";
                }
                Log.Error($"[World] GetOpenID failed: {errorMsg}");
            }
        }
    }

    partial class World
    {
        /// <summary>
        /// 统一登录入口（使用平台登录管理器）
        /// </summary>
        public async UniTask<bool> Login()
        {
            // 检查是否需要登录
            if (!NetManager.Instance.NeedLogin)
            {
                Log.Info($"[World] Using cached token, remaining: {NetManager.Instance.GetTokenRemainingSeconds()}s");
                return true;
            }
            
            Log.Info("[World] Starting platform login");
            bool success = await PlatformManager.Login();
            if (success)
            {
                // 登录成功后，更新本地用户信息（从token中解析或从服务器获取）
                await UpdateUserInfoFromLogin();
            }
            
            return success;
        }
        
        /// <summary>
        /// 登录成功后更新用户信息
        /// </summary>
        private async UniTask UpdateUserInfoFromLogin()
        {
            // 方式1: 从getCode2Session响应中获取openid（如果返回了）
            // 方式2: 调用getUserWXContext获取（如果需要更多信息）
            // 这里先尝试从登录响应中获取，如果没有则调用接口
            
            // 暂时先调用getUserWXContext获取openid
            var response = await NetManager.Instance.CallHttp<WXContextData>("getUserWXContext");
            if (response.IsSuccess && response.data != null && !string.IsNullOrEmpty(response.data.openid))
            {
                var currentUserInfo = GameData.UserInfo;
                currentUserInfo.openID = response.data.openid;
                GameData.UserInfo = currentUserInfo;
                Log.Info($"[World] Updated user info from login: openid={response.data.openid}");
            }
        }
    }

    partial class World
    {
        public async void FetchUserGameInfo(Action<bool> callback = null)
        {
            var response = await NetManager.Instance.CallHttp<UserGameInfoData>("getUserGameInfoV2");
            if (!response.IsSuccess)
            {
                Log.Error($"[World] fetch user game info failed: {response.ErrorMessage}");
                callback?.Invoke(false);
                return;
            }
            var userInfo = response.data;
            if (userInfo == null)
            {
                Log.Error($"[World] fetch user game info failed, userInfo is null");
                callback?.Invoke(false);
                return;
            }

            var curUserInfo = GameData.UserInfo;
            curUserInfo.openID = userInfo.openID;
            curUserInfo.nickName = userInfo.nickName;
            curUserInfo.avatarUrl = userInfo.avatarUrl;
            GameData.UserInfo = curUserInfo;
            GameData.SetProgressLevelID(userInfo.progressLevelID);
            
            // 同步资源数据
            if (userInfo.resources != null)
            {
                foreach (var kvp in userInfo.resources)
                {
                    if (int.TryParse(kvp.Key, out int typeId) && System.Enum.IsDefined(typeof(ResourceType), typeId))
                    {
                        GameData.UpdateResource((ResourceType)typeId, kvp.Value);
                    }
                }
            }
            callback?.Invoke(true);
            Log.Info($"[World] fetch user game info success: {userInfo.ToJson()}");
        }

        /// <summary>
        /// 更新微信排行榜（WX 开放数据域 + 云存储）
        /// </summary>
        public void UpdateWXLeaderboard(int levelId)
        {
            var kvDataList = new List<KVData>
            {
                new() { key = "progressLevelID", value = levelId.ToString() }
            };
            var nickName = GameData.UserInfo.nickName;
            var avatarUrl = GameData.UserInfo.avatarUrl;
            if (!string.IsNullOrEmpty(nickName)) kvDataList.Add(new KVData() { key = "nickName", value = nickName });
            if (!string.IsNullOrEmpty(avatarUrl)) kvDataList.Add(new KVData() { key = "avatarUrl", value = avatarUrl });
            WX.SetUserCloudStorage(new SetUserCloudStorageOption()
            {
                KVDataList = kvDataList.ToArray(),
            });

            var msgData = new
            {
                type = "setUserRecord",
                score = levelId,
            };
            WX.GetOpenDataContext().PostMessage(msgData.ToJson());

            Log.Info($"[World] UpdateWXLeaderboard: levelId={levelId}");
        }

        // 排行榜查询缓存：每1分钟只真正查询一次
        private List<PlayerRankInfo> m_CachedRankList = null;
        private float m_RankListCacheTime = 0f;
        private const float RANK_LIST_CACHE_DURATION = 60f;

        public async UniTask<List<PlayerRankInfo>> GetUserRankList()
        {
            Log.Info("[World] GetUserRankList");

#if UNITY_EDITOR
            // var list = MockData.GetMockGameInfoData(15, true);
            // return GetRankListFromResponse(list);
#endif
            float currentTime = Time.realtimeSinceStartup;
            if (m_CachedRankList != null && (currentTime - m_RankListCacheTime) < RANK_LIST_CACHE_DURATION)
            {
                Log.Info($"[World] GetUserRankList: return cached data, cache age: {currentTime - m_RankListCacheTime:F1}s");
                return m_CachedRankList;
            }

            var response = await NetManager.Instance.CallHttp<List<UserGameInfoData>>("getUserRankListV2");
            if (response.IsSuccess && response.data != null)
            {
                var rankList = GetRankListFromResponse(response.data);
                m_CachedRankList = rankList;
                m_RankListCacheTime = currentTime;
                Log.Info($"[World] GetUserRankList: cache updated");
                return rankList;
            }
            else
            {
                Log.Error($"[World] GetUserRankList failed: {response.ErrorMessage}");
                return m_CachedRankList;
            }

            List<PlayerRankInfo> GetRankListFromResponse(List<UserGameInfoData> userList)
            {
                Log.Info($"[World] GetRankListFromResponse: user count = {userList.Count}");
                
                List<PlayerRankInfo> rankList = new List<PlayerRankInfo>();
                foreach (var userData in userList)
                {
                    // 云函数已做筛选，这里只做基本验证
                    int progressLevelID = userData.progressLevelID;
                    string nickName = userData.nickName ?? "";
                    string avatarUrl = userData.avatarUrl ?? "";
                    string openID = userData.openID ?? "";
                    
                    if (progressLevelID <= 0 || 
                        string.IsNullOrEmpty(nickName) || 
                        string.IsNullOrEmpty(avatarUrl) ||
                        string.IsNullOrEmpty(openID))
                    {
                        continue;
                    }
                    
                    PlayerRankInfo rankInfo = new PlayerRankInfo();
                    rankInfo.openID = openID;
                    rankInfo.progressLevelID = progressLevelID;
                    rankInfo.nickName = nickName;
                    rankInfo.avatarUrl = avatarUrl;
                    
                    if (!rankInfo.IsValid()) continue;
                    rankList.Add(rankInfo);
                }

                // 排序：关卡等级从高到低；同一关卡等级内，把玩家自己排到最前面
                string selfOpenId = GameData.UserInfo != null ? (GameData.UserInfo.openID ?? "") : "";
                rankList.Sort((a, b) =>
                {
                    int levelCompare = b.progressLevelID.CompareTo(a.progressLevelID);
                    if (levelCompare != 0) return levelCompare;

                    if (!string.IsNullOrEmpty(selfOpenId))
                    {
                        bool aIsSelf = a != null && a.openID == selfOpenId;
                        bool bIsSelf = b != null && b.openID == selfOpenId;
                        if (aIsSelf != bIsSelf) return aIsSelf ? -1 : 1;
                    }

                    string aId = a != null ? (a.openID ?? "") : "";
                    string bId = b != null ? (b.openID ?? "") : "";
                    return string.CompareOrdinal(aId, bId);
                });
                
                for (int i = 0; i < rankList.Count; i++)
                {
                    rankList[i].playerRank = i + 1;
                }
                
                Log.Info($"[World] GetRankListFromResponse success, valid rank info count: {rankList.Count}");
                return rankList;
            }
        }

        public void ShowFriendsRank(RawImage rawImage)
        {
            Log.Info("[World] ShowFriendsRank");
            if (rawImage == null)
            {
                Log.Error("[World] ShowFriendsRank rawImage is null!");
                return;
            }
            CanvasScaler scaler = UIModule.UIRootStatic.GetComponentInParent<CanvasScaler>();
            Vector2 referenceResolution = scaler.referenceResolution;
            if (scaler == null)
            {
                Log.Error($"[World] ShowFriendsRank Not found {nameof(CanvasScaler)} !");
                return;
            }

            Vector3 imgPos = rawImage.transform.position;
            WX.ShowOpenData(rawImage.texture,
                (int)imgPos.x,
                Screen.height - (int)imgPos.y,
                (int)(Screen.width / referenceResolution.x * rawImage.rectTransform.rect.width),
                (int)(Screen.width / referenceResolution.x * rawImage.rectTransform.rect.height));
            var msgData = new
            {
                type = "showFriendsRank",
            };
            WX.GetOpenDataContext().PostMessage(msgData.ToJson());
        }

        public void DestroyOpenDataRenderer()
        {
            var msgData = new
            {
                type = "WXDestroy",
            };
            WX.GetOpenDataContext().PostMessage(msgData.ToJson());
        }
    }
}