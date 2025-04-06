using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TEngine;
using WeChatWASM;

namespace GameLogic
{
    partial class World
    {
        public SlimeGameData gameData =
#if UNITY_EDITOR
            // new EditorUserData();
            new WXUserData();
#else
            new WXUserData();
#endif
        private static string WXCloudENV = "slimecloudservice-6enxmrfbc5bddc";

        public void InitWX(System.Action<bool> callback)
        {
            Log.Info("World WX.InitSDK");
            WX.InitSDK((code) =>
            {
                Log.Info($"World InitWX WX.InitSDK code: {code}");
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
            Log.Info("World WX.GetSetting");
            WX.GetSetting(new GetSettingOption()
            {
                // {"authSetting":{"scope.address":true,"scope.invoice":true,"scope.invoiceTitle":true},"subscriptionsSetting":{"mainSwitch":false,"itemSettings":{}},"miniprogramAuthSetting":{},"errMsg":"getSetting:ok"}
                success = (res) =>
                {
                    Log.Info($"World GetSetting success: {res.ToJson()}");
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
                    Log.Error($"World GetSetting fail: {err.ToJson()}");
                }
            });
        }

        private void GetUserInfo(string log)
        {
            Log.Info($"World GetUserInfo: {log}");
            WX.GetUserInfo(new GetUserInfoOption()
            {
                success = (res) =>
                {
                    Log.Info($"World GetUserInfo success: {res.ToJson()}");
                    var result = res.userInfo;
                    var currentUserInfo = gameData.UserInfo;
                    currentUserInfo.nickName = result.nickName;
                    currentUserInfo.avatarUrl = result.avatarUrl;
                    currentUserInfo.gender = result.gender;
                    currentUserInfo.province = result.province;
                    currentUserInfo.city = result.city;
                    currentUserInfo.country = result.country;
                    currentUserInfo.language = result.language;
                    gameData.UserInfo = currentUserInfo;
                },
                fail = (err) =>
                {
                    Log.Error($"World GetUserInfo fail: {err.ToJson()}");
                }
            });
        }

        private void CreateUserInfoButton()
        {
            Log.Info("World CreateUserInfoButton");
            var button = WX.CreateUserInfoButton(0, 0, Screen.width, Screen.height, "zh_CN", false);
            button.OnTap((tapRes) =>
            {
                Log.Info("CreateUserInfoButton OnTap: " + tapRes.ToJson());
                button.Hide();
                GetUserInfo("by create user info button");
            });
        }

        private void WXLogin()
        {
            Log.Info("World WXLogin");
            WX.Login(new LoginOption()
            {
                // {"code":"0b1KH20w3mbkG43mRW1w3Z5cZz4KH20o","errMsg":"login:ok"}
                success = (res) =>
                {
                    Log.Info("WX.Login success: " + res.ToJson());
                    GetOpenID(res.code);
                },
                fail = (err) =>
                {
                    Log.Error("WX.Login fail: " + err.ToJson());
                }
            });
        }

        private void GetOpenID(string code)
        {
            Log.Info("World GetOpenID");
            var param = new
            {
                code = "0a1HYj0w3dCzE43twJ2w36iCMz1HYj02"
            };
            WX.cloud.CallFunction(new CallFunctionParam()
            {
                name = "getCode2Session",
                data = param,
                // {"result":"{\"event\":{\"code\":\"0a1HYj0w3dCzE43twJ2w36iCMz1HYj02\",\"tcbContext\":{},\"userInfo\":{\"appId\":\"wxf55f604f65c8f87b\",\"openId\":\"ox0H160OiHbng6giS50wOp6YZ7R4\"}},\"openid\":\"ox0H160OiHbng6giS50wOp6YZ7R4\",\"appid\":\"wxf55f604f65c8f87b\",\"unionid\":\"\"}","requestID":"d54da294-36e2-40c8-9765-4d68a8134d98","errMsg":"cloud.callFunction:ok"}
                success = (res) =>
                {
                    Log.Info("call cloud function getCode2Session success: " + res.ToJson().ToString());
                    var resultDict = res.result.ToObject<Dictionary<string, object>>();
                    var currentUserInfo = gameData.UserInfo;
                    currentUserInfo.openId = resultDict["openid"].ToString();
                    gameData.UserInfo = currentUserInfo;
                },
                fail = (err) =>
                {
                    Log.Error("call cloud function getCode2Session failed: " + err.ToJson().ToString());
                }
            });
        }
    }
}