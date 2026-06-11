using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameConfig;
using TEngine;
using GameLogic.Network;

namespace GameLogic
{
    public partial class World
    {
        // ========== 列表型配置（使用 SlimeListConfigReader 获得通用查询能力） ==========
        public SlimeListConfigReader<LevelConfig> levelReader { get; private set; }
        
        public VisibleGameConfig gameConfig { get; private set; }

        private const string GAME_CONFIG_PATH = "Assets/AssetRaw/Configs/VisibleGameConfig.asset";
        private const string UNIT_PREFAB_PATH = "Assets/AssetRaw/Prefabs/UI/Units/Unit_Slime.prefab";
        private const float DEFAULT_UNIT_MOVE_DURATION = 7.5f;

        // ========== Luban 全局配置快捷访问 ==========
        public TbGlobalConfig GlobalConfig => ConfigSystem.Instance.Tables.TbGlobalConfig;
        
        private async UniTask LoadConfig()
        {
            levelReader = new SlimeListConfigReader<LevelConfig>();
            await levelReader.LoadLocalConfig("levels");
            
            await LoadGameConfig();
            
            TestLubanConfig();
        }

        /// <summary>
        /// 测试 Luban 配置表加载（验证 JSON 模式是否正常工作）。
        /// </summary>
        private void TestLubanConfig()
        {
            try
            {
                var tbItem = ConfigSystem.Instance.Tables.TbUnit;
                var item = tbItem.GetOrDefault(1001);
                if (item != null)
                    Log.Info($"[ConfigSystem] TbItem loaded OK, {item.ToString()}");
                else
                    Log.Error("[ConfigSystem] TbItem load FAILED: id=10000 not found");

                var globalConfig = ConfigSystem.Instance.Tables.TbGlobalConfig;
                if (globalConfig != null)
                    Log.Info($"[ConfigSystem] TbGlobalConfig loaded OK, {globalConfig.Data.ToString()}");
                else
                    Log.Error("[ConfigSystem] TbGlobalConfig load FAILED: not found");
            }
            catch (System.Exception e)
            {
                Log.Error($"[ConfigSystem] TbItem load EXCEPTION: {e.Message}");
            }
        }
        
        private async UniTask LoadGameConfig()
        {
            if (gameConfig != null)
            {
                return;
            }

            try
            {
                gameConfig = await GameModule.Resource.LoadAssetAsync<VisibleGameConfig>(GAME_CONFIG_PATH);

                if (gameConfig == null)
                {
                    Log.Error("[World] LoadGameConfig failed, config file not found");
                    return;
                }

                Log.Info("[World] LoadGameConfig success");
            }
            catch (ArgumentException e) when (e.Message.Contains("VisibleGameConfig"))
            {
                gameConfig = null;
                Log.Warning("[World] LoadGameConfig skipped due to duplicated type key. fallback curve will be used.");
            }
            catch (Exception e)
            {
                Log.Error($"[World] LoadGameConfig error: {e}");
            }
        }

        private async UniTask<bool> LoadRemoteLevelsConfig()
        {
            GameModule.UI.ShowLoading();
            try
            {
                var response = await NetManager.Instance.CallHttp<List<LevelsConfigData>>("getLevelsConfigV2");
                GameModule.UI.ShowLoading(false);
                
                if (response.IsSuccess && response.data != null && response.data.Count > 0)
                {
                    var firstData = response.data[0];
                    if (firstData?.configs?.levels != null)
                    {
                        var levelsArray = firstData.configs.levels.ToObject<LevelConfig[]>();
                        if (levelsArray != null && levelsArray.Length > 0)
                        {
                            Log.Info($"[World] LoadRemoteLevelsConfig success: {levelsArray.Length} levels");
                            levelReader.SetValue(levelsArray.ToList());
                            return true;
                        }
                    }
                    Log.Error("[World] LoadRemoteLevelsConfig: levels data is null or empty");
                    return false;
                }
                else
                {
                    Log.Error($"[World] LoadRemoteLevelsConfig failed: {response.ErrorMessage}");
                    return false;
                }
            }
            catch (Exception e)
            {
                GameModule.UI.ShowLoading(false);
                Log.Error($"[World] LoadRemoteLevelsConfig error: {e}");
                return false;
            }
        }

        // ========== Level 查询 ==========

        public List<LevelConfig> GetAllLevels() => levelReader.GetAll();

        public LevelConfig GetLevel(int levelId) => levelReader.FindById(l => l.levelId, levelId);

        public LevelConfig GetCurrentLevel() => GetLevel(playingLevelId);

        public LevelConfig.Config GetCurrentLevelConfig() => GetCurrentLevel()?.config;

        // ========== Unit 查询 ==========

        public string GetUnitPrefabPath() => UNIT_PREFAB_PATH;

        public float GetUnitMoveDuration(UnitType unitType)
        {
            var unit = GetUnitTableConfig(unitType);
            if (unit == null)
            {
                return DEFAULT_UNIT_MOVE_DURATION;
            }

            if (unit.MoveDuration > 0f)
            {
                return unit.MoveDuration;
            }

            Log.Warning($"[World] Invalid move duration in TbUnit, id: {unit.Id}, value: {unit.MoveDuration}");
            return DEFAULT_UNIT_MOVE_DURATION;
        }

        public string GetUnitImagePath(UnitType unitType)
        {
            var unit = GetUnitTableConfig(unitType);
            if (unit == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(unit.ImgPath))
            {
                Log.Warning($"[World] TbUnit img path empty, id: {unit.Id}");
                return string.Empty;
            }

            return unit.ImgPath;
        }

        private GameConfig.Unit GetUnitTableConfig(UnitType unitType)
        {
            if (!TryGetConfigUnitType(unitType, out var targetType))
            {
                Log.Warning($"[World] Unsupported unitType when querying unit config: {unitType}");
                return null;
            }

            var tbUnit = ConfigSystem.Instance.Tables.TbUnit;
            if (tbUnit == null || tbUnit.DataList == null || tbUnit.DataList.Count == 0)
            {
                Log.Warning("[World] TbUnit is empty when querying unit config");
                return null;
            }

            var unit = tbUnit.DataList.FirstOrDefault(item => item.UnitType == targetType);
            if (unit != null)
            {
                return unit;
            }

            Log.Warning($"[World] TbUnit record missing for type: {targetType}");
            return null;
        }

        private bool TryGetConfigUnitType(UnitType unitType, out GameConfig.unit.EUnitType configUnitType)
        {
            switch (unitType)
            {
                case UnitType.Player:
                    configUnitType = GameConfig.unit.EUnitType.BLUE;
                    return true;
                case UnitType.Enemy_1:
                    configUnitType = GameConfig.unit.EUnitType.RED;
                    return true;
                case UnitType.Enemy_2:
                    configUnitType = GameConfig.unit.EUnitType.GREEN;
                    return true;
                case UnitType.Enemy_3:
                    configUnitType = GameConfig.unit.EUnitType.YELLOW;
                    return true;
                case UnitType.Enemy_4:
                    configUnitType = GameConfig.unit.EUnitType.PINK;
                    return true;
                default:
                    configUnitType = default;
                    return false;
            }
        }

        // ========== 阵营参数查询（优先factions字典，fallback到旧字段） ==========

        public float GetFactionSpawnInterval(UnitType type)
        {
            var cfg = GetCurrentLevelConfig();
            if (cfg == null) return 1f;

            if (cfg.factions != null && cfg.factions.TryGetValue((int)type, out var fc))
                return fc.spawnInterval;

            return type switch
            {
                UnitType.Player  => cfg.playerSpawnInterval,
                UnitType.Enemy_1 => cfg.enemy_1_SpawnInterval,
                _ => cfg.enemy_1_SpawnInterval,
            };
        }

        public float GetFactionAttackInterval(UnitType type)
        {
            var cfg = GetCurrentLevelConfig();
            if (cfg == null) return 0.275f;

            if (cfg.factions != null && cfg.factions.TryGetValue((int)type, out var fc))
                return fc.attackInterval;

            return type switch
            {
                UnitType.Player  => cfg.playerAttackInterval,
                UnitType.Enemy_1 => cfg.enemy_1_AttackInterval,
                _ => cfg.enemy_1_AttackInterval,
            };
        }
    }
}
