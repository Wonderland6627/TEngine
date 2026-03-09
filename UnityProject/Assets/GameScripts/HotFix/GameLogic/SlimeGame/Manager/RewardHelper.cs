using Cysharp.Threading.Tasks;
using GameConfig;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 通用奖励发放工具类，基于 TbReward 配置表
    /// </summary>
    public static class RewardHelper
    {
        public static Reward GetReward(int rewardId)
        {
            var reward = ConfigSystem.Instance.Tables.TbReward.GetOrDefault(rewardId);
            if (reward == null)
            {
                Log.Error($"[RewardHelper] Reward not found: id={rewardId}");
            }
            return reward;
        }

        /// <summary>
        /// 发放奖励包中的所有条目
        /// </summary>
        /// <param name="rewardId">TbReward 表的 id</param>
        /// <param name="source">资源来源（ResourceSource 常量）</param>
        /// <returns>是否全部发放成功</returns>
        public static async UniTask<bool> ClaimReward(int rewardId, string source)
        {
            var reward = GetReward(rewardId);
            if (reward == null) return false;

            foreach (var entry in reward.RewardItems)
            {
                switch ((ItemType)entry.ItemType)
                {
                    case ItemType.Resource:
                        int result = await World.Instance.UpdateResource((ResourceType)entry.ItemId, entry.Amount, source);
                        if (result < 0)
                        {
                            Log.Error($"[RewardHelper] ClaimReward failed: rewardId={rewardId}, itemId={entry.ItemId}, amount={entry.Amount}");
                            return false;
                        }
                        Log.Info($"[RewardHelper] ClaimReward success: resource={((ResourceType)entry.ItemId)}, amount={entry.Amount}, source={source}");
                        break;

                    case ItemType.Goods:
                        Log.Warning($"[RewardHelper] Goods reward not implemented yet: itemId={entry.ItemId}, amount={entry.Amount}");
                        break;

                    default:
                        Log.Warning($"[RewardHelper] Unknown item type: {entry.ItemType}");
                        break;
                }
            }

            return true;
        }
    }
}
