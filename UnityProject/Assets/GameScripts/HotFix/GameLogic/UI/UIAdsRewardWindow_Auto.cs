using UnityEngine;
using UnityEngine.UI;
using TEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: true)]
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
		private Transform m_tfTapArea;
		private GameObject m_itemRewardCell;
		private RectTransform m_rectRewardContent;
		private Button m_btnRefresh;
		protected override void ScriptGenerator()
		{
			m_tfTapArea = FindChild("bg/m_tfTapArea");
			m_itemRewardCell = FindChild("bg/m_itemRewardCell").gameObject;
			m_rectRewardContent = FindChildComponent<RectTransform>("bg/m_rectRewardContent");
			m_btnRefresh = FindChildComponent<Button>("bg/m_btnRefresh");
		}
		#endregion
	}
}