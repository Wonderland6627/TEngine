using UnityEngine;
using UnityEngine.UI;
using TEngine;
using Cysharp.Threading.Tasks;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: true)]
	partial class UIMenuWindow : UIWindow
	{
        protected override void OnCreate()
        {
            base.OnCreate();
            m_textVersion.text = $"Version: {GameModule.Resource.GetPackageVersion()}: {World.Instance.GameData.UserInfo.nickName}";
            EventTriggerListener.Get(m_btnStartGame).OnClick = OnStartGameClick;
            EventTriggerListener.Get(m_btnRank).OnClick = OnRankClick;
            EventTriggerListener.Get(m_btnSettings).OnClick = OnSettingsClick;
            GameEvent.AddEventListener<UserInfo>(SlimeEvent.OnUserInfoUpdate, OnUserInfoUpdate);
        }

        protected override void OnDestroy()
        {
            GameEvent.RemoveEventListener<UserInfo>(SlimeEvent.OnUserInfoUpdate, OnUserInfoUpdate);
            base.OnDestroy();
        }

        private void OnUserInfoUpdate(UserInfo userInfo)
        {
            Log.Info($"[UIMenuWindow] trigger OnUserInfoUpdate: {GameModule.Resource.GetPackageVersion()}: {World.Instance.GameData.UserInfo.nickName}");
            m_textVersion.text = $"Version: {GameModule.Resource.GetPackageVersion()}: {World.Instance.GameData.UserInfo.nickName}";
        }

        private void OnStartGameClick(GameObject go)
        {
            Log.Info("[UIMenuWindow] OnStartGameClick");
            GameModule.UI.ShowUIAsync<UILevelWindow>();
        }

        private void OnRankClick(GameObject go)
        {
            Log.Info("[UIMenuWindow] OnRankClick");
            GameModule.UI.ShowUIAsync<UIRankWindow>();
        }

        private void OnSettingsClick(GameObject go)
        {
            Log.Info("[UIMenuWindow] OnSettingsClick");
            GameModule.UI.ShowUIAsync<UISettingsWindow>();
        }
	}

	partial class UIMenuWindow
	{
        #region 脚本工具生成的代码
		private Image m_img_bg;
		private Image m_img_title;
		private Text m_textVersion;
		private Button m_btnStartGame;
		private Button m_btnRank;
		private Button m_btnSettings;
		protected override void ScriptGenerator()
		{
			m_img_bg = FindChildComponent<Image>("Content/m_img_bg");
			m_img_title = FindChildComponent<Image>("Content/m_img_title");
			m_textVersion = FindChildComponent<Text>("Content/m_textVersion");
			m_btnStartGame = FindChildComponent<Button>("Content/m_btnStartGame");
			m_btnRank = FindChildComponent<Button>("Content/m_btnRank");
			m_btnSettings = FindChildComponent<Button>("Content/m_btnSettings");
		}
		#endregion
	}
}
