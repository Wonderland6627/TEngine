using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using GameLogic;
using TEngine;

[System.Serializable]
public class Road
{
    public List<RectTransform> points; //限制points的长度为2
}

public class LevelReader
{
    public class LevelConfig
    {
        public class Level
        {
            public int levelId { get; set; }
            public List<Castle> castles { get; set; }
            public List<Road> roads { get; set; }
        }
        
        public class Castle
        {
            public int id { get; set; }
            public int type { get; set; }
            public bool occupiedOnStart { get; set; }
            public int occupiedType { get; set; }
            public int occupiedUnitCount { get; set; }
            public Position position { get; set; }
            
            public class Position
            {
                public int x { get; set; }
                public int y { get; set; }
            }
        }

        public class Road
        {
            public int startCastleId { get; set; }
            public int endCastleId { get; set; }
        }
        
        public List<Level> levels { get; set; }
    }
    
    public LevelConfig config { get; private set; }

    public async UniTask LoadLevelConfig(string jsonPath)
    {
        var res = await GameModule.Resource.LoadAssetAsync<TextAsset>(jsonPath);
        var json = res.text;
        config = Utility.Json.ToObject<LevelConfig>(json);
    }
}

public class World : SingletonBehaviour<World>
{
    public List<BaseCastle> castles = new List<BaseCastle>();
    public List<Road> roads = new List<Road>();

    public async void Init()
    {
        // castles.AddRange(GameObject.FindObjectsOfType<BaseCastle>());
        await LoadConfig();
        GameModule.UI.ShowUIAsync<UIMainWindow>();
    }

    private async UniTask LoadConfig()
    {
        var reader = new LevelReader(); 
        await reader.LoadLevelConfig("Assets/AssetRaw/Configs/jsons/levels.json");
        
        Log.Info($"{reader.config.levels.Count}");
    }
    
    public async void CreateUnit(BaseCastle spawnCastle, SlimeType slimeType, BaseCastle targetCastle)
    {
        var res = await GameModule.Resource.LoadAssetAsync<BaseUnit>($"Assets/AssetRaw/UI/InGame/{slimeType}.prefab");
        var unitRoot = GameModule.UI.UIRoot.transform;
        var unit = GameObject.Instantiate(res, unitRoot);
        unit.transform.position = spawnCastle.transform.position;
        unit.SetTarget(targetCastle);
    }

    public bool FindCastle(BaseCastle origin, Vector2 dir, out BaseCastle target)
    {
        target = null;
        List<BaseCastle> inAngleCastles = new List<BaseCastle>();
        if (castles.Count == 0)
        {
            castles.AddRange(GameObject.FindObjectsOfType<BaseCastle>());
        }
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

    public async void CreateRoads(RectTransform roadContainer)
    {
        GameObject roadImgRes = await GameModule.Resource.LoadAssetAsync<GameObject>("Assets/AssetRaw/UI/InGame/Road.prefab");
        for (int i = 0; i < roads.Count; i++)
        {
            Road road = roads[i];
            if (road.points.Count != 2)
            {
                continue;
            }

            GameObject roadImg = GameObject.Instantiate(roadImgRes, roadContainer);
            RectTransform roadImgRect = roadImg.GetComponent<RectTransform>();
            Vector2 start = road.points[0].anchoredPosition;
            Vector2 end = road.points[1].anchoredPosition;

            float distance = Vector2.Distance(start, end);
            roadImgRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, distance);
            roadImgRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100);

            float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
            roadImgRect.rotation = Quaternion.Euler(0, 0, angle);

            Vector2 center = (start + end) / 2;
            roadImgRect.anchoredPosition = center;
        }
    }

    public bool IsOnSameRoad(BaseCastle castle1, BaseCastle castle2)
    {
        return roads.Any(road => road.points.Contains(castle1.transform as RectTransform) && road.points.Contains(castle2.transform as RectTransform));
    }
}
