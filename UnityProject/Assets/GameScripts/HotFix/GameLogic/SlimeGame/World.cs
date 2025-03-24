using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using GameBase;
using GameLogic;
using TEngine;

public class Road
{
    public List<BaseCastle> points; //限制points的长度为2
}

public partial class World : Singleton<World>
{
    public LevelReader levelReader { get; private set; }
    public UnitReader unitReader { get; private set; }
    
    public List<BaseCastle> castles = new List<BaseCastle>();

    public List<BaseRoad> roads = new List<BaseRoad>();

    public int currentLevelId = 0;

    public async void AsyncInit()
    {
        await LoadConfig();
        // GameModule.UI.ShowUIAsync<UILevelWindow>();

        currentLevelId = 1;
        GameModule.UI.ShowUIAsync<UIMainWindow>();
    }

    public void StartGame(int levelId) 
    {
        currentLevelId = levelId;
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
        return GetLevel(currentLevelId);
    }

    public LevelConfig.Config GetCurrentLevelConfig()
    {
        var currentLevel = GetLevel(currentLevelId);
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