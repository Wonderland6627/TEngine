using UnityEngine;
using UnityEngine.UI;
using TEngine;
using UnityEngine.Events;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: false)]
	partial class UISettingsWindow
	{
		protected override void OnCreate()
		{
			base.OnCreate();

			m_textTitle.raycastTarget = true;
			m_textVersion.raycastTarget = true;

			m_togSound.isOn = World.Instance.GameData.EnableSound;
			m_togVibration.isOn = World.Instance.GameData.EnableVibration;

			m_togSound.onValueChanged.AddListener(value =>
			{
				World.Instance.GameData.EnableSound = value;
				GameModule.Audio.MusicVolume = value ? 0.5f : 0;
			});
			m_togVibration.onValueChanged.AddListener(value =>
			{
				World.Instance.GameData.EnableVibration = value;
			});

			EventTriggerListener.Get(m_imgClose.gameObject).OnClick = go =>
			{
				Close();
			};
			EventTriggerListener.Get(m_btnSave.gameObject).OnClick = go =>
			{
				Close();
			};

			EventTriggerListener.Get(m_textTitle.gameObject).OnClick = go =>
			{
				World.Instance.ToggleDebugWindow();
			};

			EventTriggerListener.Get(m_textVersion.gameObject).OnClick = go =>
			{
				World.Instance.ToggleShowAds();
			};
			
			m_textVersion.text = $"Version: {GameModule.Resource.GetPackageVersion()} {World.Instance.GameData.UserInfo.nickName}";
			
			World.Instance.ShowBannerAd();
		}

        protected override void OnDestroy()
        {
	        World.Instance.HideBannerAd();
            base.OnDestroy();
        }
	}
}

