using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    partial class UIAdsRewardCell : UIWidget
    {
        protected override void OnCreate()
        {
            base.OnCreate();
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
