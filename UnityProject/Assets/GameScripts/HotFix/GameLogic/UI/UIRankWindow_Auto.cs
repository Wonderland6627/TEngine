using UnityEngine;
using UnityEngine.UI;
using TEngine;
using System.Collections.Generic;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: true)]
	partial class UIRankWindow : UIWindow
	{
		private List<UIWidget> childCells = new();

		protected override void OnCreate()
		{
			base.OnCreate();

			GetRankList();
			EventTriggerListener.Get(m_btnFriendsRank.gameObject).OnClick = go =>
            {
            	UIFriendsRankPanel friendsRankPanel = CreateWidgetByPrefab<UIFriendsRankPanel>(m_itemFriendsRankPanel, gameObject.transform);
            };

			EventTriggerListener.Get(m_btnBack).OnClick = go =>
			{
				Close();
			};
        }

		private async void GetRankList()
		{
			var rankInfos = await World.Instance.GetUserRankList();
			if (gameObject == null) return;
			for (int i = 0; i < rankInfos.Count; i++)
			{
				UIRankCell rankCell = CreateWidgetByPrefab<UIRankCell>(m_itemPlayerRankCell, m_rectRankContent);
				rankCell.SetData(rankInfos[i]);
				childCells.Add(rankCell);
			}
		}
	}

	partial class UIRankWindow
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
		}
		#endregion
	}
}