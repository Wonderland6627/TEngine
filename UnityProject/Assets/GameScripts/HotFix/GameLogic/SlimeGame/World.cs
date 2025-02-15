using System;
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

public class LevelConfig
{
    public int levelId { get; set; }
    public List<Castle> castles { get; set; }
    public List<Road> roads { get; set; }

    public class Castle
    {
        public int id { get; set; }
        public int castleType { get; set; }
        public bool occupiedOnStart { get; set; }
        public int occupiedSlimeType { get; set; }
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
}

public class LevelReader
{
    public List<LevelConfig> configs { get; private set; }

    public async UniTask LoadLevelConfig(string jsonPath)
    {
        var res = await GameModule.Resource.LoadAssetAsync<TextAsset>(jsonPath);
        var json = res.text;
        configs = Utility.Json.ToObject<List<LevelConfig>>(json);
    }
}

public partial class World : SingletonBehaviour<World>
{
    public LevelReader reader { get; private set; }
    
    public List<BaseCastle> castles = new List<BaseCastle>();
    public List<Road> roads = new List<Road>();

    public async void Init()
    {
        await LoadConfig();
        // GameModule.UI.ShowUIAsync<UILevelWindow>();
        GameModule.UI.ShowUIAsync<UIMainWindow>();
    }
}

partial class World
{
    private async UniTask LoadConfig()
    {
        reader = new LevelReader();
        await reader.LoadLevelConfig("levels");
    }

    public LevelConfig GetLevel(int levelId)
    {
        if (reader.configs == null ||
            reader.configs.Count == 0)
        {
            return null;
        }
        
        return reader.configs.Find(level => level.levelId == levelId);
    }
}

partial class World
{
    public void SetCastles(List<BaseCastle> castles)
    {
        this.castles = castles;
    }

    public void SetRoads(List<Road> roads)
    {
        this.roads = roads;
    }
    
    public async void CreateUnit(BaseCastle spawnCastle, UnitType unitType, BaseCastle targetCastle)
    {
        var mainWindow = await GameModule.UI.GetUIAsyncAwait<UIMainWindow>();
        var res = await GameModule.Resource.LoadGameObjectAsync($"Assets/AssetRaw/UI/InGame/Unit_{unitType}.prefab");
        var unit = res.GetComponent<BaseUnit>();
        unit.transform.SetParent(mainWindow.SlimeContainer);
        unit.transform.position = spawnCastle.transform.position;
        unit.transform.localScale = Vector3.one;
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

    public bool IsOnSameRoad(BaseCastle castle1, BaseCastle castle2)
    {
        return roads.Any(road => road.points.Contains(castle1.transform as RectTransform) && road.points.Contains(castle2.transform as RectTransform));
    }
}