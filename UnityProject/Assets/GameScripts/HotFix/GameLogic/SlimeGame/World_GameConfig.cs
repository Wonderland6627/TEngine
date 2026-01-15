using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TEngine;
using GameLogic.Network;

namespace GameLogic
{
    public partial class World
    {
        public SlimeConfigReader<LevelConfig> levelReader { get; private set; }
        public SlimeConfigReader<UnitConfig> unitReader { get; private set; }
        
        public GameConfig gameConfig { get; private set; }

        // 游戏配置资源路径
        private const string GAME_CONFIG_PATH = "Assets/AssetRaw/Configs/GameConfig.asset";
        
        private async UniTask LoadConfig()
        {
            levelReader = new();
            await levelReader.LoadLocalConfig("levels");
            unitReader = new();
            await unitReader.LoadLocalConfig("units");
            
            await LoadGameConfig();
        }
        
        // 加载游戏配置
        private async UniTask LoadGameConfig()
        {
            try
            {
                gameConfig = await GameModule.Resource.LoadAssetAsync<GameConfig>(GAME_CONFIG_PATH);
                if (gameConfig != null)
                {
                    Log.Info("[World] LoadGameConfig success");
                }
                else
                {
                    Log.Warning("[World] LoadGameConfig failed, config file not found");
                }
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
                var response = await NetManager.CallHttp<List<LevelsConfigData>>("getLevelsConfigV2");
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
                            levelReader.SetConfigs(levelsArray.ToList());
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

        public List<LevelConfig> GetAllLevels()
        {
            return levelReader.configs;
        }

        public LevelConfig GetLevel(int levelId)
        {
            if (levelReader.configs == null ||
                levelReader.configs.Count == 0)
            {
                return null;
            }

            return levelReader.configs.Find(level => level.levelId == levelId);
        }

        public LevelConfig GetCurrentLevel()
        {
            return GetLevel(playingLevelId);
        }

        public LevelConfig.Config GetCurrentLevelConfig()
        {
            var currentLevel = GetLevel(playingLevelId);
            if (currentLevel == null)
            {
                return null;
            }
            return currentLevel.config;
        }

        public UnitConfig GetUnitConfig(UnitType unitType)
        {
            if (unitReader.configs == null ||
                unitReader.configs.Count == 0)
            {
                return null;
            }

            return unitReader.configs.Find(unit => unit.unitType == (int)unitType);
        }
    }
}
