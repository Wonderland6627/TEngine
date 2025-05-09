using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace GameLogic
{
    public partial class World
    {
        public SlimeConfigReader<LevelConfig> levelReader { get; private set; }
        public SlimeConfigReader<UnitConfig> unitReader { get; private set; }
        public SlimeConfigReader<RewardConfig> rewardReader { get; private set; }

        private async UniTask LoadConfig()
        {
            levelReader = new();
            await levelReader.LoadConfig("levels");
            unitReader = new();
            await unitReader.LoadConfig("units");
            rewardReader = new();
            await rewardReader.LoadConfig("rewards");
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
