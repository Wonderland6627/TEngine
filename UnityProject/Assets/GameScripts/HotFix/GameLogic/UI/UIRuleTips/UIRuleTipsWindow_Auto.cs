using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	partial class UIRuleTipsWindow: UIWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgBG;
		private Image m_imgPopupBG;
		private Image m_imgClose;
		private Text m_textTitle;
		private Image m_imgUnit;

		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_imgPopupBG = FindChildComponent<Image>("Content/m_imgPopupBG");
			m_imgClose = FindChildComponent<Image>("Content/m_imgPopupBG/m_imgClose");
			m_textTitle = FindChildComponent<Text>("Content/m_imgPopupBG/m_textTitle");
			m_imgUnit = FindChildComponent<Image>("Content/m_imgPopupBG/layout/RuleCell/TipsImg/TipsArrowImg/Unit_Player/m_imgUnit");
		}
		#endregion
	}
}