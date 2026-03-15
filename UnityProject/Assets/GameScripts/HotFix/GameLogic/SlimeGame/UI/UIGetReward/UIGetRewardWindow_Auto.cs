using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TEngine;

namespace GameLogic
{
	partial class UIGetRewardWindow: UIWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgBG;
		private Image m_imgTitleBG;
		private Text m_txtTitle;
		private Transform m_tfRewardContent;
		private Text m_txtTips;

		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_imgTitleBG = FindChildComponent<Image>("Content/m_imgTitleBG");
			m_txtTitle = FindChildComponent<Text>("Content/m_imgTitleBG/m_txtTitle");
			m_tfRewardContent = FindChild("Content/Content/m_tfRewardContent");
			m_txtTips = FindChildComponent<Text>("Content/m_txtTips");
		}
		#endregion
	}
}