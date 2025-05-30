using UnityEngine;
using UnityEngine.UI;
using TEngine;
using System.Collections.Generic;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: false)]
    partial class UIGameOverWindow : UIWindow
	{
		private GameOverParam m_Param = null;

		protected override void OnCreate()
        {
            base.OnCreate();

			if (userDatas != null && userDatas.Length > 0)
			{
				if (userDatas[0] is GameOverParam param)
				{
					m_Param = param;
				}
			}
			if (m_Param == null)
			{
				Debug.LogError("UIGameOverWindow OnCreate GameOverParam is null");
				Close();
				return;
			}

			m_textResult.text = m_Param.IsWin() ? "Victory" : "Defeat";

			EventTriggerListener.Get(m_tfTapArea.gameObject).OnClick = go =>
			{
				Close();
				World.Instance.EndGame();
			};
        }
	}

	partial class UIGameOverWindow
	{
		#region 脚本工具生成的代码
		private Transform m_tfTapArea;
		private Text m_textResult;
		private Text m_textTips;
		protected override void ScriptGenerator()
		{
			m_tfTapArea = FindChild("bg/m_tfTapArea");
			m_textResult = FindChildComponent<Text>("bg/m_textResult");
			m_textTips = FindChildComponent<Text>("bg/m_textTips");
		}
		#endregion
	}
}