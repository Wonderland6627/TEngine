using UnityEngine;
using UnityEngine.UI;
using TEngine;
using WeChatWASM;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

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
