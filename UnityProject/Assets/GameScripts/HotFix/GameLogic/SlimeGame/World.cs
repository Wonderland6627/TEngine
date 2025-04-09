using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using GameBase;
using GameLogic;
using TEngine;
using WeChatWASM;

namespace GameLogic
{

    public class Road
    {
        public List<BaseCastle> points; //限制points的长度为2
    }

    public partial class World : BaseLogicSys<World>
    {
        public LevelReader levelReader { get; private set; }
        public UnitReader unitReader { get; private set; }

        public List<BaseCastle> castles = new List<BaseCastle>();

        public List<BaseRoad> roads = new List<BaseRoad>();

        public int playingLevelId = 0;

        public bool isPlaying = false;

        private int aiPlayerExeTimer = -1;

        public async void AsyncInit()
        {
            await LoadConfig();

            bool hasOpenID = !string.IsNullOrEmpty(gameData.UserInfo.openId);
            bool hasBasicInfo = !string.IsNullOrEmpty(gameData.UserInfo.nickName);
#if UNITY_EDITOR
            InitEditor();
#else
            InitWX((success) =>
            {
                if (success)
                {
                    Log.Info($"[World] Init WX SDK success, hasOpenID: {hasOpenID}, hasBasicInfo: {hasBasicInfo}");
                    if (!hasOpenID)
                    {
                        GetSetting();
                    }
                    if (!hasBasicInfo)
                    {
                        WXLogin();
                    }
                }
                else
                {
                    Log.Error("[World] Init WX SDK failed");
                }
            });
#endif
            GameModule.UI.ShowUIAsync<UIMenuWindow>();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            CheckGameOver();
            DebugUpdate();
        }

        public void StartGame(int levelId)
        {
            playingLevelId = levelId;
            aiPlayerExeTimer = GameModule.Timer.AddTimer(ExecuteAI, 5f, true);
            isPlaying = true;
        }

        public void EndGame()
        {
            isPlaying = false;
            foreach (var castle in castles)
            {
                castle.Destroy();
            }
            castles.Clear();

            foreach (var road in roads)
            {
                road.Destroy();
            }
            roads.Clear();

            GameModule.Timer.RemoveTimer(aiPlayerExeTimer);
            GameModule.UI.ShowUIAsync<UILevelWindow>();
        }

        public void PauseGame()
        {
            isPlaying = false;
            GameModule.Base.PauseGame();
        }

        public void ResumeGame()
        {
            GameModule.Base.ResumeGame();
            isPlaying = true;
        }

        private void CheckGameOver()
        {
            if (!isPlaying) return;
            if (castles.Count == 0) return;

            //检查所有的castle的被占领类型是否都相同
            List<UnitType> unitTypes = castles.Select(castle => castle.occupiedUnitType).Distinct().ToList();
            if (unitTypes.Count > 1) return;
            UnitType winUnitType = unitTypes[0];
            if (winUnitType == UnitType.Player)
            {
                if (gameData.UnlockedLevelId < playingLevelId)
                {
                    gameData.UnlockedLevelId = playingLevelId;
                }
            }

            isPlaying = false;
            GameEvent.Get<IActorLogicEvent>().OnGameOver(new GameOverParam() { winUnitType = winUnitType });
        }

        void DebugUpdate()
        {
            return;
            string log = "[World]";
            foreach (var road in roads)
            {
                log += $"[road:{road.gameObject.name}, units count: {road.Units.Count}]";
            }
            foreach (var castle in castles)
            {
                log += $"[castle:{castle.gameObject.name}]";
            }
            Log.Info(log);
        }
    }

    partial class World
    {
        private async UniTask LoadConfig()
        {
            levelReader = new LevelReader();
            await levelReader.LoadLevelConfig("levels");
            unitReader = new UnitReader();
            await unitReader.LoadUnitConfig("units");
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

    //Game Items
    partial class World
    {
        public void SetCastles(List<BaseCastle> castles)
        {
            this.castles = castles;
        }

        public void SetRoads(List<BaseRoad> roads)
        {
            this.roads = roads;
        }

        public async void CreateUnit(BaseCastle spawnCastle, UnitType unitType, BaseCastle targetCastle, Transform container)
        {
            var unitConfig = GetUnitConfig(unitType);
            if (unitConfig == null)
            {
                return;
            }
            var mainWindow = await GameModule.UI.GetUIAsyncAwait<UIMainWindow>();
            if (mainWindow == null)
            {
                return;
            }
            var unit = await mainWindow.CreateWidgetByPathAsync<BaseUnit>(container, unitConfig.unitPrefabPath);
            unit.gameObject.name = $"{unitType}_{unit.gameObject.GetInstanceID()}";
            unit.unitType = unitType;
            unit.moveDuration = unitConfig.moveDuration;
            unit.transform.position = spawnCastle.transform.position;
            unit.transform.localScale = Vector3.one;
            unit.SetTarget(targetCastle);

            RegisterUnit2Road(unit, spawnCastle, targetCastle);
        }

        public bool FindCastle(BaseCastle origin, Vector2 dir, out BaseCastle target)
        {
            target = null;
            List<BaseCastle> inAngleCastles = new List<BaseCastle>();
            foreach (var castle in castles)
            {
                if (castle == origin)
                {
                    continue;
                }

                Vector3 dirToTarget = castle.transform.position - origin.transform.position;
                float angle = Vector3.Angle(dir, dirToTarget.normalized);
                if (angle > 20f)
                {
                    continue;
                }

                inAngleCastles.Add(castle);
            }

            if (inAngleCastles.Count == 0)
            {
                return false;
            }

            target = inAngleCastles.OrderBy(castle => Vector3.Distance(castle.transform.position, origin.transform.position)).First();

            return IsOnSameRoad(origin, target);
        }

        public BaseRoad FindRoad(BaseCastle castle1, BaseCastle castle2)
        {
            return roads.FirstOrDefault(road => road.data.points.Contains(castle1) && road.data.points.Contains(castle2));
        }

        public bool RegisterUnit2Road(BaseUnit unit, BaseCastle spawnCastle, BaseCastle targetCastle)
        {
            BaseRoad road = FindRoad(spawnCastle, targetCastle);
            if (road == null)
            {
                Log.Error($"road is null between {spawnCastle.gameObject.name} and {targetCastle.gameObject.name}");
                return false;
            }
            road.RegisterUnit(unit);
            return true;
        }

        public bool IsOnSameRoad(BaseCastle castle1, BaseCastle castle2)
        {
            return roads.Any(road => road.data.points.Contains(castle1) && road.data.points.Contains(castle2));
        }
    }

    //AI
    partial class World
    {
        void ExecuteAI(object[] args)
        {
            if (castles == null || castles.Count == 0)
            {
                return;
            }
            List<BaseCastle> aiCastles = castles.FindAll(castle => castle.occupiedUnitType != UnitType.Player);
            for (int i = 0; i < aiCastles.Count; i++)
            {
                BaseCastle aiCastle = aiCastles[i];
                if (aiCastle == null)
                {
                    continue;
                }
                if (aiCastle.occupiedUnitCount < 5)
                {
                    continue;
                }

                //蚂蚁首先找最近的蜜蜂点位 找到路径 攻占路径上的点位
                List<BaseCastle> playerCastles = castles
                    .FindAll(castle => castle != aiCastle && castle.occupiedUnitType == UnitType.Player || !castle.isOccupied)
                    .OrderBy(castle => Vector2.Distance(castle.transform.position, aiCastle.transform.position))
                    .ToList();
                foreach (var playerCastle in playerCastles)
                {
                    if (!IsOnSameRoad(aiCastle, playerCastle))
                    {
                        //找交叉点
                        var allCrossCastles = castles.FindAll(castle =>
                            IsOnSameRoad(aiCastle, castle) &&
                            IsOnSameRoad(playerCastle, castle));
                        var nearestCrossCastle = allCrossCastles
                           .OrderBy(castle => Vector2.Distance(castle.transform.position, aiCastle.transform.position))
                           .FirstOrDefault();
                        if (nearestCrossCastle != null)
                        {
                            aiCastle.MoveTo(nearestCrossCastle);
                        }
                        continue;
                    }
                    aiCastle.MoveTo(playerCastle);
                }
            }
        }
    }
}