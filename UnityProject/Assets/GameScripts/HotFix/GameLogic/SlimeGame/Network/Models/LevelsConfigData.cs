using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace GameLogic.Network
{
    /// <summary>
    /// 关卡配置数据模型
    /// </summary>
    [Serializable]
    public class LevelsConfigData
    {
        public string _id;
        public ConfigsData configs;
    }
    
    [Serializable]
    public class ConfigsData
    {
        [Newtonsoft.Json.JsonProperty("levels")]
        public JArray levels;
    }
}

