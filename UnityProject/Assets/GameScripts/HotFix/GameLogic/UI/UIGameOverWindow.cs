using UnityEngine;
using UnityEngine.UI;
using TEngine;
using System.Collections.Generic;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: false)]
    partial class UIGameOverWindow
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

			bool isWin = m_Param.IsWin();

			// 胜利后检查用户信息：若未获取到昵称 则主动触发一次授权按钮
			if (isWin)
			{
 				if (World.Instance.GameData != null)
 				{
 					var userInfo = World.Instance.GameData.UserInfo;
 					bool hasUserName = userInfo != null && !string.IsNullOrEmpty(userInfo.nickName);
 					bool requestedToday = World.Instance.GameData.HasRequestedUserInfoToday();
 					if (!hasUserName && !requestedToday)
 					{
 						Log.Info("[UIGameOverWindow] Win but user nickname is empty, request user info (once per day)");
 						World.Instance.GameData.MarkRequestedUserInfoToday();
 						World.Instance.RequestUserInfo();
 					}
 				}
			}

			Color winColor = new Color32(0, 200, 255, 255);
			Color defeatColor = new Color32(175, 175, 175, 255);

			m_textResult.text = isWin ? "胜 利" : "失 败";
			m_imgGameResultBG.color = isWin ? winColor : defeatColor;
			Animator gameResultAnim = m_imgGameResultBG.GetComponent<Animator>();
			gameResultAnim.enabled = !isWin;

			EventTriggerListener.Get(m_btnBack).OnClick = go =>
			{
				Close();
				World.Instance.EndGame();
			};
        }
	}
}

