using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	partial class UIRankWindow: UIWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgBG;
		private Button m_btnBack;
		private Image m_imgRankListContent;
		private Image m_imgRankTitleBG;
		private Text m_textRankTitle;
		private GameObject m_itemPlayerRankCell;
		private Button m_btnFriendsRank;
		private GameObject m_itemFriendsRankPanel;
		private RectTransform m_rectRankContent;
		private GameObject m_itemPlayerRankMine;

		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_btnBack = FindChildComponent<Button>("Content/m_btnBack");
			m_imgRankListContent = FindChildComponent<Image>("Content/m_imgRankListContent");
			m_imgRankTitleBG = FindChildComponent<Image>("Content/m_imgRankListContent/m_imgRankTitleBG");
			m_textRankTitle = FindChildComponent<Text>("Content/m_imgRankListContent/m_imgRankTitleBG/m_textRankTitle");
			m_itemPlayerRankCell = FindChild("Content/m_imgRankListContent/m_itemPlayerRankCell").gameObject;
			m_btnFriendsRank = FindChildComponent<Button>("Content/m_imgRankListContent/m_btnFriendsRank");
			m_itemFriendsRankPanel = FindChild("Content/m_imgRankListContent/m_itemFriendsRankPanel").gameObject;
			m_rectRankContent = FindChildComponent<RectTransform>("Content/m_imgRankListContent/Scroll View/Viewport/m_rectRankContent");
			m_itemPlayerRankMine = FindChild("Content/m_imgRankListContent/m_itemPlayerRankMine").gameObject;
		}
		#endregion
	}
}