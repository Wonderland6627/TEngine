using Cysharp.Threading.Tasks;
using GameLogic.Network;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 统一资源管理（World 的部分类）
    /// 替代原 World_Currency + World_Energy
    /// </summary>
    public partial class World
    {
        /// <summary>
        /// 通用资源增减（调用服务器接口）
        /// </summary>
        /// <param name="type">资源类型</param>
        /// <param name="change">变化量（正数增加，负数扣除）</param>
        /// <param name="source">来源（ResourceSource 常量）</param>
        /// <returns>更新后的值，失败返回 -1</returns>
        public async UniTask<int> UpdateResource(ResourceType type, int change, string source)
        {
            var request = new
            {
                resourceType = (int)type,
                change = change,
                source = source
            };

            var response = await NetManager.Instance.CallHttp<UpdateResourceResponse>("updateResource", request);

            if (!response.IsSuccess || response.data == null)
            {
                Log.Error($"[World] UpdateResource failed: type={type}, change={change}, err={response.ErrorMessage}");
                return -1;
            }

            GameData.UpdateResource(type, response.data.value);
            return response.data.value;
        }

        /// <summary>
        /// 尝试消耗体力（进入关卡前调用）
        /// </summary>
        public async UniTask<bool> TryConsumeEnergy()
        {
            int consumeAmount = GlobalConfig.LevelEnergyConsume;
            int currentEnergy = GameData.Energy;

            if (currentEnergy < consumeAmount)
            {
                Log.Warning($"[World] Energy not enough: {currentEnergy} < {consumeAmount}");
                GameEvent.Send(SlimeEvent.OnResourceNotEnough, ResourceType.Energy);
                return false;
            }

            int result = await UpdateResource(ResourceType.Energy, -consumeAmount, ResourceSource.LEVEL_CONSUME);
            if (result >= 0)
            {
                Log.Info($"[World] Consume energy success: -{consumeAmount}");
                return true;
            }
            return false;
        }
    }
}
