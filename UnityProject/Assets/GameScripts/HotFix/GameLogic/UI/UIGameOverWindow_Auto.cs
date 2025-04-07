using UnityEngine;
using UnityEngine.UI;
using TEngine;
using System.Collections.Generic;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: false)]
    partial class UIGameOverWindow : UIWindow
	{
		private List<Image> m_Stars = new List<Image>();

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

			m_Stars.Add(m_imgStar1);
			m_Stars.Add(m_imgStar2);
			m_Stars.Add(m_imgStar3);

			int starCount = m_Param.GetStarCount();
			m_Stars.ForEach(star =>
			{
				star.gameObject.SetActive(false);
			});
			for (int i = 0; i < starCount; i++)
			{
				m_Stars[i].gameObject.SetActive(true);
			}

			EventTriggerListener.Get(m_tfTapArea.gameObject).OnClick = go =>
			{
				Close();
			};
        }
	}

	partial class UIGameOverWindow
	{
		#region 脚本工具生成的代码
		private Transform m_tfTapArea;
		private Image m_imgStar1;
		private Image m_imgStar2;
		private Image m_imgStar3;
		private Text m_textResult;
		private Text m_textTips;
		protected override void ScriptGenerator()
		{
			m_tfTapArea = FindChild("bg/m_tfTapArea");
			m_imgStar1 = FindChildComponent<Image>("bg/StarsContent/m_imgStar1");
			m_imgStar2 = FindChildComponent<Image>("bg/StarsContent/m_imgStar2");
			m_imgStar3 = FindChildComponent<Image>("bg/StarsContent/m_imgStar3");
			m_textResult = FindChildComponent<Text>("bg/m_textResult");
			m_textTips = FindChildComponent<Text>("bg/m_textTips");
		}
		#endregion
	}
}