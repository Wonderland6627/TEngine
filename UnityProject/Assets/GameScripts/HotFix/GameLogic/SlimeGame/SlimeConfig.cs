using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 配置读取器（泛型参数T直接表示实际存储的类型）
    /// </summary>
    public class SlimeConfigReader<T>
    {
        public T Value { get; protected set; }

        public async UniTask LoadLocalConfig(string jsonPath)
        {
            var res = await GameModule.Resource.LoadAssetAsync<TextAsset>(jsonPath);
            if (res == null)
            {
                Log.Error($"[SlimeConfigReader] Config file not found: {jsonPath}");
                return;
            }

            var json = res.text;
            Value = Utility.Json.ToObject<T>(json);
        }

        public void SetValue(T value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// 列表配置读取器，提供通用的按条件查询能力
    /// 用法：SlimeListConfigReader&lt;LevelConfig&gt; 等价于 SlimeConfigReader&lt;List&lt;LevelConfig&gt;&gt; + 查询方法
    /// </summary>
    public class SlimeListConfigReader<TItem> : SlimeConfigReader<List<TItem>>
    {
        public List<TItem> GetAll()
        {
            return Value;
        }

        public TItem Find(Func<TItem, bool> predicate)
        {
            if (Value == null || Value.Count == 0) return default;
            return Value.Find(item => predicate(item));
        }

        public List<TItem> FindAll(Func<TItem, bool> predicate)
        {
            if (Value == null || Value.Count == 0) return new List<TItem>();
            return Value.FindAll(item => predicate(item));
        }

        public TItem FindById<TKey>(Func<TItem, TKey> keySelector, TKey id)
        {
            if (Value == null || Value.Count == 0) return default;
            return Value.Find(item => EqualityComparer<TKey>.Default.Equals(keySelector(item), id));
        }

        public int Count => Value?.Count ?? 0;
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
            public int emptyCastleOccupyRequirement { get; set; } = -10; // 空城堡需要多少个单位数量才能被占领，默认为-10
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
            // 旧字段，向后兼容关卡1-2
            public float playerSpawnInterval { get; set; }
            public float playerAttackInterval { get; set; }
            public float enemy_1_SpawnInterval { get; set; }
            public float enemy_1_AttackInterval { get; set; }

            // 新增：按阵营配置（key = UnitType int值），优先使用
            public Dictionary<int, FactionConfig> factions { get; set; }
        }

        public class FactionConfig
        {
            public float spawnInterval { get; set; } = 1f;
            public float attackInterval { get; set; } = 0.275f;
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

    //===================================================================================

    //===================================================================================

    public enum LevelChestState
    {
        Locked,     // 未达到关卡要求
        Claimable,  // 可领取（已通关且未领取）
        Claimed     // 已领取
    }

    public class LevelChestDisplayInfo
    {
        public GameConfig.LevelChest config;
        public LevelChestState state;
    }

    //===================================================================================

    public class RewardItem
    {
        public ResourceType resourceType { get; set; }
        public int amount { get; set; }
        public string source { get; set; }
    }

    /// <summary>
    /// 关卡通关奖励计算结果
    /// </summary>
    public class LevelRewardResult
    {
        public int levelId;
        public bool isFirstClear;
        public List<RewardItem> baseRewards = new();
        public List<RewardItem> firstClearRewards = new();
        public List<RewardItem> adBonusRewards = new();

        /// <summary>
        /// 获取最终可领取的奖励列表
        /// </summary>
        public List<RewardItem> GetClaimableRewards(bool watchedAd)
        {
            var result = new List<RewardItem>(baseRewards);
            if (isFirstClear)
            {
                result.AddRange(firstClearRewards);
            }
            if (watchedAd)
            {
                result.AddRange(adBonusRewards);
            }
            return result;
        }

        /// <summary>
        /// 按 ResourceType 汇总奖励数量（用于UI展示）
        /// </summary>
        public Dictionary<ResourceType, int> SumByType(bool watchedAd)
        {
            var dict = new Dictionary<ResourceType, int>();
            foreach (var item in GetClaimableRewards(watchedAd))
            {
                if (dict.ContainsKey(item.resourceType))
                    dict[item.resourceType] += item.amount;
                else
                    dict[item.resourceType] = item.amount;
            }
            return dict;
        }
    }

}
