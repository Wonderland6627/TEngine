using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public class SlimeConfigReader<T> where T : SlimeConfig
    {
        public List<T> configs { get; protected set; }

        public async UniTask LoadLocalConfig(string jsonPath)
        {
            var res = await GameModule.Resource.LoadAssetAsync<TextAsset>(jsonPath);
            var json = res.text;
            configs = Utility.Json.ToObject<List<T>>(json);
        }

        public void SetConfigs(List<T> configs)
        {
            this.configs = configs;
        }
    }

    public abstract class SlimeConfig {}

    //===================================================================================
    
    public class LevelConfig : SlimeConfig
    {
        public int levelId { get; set; }
        public List<Castle> castles { get; set; }
        public List<Road> roads { get; set; }

        public Config config { get; set; }

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

        public class Config
        {
            public float playerSpawnInterval { get; set; }
            public float playerAttackInterval { get; set; }
            public float enemy_1_SpawnInterval { get; set; }
            public float enemy_1_AttackInterval { get; set; }
        }
    }

    //===================================================================================

    public class UnitConfig : SlimeConfig
    {
        public int unitType { get; set; }
        public string unitImageName { get; set; }
        public string unitPrefabPath { get; set; }
        public float moveDuration { get; set; }
        public float moveSpeed { get; set; }
    }

    //===================================================================================

    public class RewardConfig : SlimeConfig
    {
        public int rewardId { get; set; }                    // 锦囊ID
        public string rewardName { get; set; }               // 锦囊名称
        public string rewardDesc { get; set; }               // 锦囊描述
        public string iconPath { get; set; }                 // 锦囊图标路径
        public RewardType rewardType { get; set; }           // 锦囊类型
        public float[] effectValues { get; set; }            // 不同稀有度的效果数值[普通,稀有,史诗,传说]
        public float[] durations { get; set; }               // 不同稀有度的持续时间[普通,稀有,史诗,传说]
        public bool[] needAds { get; set; }                  // 不同稀有度是否需要看广告[普通,稀有,史诗,传说]
    }

    public enum RewardType: int
    {
        // 单个城堡固定数量变化
        AddSingleCastleSlime = 0,      // 指定城堡增加n个史莱姆
        ReduceSingleCastleSlime = 1,   // 指定城堡减少n个史莱姆

        // 单个城堡百分比变化
        AddSingleCastleSlimePercent = 2,   // 指定城堡增加n%个史莱姆
        ReduceSingleCastleSlimePercent = 3, // 指定城堡减少n%个史莱姆

        // 所有城堡固定数量变化
        AddAllCastleSlimes = 4,      // 所有友方城堡增加n个史莱姆
        ReduceAllCastleSlimes = 5,      // 所有敌方城堡减少n个史莱姆

        // 所有城堡百分比变化
        AddAllCastleSlimesPercent = 6,      // 所有友方城堡增加n%个史莱姆
        ReduceAllCastleSlimesPercent = 7,      // 所有敌方城堡减少n%个史莱姆
        
        // 速度修改类（持续时间）
        IncreaseSlimeSpawnSpeed = 8,        // 增加生产速度n%，持续t秒
        DecreaseSlimeEnemySpawnSpeed = 9,   // 降低敌方生产速度n%，持续t秒
        IncreaseSlimeMoveSpeed = 10,         // 增加移动速度n%，持续t秒
        DecreaseSlimeEnemyMoveSpeed = 11,    // 降低敌方移动速度n%，持续t秒

        // 连锁效果
        ChainOccupationBonus = 12,      // t秒内占领新城堡后获得n个史莱姆

        // 特殊效果
        OccupyRandomCastle = 13,        // 随机占领一个未被占领的城堡
    }
}
