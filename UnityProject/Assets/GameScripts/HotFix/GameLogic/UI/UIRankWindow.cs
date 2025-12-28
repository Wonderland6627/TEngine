using UnityEngine;
using UnityEngine.UI;
using TEngine;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: true)]
	partial class UIRankWindow
	{
		private UIRankCell myRankCell;
		private List<UIWidget> childCells = new();
		private List<PlayerRankInfo> rankInfos = null;
		private bool isRefreshing = false;

		protected override void OnCreate()
		{
			base.OnCreate();

			myRankCell = CreateWidget<UIRankCell>(m_itemPlayerRankMine, false);
			RefreshMyRankCell();
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

		public PlayerRankInfo GetMyRankInfo()
		{
			if (rankInfos == null || rankInfos.Count == 0)
			{
				Log.Warning("[UIRankWindow] GetMyRankInfo: rankInfos is empty");
				return null;
			}

			return rankInfos.FirstOrDefault(info => info.IsSelf());
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
			if (isRefreshing) return;
			isRefreshing = true;
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
				this.rankInfos = rankInfos;
				if (rankInfos == null) return;
				if (gameObject == null) return;

				for (int i = 0; i < rankInfos.Count; i++)
				{
					UIRankCell rankCell = CreateWidgetByPrefab<UIRankCell>(m_itemPlayerRankCell, m_rectRankContent);
					rankCell.SetData(rankInfos[i]);
					childCells.Add(rankCell);
				}

				RefreshMyRankCell();
			}
			finally
			{
				isRefreshing = false;
			}
		}

		private void RefreshMyRankCell()
		{
			PlayerRankInfo info = GetMyRankInfo();
			if (info == null) 
			{
				info = new PlayerRankInfo();
				info.playerRank = -1;
				info.progressLevelID = World.Instance.GameData.ProgressLevelID;
				info.openID = World.Instance.GameData.UserInfo.openID;
				info.nickName = World.Instance.GameData.UserInfo.nickName;
				info.avatarUrl = World.Instance.GameData.UserInfo.avatarUrl;
			}
			myRankCell.SetData(info);
			myRankCell.Visible = true;
		}
	}
}

