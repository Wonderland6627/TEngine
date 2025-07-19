using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: false)]
	partial class UIRuleTipsWindow : UIWindow
	{
		private bool m_IsGuideMode = false;

        protected override void OnCreate()
        {
            base.OnCreate();

			if (userDatas != null && userDatas.Length > 0)
			{
				if (userDatas[0] is bool isGuideMode)
				{
					m_IsGuideMode = isGuideMode;
				}
			}

			EventTriggerListener.Get(m_imgClose.gameObject).OnClick = go =>
			{
				Close();
			};

			if (m_IsGuideMode)
			{
				// World.Instance.PauseGame();
			}

			EventTriggerListener.Get(m_textTitle.gameObject).OnClick = go =>
			{
				World.Instance.ToggleDebugWindow();
			};

			if (World.Instance.GameData.GuideFinish)
			{
				World.Instance.ShowBannerAd();
			}
        }

        protected override void OnDestroy()
        {
			World.Instance.HideBannerAd();
			if (m_IsGuideMode)
			{
            	// World.Instance.ResumeGame();
			}
            base.OnDestroy();
        }
	}

	partial class UIRuleTipsWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgBG;
		private Image m_imgPopupBG;
		private Image m_imgClose;
		private Text m_textTitle;
		private Image m_imgUnit;
		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_imgPopupBG = FindChildComponent<Image>("Content/m_imgPopupBG");
			m_imgClose = FindChildComponent<Image>("Content/m_imgPopupBG/m_imgClose");
			m_textTitle = FindChildComponent<Text>("Content/m_imgPopupBG/m_textTitle");
			m_imgUnit = FindChildComponent<Image>("Content/m_imgPopupBG/layout/RuleCell/TipsImg/TipsArrowImg/Unit_Player/m_imgUnit");
		}
		#endregion
	}
}