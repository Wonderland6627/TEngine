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
		private List<PlayerRankInfo> m_LastRankInfos = null;
		private bool m_IsRefreshing = false;

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

		/// <summary>
		/// 是否在拉取到的排行榜名单中
		/// </summary>
		public bool CheckSelfInRankList(List<PlayerRankInfo> rankInfos)
		{
			if (rankInfos == null || rankInfos.Count == 0)
			{
				Log.Info("[UIRankWindow] CheckSelfInRankList: rankInfos is empty");
				return false;
			}

			string selfOpenId = World.Instance.GameData.UserInfo;
			if (string.IsNullOrEmpty(selfOpenId))
			{
				Log.Warning("[UIRankWindow] CheckSelfInRankList: selfOpenId is empty");
				return false;
			}

			return rankInfos.Any(info => info.openID == selfOpenId);
		}

		/// <summary>
		/// 调用请求用户信息（用于补齐昵称/头像，避免自己无法进入排行榜）
		/// </summary>
		public void CallRequestUserInfo()
		{
			World.Instance.RequestUserInfo();
		}

		/// <summary>
		/// 刷新排行榜：重新拉取并重建列表
		/// </summary>
		public void RefreshRankList()
		{
			GetRankList();
		}

		private async void GetRankList()
		{
			if (m_IsRefreshing) return;
			m_IsRefreshing = true;
			try
			{
				if (World.Instance == null)
				{
					Log.Warning("[UIRankWindow] GetRankList: World is null");
					return;
				}

				for (int i = 0; i < childCells.Count; i++)
				{
					childCells[i]?.Destroy();
				}
				childCells.Clear();

				var rankInfos = await World.Instance.GetUserRankList();
				m_LastRankInfos = rankInfos;
				if (rankInfos == null) return;
				if (gameObject == null) return;

				for (int i = 0; i < rankInfos.Count; i++)
				{
					UIRankCell rankCell = CreateWidgetByPrefab<UIRankCell>(m_itemPlayerRankCell, m_rectRankContent);
					rankCell.SetData(rankInfos[i]);
					childCells.Add(rankCell);
				}
			}
			finally
			{
				m_IsRefreshing = false;
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