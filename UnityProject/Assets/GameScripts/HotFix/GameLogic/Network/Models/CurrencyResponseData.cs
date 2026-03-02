using System;

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
        public System.Collections.Generic.Dictionary<string, int> resources;
    }
}
