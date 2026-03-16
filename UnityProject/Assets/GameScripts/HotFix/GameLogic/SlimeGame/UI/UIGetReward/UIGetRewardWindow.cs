using UnityEngine;
using UnityEngine.UI;
using TEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace GameLogic
{
    /// <summary>
    /// 奖励展示参数，任何地方都可以构建后打开 UIGetRewardWindow
    /// 用法:
    ///   GameModule.UI.ShowUIAsync&lt;UIGetRewardWindow&gt;(new RewardParam(rewardDict));
    ///   GameModule.UI.ShowUIAsync&lt;UIGetRewardWindow&gt;(new RewardParam().AddReward(ResourceType.Coin, 100));
    /// </summary>
    public class RewardParam
    {
        public List<RewardEntry> Rewards { get; private set; } = new List<RewardEntry>();

        public RewardParam() { }

        public RewardParam(Dictionary<ResourceType, int> rewards)
        {
            foreach (var kvp in rewards)
            {
                Rewards.Add(new RewardEntry(kvp.Key, kvp.Value));
            }
        }

        public RewardParam AddReward(ResourceType type, int amount)
        {
            Rewards.Add(new RewardEntry(type, amount));
            return this;
        }
    }

    public struct RewardEntry
    {
        public ResourceType Type;
        public int Amount;

        public RewardEntry(ResourceType type, int amount)
        {
            Type = type;
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
                item.SetData(reward.Type, reward.Amount);
                m_RewardItems.Add(item);
            }
        }
    }
}
