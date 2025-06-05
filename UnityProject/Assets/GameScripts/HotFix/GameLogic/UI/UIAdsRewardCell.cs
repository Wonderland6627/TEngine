using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    partial class UIAdsRewardCell : UIWidget
    {
        public RewardAction rewardAction;

        protected override void OnCreate()
        {
            base.OnCreate();
            EventTriggerListener.Get(gameObject).OnClick = OnCellClick;
        }

        public void SetData(RewardAction data)
        {
            rewardAction = data;
            m_textRewardTxt.resizeTextForBestFit = true;
            m_textRewardTxt.text = data.GetDescription();
            m_imgReward.sprite = GameModule.Resource.LoadAsset<Sprite>(data.config.iconPath);
            m_imgAds.gameObject.SetActive(false);
            // m_imgAds.gameObject.SetActive(data.NeedAds());
        }

        private void OnCellClick(GameObject go) 
        {
            Log.Info($"[UIAdsRewardCell] OnCellClick [{rewardAction.toString()}]");
            World.Instance.TryTriggerReward(rewardAction);
            GameModule.UI.CloseUI<UIAdsRewardWindow>();
        }
    }

    partial class UIAdsRewardCell
    {
		#region 脚本工具生成的代码
		private Image m_imgReward;
		private Text m_textRewardTxt;
		private Image m_imgAds;
		protected override void ScriptGenerator()
		{
			m_imgReward = FindChildComponent<Image>("m_imgReward");
			m_textRewardTxt = FindChildComponent<Text>("m_textRewardTxt");
			m_imgAds = FindChildComponent<Image>("m_imgAds");
		}
		#endregion
    }
}
