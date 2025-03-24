using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public class LevelConfig
    {
        public int levelId { get; set; }
        public List<Castle> castles { get; set; }
        public List<Road> roads { get; set; }

        public class Castle
        {
            public int id { get; set; }
            public int castleType { get; set; }
            public bool occupiedOnStart { get; set; }
            public int occupiedSlimeType { get; set; }
            public int occupiedUnitCount { get; set; }
            public Position position { get; set; }

            public class Position
            {
                public int x { get; set; }
                public int y { get; set; }
            }
        }

        public class Road
        {
            public int startCastleId { get; set; }
            public int endCastleId { get; set; }
        }
    }

    public class LevelReader
    {
        public List<LevelConfig> configs { get; private set; }

        public async UniTask LoadLevelConfig(string jsonPath)
        {
            var res = await GameModule.Resource.LoadAssetAsync<TextAsset>(jsonPath);
            var json = res.text;
            configs = Utility.Json.ToObject<List<LevelConfig>>(json);
        }
    }

    //===================================================================================

    public class UnitConfig
    {
        public int unitType { get; set; }
        public string unitImageName { get; set; }
        public string unitPrefabPath { get; set; }
        public float moveDuration { get; set; }
        public float moveSpeed { get; set; }
    }

    public class UnitReader
    {
        public List<UnitConfig> configs { get; private set; }

        public async UniTask LoadUnitConfig(string jsonPath)
        {
            var res = await GameModule.Resource.LoadAssetAsync<TextAsset>(jsonPath);
            var json = res.text;
            configs = Utility.Json.ToObject<List<UnitConfig>>(json);
        }
    }
}
