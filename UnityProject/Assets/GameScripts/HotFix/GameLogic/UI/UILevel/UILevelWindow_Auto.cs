using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	partial class UILevelWindow: UIWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgBG;
		private Button m_btnBack;
		private Button m_btnRule;
		private Image m_imgLevelPreviewContent;
		private Image m_imgLevelPreviewBG;
		private Image m_imgLevelPreview;
		private Image m_imgLevelLock;
		private Image m_imgLevelTitleBG;
		private Text m_textLevelTitle;
		private Image m_imgLevelNext;
		private Image m_imgLevelPrevious;
		private Image m_imgLevelBG;
		private Text m_textLevel;
		private Button m_btnStart;
		private Text m_textStart;

		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_btnBack = FindChildComponent<Button>("Content/m_btnBack");
			m_btnRule = FindChildComponent<Button>("Content/m_btnRule");
			m_imgLevelPreviewContent = FindChildComponent<Image>("Content/m_imgLevelPreviewContent");
			m_imgLevelPreviewBG = FindChildComponent<Image>("Content/m_imgLevelPreviewContent/m_imgLevelPreviewBG");
			m_imgLevelPreview = FindChildComponent<Image>("Content/m_imgLevelPreviewContent/m_imgLevelPreview");
			m_imgLevelLock = FindChildComponent<Image>("Content/m_imgLevelPreviewContent/m_imgLevelLock");
			m_imgLevelTitleBG = FindChildComponent<Image>("Content/m_imgLevelPreviewContent/m_imgLevelTitleBG");
			m_textLevelTitle = FindChildComponent<Text>("Content/m_imgLevelPreviewContent/m_imgLevelTitleBG/m_textLevelTitle");
			m_imgLevelNext = FindChildComponent<Image>("Content/m_imgLevelPreviewContent/m_imgLevelNext");
			m_imgLevelPrevious = FindChildComponent<Image>("Content/m_imgLevelPreviewContent/m_imgLevelPrevious");
			m_imgLevelBG = FindChildComponent<Image>("Content/m_imgLevelPreviewContent/m_imgLevelBG");
			m_textLevel = FindChildComponent<Text>("Content/m_imgLevelPreviewContent/m_imgLevelBG/m_textLevel");
			m_btnStart = FindChildComponent<Button>("Content/m_imgLevelPreviewContent/m_btnStart");
			m_textStart = FindChildComponent<Text>("Content/m_imgLevelPreviewContent/m_btnStart/m_textStart");
		}
		#endregion
	}
}