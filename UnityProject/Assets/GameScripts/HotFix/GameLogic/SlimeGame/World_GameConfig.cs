using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TEngine;
using WeChatWASM;

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
            await levelReader.LoadLocalConfig("levels");
            unitReader = new();
            await unitReader.LoadLocalConfig("units");
            rewardReader = new();
            await rewardReader.LoadLocalConfig("rewards");
        }

        private async UniTask<bool> LoadRemoteLevelsConfig()
        {
            GameModule.UI.ShowLoading();
            try
            {
                var tcs = new UniTaskCompletionSource<CallFunctionResult>();
                WX.cloud.CallFunction(new CallFunctionParam()
                {
                    name = "getLevelsConfig",
                    success = (res) =>
                    {
                        Log.Info("[World] call cloud function getLevelsConfig success: " + res.ToJson().ToString());
                        tcs.TrySetResult(res);
                    },
                    fail = (err) =>
                    {
                        Log.Error("[World] call cloud function getLevelsConfig failed: " + err.ToJson().ToString());
                        tcs.TrySetException(new Exception(err.ToJson().ToString()));
                    }
                });
                var res = await tcs.Task;
                GameModule.UI.ShowLoading(false);
                Log.Info("[World] call cloud function getLevelsConfig result");
                JObject resultJson = JObject.Parse(res.result);
                if (!resultJson.TryGetValue("data", out var dataJson) || dataJson == null)
                {
                    Log.Error("[World] call cloud function getLevelsConfig result data is null");
                    return false;
                }
                var firstData = dataJson.First;
                if (firstData == null || firstData["configs"] == null)
                {
                    Log.Error("[World] call cloud function getLevelsConfig result first data is null");
                    return false;
                }

                var levelsJson = firstData["configs"].ToString();
                var levelsArray = levelsJson.ToObject<LevelConfig[]>();
                if (levelsArray == null || levelsArray.Length == 0)
                {
                    Log.Error("[World] call cloud function getLevelsConfig result levels is null");
                    return false;
                }
                Log.Info("[World] call cloud function getLevelsConfig result data: " + levelsArray.Length);
                levelReader.SetConfigs(levelsArray.ToList());
                return true;
            }
            catch (Exception e)
            {
                Log.Error("[World] GetUserRankList error: " + e.ToString());
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
