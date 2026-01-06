using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	partial class UIGameOverWindow: UIWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgBG;
		private Image m_imgGameResultContent;
		private Image m_imgGameResultTitleBG;
		private Text m_textGameResultTitle;
		private Image m_imgGameResultBGRoot;
		private Image m_imgGameResultBG;
		private Text m_textResult;
		private Button m_btnBack;

		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_imgGameResultContent = FindChildComponent<Image>("Content/m_imgGameResultContent");
			m_imgGameResultTitleBG = FindChildComponent<Image>("Content/m_imgGameResultContent/m_imgGameResultTitleBG");
			m_textGameResultTitle = FindChildComponent<Text>("Content/m_imgGameResultContent/m_imgGameResultTitleBG/m_textGameResultTitle");
			m_imgGameResultBGRoot = FindChildComponent<Image>("Content/m_imgGameResultContent/m_imgGameResultBGRoot");
			m_imgGameResultBG = FindChildComponent<Image>("Content/m_imgGameResultContent/m_imgGameResultBGRoot/m_imgGameResultBG");
			m_textResult = FindChildComponent<Text>("Content/m_imgGameResultContent/m_imgGameResultBGRoot/m_imgGameResultBG/m_textResult");
			m_btnBack = FindChildComponent<Button>("Content/m_imgGameResultContent/m_btnBack");
		}
		#endregion
	}
}