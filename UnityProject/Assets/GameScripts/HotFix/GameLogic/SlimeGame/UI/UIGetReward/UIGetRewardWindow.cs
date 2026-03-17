using UnityEngine;
using UnityEngine.UI;
using TEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using GameConfig;

namespace GameLogic
{
    /// <summary>
    /// 奖励展示参数，任何地方都可以构建后打开 UIGetRewardWindow
    /// 用法:
    ///   GameModule.UI.ShowUIAsync&lt;UIGetRewardWindow&gt;(new RewardParam(rewardDict));
    ///   GameModule.UI.ShowUIAsync&lt;UIGetRewardWindow&gt;(new RewardParam().AddReward(ResourceType.Coin, 100));
    ///   GameModule.UI.ShowUIAsync&lt;UIGetRewardWindow&gt;(new RewardParam().AddReward(EItemType.GOODS, 1001, 2));
    /// </summary>
    public class RewardParam
    {
        public List<RewardDisplayEntry> Rewards { get; private set; } = new List<RewardDisplayEntry>();

        public RewardParam() { }

        /// <summary>
        /// 便捷接口：添加资源类型奖励
        /// </summary>
        public RewardParam AddReward(ResourceType type, int amount)
        {
            Rewards.Add(new RewardDisplayEntry(EItemType.RESOURCE, (int)type, amount));
            return this;
        }

        /// <summary>
        /// 通用接口：按 item_type 添加奖励，支持任何物品类型
        /// </summary>
        public RewardParam AddReward(EItemType itemType, int itemId, int amount)
        {
            Rewards.Add(new RewardDisplayEntry(itemType, itemId, amount));
            return this;
        }
    }

    /// <summary>
    /// 奖励展示条目，统一使用 (ItemType, ItemId, Amount) 三元组
    /// </summary>
    public struct RewardDisplayEntry
    {
        public EItemType ItemType;
        public int ItemId;
        public int Amount;

        public RewardDisplayEntry(EItemType itemType, int itemId, int amount)
        {
            ItemType = itemType;
            ItemId = itemId;
            Amount = amount;
        }
    }

    [Window(UILayer.UI, fullScreen: false)]
    partial class UIGetRewardWindow
    {
        private const string REWARD_ITEM_PATH = "Assets/AssetRaw/Prefabs/UI/Widget/UIReward/m_itemRewardItem.prefab";

        private RewardParam m_Param;
        private List<UIRewardItem> m_RewardItems = new List<UIRewardItem>();

        protected override void OnCreate()
        {
            base.OnCreate();

            if (userDatas != null && userDatas.Length > 0 && userDatas[0] is RewardParam param)
            {
                m_Param = param;
            }
            if (m_Param == null || m_Param.Rewards.Count == 0)
            {
                Log.Error("[UIGetRewardWindow] RewardParam is null or empty");
                Close();
                return;
            }

            EventTriggerListener.Get(m_imgBG.gameObject).OnClick = go => Close();

            CreateRewardItems().Forget();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var item in m_RewardItems)
            {
                item.Destroy();
            }
            m_RewardItems.Clear();
        }

        private async UniTaskVoid CreateRewardItems()
        {
            foreach (var reward in m_Param.Rewards)
            {
                var item = await CreateWidgetByPathAsync<UIRewardItem>(
                    m_tfRewardContent, REWARD_ITEM_PATH);
                item.SetData(reward.ItemType, reward.ItemId, reward.Amount);
                m_RewardItems.Add(item);
            }
        }
    }
}
