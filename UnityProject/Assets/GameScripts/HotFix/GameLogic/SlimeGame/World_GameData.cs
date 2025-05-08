using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TEngine;
using WeChatWASM;
using System;
using Newtonsoft.Json.Linq;

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

        public void GetSetting()
        {
            Log.Info("[World] WX.GetSetting");
            WX.GetSetting(new GetSettingOption()
            {
                // {"authSetting":{"scope.address":true,"scope.invoice":true,"scope.invoiceTitle":true},"subscriptionsSetting":{"mainSwitch":false,"itemSettings":{}},"miniprogramAuthSetting":{},"errMsg":"getSetting:ok"}
                success = (res) =>
                {
                    Log.Info($"[World] WX.GetSetting success: {res.ToJson()}");
                    if (res.authSetting.TryGetValue("scope.userInfo", out var hasUserInfo))
                    {
                        if (hasUserInfo)
                        {
                            // 用户已授权，直接获取用户信息
                            GetUserInfo("by auth setting has user info");
                        }
                        else
                        {
                            // 用户未授权，创建用户信息按钮
                            CreateUserInfoButton();
                        }
                    }
                    else
                    {
                        // 未获取到授权信息，创建用户信息按钮
                        CreateUserInfoButton();
                    }
                },
                fail = (err) =>
                {
                    Log.Error($"[World] WX.GetSetting fail: {err.ToJson()}");
                }
            });
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

        private void WXLogin()
        {
            Log.Info("[World] WX.WXLogin");
            WX.Login(new LoginOption()
            {
                // {"code":"0b1KH20w3mbkG43mRW1w3Z5cZz4KH20o","errMsg":"login:ok"}
                success = (res) =>
                {
                    Log.Info("[World] WX.Login success: " + res.ToJson());
                    GetOpenID(res.code);
                },
                fail = (err) =>
                {
                    Log.Error("[World] WX.Login fail: " + err.ToJson());
                }
            });
        }

        private void GetOpenID(string code)
        {
            Log.Info("[World] GetOpenID");
            var param = new
            {
                code
            };
            WX.cloud.CallFunction(new CallFunctionParam()
            {
                name = "getCode2Session",
                data = param,
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
                    Log.Info("[World] call cloud function getCode2Session success: " + res.ToJson().ToString());
                    var resultDict = res.result.ToObject<Dictionary<string, object>>();
                    var currentUserInfo = GameData.UserInfo;
                    currentUserInfo.openId = resultDict["openid"].ToString();
                    GameData.UserInfo = currentUserInfo;
                },
                fail = (err) =>
                {
                    Log.Error("[World] call cloud function getCode2Session failed: " + err.ToJson().ToString());
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
                        GameData.ProgressLevelID = progressLevelID;
                    }
                }
                catch (Exception e)
                {
                    Log.Error("[World] parse userGameInfo error: " + e.ToString());
                }
            }
        }

        public void SetUserGameInfo(int progressLevelID)
        {
            Log.Info($"[World] SetUserGameInfo, progressLevelID: {progressLevelID}");
            var param = new
            {
                progressLevelID
            };
            WX.cloud.CallFunction(new CallFunctionParam()
            {
                name = "setUserGameInfo",
                data = param,
                success = (res) =>
                {
                    Log.Info("[World] call cloud function setUserGameInfo success: " + res.ToJson().ToString());
                },
                fail = (err) =>
                {
                    Log.Error("[World] call cloud function setUserGameInfo failed: " + err.ToJson().ToString());
                }
            });
        }
    }
}