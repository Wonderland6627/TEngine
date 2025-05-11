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
        private Text m_textRewardTxt;
        protected override void ScriptGenerator()
        {
            m_textRewardTxt = FindChildComponent<Text>("m_textRewardTxt");
        }
        #endregion
    }
}
