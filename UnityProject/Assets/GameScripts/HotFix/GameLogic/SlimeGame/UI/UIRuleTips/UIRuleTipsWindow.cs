using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: false)]
	partial class UIRuleTipsWindow
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
}

