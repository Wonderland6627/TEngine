using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections;

namespace GameLogic
{
    public class Road
    {
        public List<BaseCastle> points; //限制points的长度为2
    }

    [Window(UILayer.UI, fullScreen: true)]
    partial class UIGameWindow
    {
        private int curLevelID = 0;
        private List<BaseCastle> m_Castles = new List<BaseCastle>();
        private List<BaseRoad> m_Roads = new List<BaseRoad>();

        private bool m_HasKnownTutorial = false;
        private Tweener m_TutorialTweener;
        private Coroutine m_TutorialCoroutine;
        
        private AnimationCurve DisappearIntervalCurve
        {
            get
            {
                if (World.Instance.gameConfig == null || World.Instance.gameConfig.disappearIntervalCurve == null)
                {
                    return AnimationCurve.EaseInOut(0f, 0.15f, 1f, 0.02f);
                }
                
                return World.Instance.gameConfig.disappearIntervalCurve;
            }
        }
        
        protected override void OnCreate()
        {
            base.OnCreate();
            LoadLevel(World.Instance.playingLevelId);
            GameEvent.AddEventListener<GameOverParam>(SlimeEvent.OnGameOver, OnGameOver);
            if (curLevelID == 1)
            {
                GameEvent.AddEventListener(SlimeEvent.OnKnownTutorial, StopTutorialTips);
            }
            EventTriggerListener.Get(m_btnBack).OnClick = go =>
			{
				World.Instance.EndGame();
			};
            EventTriggerListener.Get(m_btnSettings).OnClick = go =>
			{
                GameModule.UI.ShowUIAsync<UISettingsWindow>();
			};
            EventTriggerListener.Get(m_btnRule).OnClick = go =>
            {
                GameModule.UI.ShowUIAsync<UIRuleTipsWindow>(false);
            };
            World.Instance.ReportGameStart();
        }
        
        protected override void OnTopWindowChanged(bool isTop)
        {
            base.OnTopWindowChanged(isTop);
            Log.Info("[UIMainWindow] OnTopWindowChanged, isTop: " + isTop);
            if (isTop)
            {
                World.Instance.ResumeGame();
            }
            else
            {
                World.Instance.PauseGame();
            }
        }

        protected override void OnDestroy()
        {
            StopTutorialTips();
            m_TutorialTweener?.Kill();
            m_TutorialTweener = null;
            if (curLevelID == 1)
            {
                GameEvent.RemoveEventListener(SlimeEvent.OnKnownTutorial, StopTutorialTips);
            }
            GameEvent.RemoveEventListener<GameOverParam>(SlimeEvent.OnGameOver, OnGameOver);
            base.OnDestroy();
        }

        private async void OnGameOver(GameOverParam param)
        {
            Log.Info("[UIMainWindow] trigger GameOver, winner: " + param.winUnitType);
            
            // 如果胜利，先执行连锁消除效果
            if (param.IsWin())
            {
                await PlayWinDisappearEffect();
            }
            
            // 显示游戏结束页
            GameModule.UI.ShowUIAsync<UIGameOverWindow>(param);
        }

        // 胜利时的连锁消除效果
        private async UniTask PlayWinDisappearEffect()
        {
            // 收集所有道路上的单位
            List<BaseUnit> allUnits = new List<BaseUnit>();
            foreach (var road in World.Instance.roads)
            {
                if (road != null && road.Units != null)
                {
                    foreach (var unit in road.Units)
                    {
                        if (unit != null && unit.transform != null)
                        {
                            allUnits.Add(unit);
                        }
                    }
                }
            }
            
            Log.Info($"[UIMainWindow] PlayWinDisappearEffect, units count: {allUnits.Count}");
            
            if (allUnits.Count == 0) return;
            
            // 逐个调用Disappear方法，间隔时间逐渐缩短
            for (int i = 0; i < allUnits.Count; i++)
            {
                var unit = allUnits[i];
                if (unit != null && unit.transform != null)
                {
                    unit.Disappear();
                    
                    // 计算当前单位的间隔时间
                    float interval = CalculateDisappearInterval(i, allUnits.Count);
                    if (i < allUnits.Count - 1) // 最后一个单位不需要等待
                    {
                        await UniTask.WaitForSeconds(interval);
                    }
                }
            }
        }
        
        // 计算消失间隔时间：使用AnimationCurve曲线配置
        private float CalculateDisappearInterval(int index, int totalCount)
        {
            AnimationCurve curve = DisappearIntervalCurve;
            
            if (totalCount <= 1) 
            {
                // 如果只有一个单位，使用曲线在0位置的值
                return curve.Evaluate(0f);
            }
            
            // 将索引映射到0-1的范围（0表示第一个单位，1表示最后一个单位）
            float progress = (float)index / (totalCount - 1);
            progress = Mathf.Clamp01(progress);
            
            // 通过曲线直接获取间隔时间（曲线Y轴值就是间隔时间）
            return curve.Evaluate(progress);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                World.Instance.EndGame();
            }
#endif
        }

        private async void LoadLevel(int levelID)
        {
            var level = World.Instance.GetLevel(levelID);
            if (level == null ||
                level.castles == null ||
                level.roads == null)
            {
                Log.Error("level invalid: " + levelID);
                return;
            }

            curLevelID = levelID;
            await CreateCastles(level.castles);
            await CreateRoads(level.roads);
        }

        private async UniTask CreateCastles(List<LevelConfig.Castle> castles)
        {
            m_Castles.Clear();
            foreach (var config in castles)
            {
                var castle = await CreateWidgetByPathAsync<BaseCastle>(m_tfCastleContainer, "Assets/AssetRaw/Prefabs/UI/UIGame/Castle.prefab");
                castle.ID = config.id;
                castle.gameObject.name = $"Castle_{config.id}";
                castle.castleType = (CastleType)config.castleType;
                castle.isOccupiedOnStart = config.occupiedOnStart;
                castle.occupiedUnitType = (UnitType)config.occupiedSlimeType;
                castle.occupiedUnitCount = config.occupiedUnitCount;
                castle.emptyCastleOccupyRequirement = config.emptyCastleOccupyRequirement;
                castle.unitContainer = m_tfUnitContainer;
                castle.transform.localPosition = new Vector2(config.position.x, config.position.y);
                castle.transform.localScale = Vector3.one;
                castle.Init();
                m_Castles.Add(castle);
            }
            World.Instance.SetCastles(m_Castles);
        }

        private async UniTask CreateRoads(List<LevelConfig.Road> roads)
        {
            m_Roads.Clear();
            foreach (var config in roads)
            {
                if (config.startCastleId >= m_Castles.Count ||
                    config.endCastleId >= m_Castles.Count)
                {
                    Log.Error("road invalid: " + config.startCastleId + " " + config.endCastleId);
                    continue;
                }
                
                var startCastle = FindCastleByID(config.startCastleId);
                var endCastle = FindCastleByID(config.endCastleId);
                if (startCastle == null || endCastle == null)
                {
                    Log.Error("road invalid: " + config.startCastleId + " " + config.endCastleId);
                    continue;
                }
                var roadData = new Road
                {
                    points = new List<BaseCastle>
                    {
                        startCastle,
                        endCastle
                    }
                };

                var roadIns = await CreateWidgetByPathAsync<BaseRoad>(m_tfCastleContainer, "Assets/AssetRaw/Prefabs/UI/UIGame/Road.prefab");
                roadIns.transform.SetParent(m_tfRoadContainer);
                roadIns.gameObject.name = $"Road_{config.startCastleId}_{config.endCastleId}";
                roadIns.data = roadData;
                m_Roads.Add(roadIns);

                var roadRect = roadIns.transform.RectTransform();
                Vector2 start = roadData.points[0].transform.RectTransform().anchoredPosition;
                Vector2 end = roadData.points[1].transform.RectTransform().anchoredPosition;
                
                float distance = Vector2.Distance(start, end);
                roadRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, distance);
                roadRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, roadRect.rect.height);

                float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
                roadRect.rotation = Quaternion.Euler(0, 0, angle);
                roadRect.localScale = Vector3.one;

                Vector2 center = (start + end) / 2;
                roadRect.localPosition = center;
            }
            World.Instance.SetRoads(m_Roads);
        }

        private BaseCastle FindCastleByID(int castleID)
        {
            return m_Castles.Find(castle => castle.ID == castleID);
        }

        private UITutorialTips m_TutorialTips;
        public void ShowTutorialTips()
        {
            if (curLevelID != 1) return;
            if (m_Castles.Count == 0) return;
            if (m_Castles.Count < 2) return;

            m_HasKnownTutorial = false;
            m_TutorialTips = CreateWidgetByPrefab<UITutorialTips>(m_itemTutorialTips, transform);
            m_TutorialTips.gameObject.SetActive(true);
            m_TutorialTips.DoScale();
            DoTutorial(m_Castles[0].rectTransform, m_Castles[1].rectTransform);
        }

        public void StopTutorialTips()
        {
            m_HasKnownTutorial = true;
            m_TutorialTips?.StopScale();
            m_TutorialTips?.gameObject.SetActive(false);
            Utility.Unity.StopCoroutine(m_TutorialCoroutine);
        }

        private void DoTutorial(RectTransform start, RectTransform target)
        {
            m_TutorialCoroutine = Utility.Unity.StartCoroutine(DoTutorialCoroutine(start, target));
        }

        private IEnumerator DoTutorialCoroutine(RectTransform start, RectTransform target)
        {
            var seconds = new WaitForSeconds(0.5f);
            while (!m_HasKnownTutorial)
            {
                if (m_TutorialTips == null) yield break;
                m_TutorialTips.transform.localPosition = start.localPosition;
                yield return seconds;
                m_TutorialTweener = m_TutorialTips.transform.DOLocalMove(target.localPosition, 1.25f)
                    .SetEase(Ease.Linear);
                yield return m_TutorialTweener.WaitForCompletion();
                yield return seconds;
            }
        }
    }
}

