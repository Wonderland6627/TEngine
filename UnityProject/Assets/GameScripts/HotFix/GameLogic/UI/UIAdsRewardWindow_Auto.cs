using UnityEngine;
using UnityEngine.UI;
using TEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: false)]
	partial class UIAdsRewardWindow : UIWindow
	{
		private UnityAction closeAction;
		private List<UIWidget> childCells = new();
		
		protected override void OnCreate()
		{
			base.OnCreate();

			if (userDatas != null && userDatas.Length > 0)
			{
				if (userDatas[0] is UnityAction ca)
				{
					closeAction = ca;
				}
			}

			RefreshRewardCells();
			GetIfRefreshRewardsWithAds();

			GameEvent.AddEventListener<AdsEventParam>(SlimeEvent.OnAdsResultReceived, OnAdsResultReceived);

			EventTriggerListener.Get(m_btnRefresh.gameObject).OnClick = go =>
			{
				TryRefreshRewardWithAds();
			};
		}

		private void TryRefreshRewardWithAds()
		{
			bool needAds = GetIfRefreshRewardsWithAds();
			if (!needAds)
			{
				RefreshRewardCells();
				return;
			}

			World.Instance.ShowAds(AdsType.RefreshRewardsList);
		}
		
		private void OnAdsResultReceived(AdsEventParam param)
		{
			if (param == null || !param.isCompleted)
			{
				Log.Info("[UIAdsRewardWindow] ads result is null or not completed");
				return;
			}

			if (param.adsType == AdsType.RefreshRewardsList)
			{
				RefreshRewardCells();
				Log.Info("[UIAdsRewardWindow] refresh rewards list with ads");
			}
		}

		private void RefreshRewardCells()
		{
			for (int i = 0; i < childCells.Count; i++)
			{
				childCells[i].Destroy();
			}
		}

		private bool GetIfRefreshRewardsWithAds()
		{
			bool needAds = false;
			needAds = !Application.isEditor;
			m_imgAds.gameObject.SetActive(needAds);

			return needAds;
		}

		protected override void OnDestroy()
        {
			GameEvent.RemoveEventListener<AdsEventParam>(SlimeEvent.OnAdsResultReceived, OnAdsResultReceived);
			
			closeAction?.Invoke();
            base.OnDestroy();
        }
	}

	partial class UIAdsRewardWindow
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