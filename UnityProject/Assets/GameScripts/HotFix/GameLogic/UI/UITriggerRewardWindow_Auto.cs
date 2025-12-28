using UnityEngine;
using UnityEngine.UI;
using TEngine;
using UnityEngine.Events;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: false)]
	partial class UITriggerRewardWindow : UIWindow
	{
		private UnityAction closeAction;
		
		protected override void OnCreate()
		{
			base.OnCreate();
			
			EventTriggerListener.Get(m_imgBG.gameObject).OnClick = go =>
			{
				Close();
			};
        }

        protected override void OnDestroy()
        {
			closeAction?.Invoke();
            base.OnDestroy();
        }
	}
	
	partial class UITriggerRewardWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgBG;
		private Image m_imgGameResultBGRoot;
		private Image m_imgGameResultBG;
		private Text m_textResult;
		private RectTransform m_rectEffect;
		private Image m_imgReward;
		private Text m_textRewardTxt;
		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_imgGameResultBGRoot = FindChildComponent<Image>("Content/m_imgGameResultBGRoot");
			m_imgGameResultBG = FindChildComponent<Image>("Content/m_imgGameResultBGRoot/m_imgGameResultBG");
			m_textResult = FindChildComponent<Text>("Content/m_imgGameResultBGRoot/m_imgGameResultBG/m_textResult");
			m_rectEffect = FindChildComponent<RectTransform>("Content/m_rectEffect");
			m_imgReward = FindChildComponent<Image>("Content/m_imgReward");
			m_textRewardTxt = FindChildComponent<Text>("Content/m_textRewardTxt");
		}
		#endregion
	}
}