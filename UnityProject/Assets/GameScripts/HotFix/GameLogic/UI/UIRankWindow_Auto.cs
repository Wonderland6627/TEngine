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
            EventTriggerListener.Get(m_tfTapArea.gameObject).OnClick = go =>
            {
            	Close();
            };
        }

		private async void GetRankList()
		{
			var rankInfos = await World.Instance.GetUserRankList();
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
		private Transform m_tfTapArea;
		private GameObject m_itemPlayerRankCell;
		private ScrollRect m_scrollRectRankScroll;
		private RectTransform m_rectRankContent;
		private GameObject m_itemMainPlayerRankCell;
		protected override void ScriptGenerator()
		{
			m_tfTapArea = FindChild("bg/m_tfTapArea");
			m_itemPlayerRankCell = FindChild("bg/m_itemPlayerRankCell").gameObject;
			m_scrollRectRankScroll = FindChildComponent<ScrollRect>("bg/m_scrollRectRankScroll");
			m_rectRankContent = FindChildComponent<RectTransform>("bg/m_scrollRectRankScroll/Viewport/m_rectRankContent");
			m_itemMainPlayerRankCell = FindChild("bg/m_itemMainPlayerRankCell").gameObject;
		}
		#endregion
	}
}