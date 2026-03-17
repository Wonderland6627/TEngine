using System;
using System.Collections.Generic;

namespace GameLogic.Network
{
    /// <summary>
    /// 资源更新响应数据模型
    /// </summary>
    [Serializable]
    public class UpdateResourceResponse
    {
        public int resourceType;
        public int value;
        public int change;
    }

    /// <summary>
    /// 获取所有资源响应数据模型
    /// </summary>
    [Serializable]
    public class GetResourcesResponse
    {
        public Dictionary<string, int> resources;
    }

    /// <summary>
    /// 每日签到响应数据模型
    /// </summary>
    [Serializable]
    public class ClaimDailyCheckinResponse
    {
        public List<RewardItemData> rewards;
        public Dictionary<string, int> resources;
        public Dictionary<string, int> goods;
    }
}
