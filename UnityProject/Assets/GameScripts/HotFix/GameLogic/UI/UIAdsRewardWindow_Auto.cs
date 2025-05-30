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

			// EventTriggerListener.Get(m_tfTapArea.gameObject).OnClick = go =>
			// {
			// 	Close();
			// };

			EventTriggerListener.Get(m_btnRefresh.gameObject).OnClick = go =>
			{
				RefreshRewardCells();
			};
		}

		private void RefreshRewardCells()
		{
			for (int i = 0; i < childCells.Count; i++)
			{
				childCells[i].Destroy();
			}
			var rewardActions = World.Instance.GetRewardActions();
			for (int i = 0; i < rewardActions.Count; i++)
			{
				UIAdsRewardCell rewardCell = CreateWidgetByPrefab<UIAdsRewardCell>(m_itemRewardCell, m_rectRewardContent);
				rewardCell.SetData(rewardActions[i]);
				childCells.Add(rewardCell);
			}
		}

		protected override void OnDestroy()
        {
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
		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_imgRewardContent = FindChildComponent<Image>("Content/m_imgRewardContent");
			m_imgRewardTitleBG = FindChildComponent<Image>("Content/m_imgRewardContent/m_imgRewardTitleBG");
			m_textRewardTitle = FindChildComponent<Text>("Content/m_imgRewardContent/m_imgRewardTitleBG/m_textRewardTitle");
			m_itemRewardCell = FindChild("Content/m_imgRewardContent/m_itemRewardCell").gameObject;
			m_rectRewardContent = FindChildComponent<RectTransform>("Content/m_imgRewardContent/m_rectRewardContent");
			m_btnRefresh = FindChildComponent<Button>("Content/m_btnRefresh");
		}
		#endregion
	}
}