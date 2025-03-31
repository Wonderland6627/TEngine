using UnityEngine;
using UnityEngine.UI;
using TEngine;
using WeChatWASM;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using System;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: true)]
	partial class UIMenuWindow : UIWindow
	{
		protected override void OnCreate()
		{
			base.OnCreate();
			EventTriggerListener.Get(m_btnStartGame).OnClick = OnStartGameClick;
		}

		private void OnStartGameClick(GameObject go) 
		{
			Log.Info("[UIMenuWindow] OnStartGameClick");
			WX.GetSetting(new GetSettingOption()
			{
				success = (res) =>
				{
					Log.Info($"[UIMenuWindow] OnStartGameClick WX.GetSetting success: {res.ToJson()}");
					// webgl.wasm.framework.unityweb.js:4 <color=#CFCFCF><b>[INFO] ► </b></color> - <color=#CFCFCF>[UIMenuWindow] OnStartGameClick WX.GetSetting success: {"authSetting":{"scope.address":true,"scope.invoice":true,"scope.invoiceTitle":true},"subscriptionsSetting":{"mainSwitch":false,"itemSettings":{}},"miniprogramAuthSetting":{},"errMsg":"getSetting:ok"}</color>

					if (res.authSetting.TryGetValue("scope.userInfo", out var hasUserInfo))
					{
                        if (hasUserInfo)
                        {
                            // 用户已授权，直接获取用户信息
                            GetUserInfo();
                        }
                        else
                        {
                            // 用户未授权，创建用户信息按钮
                            CreateUserInfoButton();
                        }
					} else
					{
						// 未获取到授权信息，创建用户信息按钮
						CreateUserInfoButton();
					}
				},
				fail = (err) =>
				{
					Log.Error($"[UIMenuWindow] OnStartGameClick WX.GetSetting fail: {err.ToJson()}");
				}
			});
			GameModule.UI.ShowUIAsync<UILevelWindow>();
		}

        private void GetUserInfo()
        {
			Log.Info("[UIMenuWindow] GetUserInfo");
            WX.GetUserInfo(new GetUserInfoOption()
            {
                success = (res) =>
                {
                    Log.Info("GetUserInfo success: " + res.ToJson());
					GetOpenID();
                },
                fail = (err) =>
                {
                    Log.Error("GetUserInfo fail: " + err.ToJson());
                }
            });
        }

        private void CreateUserInfoButton()
        {
			Log.Info("[UIMenuWindow] CreateUserInfoButton");
            var button = WX.CreateUserInfoButton(0, 0, Screen.width, Screen.height, "zh_CN", false);
            button.OnTap((tapRes) =>
            {
				Log.Info("CreateUserInfoButton OnTap: " + tapRes.ToJson());
                button.Hide();
                GetUserInfo();
            });
        }

        private void GetOpenID()
        {
			Log.Info("[UIMenuWindow] GetOpenID");
            WX.Login(new LoginOption()
            {
                success = (res) =>
                {
                    Debug.Log("WX.Login success: " + res.ToJson());
                    SendCodeToServer(res.code).Forget();
                },
                fail = (err) =>
                {
                    Debug.LogError("WX.Login fail: " + err.ToJson());
                }
            });
        }

        private async UniTask SendCodeToServer(string code)
        {
			Log.Info("[UIMenuWindow] SendCodeToServer");
            string appID = "wxf55f604f65c8f87b";
            string appSecret = "2fd10e01dd726937a1651bea03d58f70";
            string url = $"https://api.weixin.qq.com/sns/jscode2session?appid={appID}&secret={appSecret}&js_code={code}&grant_type=authorization_code";

            try
            {
                using var www = UnityWebRequest.Get(url);
                await www.SendWebRequest().ToUniTask();
                
                if (www.result == UnityWebRequest.Result.Success)
                {
                    string result = www.downloadHandler.text;
                    Debug.Log("OpenID result: " + result);
                    // 解析返回的 JSON 数据，获取 OpenID
                }
                else
                {
                    Debug.LogError("Failed to get OpenID: " + www.error);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SendCodeToServer exception: {e}");
            }
        }
	}

	partial class UIMenuWindow
	{
		#region 脚本工具生成的代码
		private Text m_textTitle;
		private Button m_btnStartGame;
		private Button m_btnRank;
		private Button m_btnSettings;
		protected override void ScriptGenerator()
		{
			m_textTitle = FindChildComponent<Text>("bg/m_textTitle");
			m_btnStartGame = FindChildComponent<Button>("bg/m_btnStartGame");
			m_btnRank = FindChildComponent<Button>("bg/m_btnRank");
			m_btnSettings = FindChildComponent<Button>("bg/m_btnSettings");
		}
		#endregion
	}
}
