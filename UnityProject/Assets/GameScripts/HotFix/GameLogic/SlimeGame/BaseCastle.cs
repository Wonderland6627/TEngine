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
    public bool isOccupied => occupiedUnitCount > 0 || occupiedTime > 0; //是否被任何单位占领
    public int occupiedTime = 0; //被占领次数
    public int occupiedUnitCount; //占领单位数量

    public Vector2 startDragPos;
    public Vector2 dragDir;
    
    private float spawnDuration = 0f;
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

        UpdateCountText();
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
        UpdateCountText();
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
        spawnDuration += Time.deltaTime * spawnSpeedCoe;
        if (spawnDuration < spawnInterval) return;
        spawnDuration = 0f;
        SpawnUnit(null);
    }

    private void UpdateCastleImage(bool animate = false)
    {
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

    private void UpdateCountText()
    {
        m_textCountTxt.text = $"{Mathf.Abs(occupiedUnitCount)}";
#if UNITY_EDITOR
        m_textCountTxt.text += GetUnitTypeFlag();
#endif
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
        UpdateCountText();
    }
    
    public void OnTriggeredByUnit(UnitType unitType)
    {
        if (occupiedUnitCount == 0)
        {
            occupiedUnitType = unitType;
            GameModule.Timer.RemoveTimer(attackTimer);
            attackTimer = -1;
            occupiedTime++;
            if (unitType == UnitType.Player) //RewardType.ChainOccupation
            {
                if (World.Instance.playerGetMoreSlimeAfterOccupy)
                {
                    RewardAction action = World.Instance.activeRewardAction;
                    if (action == null) return;
                    int adddCount = (int)action.GetEffectValue();
                    for (int i = 0; i < adddCount; i++)
                    {
                        SpawnUnit(null);
                    }
                }
            }
        }
        // Debug.Log($"[{GetType().Name}] occupied by {unitType}, count = {occupiedUnitCount}");

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
        UpdateCountText();
        UpdateCastleImage();
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
        UpdateCountText();
        UpdateCastleImage();
    }

    private void TryRemoveRewardAction()
    {
        
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
            Debug.Log($"[{GetType().Name}] no find target castle");
            return;
        }
        MoveTo(target);
    }

    public void MoveTo(BaseCastle target)
    {
        if (occupiedUnitCount <= 0)
        {
            Debug.Log($"[{GetType().Name}] unit not enough");
            return;
        }

        SendSlime(occupiedUnitCount, target);
        Debug.Log($"[{GetType().Name}] [{gameObject.name}_{GetUnitTypeFlag()}] attack [{target.gameObject.name}_{target.GetUnitTypeFlag()}], send count: {occupiedUnitCount}");
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
            if (remainingCount == 0) {
                GameModule.Timer.RemoveTimer(attackTimer);
                attackTimer = -1;
                return;
            }

            World.Instance.CreateUnit(this, occupiedUnitType, target, unitContainer);
            remainingCount--;
            occupiedUnitCount--;
            UpdateCountText();
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
        Vector2 dragVector = mousePos - startDragPos;
        float angle = Mathf.Atan2(dragVector.y, dragVector.x) * Mathf.Rad2Deg;
        float length = dragVector.magnitude;
        m_rectDragArrow.rotation = Quaternion.Euler(0, 0, angle);
        float minLength = 50f;
        length = Mathf.Clamp(length, minLength, length);
        m_rectDragArrow.sizeDelta = new Vector2(length, m_rectDragArrow.sizeDelta.y);
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