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

			if (userDatas != null && userDatas.Length > 0)
			{
				if (userDatas[0] is UnityAction ca)
				{
					closeAction = ca;
				}
			}

			EventTriggerListener.Get(m_btnBack).OnClick = go =>
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

	partial class UISettingsWindow
	{
		#region 脚本工具生成的代码
		private Toggle m_togCommon;
		private Toggle m_togVibration;
		private Button m_btnCommon;
		private Button m_btnBack;
		protected override void ScriptGenerator()
		{
			m_togCommon = FindChildComponent<Toggle>("bg/m_togCommon");
			m_togVibration = FindChildComponent<Toggle>("bg/m_togVibration");
			m_btnCommon = FindChildComponent<Button>("bg/m_btnCommon");
			m_btnBack = FindChildComponent<Button>("bg/m_btnBack");
		}
		#endregion
	}
}