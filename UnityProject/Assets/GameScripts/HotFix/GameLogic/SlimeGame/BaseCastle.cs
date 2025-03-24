using System;
using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CastleType: int
{
    Nest = 0, //巢穴
    Tower = 1, //塔楼
}

public partial class BaseCastle : UIWidget
{
    public Transform unitContainer;
    
    public int ID;
    public CastleType castleType;
    public bool isOccupiedOnStart = true; //初始不为空塔
    public bool isOccupied => occupiedUnitCount > 0; //是否被任何单位占领
    public UnitType occupiedUnitType; //占领单位类型
    public int occupiedUnitCount; //占领单位数量

    public Vector2 startDragPos;
    public Vector2 dragDir;
    
    private int spawnTimer = -1;
    private int attackTimer = -1;

    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        GameModule.Timer.RemoveTimer(spawnTimer);
        base.OnDestroy();
    }

    public void Init()
    {
        if (!isOccupiedOnStart)
        {
            occupiedUnitCount = -10;
        }

        UpdateCountText();
        UpdateCastleImage();
        
        //todo: 拆出来 当占领单位变化的时候重置间隔
        var curLevelConfig = World.Instance.GetCurrentLevelConfig();
        if (curLevelConfig == null) 
        {
            Log.Error($"[{GetType().Name}] no find current level config");
            return;
        }
        float spawnInterval = occupiedUnitType == UnitType.Player ? curLevelConfig.playerSpawnInterval : curLevelConfig.enemy_1_SpawnInterval;
        spawnTimer = GameModule.Timer.AddTimer(SpawnUnit, spawnInterval, true);

        var trigger = EventTriggerListener.Get(gameObject);
        trigger.OnDragBegin = OnBeginDrag;
        trigger.OnDragEvent = OnDrag;
        trigger.OnDragEnd = OnEndDrag;
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
        m_textCountTxt.text = $"{occupiedUnitCount}";
    }
    
    public void OnOccupyByUnit(UnitType unitType)
    {
        if (occupiedUnitCount == 0)
        {
            occupiedUnitType = unitType;
            GameModule.Timer.RemoveTimer(attackTimer);
            attackTimer = -1;
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
    }

    public void OnDrag(GameObject go, PointerEventData eventData)
    {
        if (!CanDrag())
        {
            return;
        }
        dragDir = eventData.position - startDragPos;
        dragDir.Normalize();
    }

    public void OnEndDrag(GameObject go, PointerEventData eventData)
    {
        if (!CanDrag())
        {
            return;
        }
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
        Debug.Log($"[{GetType().Name}] [{gameObject.name}] attack [{target.gameObject.name}], send count: {occupiedUnitCount}");
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
            m_textCountTxt.text = $"{occupiedUnitCount}";
        }
    }
}

partial class BaseCastle
{
    #region 脚本工具生成的代码
    private Image m_imgPlayerImg;
    private Image m_imgEnemy_1_Img;
    private Image m_imgFreeImg;
    private Text m_textCountTxt;
    protected override void ScriptGenerator()
    {
        m_imgPlayerImg = FindChildComponent<Image>("m_imgPlayerImg");
        m_imgEnemy_1_Img = FindChildComponent<Image>("m_imgEnemy_1_Img");
        m_imgFreeImg = FindChildComponent<Image>("m_imgFreeImg");
        m_textCountTxt = FindChildComponent<Text>("m_textCountTxt");
    }
    #endregion
}