using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TEngine;
using WeChatWASM;

namespace GameLogic
{
    public partial class World : BaseLogicSys<World>
    {
        public List<BaseCastle> castles = new List<BaseCastle>();

        public List<BaseRoad> roads = new List<BaseRoad>();

        public int playingLevelId = 0;

        public bool isPlaying = false;

        private int aiPlayerExeTimer = -1;
        
        // 游戏状态变量（保留旧字段用于兼容外部引用）
        public float playerSlimeMoveSpeedCoe
        {
            get => GetFactionMoveSpeedCoe(UnitType.Player);
            set => SetFactionMoveSpeedCoe(UnitType.Player, value);
        }
        public float enemySlimeMoveSpeedCoe
        {
            get => GetFactionMoveSpeedCoe(UnitType.Enemy_1);
            set => SetFactionMoveSpeedCoe(UnitType.Enemy_1, value);
        }
        public float playerSlimeSpawnSpeedCoe
        {
            get => GetFactionSpawnSpeedCoe(UnitType.Player);
            set => SetFactionSpawnSpeedCoe(UnitType.Player, value);
        }
        public float enemySlimeSpawnSpeedCoe
        {
            get => GetFactionSpawnSpeedCoe(UnitType.Enemy_1);
            set => SetFactionSpawnSpeedCoe(UnitType.Enemy_1, value);
        }

        private Dictionary<UnitType, float> _moveSpeedCoe = new();
        private Dictionary<UnitType, float> _spawnSpeedCoe = new();

        public float GetFactionMoveSpeedCoe(UnitType type)
            => _moveSpeedCoe.TryGetValue(type, out var v) ? v : 1f;

        public void SetFactionMoveSpeedCoe(UnitType type, float value)
            => _moveSpeedCoe[type] = value;

        public float GetFactionSpawnSpeedCoe(UnitType type)
            => _spawnSpeedCoe.TryGetValue(type, out var v) ? v : 1f;

        public void SetFactionSpawnSpeedCoe(UnitType type, float value)
            => _spawnSpeedCoe[type] = value;

        private void ResetFactionSpeedCoe()
        {
            _moveSpeedCoe.Clear();
            _spawnSpeedCoe.Clear();
        }

        public async UniTaskVoid AsyncInit()
        {
            GameData.GuideFinish = false;
            
            // 统一登录流程（自动根据平台选择）
            bool loginSuccess = await Login();
            if (!loginSuccess)
            {
                Log.Error("[World] Login failed");
                return;
            }
            
            await LoadConfig();
            FetchUserGameInfo((success) => 
            {
                if (!success) return;
                
                // 2021年后新版本必须通过用户主动触发获取用户信息
                // 这里不在启动时自动弹授权，改为：第一关胜利后若未获取到昵称再触发（见 UIGameOverWindow）
            });
            
#if !UNITY_EDITOR
            InitAds();
#endif
            
            GameModule.UI.ShowUIAsync<UIMainWindow>();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            CheckGameOver();
            DebugUpdate();
        }

        public void StartGame(int levelId)
        {
            StartGameAsync(levelId).Forget();
        }

        private async UniTaskVoid StartGameAsync(int levelId)
        {
            // 检查体力值
            bool hasEnergy = await TryConsumeEnergy();
            if (!hasEnergy)
            {
                Log.Warning($"[World] StartGame failed: energy not enough");
                return;
            }

            playingLevelId = levelId;
            aiPlayerExeTimer = GameModule.Timer.AddTimer(ExecuteAI, 5f, true);
            isPlaying = true;
            GameModule.UI.ShowUIAsync<UIGameWindow>();
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

            ResetFactionSpeedCoe();
            ClearAdsState();
            
            GameModule.Timer.RemoveTimer(aiPlayerExeTimer);
            GameModule.UI.CloseUI<UIGameWindow>();
            // GameModule.UI.ShowUIAsync<UILevelWindow>();

            Log.Info($"[World] EndGame");
        }

        public void PauseGame()
        {
            isPlaying = false;
            BaseObject.PauseAll();

            Log.Info($"[World] PauseGame");
        }

        public void ResumeGame()
        {
            BaseObject.ResumeAll();
            isPlaying = true;

            Log.Info($"[World] ResumeGame");
        }

        private void CheckGameOver()
        {
            if (!isPlaying) return;
            if (castles.Count == 0) return;

            // 玩家没有任何已占领的城堡 -> 立即判负
            bool playerHasCastle = castles.Any(c => c.isOccupied && c.occupiedUnitType == UnitType.Player);
            if (!playerHasCastle)
            {
                // 找到拥有最多城堡的AI阵营作为赢家
                var aliveFactions = castles
                    .Where(c => c.isOccupied)
                    .Select(c => c.occupiedUnitType)
                    .Distinct()
                    .ToList();
                UnitType winner = aliveFactions.Count > 0 ? aliveFactions[0] : UnitType.Enemy_1;
                PauseGame();
                GameEvent.Send(SlimeEvent.OnGameOver, new GameOverParam() { winUnitType = winner });
                return;
            }

            // 统计存活阵营数（拥有至少一个已占领城堡的阵营）
            var survivingFactions = castles
                .Where(c => c.isOccupied)
                .Select(c => c.occupiedUnitType)
                .Distinct()
                .ToList();

            // 还有未占领的城堡 且 存活阵营 > 1 -> 继续
            bool hasFreeCastle = castles.Any(c => !c.isOccupied);
            if (survivingFactions.Count > 1 || hasFreeCastle) return;

            // 只剩1个阵营，且没有空城堡
            UnitType winUnitType = survivingFactions[0];
            PauseGame();
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

        public void ReportGameStart()
        {
            WX.ReportGameStart();
        }

        public void ReportScene(ReportSceneOption option)
        {
            WX.ReportScene(option);
        }

        private int toggleDebugWindowCount = 0;
        public void ToggleDebugWindow()
        {
            toggleDebugWindowCount++;
            if (toggleDebugWindowCount < 5) return;
            toggleDebugWindowCount = 0;
            GameModule.Debugger.ActiveWindow = !GameModule.Debugger.ActiveWindow;
            // GameModule.UI.ShowUIAsync<UIDebugWindow>();
        }

        private int toggleShowAdsCount = 0;
        public void ToggleShowAds()
        {
            toggleShowAdsCount++;
            if (toggleShowAdsCount < 5) return;
            toggleShowAdsCount = 0;
            ShowRewardedVideoAd();
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
            var mainWindow = await GameModule.UI.GetUIAsyncAwait<UIGameWindow>();
            if (mainWindow == null)
            {
                return;
            }
            var unit = await mainWindow.CreateWidgetByPathAsync<BaseUnit>(container, GetUnitPrefabPath());
            if (unit == null)
            {
                return;
            }

            unit.gameObject.name = $"{unitType}_{unit.gameObject.GetInstanceID()}";
            unit.unitType = unitType;
            unit.moveDuration = GetUnitMoveDuration(unitType);
            unit.SetUnitImagePath(GetUnitImagePath(unitType));
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
            if (playingLevelId == 1 && !GameData.GuideFinish) return;
            if (castles == null || castles.Count == 0) return;

            // 收集当前存活的非玩家阵营
            var aiFactions = castles
                .Where(c => c.isOccupied && !FactionUtil.IsPlayer(c.occupiedUnitType))
                .Select(c => c.occupiedUnitType)
                .Distinct()
                .ToList();

            foreach (var faction in aiFactions)
            {
                ExecuteAIForFaction(faction);
            }
        }

        private void ExecuteAIForFaction(UnitType faction)
        {
            var myCastles = castles.FindAll(c => c.isOccupied && c.occupiedUnitType == faction);
            int myCastleCount = myCastles.Count;
            int totalOccupied = castles.Count(c => c.isOccupied);
            bool isAdvantage = myCastleCount * 2 > totalOccupied;

            foreach (var aiCastle in myCastles)
            {
                if (aiCastle == null) continue;

                if (isAdvantage && GetRandomFlag(25f)) continue;

                int attackStartCount = GetRandomValue(5, 15);
                if (aiCastle.occupiedUnitCount < attackStartCount) continue;

                // 寻找目标：非己方的城堡（包括其他AI和空城堡），按距离排序
                var targets = castles
                    .Where(c => c != aiCastle && (c.occupiedUnitType != faction || !c.isOccupied))
                    .OrderBy(c => Vector2.Distance(c.transform.position, aiCastle.transform.position))
                    .ToList();

                foreach (var target in targets)
                {
                    if (!IsOnSameRoad(aiCastle, target))
                    {
                        var crossCastles = castles.FindAll(c =>
                            IsOnSameRoad(aiCastle, c) && IsOnSameRoad(target, c));
                        var nearest = crossCastles
                            .OrderBy(c => Vector2.Distance(c.transform.position, aiCastle.transform.position))
                            .FirstOrDefault();
                        if (nearest != null)
                        {
                            aiCastle.MoveTo(nearest);
                        }
                        continue;
                    }
                    aiCastle.MoveTo(target);
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