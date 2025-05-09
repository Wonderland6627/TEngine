using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: true)]
	partial class UIAdsRewardWindow : UIWindow
	{
		protected override void OnCreate()
		{
			base.OnCreate();

			var rewardActions = World.Instance.GetRewardActions();
			for (int i = 0; i < rewardActions.Count; i++)
			{
				UIAdsRewardCell rewardCell = CreateWidgetByPrefab<UIAdsRewardCell>(m_itemRewardCell, m_rectRewardContent);
				rewardCell.SetData(rewardActions[i]);
			}

			EventTriggerListener.Get(m_tfTapArea.gameObject).OnClick = go =>
			{
				Close();
			};
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