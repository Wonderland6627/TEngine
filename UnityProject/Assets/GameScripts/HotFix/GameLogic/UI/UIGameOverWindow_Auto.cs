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

			m_textResult.text = m_Param.IsWin() ? "胜 利" : "失 败";

			EventTriggerListener.Get(m_btnBack).OnClick = go =>
			{
				Close();
				World.Instance.EndGame();
			};
        }
	}

	partial class UIGameOverWindow
	{
		#region 脚本工具生成的代码
		private Image m_imgBG;
		private Image m_imgGameResultContent;
		private Image m_imgGameResultTitleBG;
		private Text m_textGameResultTitle;
		private Image m_imgGameResultBG;
		private Text m_textResult;
		private Button m_btnBack;
		protected override void ScriptGenerator()
		{
			m_imgBG = FindChildComponent<Image>("Content/m_imgBG");
			m_imgGameResultContent = FindChildComponent<Image>("Content/m_imgGameResultContent");
			m_imgGameResultTitleBG = FindChildComponent<Image>("Content/m_imgGameResultContent/m_imgGameResultTitleBG");
			m_textGameResultTitle = FindChildComponent<Text>("Content/m_imgGameResultContent/m_imgGameResultTitleBG/m_textGameResultTitle");
			m_imgGameResultBG = FindChildComponent<Image>("Content/m_imgGameResultContent/m_imgGameResultBG");
			m_textResult = FindChildComponent<Text>("Content/m_imgGameResultContent/m_imgGameResultBG/m_textResult");
			m_btnBack = FindChildComponent<Button>("Content/m_imgGameResultContent/m_btnBack");
		}
		#endregion
	}
}