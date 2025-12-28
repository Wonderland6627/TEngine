using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	partial class UIAdsRewardWindow: UIWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgBG;
		private Image m_imgRewardContent;
		private Image m_imgRewardTitleBG;
		private Text m_textRewardTitle;
		private GameObject m_itemRewardCell;
		private RectTransform m_rectRewardContent;
		private Button m_btnRefresh;
		private Image m_imgAds;

		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_imgRewardContent = FindChildComponent<Image>("Content/m_imgRewardContent");
			m_imgRewardTitleBG = FindChildComponent<Image>("Content/m_imgRewardContent/m_imgRewardTitleBG");
			m_textRewardTitle = FindChildComponent<Text>("Content/m_imgRewardContent/m_imgRewardTitleBG/m_textRewardTitle");
			m_itemRewardCell = FindChild("Content/m_imgRewardContent/m_itemRewardCell").gameObject;
			m_rectRewardContent = FindChildComponent<RectTransform>("Content/m_imgRewardContent/m_rectRewardContent");
			m_btnRefresh = FindChildComponent<Button>("Content/m_btnRefresh");
			m_imgAds = FindChildComponent<Image>("Content/m_imgAds");
		}
		#endregion
	}
}