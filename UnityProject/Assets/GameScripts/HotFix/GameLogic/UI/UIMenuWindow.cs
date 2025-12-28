using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: true)]
	partial class UIMenuWindow
	{
        protected override void OnCreate()
        {
            base.OnCreate();
            m_textVersion.text = $"Version: {GameModule.Resource.GetPackageVersion()} {World.Instance.GameData.UserInfo.nickName}";
            EventTriggerListener.Get(m_btnStartGame).OnClick = OnStartGameClick;
            EventTriggerListener.Get(m_btnRank).OnClick = OnRankClick;
            EventTriggerListener.Get(m_btnSettings).OnClick = OnSettingsClick;
            GameEvent.AddEventListener<UserInfo>(SlimeEvent.OnUserInfoUpdate, OnUserInfoUpdate);

            GameModule.Audio.Play(TEngine.AudioType.Music, "BGM", true, 0.5f, true);
        }

        protected override void OnDestroy()
        {
            GameEvent.RemoveEventListener<UserInfo>(SlimeEvent.OnUserInfoUpdate, OnUserInfoUpdate);
            base.OnDestroy();
        }

        private void OnUserInfoUpdate(UserInfo userInfo)
        {
            Log.Info($"[UIMenuWindow] trigger OnUserInfoUpdate: {GameModule.Resource.GetPackageVersion()} {World.Instance.GameData.UserInfo.nickName}");
            m_textVersion.text = $"Version: {GameModule.Resource.GetPackageVersion()} {World.Instance.GameData.UserInfo.nickName}";
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
}

