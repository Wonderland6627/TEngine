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
        // ========== 列表型配置（使用 SlimeListConfigReader 获得通用查询能力） ==========
        public SlimeListConfigReader<LevelConfig> levelReader { get; private set; }
        public SlimeListConfigReader<UnitConfig> unitReader { get; private set; }

        // ========== 单对象型配置 ==========
        public SlimeConfigReader<EnergyConfig> energyReader { get; private set; }
        public SlimeConfigReader<LevelRewardFormulaConfig> levelRewardReader { get; private set; }
        
        public GameConfig gameConfig { get; private set; }

        private const string GAME_CONFIG_PATH = "Assets/AssetRaw/Configs/GameConfig.asset";
        
        private async UniTask LoadConfig()
        {
            levelReader = new SlimeListConfigReader<LevelConfig>();
            await levelReader.LoadLocalConfig("levels");

            unitReader = new SlimeListConfigReader<UnitConfig>();
            await unitReader.LoadLocalConfig("units");

            energyReader = new SlimeConfigReader<EnergyConfig>();
            await energyReader.LoadLocalConfig("energy_config");

            levelRewardReader = new SlimeConfigReader<LevelRewardFormulaConfig>();
            await levelRewardReader.LoadLocalConfig("level_reward_config");
            
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
            }
            catch (System.Exception e)
            {
                Log.Error($"[ConfigSystem] TbItem load EXCEPTION: {e.Message}");
            }
        }
        
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

        public UnitConfig GetUnitConfig(UnitType unitType) => unitReader.Find(u => u.unitType == (int)unitType);

        // ========== 单对象配置直接获取 ==========

        public EnergyConfig GetEnergyConfig() => energyReader.Value;

        public LevelRewardFormulaConfig GetLevelRewardConfig() => levelRewardReader.Value;
    }
}
