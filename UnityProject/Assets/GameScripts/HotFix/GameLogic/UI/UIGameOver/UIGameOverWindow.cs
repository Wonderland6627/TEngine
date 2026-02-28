using UnityEngine;
using UnityEngine.UI;
using TEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: false)]
    partial class UIGameOverWindow
	{
		private GameOverParam m_Param = null;
		private LevelRewardResult m_RewardResult = null;
		private bool m_RewardClaimed = false;

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
			if (isWin)
			{
				TryRequestUserInfo();
				CalculateReward();
			}

			Color winColor = new Color32(0, 200, 255, 255);
			Color defeatColor = new Color32(175, 175, 175, 255);

			m_txtResult.text = isWin ? "恭 喜 获 得" : "惜 败";
			m_imgGameResultBG.color = isWin ? winColor : defeatColor;
			Animator gameResultAnim = m_imgGameResultBG.GetComponent<Animator>();
			gameResultAnim.enabled = !isWin;

			EventTriggerListener.Get(m_btnBack).OnClick = go =>
			{
				OnClickBack();
			};
        }

		protected override void OnDestroy()
		{
			base.OnDestroy();
			GameEvent.RemoveEventListener<AdsEventParam>(SlimeEvent.OnAdsResultReceived, OnAdsResult);
		}

		// 胜利后检查用户信息：若未获取到昵称 则主动触发一次授权按钮
		private void TryRequestUserInfo()
		{
			if (World.Instance.GameData == null) return;

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

		private void CalculateReward()
		{
			int levelId = World.Instance.playingLevelId;
			bool isFirstClear = levelId > World.Instance.GameData.ProgressLevelID;
			m_RewardResult = World.Instance.CalculateLevelReward(levelId, isFirstClear);

			if (m_RewardResult != null)
			{
				ShowRewardInfo();
			}
		}

		/// <summary>
		/// 展示奖励信息（子类或后续可重写此方法来定制UI表现）
		/// </summary>
		private void ShowRewardInfo()
		{
			if (m_RewardResult == null) return;

			var summary = m_RewardResult.SumByType(false);
			summary.TryGetValue(CurrencyTypes.COIN, out int totalCoin);
			summary.TryGetValue("energy", out int totalEnergy);

			string rewardText = $"金币 +{totalCoin}";
			if (totalEnergy > 0)
			{
				rewardText += $"  体力 +{totalEnergy}";
			}
			if (m_RewardResult.isFirstClear)
			{
				rewardText += "  (首通奖励!)";
			}

			Log.Info($"[UIGameOverWindow] ShowRewardInfo: {rewardText}");
		}

		/// <summary>
		/// 请求看广告翻倍奖励（供外部按钮调用）
		/// </summary>
		public void RequestAdDoubleReward()
		{
			if (m_RewardResult == null || m_RewardClaimed) return;

			GameEvent.AddEventListener<AdsEventParam>(SlimeEvent.OnAdsResultReceived, OnAdsResult);
			World.Instance.ShowAds(AdsType.LevelRewardDouble);
		}

		private void OnAdsResult(AdsEventParam param)
		{
			if (param.adsType != AdsType.LevelRewardDouble) return;

			GameEvent.RemoveEventListener<AdsEventParam>(SlimeEvent.OnAdsResultReceived, OnAdsResult);

			if (param.isCompleted)
			{
				ClaimReward(true).Forget();
			}
		}

		private void OnClickBack()
		{
			if (m_Param.IsWin() && !m_RewardClaimed && m_RewardResult != null)
			{
				ClaimReward(false).Forget();
				return;
			}

			Close();
			World.Instance.EndGame();
		}

		private async UniTaskVoid ClaimReward(bool watchedAd)
		{
			if (m_RewardClaimed) return;
			m_RewardClaimed = true;

			bool success = await World.Instance.ClaimLevelReward(m_RewardResult.levelId, watchedAd);
			if (success)
			{
				var summary = m_RewardResult.SumByType(watchedAd);
				summary.TryGetValue(CurrencyTypes.COIN, out int totalCoin);
				summary.TryGetValue("energy", out int totalEnergy);

				string claimedText = $"胜 利\n获得 金币 +{totalCoin}";
				if (totalEnergy > 0)
				{
					claimedText += $"  体力 +{totalEnergy}";
				}
				if (watchedAd)
				{
					claimedText += "  (翻倍!)";
				}
				m_txtResult.text = claimedText;

				Log.Info($"[UIGameOverWindow] Reward claimed: coin={totalCoin}, energy={totalEnergy}, ad={watchedAd}");
			}

			Close();
			World.Instance.EndGame();
		}
	}
}

