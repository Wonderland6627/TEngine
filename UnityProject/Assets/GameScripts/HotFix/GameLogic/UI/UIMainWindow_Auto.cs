using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TEngine;
using UnityEngine.Events;

namespace GameLogic
{
    [Window(UILayer.UI, fullScreen: true)]
    partial class UIMainWindow : UIWindow
    {
        private List<BaseCastle> m_Castles = new List<BaseCastle>();
        private List<BaseRoad> m_Roads = new List<BaseRoad>();
        
        protected override void OnCreate()
        {
            base.OnCreate();
            LoadLevel(World.Instance.playingLevelId);
            GameEvent.AddEventListener<GameOverParam>(IActorLogicEvent_Event.OnGameOver, OnGameOver);
            EventTriggerListener.Get(m_btnBack).OnClick = go =>
			{
				Close();
			};
            EventTriggerListener.Get(m_btnSettings).OnClick = go =>
			{
                World.Instance.PauseGame();
                UnityAction closeAction = () =>
                {
                    World.Instance.ResumeGame();
                };
                GameModule.UI.ShowUIAsync<UISettingsWindow>(closeAction);
			};
        }

        protected override void OnDestroy()
        {
            GameEvent.RemoveEventListener<GameOverParam>(IActorLogicEvent_Event.OnGameOver, OnGameOver);
            base.OnDestroy();
        }

        private void OnGameOver(GameOverParam  param)
        {
            Log.Info("GameOver" + param.winUnitType);
            GameModule.UI.ShowUIAsync<UIGameOverWindow>(param);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (Input.GetKeyDown(KeyCode.Escape)) {
                
                World.Instance.EndGame();
                Close();
            }
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

            await CreateCastles(level.castles);
            await CreateRoads(level.roads);
        }

        private async UniTask CreateCastles(List<LevelConfig.Castle> castles)
        {
            m_Castles.Clear();
            foreach (var config in castles)
            {
                var castle = await CreateWidgetByPathAsync<BaseCastle>(m_tfCastleContainer, "Assets/AssetRaw/UI/InGame/Castle.prefab");
                castle.ID = config.id;
                castle.gameObject.name = $"Castle_{config.id}";
                castle.castleType = (CastleType)config.castleType;
                castle.isOccupiedOnStart = config.occupiedOnStart;
                castle.occupiedUnitType = (UnitType)config.occupiedSlimeType;
                castle.occupiedUnitCount = config.occupiedUnitCount;
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

                var roadIns = await CreateWidgetByPathAsync<BaseRoad>(m_tfCastleContainer, "Assets/AssetRaw/UI/InGame/Road.prefab");
                roadIns.transform.SetParent(m_tfRoadContainer);
                roadIns.gameObject.name = $"Road_{config.startCastleId}_{config.endCastleId}";
                roadIns.data = roadData;
                m_Roads.Add(roadIns);

                var roadRect = roadIns.transform.RectTransform();
                Vector2 start = roadData.points[0].transform.RectTransform().anchoredPosition;
                Vector2 end = roadData.points[1].transform.RectTransform().anchoredPosition;
                
                float distance = Vector2.Distance(start, end);
                roadRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, distance);
                roadRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40);

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
    }

    partial class UIMainWindow
    {
		#region 脚本工具生成的代码
		private Transform m_tfRoadContainer;
		private Transform m_tfCastleContainer;
		private Transform m_tfUnitContainer;
		private Button m_btnBack;
		private Button m_btnSettings;
		protected override void ScriptGenerator()
		{
			m_tfRoadContainer = FindChild("bg/m_tfRoadContainer");
			m_tfCastleContainer = FindChild("bg/m_tfCastleContainer");
			m_tfUnitContainer = FindChild("bg/m_tfUnitContainer");
			m_btnBack = FindChildComponent<Button>("bg/m_btnBack");
			m_btnSettings = FindChildComponent<Button>("bg/m_btnSettings");
		}
		#endregion
    }
}