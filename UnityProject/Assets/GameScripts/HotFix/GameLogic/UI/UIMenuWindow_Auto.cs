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
            m_textTitle.text = $"Hello, {World.Instance.gameData.UserInfo.nickName}!";
            EventTriggerListener.Get(m_btnStartGame).OnClick = OnStartGameClick;
            GameEvent.AddEventListener<UserInfo>(IActorLogicEvent_Event.OnUserInfoUpdate, OnUserInfoUpdate);
        }

        protected override void OnDestroy()
        {
            GameEvent.RemoveEventListener<UserInfo>(IActorLogicEvent_Event.OnUserInfoUpdate, OnUserInfoUpdate);
            base.OnDestroy();
        }

        private void OnUserInfoUpdate(UserInfo userInfo)
        {
            Log.Info("[UIMenuWindow] OnUserInfoUpdate");
            m_textTitle.text = $"Hello, {userInfo.nickName}!";
        }

        private void OnStartGameClick(GameObject go)
        {
            Log.Info("[UIMenuWindow] OnStartGameClick");
            GameModule.UI.ShowUIAsync<UILevelWindow>();
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
