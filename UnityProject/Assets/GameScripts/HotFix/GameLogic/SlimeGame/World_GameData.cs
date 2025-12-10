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
        // new WXUserData();
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
            CreateUserInfoButton();
        }

        private void GetUserInfo(string log)
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
                    NetManager.Call<object>("setUserGameInfoV2", new Dictionary<string, object> { { "nickName", result.nickName }, { "avatarUrl", result.avatarUrl } }).Forget();
                },
                fail = (err) =>
                {
                    Log.Error($"[World] WX.GetUserInfo fail: {err.ToJson()}");
                }
            });
        }

        private void CreateUserInfoButton()
        {
            Log.Info("[World] WX.CreateUserInfoButton");
            var button = WX.CreateUserInfoButton(0, 0, Screen.width, Screen.height, "zh_CN", false);
            button.OnTap((tapRes) =>
            {
                Log.Info("[World] WX.CreateUserInfoButton OnTap: " + tapRes.ToJson());
                button.Hide();
                GetUserInfo("by create user info button");
            });
        }

        private async void GetOpenID()
        {
            Log.Info("[World] GetOpenID");
            try
            {
                var response = await NetManager.Call<WXContextData>("getUserWXContext");
                if (response.IsSuccess && response.data != null)
                {
                    var currentUserInfo = GameData.UserInfo;
                    currentUserInfo.openId = response.data.openid;
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
            catch (Exception e)
            {
                Log.Error($"[World] GetOpenID exception: {e}");
                Log.Error($"[World] GetOpenID exception stack: {e.StackTrace}");
            }
        }
    }

    partial class World
    {
        public void InitEditor()
        {
            if (string.IsNullOrEmpty(GameData.UserInfo.openId))
            {
                GameData.UserInfo = new UserInfo()
                {
                    openId = "test",
                    nickName = "EditorPlayer",
                    avatarUrl = "https://i1.hdslb.com/bfs/face/6532675fc11c0451826ab97f53a771a0d9d4ef79.jpg@150w_150h.jpg",
                    gender = 1,
                    province = "",
                    city = "",
                    country = "",
                    language = ""
                };
            }
        }
    }

    partial class World
    {
        public async void GetUserGameInfo()
        {
            Log.Info("[World] GetUserGameInfo");
            try
            {
                // 使用 V2 版本，返回新格式数据
                var response = await NetManager.Call<UserGameInfoData>("getUserGameInfoV2");
                if (response.IsSuccess && response.data != null)
            {
                    var userData = response.data;
                    // 直接使用新字段，不再使用兼容属性
                    int progressLevelID = userData.progressLevelID ?? 0;
                    if (progressLevelID > 0)
                        {
                        GameData.SetProgressLevelID(progressLevelID);
                        Log.Info($"[World] GetUserGameInfo success: progressLevelID = {progressLevelID}");
                    }
                    else
                    {
                        Log.Warning("[World] GetUserGameInfo success, but progressLevelID is 0, new user");
                    }
                }
                else
                    {
                    Log.Error($"[World] GetUserGameInfo failed: {response.ErrorMessage}");
                    }
                }
                catch (Exception e)
                {
                Log.Error($"[World] GetUserGameInfo exception: {e}");
            }
        }

        public async void SetUserGameInfo(int progressLevelID, string nickName = "", string avatarUrl = "")
        {
            Log.Info($"[World] SetUserGameInfo, progressLevelID: {progressLevelID}, nickName: {nickName}, avatarUrl: {avatarUrl}");
            var paramDict = new Dictionary<string, object> 
            {
                 { "progressLevelID", progressLevelID }
            };
            if (!string.IsNullOrEmpty(nickName)) paramDict.Add("nickName", nickName);
            if (!string.IsNullOrEmpty(avatarUrl)) paramDict.Add("avatarUrl", avatarUrl);
            
            try
            {
                var response = await NetManager.Call<object>("setUserGameInfoV2", paramDict);
                if (response.IsSuccess)
                {
                    Log.Info($"[World] SetUserGameInfo success: {response.msg}");
                }
                else
                {
                    Log.Error($"[World] SetUserGameInfo failed: {response.ErrorMessage}");
                }
            }
            catch (Exception e)
            {
                Log.Error($"[World] SetUserGameInfo exception: {e}");
                }

            var kvDataList = new List<KVData>
            {
                new() { key = "progressLevelID", value = progressLevelID.ToString() }
            };
            if (!string.IsNullOrEmpty(nickName)) kvDataList.Add(new KVData() { key = "nickName", value = nickName });
            if (!string.IsNullOrEmpty(avatarUrl)) kvDataList.Add(new KVData() { key = "avatarUrl", value = avatarUrl });
            WX.SetUserCloudStorage(new SetUserCloudStorageOption()
            {
                KVDataList = kvDataList.ToArray(),
            });

            var msgData = new
            {
                type = "setUserRecord",
                score = progressLevelID,
            };
            WX.GetOpenDataContext().PostMessage(msgData.ToJson());
        }

        public async UniTask<List<PlayerRankInfo>> GetUserRankList()
        {
            Log.Info("[World] GetRankList");

#if UNITY_EDITOR
            return null;
#endif
            try
            {
                var response = await NetManager.Call<List<UserGameInfoData>>("getUserRankList");
                if (response.IsSuccess && response.data != null)
                {
                    return await GetRankListFromResponse(response.data);
                }
                else
                    {
                    Log.Error($"[World] GetUserRankList failed: {response.ErrorMessage}");
                    return null;
                }
            }
            catch (Exception e)
            {
                Log.Error($"[World] GetUserRankList error: {e}");
                return null;
            }

            async UniTask<List<PlayerRankInfo>> GetRankListFromResponse(List<UserGameInfoData> userList)
            {
            try
                {
                    Log.Info($"[World] GetRankListFromResponse: user count = {userList.Count}");
                    List<PlayerRankInfo> rankList = new List<PlayerRankInfo>();
                    
                    foreach (var userData in userList)
                    {
                        // 直接使用新字段
                        int progressLevelID = userData.progressLevelID ?? 0;
                        string nickName = userData.nickName ?? "";
                        string avatarUrl = userData.avatarUrl ?? "";
                        
                        if (progressLevelID <= 0 || string.IsNullOrEmpty(nickName) || string.IsNullOrEmpty(avatarUrl))
                        {
                            continue;
                        }
                        
                        PlayerRankInfo rankInfo = new PlayerRankInfo();
                        rankInfo.openid = userData.openid ?? "";
                        rankInfo.progressLevelID = progressLevelID;
                        rankInfo.nickName = nickName;
                        rankInfo.avatarURL = avatarUrl;
                        
                        if (!rankInfo.IsValid()) continue;
                        rankList.Add(rankInfo);
                    }
                    
                    rankList.Sort((a, b) => b.progressLevelID.CompareTo(a.progressLevelID));
                    for (int i = 0; i < rankList.Count; i++)
                    {
                        rankList[i].playerRank = i + 1;
                    }
                    
                    Log.Info($"[World] GetRankListFromResponse success, valid rank info count: {rankList.Count}");
                    return rankList;
                }
                catch (Exception e)
                {
                    Log.Error($"[World] GetRankListFromResponse error: {e}");
                    return null;
                }
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