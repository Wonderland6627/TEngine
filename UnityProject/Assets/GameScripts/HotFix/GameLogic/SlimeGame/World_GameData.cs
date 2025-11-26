using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TEngine;
using WeChatWASM;
using System;
using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

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

        private void GetOpenID()
        {
            Log.Info("[World] GetOpenID");
            // 云开发环境下无需传入code，云函数会自动从context获取openid
            WX.cloud.CallFunction(new CallFunctionParam()
            {
                name = "getUserWXContext",
                // data参数省略，云函数通过getWXContext()自动获取openid
/*
{
    "result": {
        "event": {
            "code": "0a1HYj0w3dCzE43twJ2w36iCMz1HYj02",
            "tcbContext": {},
            "userInfo": {
                "appId": "wxf55f604f65c8f87b",
                "openId": "ox0H160OiHbng6giS50wOp6YZ7R4"
            }
        },
        "openid": "ox0H160OiHbng6giS50wOp6YZ7R4",
        "appid": "wxf55f604f65c8f87b",
        "unionid": ""
    },
    "requestID": "d54da294-36e2-40c8-9765-4d68a8134d98",
    "errMsg": "cloud.callFunction:ok"
}
*/
                success = (res) =>
                {
                    Log.Info("[World] call cloud function getUserWXContext success: " + res.ToJson().ToString());
                    var resultDict = res.result.ToObject<Dictionary<string, object>>();
                    var currentUserInfo = GameData.UserInfo;
                    currentUserInfo.openId = resultDict["openid"].ToString();
                    GameData.UserInfo = currentUserInfo;
                },
                fail = (err) =>
                {
                    Log.Error("[World] call cloud function getUserWXContext failed: " + err.ToJson().ToString());
                }
            });
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
        public void GetUserGameInfo()
        {
            Log.Info("[World] GetUserGameInfo");
            WX.cloud.CallFunction(new CallFunctionParam()
            {
                name = "getUserGameInfo",
                success = (res) =>
                {
                    Log.Info("[World] call cloud function getUserGameInfo success: " + res.ToJson().ToString());
                    OnGetUserGameInfoSuccess(res);
                },
                fail = (err) =>
                {
                    Log.Error("[World] call cloud function getUserGameInfo failed: " + err.ToJson().ToString());
                }
            });

            void OnGetUserGameInfoSuccess(CallFunctionResult res)
            {
/*
{
    "result": {
        "code": 0,
        "data": {
            "_id": "073a77ac681a0c320284cee70fe9ff04",
            "openid": "ox0H160OiHbng6giS50wOp6YZ7R4",
            "userGameInfo": {
                "progressLevelID": 2,
                "userInfo": {
                    "appId": "wxf55f604f65c8f87b",
                    "openId": "ox0H160OiHbng6giS50wOp6YZ7R4"
                }
            },
            "createdAt": "2025-05-06T13:18:42.514Z",
            "updatedAt": "2025-05-06T13:50:00.139Z"
        },
        "msg": "get user game info success"
    },
    "requestID": "c50afb2b-0c6e-4fd1-bc76-dd401d6df02a",
    "errMsg": "cloud.callFunction:ok"
}
*/
                try
                {
                    JObject resultJson = JObject.Parse(res.result);
                    if (resultJson.TryGetValue("code", out var code))
                    {
                        int codeInt = code.ToObject<int>();
                        if (codeInt != 0)
                        {
                            Log.Error($"[World] parse getUserGameInfo response failed, code: {codeInt}");
                            return;
                        }
                    }

                    if (!resultJson.TryGetValue("data", out var dataJson) || dataJson == null)
                    {
                        Log.Error("[World] parse getUserGameInfo response failed, data is null");
                        return;
                    }

                    var dataDict = JObject.Parse(dataJson.ToString());
                    if (!dataDict.TryGetValue("userGameInfo", out var userGameInfoJson) || userGameInfoJson == null)
                    {
                        Log.Error("[World] parse getUserGameInfo response failed, userGameInfo is null");
                        return;
                    }

                    var userGameInfoDict = userGameInfoJson.ToObject<Dictionary<string, object>>();
                    if (userGameInfoDict == null || userGameInfoDict.Count == 0)
                    {
                        Log.Warning("[World] parse getUserGameInfo response success, but userGameInfo is empty, new user");
                        return;
                    }
                    Log.Info($"[World] parse getUserGameInfo response success: {userGameInfoDict.Count}, {userGameInfoJson}");
                    foreach (var item in userGameInfoDict)
                    {
                        Log.Info($"[World] parse getUserGameInfo response item[{item.Key}] : {item.Value} ({item.Value.GetType()})");
                    }
                    if (userGameInfoDict.TryGetValue("progressLevelID", out var value))
                    {
                        int progressLevelID = Convert.ToInt32(value);
                        GameData.SetProgressLevelID(progressLevelID);
                    }
                }
                catch (Exception e)
                {
                    Log.Error("[World] parse userGameInfo error: " + e.ToString());
                }
            }
        }

        public void SetUserGameInfo(int progressLevelID, string nickName = "", string avatarUrl = "")
        {
            Log.Info($"[World] SetUserGameInfo, progressLevelID: {progressLevelID}, nickName: {nickName}, avatarUrl: {avatarUrl}");
            var paramDict = new Dictionary<string, object> 
            {
                 { "progressLevelID", progressLevelID }
            };
            if (!string.IsNullOrEmpty(nickName)) paramDict.Add("nickName", nickName);
            if (!string.IsNullOrEmpty(avatarUrl)) paramDict.Add("avatarUrl", avatarUrl);
            WX.cloud.CallFunction(new CallFunctionParam()
            {
                name = "setUserGameInfo",
                data = paramDict,
                success = (res) =>
                {
                    Log.Info("[World] call cloud function setUserGameInfo success: " + res.ToJson().ToString());
                },
                fail = (err) =>
                {
                    Log.Error("[World] call cloud function setUserGameInfo failed: " + err.ToJson().ToString());
                }
            });

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
            var rankList = await GetRankListFromJson(null);
            return rankList;
#endif
            try
            {
                var tcs = new UniTaskCompletionSource<CallFunctionResult>();
                
                WX.cloud.CallFunction(new CallFunctionParam()
                {
                    name = "getUserRankList",
                    success = (res) =>
                    {
                        Log.Info("[World] call cloud function getUserRankList success: " + res.ToJson().ToString());
                        tcs.TrySetResult(res);
                    },
                    fail = (err) =>
                    {
                        Log.Error("[World] call cloud function getUserRankList failed: " + err.ToJson().ToString());
                        tcs.TrySetException(new Exception(err.ToJson().ToString()));
                    }
                });

                var result = await tcs.Task;
                return await GetRankListFromJson(result);
            }
            catch (Exception e)
            {
                Log.Error("[World] GetUserRankList error: " + e.ToString());
                return null;
            }

            async UniTask<List<PlayerRankInfo>> GetRankListFromJson(CallFunctionResult res)
            {
/*
{
    "result": {
        "code": 0,
        "data": [
            {
                "_id": "073a77ac681a0c320284cee70fe9ff04",
                "openid": "ox0H160OiHbng6giS50wOp6YZ7R4",
                "userGameInfo": {
                    "progressLevelID": 1,
                    "userInfo": {
                        "appId": "wxf55f604f65c8f87b",
                        "openId": "ox0H160OiHbng6giS50wOp6YZ7R4"
                    },
                    "avatarUrl": "https://thirdwx.qlogo.cn/mmopen/vi_32/mDvEsaANsJxdrRAQgeYhTMoGdnNJKVMVqqJcJYf1SvIEwaicSiaYiaicrScGpmGMIe9jwJZIjAVz1lCg319qI1Weg5Y96IrcggZb35iagib5SnUfg/132",
                    "nickName": "Indey"
                },
                "createdAt": "2025-05-06T13:18:42.514Z",
                "updatedAt": "2025-05-14T09:50:20.234Z"
            },
            {
                "_id": "2b83cb16681b842a02941e9307bb451f",
                "openid": "ox0H168lFD1nmJ_mBG7VR1lc3QwI",
                "userGameInfo": {},
                "createdAt": "2025-05-07T16:02:50.522Z",
                "updatedAt": "2025-05-07T16:02:50.522Z"
            }
        ],
        "msg": "get rank list success"
    },
    "requestID": "587b830f-a453-4500-84ba-202c7c76fd96",
    "errMsg": "cloud.callFunction:ok"
}
*/
            string jsonString = @"
            {
                ""code"": 0,
                ""data"": [
                    {
                        ""_id"": ""073a77ac681a0c320284cee70fe9ff04"",
                        ""openid"": ""ox0H160OiHbng6giS50wOp6YZ7R4"",
                        ""userGameInfo"": {
                            ""progressLevelID"": 1,
                            ""userInfo"": {
                                ""appId"": ""wxf55f604f65c8f87b"",
                                ""openId"": ""ox0H160OiHbng6giS50wOp6YZ7R4""
                            },
                            ""avatarUrl"": ""https://thirdwx.qlogo.cn/mmopen/vi_32/mDvEsaANsJxdrRAQgeYhTMoGdnNJKVMVqqJcJYf1SvIEwaicSiaYiaicrScGpmGMIe9jwJZIjAVz1lCg319qI1Weg5Y96IrcggZb35iagib5SnUfg/132"",
                            ""nickName"": ""你好，世界！Hello, World! 👋
这是一个测试文本，包含中文、英文、数字12345、标点符号！@#$%^&*()_+，以及特殊符号：★☆♥♦♣♠♤♥♦♣♠♧♨️
还有日文：こんにちは、世界！
韩文：안녕하세요, 세계!   玖.
GG Bond。
法文：Bonjour, le monde!  (=。=)
德文：Hallo, Welt!
俄文：Привет, мир!
阿拉伯文：مرحبا بالعالم
希腊文：Χαίρετε, κόσμε!
希伯来文：שלום, עולם
日文假名：あいうえお、かきくけこ
希腊字母：αβγδεζηθικλμνξοπρστυφχψω
数学符号：∑ ∫ ∏ √ ∞ ± ÷ ×
表情符号：😀 😃 😄 😁 😆 😅 😂 😊 😇
特殊符号：★☆♥♦♣♠♤♥♦♣♠♧♨️""
                        },
                        ""createdAt"": ""2025-05-06T13:18:42.514Z"",
                        ""updatedAt"": ""2025-05-14T09:50:20.234Z""
                    },
                    {
                        ""_id"": ""2b83cb16681b842a02941e9307bb451f"",
                        ""openid"": ""ox0H168lFD1nmJ_mBG7VR1lc3QwI"",
                        ""userGameInfo"": {},
                        ""createdAt"": ""2025-05-07T16:02:50.522Z"",
                        ""updatedAt"": ""2025-05-07T16:02:50.522Z""
                    }
                ],
                ""msg"": ""get rank list success""
            }";
            try
                {
                    JObject resultJson =
#if UNITY_EDITOR
                    JObject.Parse(jsonString);
#else
                    JObject.Parse(res.result);
#endif
                    if (resultJson.TryGetValue("code", out var code))
                    {
                        int codeInt = code.ToObject<int>();
                        if (codeInt != 0)
                        {
                            Log.Error($"[World] parse getUserRankList response failed, code: {codeInt}");
                            return null;
                        }
                    }

                    if (!resultJson.TryGetValue("data", out var dataJson) || dataJson == null)
                    {
                        Log.Error("[World] parse getUserRankList response failed, data is null");
                        return null;
                    }
                    var userList = dataJson.ToObject<JArray>();
                    Log.Info($"[World] parse getUserRankList response success, rank info count: {userList.Count}");
                    List<PlayerRankInfo> rankList = new List<PlayerRankInfo>();
                    foreach (JObject userData in userList)
                    {
                        if (!userData.TryGetValue("userGameInfo", out var userGameInfoJson))
                        {
                            Log.Error($"[World] parse getUserRankList response failed, userGameInfo is null: {userData.ToJson()}");
                            continue;
                        }
                        var userGameInfo = userGameInfoJson.ToObject<JObject>();
                        if (userGameInfo == null || userGameInfo.Count == 0) continue;
                        PlayerRankInfo rankInfo = new PlayerRankInfo();
                        rankInfo.openid = userData["openid"]?.ToString() ?? "";
                        rankInfo.progressLevelID = userGameInfo["progressLevelID"]?.Value<int>() ?? 0;
                        rankInfo.nickName = userGameInfo["nickName"]?.ToString() ?? "";
                        rankInfo.avatarURL = userGameInfo["avatarUrl"]?.ToString() ?? "";
                        if (!rankInfo.IsValid()) continue;
                        rankList.Add(rankInfo);
                    }
                    rankList.Sort((a, b) => b.progressLevelID.CompareTo(a.progressLevelID));
                    for (int i = 0; i < rankList.Count; i++)
                    {
                        rankList[i].playerRank = i + 1;
                    }
                    Log.Info($"[World] parse getUserRankList response success, valid rank info count: {rankList.Count}");
                    return rankList;
                }
                catch (Exception e)
                {
                    Log.Error("[World] parse getUserRankList error: " + e.ToString());
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