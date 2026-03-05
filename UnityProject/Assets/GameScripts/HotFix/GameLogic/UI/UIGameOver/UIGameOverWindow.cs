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
		private List<UIGameOverRewardItem> m_RewardItems = new List<UIGameOverRewardItem>();

		private const string REWARD_ITEM_PATH = "Assets/AssetRaw/Prefabs/UI/UIGameOver/m_itemGameOverRewardItem.prefab";

		private static readonly string[] EncourageTexts =
		{
			"再接再厉，下次一定能赢！",
			"别灰心，再试一次吧！",
			"差一点就赢了，加油！",
			"坚持就是胜利，再来一局！",
		};

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

			// 根据胜负切换面板
			m_tfSuccessContent.gameObject.SetActive(isWin);
			m_tfFailedContent.gameObject.SetActive(!isWin);
			if (m_tfRewardContent != null)
			{
				m_tfRewardContent.gameObject.SetActive(isWin);
			}

			// 结果标题与背景
			Color winColor = new Color32(0, 200, 255, 255);
			Color defeatColor = new Color32(175, 175, 175, 255);
			m_txtResult.text = isWin ? "恭 喜 获 得" : "惜 败";
			m_imgGameResultBG.color = isWin ? winColor : defeatColor;
			Animator gameResultAnim = m_imgGameResultBG.GetComponent<Animator>();
			gameResultAnim.enabled = !isWin;

			if (isWin)
			{
				TryRequestUserInfo();
				CalculateReward();
				InitSuccessButtons();
			}
			else
			{
				InitFailedButtons();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			GameEvent.RemoveEventListener<AdsEventParam>(SlimeEvent.OnAdsResultReceived, OnAdsResult);
		}

		#region 按钮初始化

		private void InitSuccessButtons()
		{
			m_txtClaimAds.text = "看广告 领双倍";
			EventTriggerListener.Get(m_btnClaimAds).OnClick = go =>
			{
				RequestAdDoubleReward();
			};

			m_txtClaim.text = "领取奖励";
			EventTriggerListener.Get(m_btnClaim).OnClick = go =>
			{
				OnClickClaim();
			};
		}

		private void InitFailedButtons()
		{
			m_txtEncourage.text = EncourageTexts[Random.Range(0, EncourageTexts.Length)];

			var energyConfig = World.Instance.GetEnergyConfig();
			int energyCost = energyConfig?.levelConsume ?? 0;
			m_txtTryAgain.text = "再来一局";
			m_txtEnergy.text = energyCost > 0 ? $"-{energyCost}" : "";

			EventTriggerListener.Get(m_btnTryAgain).OnClick = go =>
			{
				OnClickTryAgain();
			};

			EventTriggerListener.Get(m_btnBack).OnClick = go =>
			{
				OnClickBack();
			};
		}

		#endregion

		#region 奖励计算与展示

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

			if (m_RewardResult == null) return;

			if (m_RewardResult.isFirstClear)
			{
				m_txtResult.text = "首 通 奖 励";
			}
			ShowRewardItems();
		}

		/// <summary>
		/// 创建奖励道具展示项
		/// </summary>
		private void ShowRewardItems()
		{
			if (m_RewardResult == null || m_tfRewardContent == null) return;

			var summary = m_RewardResult.SumByType(false);
			CreateRewardItems(summary).Forget();

			Log.Info($"[UIGameOverWindow] ShowRewardItems: {summary.Count} reward types, isFirstClear={m_RewardResult.isFirstClear}");
		}

		private async UniTaskVoid CreateRewardItems(Dictionary<ResourceType, int> summary)
		{
			foreach (var item in m_RewardItems)
			{
				item.Destroy();
			}
			m_RewardItems.Clear();

			foreach (var kvp in summary)
			{
				var item = await CreateWidgetByPathAsync<UIGameOverRewardItem>(
					m_tfRewardContent, REWARD_ITEM_PATH);
				item.SetData(kvp.Key, kvp.Value);
				m_RewardItems.Add(item);
			}
		}

		#endregion

		#region 按钮回调

		private void OnClickClaim()
		{
			if (m_RewardClaimed || m_RewardResult == null) return;
			ClaimReward(false).Forget();
		}

		/// <summary>
		/// 请求看广告翻倍奖励
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

		private void OnClickTryAgain()
		{
			int levelId = World.Instance.playingLevelId;
			Close();
			World.Instance.EndGame();
			World.Instance.StartGame(levelId);
		}

		private void OnClickBack()
		{
			Close();
			World.Instance.EndGame();
		}

		#endregion

		#region 奖励领取

		private async UniTaskVoid ClaimReward(bool watchedAd)
		{
			if (m_RewardClaimed) return;
			m_RewardClaimed = true;

			m_btnClaimAds.interactable = false;
			m_btnClaim.interactable = false;

			bool success = await World.Instance.ClaimLevelReward(m_RewardResult.levelId, watchedAd);
			if (!success)
			{
				m_RewardClaimed = false;
				m_btnClaimAds.interactable = true;
				m_btnClaim.interactable = true;
				return;
			}

			Log.Info($"[UIGameOverWindow] Reward claimed: level={m_RewardResult.levelId}, watchedAd={watchedAd}");

			Close();
			World.Instance.EndGame();
		}

		#endregion
	}
}
