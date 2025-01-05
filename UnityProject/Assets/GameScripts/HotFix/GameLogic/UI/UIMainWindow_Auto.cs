using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
    [Window(UILayer.UI)]
    partial class UIMainWindow : UIWindow
    {
        public Transform SlimeContainer => m_tfSlimeContainer;
        
        private List<BaseCastle> m_Castles = new List<BaseCastle>();
        private List<Road> m_Roads = new List<Road>();
        
        protected override void OnCreate()
        {
            base.OnCreate();
            LoadLevel(1);
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
                var castleIns = await GameModule.Resource.LoadGameObjectAsync("Assets/AssetRaw/UI/InGame/Castle.prefab");
                castleIns.transform.SetParent(m_tfCastleContainer);
                castleIns.name = $"Castle_{config.id}";
                var castle = castleIns.GetComponent<BaseCastle>();
                castle.castleType = (CastleType)config.castleType;
                castle.isOccupiedOnStart = config.occupiedOnStart;
                castle.occupiedSlimeType = (SlimeType)config.occupiedSlimeType;
                castle.occupiedUnitCount = config.occupiedUnitCount;
                
                castle.transform.localPosition = new Vector2(config.position.x, config.position.y);
                castle.transform.localScale = Vector3.one;
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
                
                var startCastle = m_Castles[config.startCastleId];
                var endCastle = m_Castles[config.endCastleId];
                var road = new Road();
                road.points = new List<RectTransform>();
                road.points.Add(startCastle.transform.RectTransform());
                road.points.Add(endCastle.transform.RectTransform());
                
                var roadIns = await GameModule.Resource.LoadGameObjectAsync("Assets/AssetRaw/UI/InGame/Road.prefab");
                roadIns.transform.SetParent(m_tfRoadContainer);
                roadIns.name = $"Road_{config.startCastleId}_{config.endCastleId}";
                
                var roadRect = roadIns.transform.RectTransform();
                Vector2 start = road.points[0].anchoredPosition;
                Vector2 end = road.points[1].anchoredPosition;
                
                float distance = Vector2.Distance(start, end);
                roadRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, distance);
                roadRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40);

                float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
                roadRect.rotation = Quaternion.Euler(0, 0, angle);
                roadRect.localScale = Vector3.one;

                Vector2 center = (start + end) / 2;
                roadRect.localPosition = center;
                
                m_Roads.Add(road);
            }
            World.Instance.SetRoads(m_Roads);
        }
    }

    partial class UIMainWindow
    {
        #region 脚本工具生成的代码
        private Transform m_tfRoadContainer;
        private Transform m_tfCastleContainer;
        private Transform m_tfSlimeContainer;
        protected override void ScriptGenerator()
        {
            m_tfRoadContainer = FindChild("bg/m_tfRoadContainer");
            m_tfCastleContainer = FindChild("bg/m_tfCastleContainer");
            m_tfSlimeContainer = FindChild("bg/m_tfSlimeContainer");
        }
        #endregion
    }
}