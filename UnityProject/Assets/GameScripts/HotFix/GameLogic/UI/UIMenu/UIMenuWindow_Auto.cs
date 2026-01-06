using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	partial class UIMenuWindow: UIWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgBG;
		private Image m_img_title;
		private Text m_textVersion;
		private Button m_btnStartGame;
		private Button m_btnRank;
		private Button m_btnSettings;

		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_img_title = FindChildComponent<Image>("Content/m_img_title");
			m_textVersion = FindChildComponent<Text>("Content/m_textVersion");
			m_btnStartGame = FindChildComponent<Button>("Content/m_btnStartGame");
			m_btnRank = FindChildComponent<Button>("Content/m_btnRank");
			m_btnSettings = FindChildComponent<Button>("Content/m_btnSettings");
		}
		#endregion
	}
}