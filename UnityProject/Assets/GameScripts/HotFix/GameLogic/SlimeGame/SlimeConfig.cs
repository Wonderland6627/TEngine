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

        public async UniTask LoadConfig(string jsonPath)
        {
            var res = await GameModule.Resource.LoadAssetAsync<TextAsset>(jsonPath);
            var json = res.text;
            configs = Utility.Json.ToObject<List<T>>(json);
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
        ReduceEnemyCount = 0,          // 削弱敌军：随机一个敌方城堡中敌军数量-5
        AddFriendlyCount = 1,          // 增援友军：随机一个己方城堡增加3个士兵
        SpeedUpProduction = 2,         // 加速生产：随机一个己方城堡，在10秒内士兵生产速度提升50%
        EnhanceAttackSpeed = 3,        // 士气提升：所有己方城堡的士兵攻击间隔减少20%，持续15秒
        OccupyRandomCastle = 4,        // 奇袭战术：随机占领一个未被占领的城堡
        InstantArmyBoost = 5,          // 军事援助：所有己方城堡立即生成5个士兵
        EnemyArmyDisperse = 6,         // 敌军溃散：所有敌方城堡损失30%的士兵
        ChainOccupation = 7,           // 连锁进攻：15秒内，己方占领新城堡后立即获得5个士兵
    }
}
