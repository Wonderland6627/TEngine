using System;
using System.Collections;
using System.Collections.Generic;
using GameLogic;
using TEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CastleType: int
{
    Nest = 0, //巢穴
    Tower = 1, //塔楼
}

/// <summary>
/// 城堡状态：
/// 1. 空塔：任何单位进来 占领数字-1；当数字为0 被单位占领
/// 2. 被占领的塔：占领单位进来 占领数字+1；非占领单位进来 数字-1；当数字为0 被非占领单位占领
/// </summary>
public partial class BaseCastle : UIWidget
{
    public Transform unitContainer;
    
    public int ID;
    public CastleType castleType;
    public UnitType occupiedUnitType; //占领单位类型
    public bool isOccupiedOnStart = true; //初始不为空塔
    public bool isOccupied => occupiedUnitCount > 0 && occupiedTime > 0; //是否被任何单位占领
    public int occupiedTime = 0; //被占领次数
    public int occupiedUnitCount; //占领单位数量

    public Vector2 startDragPos;
    public Vector2 dragDir;
    
    private float spawnDuration = 0f;
    private float currentSpawnSpeedCoe = 1f;
    private int attackTimer = -1;

    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        GameModule.Timer.RemoveTimer(attackTimer);
        base.OnDestroy();
    }

    public void Init()
    {
        if (!isOccupiedOnStart)
        {
            occupiedUnitCount = -10;
        } 
        else 
        {
            occupiedTime = 1;
        }

        UpdateCastleImage();

        var trigger = EventTriggerListener.Get(gameObject);
        trigger.OnDragBegin = OnBeginDrag;
        trigger.OnDragEvent = OnDrag;
        trigger.OnDragEnd = OnEndDrag;
    }

    private void SetEmptyCastle()
    {
        occupiedTime = 0;
        GameModule.Timer.RemoveTimer(attackTimer);
        UpdateCastleImage();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        SpawnUnit();
    }

    private void SpawnUnit()
    {
        var curLevelConfig = World.Instance.GetCurrentLevelConfig();
        if (curLevelConfig == null) 
        {
            Log.Error($"[{GetType().Name}] no find current level config");
            return;
        }
        float spawnInterval = occupiedUnitType == UnitType.Player
            ? curLevelConfig.playerSpawnInterval
            : curLevelConfig.enemy_1_SpawnInterval;
        float spawnSpeedCoe = occupiedUnitType == UnitType.Player
            ? World.Instance.playerSlimeSpawnSpeedCoe
            : World.Instance.enemySlimeSpawnSpeedCoe;
        currentSpawnSpeedCoe = spawnSpeedCoe * GetCastleSpawnSpeedCoeByCount();
        spawnDuration += Time.deltaTime * currentSpawnSpeedCoe;
        if (spawnDuration < spawnInterval) return;
        spawnDuration = 0f;
        SpawnUnit(null);
    }

    private float GetCastleSpawnSpeedCoeByCount()
    {
        float coe = 1f;
        // 例：当城堡内数量数量大于10 小于100时 coe在1-1.75之间线性增加
        if (occupiedUnitCount > 20)
        {
            float t = Mathf.Clamp01((occupiedUnitCount - 20f) / 80f);
            coe = Mathf.Lerp(1f, 1.75f, t);
        }
        return coe;
    }

    private void UpdateCastleImage(bool animate = false)
    {
        m_textCountTxt.text = $"{Mathf.Abs(occupiedUnitCount)}";
        if (GameModule.Debugger.ActiveWindow)
        {
            m_textCountTxt.text = $"[{ID}] {Mathf.Abs(occupiedUnitCount)} {GetUnitTypeFlag()} {currentSpawnSpeedCoe}";
        }

        m_imgPlayerImg.gameObject.SetActive(false);
        m_imgEnemy_1_Img.gameObject.SetActive(false);
        m_imgFreeImg.gameObject.SetActive(false);
        
        if (!isOccupied)
        {
            m_imgFreeImg.gameObject.SetActive(true);
            return;
        }
        bool isPlayer = occupiedUnitType == UnitType.Player;
        m_imgPlayerImg.gameObject.SetActive(isPlayer);
        m_imgEnemy_1_Img.gameObject.SetActive(!isPlayer);
    }

    public string GetUnitTypeFlag()
    {
        return occupiedUnitType == UnitType.Player? "P" : "E";
    }

    private void SpawnUnit(object[] args)
    {
        if (!isOccupied)
        {
            return;
        }
        if (castleType != CastleType.Nest)
        {
            return;
        }

        occupiedUnitCount++;
        UpdateCastleImage();
    }
    
    public void OnTriggeredByUnit(UnitType unitType)
    {
        UnitType curUnitType = occupiedUnitType; // 被占领前的单位类型
        int curOccupiedTime = occupiedTime; // 被占领前的占领次数
        if (occupiedUnitCount == 0)
        {
            occupiedUnitType = unitType;
            GameModule.Timer.RemoveTimer(attackTimer);
            attackTimer = -1;
            occupiedTime++;
            // 锦囊系统已移除 - playerGetMoreSlimeAfterOccupy 相关逻辑
            if (curOccupiedTime > 0 && unitType == UnitType.Enemy_1) // 非空塔被敌方占领
            {
                OnOccupiedByUnit(unitType);
            }
            m_rectDragArrow.gameObject.SetActive(false);
            World.Instance.Vibrate();
        }
        // Log.Info($"[{GetType().Name}] occupied by {unitType}, count = {occupiedUnitCount}");

        if (!isOccupied)
        {
            //正在尝试占领
            occupiedUnitCount++;
        }
        else
        {
            //被攻击或者获得增援
            occupiedUnitCount = unitType == occupiedUnitType ? occupiedUnitCount + 1 : occupiedUnitCount - 1;
        }
        UpdateCastleImage();
    }

    private void OnOccupiedByUnit(UnitType unitType)
    {
        // 锦囊系统已移除
    }

    // 直接被占领
    public void DirectOccupiedBy(UnitType unitType)
    {
        occupiedUnitCount = 0;
        OnTriggeredByUnit(unitType);
    }
    
    // 增加占领单位的数量 按比例
    public void AddOccupiedUnit(float ratio)
    {
        float addCount = occupiedUnitCount * ratio;
        AddOccupiedUnitCount((int)addCount);
    }

    // 减少占领单位的数量 按比例
    public void ReduceOccupiedUnit(float ratio)
    {
        float reduceCount = occupiedUnitCount * ratio;
        ReduceOccupiedUnitCount((int)reduceCount);
    }

    // 增加占领单位的数量
    public void AddOccupiedUnitCount(int count)
    {
        if (!isOccupied) return;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            SpawnUnit(null);
        }
    }

    // 减少占领单位的数量
    public void ReduceOccupiedUnitCount(int count)
    {
        if (!isOccupied) return;
        if (count == 0) return;
        
        occupiedUnitCount -= count;
        if (occupiedUnitCount <= 0) // 当减少为0时 设为空塔
        {
            occupiedUnitCount = 0;
            GameModule.Timer.RemoveTimer(attackTimer);
            attackTimer = -1;
            occupiedTime = 0; //重置被占领次数
        }
        UpdateCastleImage();
    }

    private bool CanDrag()
    {
        return isOccupied && occupiedUnitType == UnitType.Player;
    }

    public void OnBeginDrag(GameObject go, PointerEventData eventData)
    {
        if (!CanDrag())
        {
            return;
        }
        startDragPos = eventData.position;
        ShowDragArrow();
    }

    public void OnDrag(GameObject go, PointerEventData eventData)
    {
        if (!CanDrag())
        {
            return;
        }
        dragDir = eventData.position - startDragPos;
        dragDir.Normalize();
        OnDragArrow(eventData.position);
    }

    public void OnEndDrag(GameObject go, PointerEventData eventData)
    {
        if (!CanDrag())
        {
            return;
        }
        HideDragArrow();
        if (!World.Instance.FindCastle(this, dragDir, out BaseCastle target))
        {
            Log.Info($"[{GetType().Name}] no find target castle");
            return;
        }
        MoveTo(target);

        if (World.Instance.playingLevelId == 1)
        {
            GameEvent.Send(SlimeEvent.OnKnownTutorial);
        }
        if (!World.Instance.GameData.GuideFinish) // 成功滑动派兵则为完成新手引导
        {
            World.Instance.GameData.GuideFinish = true;
        }
    }

    public void MoveTo(BaseCastle target)
    {
        if (occupiedUnitCount <= 1)
        {
            Log.Info($"[{GetType().Name}] unit not enough");
            return;
        }

        SendSlime(occupiedUnitCount, target);
        Log.Info($"[{GetType().Name}] [{gameObject.name}_{GetUnitTypeFlag()}] attack [{target.gameObject.name}_{target.GetUnitTypeFlag()}], send count: {occupiedUnitCount}");
    }

    private void SendSlime(int count, BaseCastle target, bool sendDirectly = true)
    {
        if (attackTimer != -1) 
        {
            GameModule.Timer.RemoveTimer(attackTimer);
        }
        int remainingCount = count;
        float attackInterval = 1f;
        var curLevelConfig = World.Instance.GetCurrentLevelConfig();
        if (curLevelConfig != null) 
        {
             attackInterval = occupiedUnitType == UnitType.Player ? curLevelConfig.playerAttackInterval : curLevelConfig.enemy_1_AttackInterval;
        }
        if (sendDirectly)
        {
            Attack();
        }
        attackTimer = GameModule.Timer.AddTimer(Attack, attackInterval, true);

        void Attack(params object[] args)
        {
            if (remainingCount <= 1 || occupiedUnitCount <= 1)
            {
                GameModule.Timer.RemoveTimer(attackTimer);
                attackTimer = -1;
                return;
            }

            World.Instance.CreateUnit(this, occupiedUnitType, target, unitContainer);
            remainingCount--;
            occupiedUnitCount--;
            UpdateCastleImage();
        }
    }
}

partial class BaseCastle
{
    private void ShowDragArrow()
    {
        m_rectDragArrow.gameObject.SetActive(true);
    }

    private void OnDragArrow(Vector2 mousePos)
    {
        Vector2 originPos = RectTransformUtility.WorldToScreenPoint(GameModule.UI.UICamera, transform.position);
        Vector2 dir = mousePos - originPos;
        Rect uiRootRect = UIModule.UIRootStatic.RectTransform().rect;
        float resolution = uiRootRect.height / uiRootRect.width ;
        float distance = Vector2.Distance(originPos, mousePos) / resolution * 1.35f;
        m_rectDragArrow.sizeDelta = new Vector2(distance, m_rectDragArrow.sizeDelta.y);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        m_rectDragArrow.localRotation = Quaternion.Euler(0, 0, angle);

        // Log.Info($"originPos = {originPos}, mousePos = {mousePos}, dir = {dir}, angle = {angle}, distance = {distance}, sizeDelta = {m_rectDragArrow.sizeDelta}, resolution = {resolution}");
    }

    private void HideDragArrow()
    {
        m_rectDragArrow.gameObject.SetActive(false);
    }
}

partial class BaseCastle
{
    #region 脚本工具生成的代码
    private Image m_imgPlayerImg;
    private Image m_imgEnemy_1_Img;
    private Image m_imgFreeImg;
    private Text m_textCountTxt;
    private RectTransform m_rectDragArrow;
    protected override void ScriptGenerator()
    {
        m_imgPlayerImg = FindChildComponent<Image>("m_imgPlayerImg");
        m_imgEnemy_1_Img = FindChildComponent<Image>("m_imgEnemy_1_Img");
        m_imgFreeImg = FindChildComponent<Image>("m_imgFreeImg");
        m_textCountTxt = FindChildComponent<Text>("m_textCountTxt");
        m_rectDragArrow = FindChildComponent<RectTransform>("m_rectDragArrow");
    }
    #endregion
}