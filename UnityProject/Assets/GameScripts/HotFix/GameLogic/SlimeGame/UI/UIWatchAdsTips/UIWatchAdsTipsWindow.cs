using UnityEngine;
using UnityEngine.UI;
using TEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using GameConfig;
using GameLogic.Network;

namespace GameLogic
{
    /// <summary>
    /// 资源不足时弹出的广告礼包提示窗
    /// 从 GlobalConfig.AdRewardPackIds 随机选取一个礼包展示预览，玩家看完广告后向服务端请求领取
    /// </summary>
    [Window(UILayer.UI, fullScreen: false)]
    partial class UIWatchAdsTipsWindow
    {
        private const string REWARD_ITEM_PATH = "Assets/AssetRaw/Prefabs/UI/Widget/UIReward/m_itemRewardItem.prefab";

        /// ---@field 当前随机选中的奖励ID
        private int _selectedRewardId;
        /// ---@field 是否正在请求中（防重复点击）
        private bool _isRequesting;
        /// ---@field 动态创建的奖励项列表
        private List<UIRewardItem> _rewardItems = new List<UIRewardItem>();

        protected override void OnCreate()
        {
            base.OnCreate();

            var adRewardIds = ConfigSystem.Instance.Tables.TbGlobalConfig.AdRewardPackIds;
            if (adRewardIds == null || adRewardIds.Count == 0)
            {
                Log.Error("[UIWatchAdsTipsWindow] AdRewardPackIds is null or empty");
                Close();
                return;
            }

            _selectedRewardId = adRewardIds[Random.Range(0, adRewardIds.Count)];

            var reward = ConfigSystem.Instance.Tables.TbReward.GetOrDefault(_selectedRewardId);
            if (reward == null)
            {
                Log.Error($"[UIWatchAdsTipsWindow] Reward not found: {_selectedRewardId}");
                Close();
                return;
            }

            m_itemRewardItem.SetActive(false);
            CreateRewardPreview(reward).Forget();

            EventTriggerListener.Get(m_btnWatch.gameObject).OnClick = _ => OnWatchAdClick();
            EventTriggerListener.Get(m_imgClose.gameObject).OnClick = _ => Close();
            EventTriggerListener.Get(m_imgBG.gameObject).OnClick = _ => Close();

            GameEvent.AddEventListener<AdsEventParam>(SlimeEvent.OnAdsResultReceived, OnAdsResult);
        }

        protected override void OnDestroy()
        {
            GameEvent.RemoveEventListener<AdsEventParam>(SlimeEvent.OnAdsResultReceived, OnAdsResult);
            foreach (var item in _rewardItems)
            {
                item.Destroy();
            }
            _rewardItems.Clear();
            base.OnDestroy();
        }

        private async UniTaskVoid CreateRewardPreview(Reward reward)
        {
            foreach (var entry in reward.RewardItems)
            {
                var item = await CreateWidgetByPathAsync<UIRewardItem>(
                    m_tfRewardsContent, REWARD_ITEM_PATH);
                item.SetData(entry.ItemType, entry.ItemId, entry.Amount);
                _rewardItems.Add(item);
            }
        }

        private void OnWatchAdClick()
        {
            if (_isRequesting) return;

            World.Instance.ShowAds(AdsType.AdsGiftPack, _selectedRewardId);
            Log.Info($"[UIWatchAdsTipsWindow] Watch ad clicked, rewardId={_selectedRewardId}");
        }

        private void OnAdsResult(AdsEventParam param)
        {
            if (param.adsType != AdsType.AdsGiftPack) return;

            if (!param.isCompleted)
            {
                Log.Warning("[UIWatchAdsTipsWindow] Ad not completed");
                return;
            }

            ClaimReward().Forget();
        }

        private async UniTaskVoid ClaimReward()
        {
            _isRequesting = true;
            m_btnWatch.interactable = false;

            var response = await NetManager.Instance.CallHttp<ClaimAdsGiftPackResponse>(
                "claimAdsGiftPack",
                new { rewardId = _selectedRewardId }
            );

            if (!response.IsSuccess || response.data == null)
            {
                Log.Error($"[UIWatchAdsTipsWindow] ClaimAdsGiftPack failed: {response.ErrorMessage}");
                _isRequesting = false;
                m_btnWatch.interactable = true;
                return;
            }

            World.Instance.SyncResources(response.data.resources);
            World.Instance.SyncGoods(response.data.goods);

            var rewardParam = new RewardParam();
            foreach (var reward in response.data.rewards)
            {
                rewardParam.AddReward((EItemType)reward.itemType, reward.itemId, reward.amount);
            }

            Close();
            GameModule.UI.ShowUIAsync<UIGetRewardWindow>(rewardParam);

            Log.Info($"[UIWatchAdsTipsWindow] Claim success, rewardId={_selectedRewardId}");
        }
    }
}
