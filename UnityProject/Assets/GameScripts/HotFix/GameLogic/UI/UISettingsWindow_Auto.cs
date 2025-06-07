using UnityEngine;
using UnityEngine.UI;
using TEngine;
using UnityEngine.Events;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: false)]
	partial class UISettingsWindow : UIWindow
	{
		private UnityAction closeAction;

		protected override void OnCreate()
		{
			base.OnCreate();

			m_textTitle.raycastTarget = true;
			if (userDatas != null && userDatas.Length > 0)
			{
				if (userDatas[0] is UnityAction ca)
				{
					closeAction = ca;
				}
			}

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
			
			m_textVersion.text = $"Version: {GameModule.Resource.GetPackageVersion()} {World.Instance.GameData.UserInfo.nickName}";
		}

        protected override void OnDestroy()
        {
			closeAction?.Invoke();
            base.OnDestroy();
        }
	}

	partial class UISettingsWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgBG;
		private Image m_imgPopupBG;
		private Image m_imgClose;
		private Text m_textTitle;
		private Toggle m_togSound;
		private Toggle m_togVibration;
		private Button m_btnSave;
		private Text m_textVersion;
		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_imgPopupBG = FindChildComponent<Image>("Content/m_imgPopupBG");
			m_imgClose = FindChildComponent<Image>("Content/m_imgPopupBG/m_imgClose");
			m_textTitle = FindChildComponent<Text>("Content/m_imgPopupBG/m_textTitle");
			m_togSound = FindChildComponent<Toggle>("Content/m_imgPopupBG/layout/m_togSound");
			m_togVibration = FindChildComponent<Toggle>("Content/m_imgPopupBG/layout/m_togVibration");
			m_btnSave = FindChildComponent<Button>("Content/m_imgPopupBG/layout/m_btnSave");
			m_textVersion = FindChildComponent<Text>("Content/m_imgPopupBG/m_textVersion");
		}
		#endregion
	}
}