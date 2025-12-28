using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	partial class UISettingsWindow: UIWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgBG;
		private Image m_imgPopupBG;
		private Image m_imgClose;
		private Text m_textTitle;
		private Toggle m_togSound;
		private Toggle m_togVibration;
		private Button m_btnSave;
		private Text m_textVersion;

		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_imgPopupBG = FindChildComponent<Image>("Content/m_imgPopupBG");
			m_imgClose = FindChildComponent<Image>("Content/m_imgPopupBG/m_imgClose");
			m_textTitle = FindChildComponent<Text>("Content/m_imgPopupBG/m_textTitle");
			m_togSound = FindChildComponent<Toggle>("Content/m_imgPopupBG/layout/m_togSound");
			m_togVibration = FindChildComponent<Toggle>("Content/m_imgPopupBG/layout/m_togVibration");
			m_btnSave = FindChildComponent<Button>("Content/m_imgPopupBG/layout/m_btnSave");
			m_textVersion = FindChildComponent<Text>("Content/m_imgPopupBG/m_textVersion");
		}
		#endregion
	}
}