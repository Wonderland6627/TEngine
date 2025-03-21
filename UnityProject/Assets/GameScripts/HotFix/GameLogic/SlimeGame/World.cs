using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using GameBase;
using GameLogic;
using TEngine;

[System.Serializable]
public class Road
{
    public List<RectTransform> points; //限制points的长度为2
}

public partial class World : 
#if UNITY_EDITOR
    SingletonBehaviour<World>
#else
    Singleton<World>
#endif
{
    public LevelReader levelReader { get; private set; }
    public UnitReader unitReader { get; private set; }
    
    public List<BaseCastle> castles = new List<BaseCastle>();
    public List<Road> roads = new List<Road>();

    public async void AsyncInit()
    {
        await LoadConfig();
        GameModule.UI.ShowUIAsync<UILevelWindow>();
        // GameModule.UI.ShowUIAsync<UIMainWindow>();
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

partial class World
{
    public void ClearItems() 
    {
        castles.Clear();
        roads.Clear();
    }

    public void SetCastles(List<BaseCastle> castles)
    {
        this.castles = castles;
    }

    public void SetRoads(List<Road> roads)
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
        var unit = await mainWindow.CreateWidgetByPathAsync<BaseUnit>(container, unitConfig.unitPrefabPath);
        unit.gameObject.name = $"{unitType}_{unit.gameObject.GetInstanceID()}";
        unit.unitType = unitType;
        unit.moveDuration = unitConfig.moveDuration;
        unit.transform.position = spawnCastle.transform.position;
        unit.transform.localScale = Vector3.one;
        unit.SetTarget(targetCastle);
    }

    public bool FindCastle(BaseCastle origin, Vector2 dir, out BaseCastle target)
    {
        target = null;
        List<BaseCastle> inAngleCastles = new List<BaseCastle>();
        // if (castles.Count == 0)
        // {
        //     castles.AddRange(GameObject.FindObjectsOfType<BaseCastle>());
        // }
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