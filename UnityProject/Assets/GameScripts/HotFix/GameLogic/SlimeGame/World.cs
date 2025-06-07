using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
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
        public List<BaseCastle> castles = new List<BaseCastle>();

        public List<BaseRoad> roads = new List<BaseRoad>();

        public int playingLevelId = 0;

        public bool isPlaying = false;

        private int aiPlayerExeTimer = -1;

        public async void AsyncInit()
        {
            GameData.GuideFinish = false;
            await LoadConfig();

            bool hasOpenID = !string.IsNullOrEmpty(GameData.UserInfo.openId);
            bool hasBasicInfo = !string.IsNullOrEmpty(GameData.UserInfo.nickName);
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
                        WXLogin();
                    }
                    if (!hasBasicInfo)
                    {
                        GetSetting();
                    }
                    GetUserGameInfo();
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
            GameModule.UI.ShowUIAsync<UIMainWindow>();
            TryStartGameGuide();

            Log.Info($"[World] StartGame, levelId: {levelId}");
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

            occupiedCastleTimes = 0;
            ResetRewardAction();
            
            GameModule.Timer.RemoveTimer(aiPlayerExeTimer);
            GameModule.UI.CloseUI<UIMainWindow>();
            GameModule.UI.ShowUIAsync<UILevelWindow>();

            Log.Info($"[World] EndGame");
        }

        public void PauseGame()
        {
            isPlaying = false;
            GameModule.Base.PauseGame();

            Log.Info($"[World] PauseGame");
        }

        public void ResumeGame()
        {
            GameModule.Base.ResumeGame();
            isPlaying = true;

            Log.Info($"[World] ResumeGame");
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
                if (GameData.ProgressLevelID < playingLevelId)
                {
                    GameData.SetProgressLevelID(playingLevelId, true);
                }
            }

            isPlaying = false;
            GameEvent.Send(SlimeEvent.OnGameOver, new GameOverParam() { winUnitType = winUnitType });
        }

        public void Vibrate(bool isShort = true)
        {
            if (!GameData.EnableVibration) return;
            if (isShort)
            {
                WX.VibrateShort(new VibrateShortOption() 
                { 
                    success = _ => { },
                    fail = _ => { },
                    complete = _ => { Log.Info("[World] VibrateShort success"); },
                });
            }
            else
            {
                WX.VibrateLong(new VibrateLongOption() 
                { 
                    success = _ => { },
                    fail = _ => { },
                    complete = _ => { Log.Info("[World] VibrateLong success"); },
                });
            }
        }

        private int toggleCount = 0;
        public void ToggleDebugWindow()
        {
            toggleCount++;
            if (toggleCount < 5) return;
            toggleCount = 0;
            GameModule.Debugger.ActiveWindow = !GameModule.Debugger.ActiveWindow;
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
            if (playingLevelId == 1 && !GameData.GuideFinish) return; //第一关且新手引导未完成 不执行AI
            if (castles == null || castles.Count == 0) return;

            List<BaseCastle> aiCastles = castles.FindAll(castle => castle.occupiedUnitType != UnitType.Player);
            int aiCounts = aiCastles.Count;
            int playerCounts = castles.Count - aiCounts;
            bool isAIMore = aiCounts > playerCounts;
            for (int i = 0; i < aiCastles.Count; i++)
            {
                if (isAIMore)
                {
                    bool ignoreMove = GetRandomFlag(25f); //25%的概率忽略本次攻占
                    if (ignoreMove)
                    {
                        continue;
                    }
                }
                BaseCastle aiCastle = aiCastles[i];
                if (aiCastle == null)
                {
                    continue;
                }
                int attackStartCount = GetRandomValue(5, 15);
                if (aiCastle.occupiedUnitCount < attackStartCount)
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

        private bool GetRandomFlag(float chance = 50f)
        {
            return Random.Range(0, 100f) < chance;
        }

        private int GetRandomValue(int min, int max)
        {
            return Random.Range(min, max);
        }
    }
}