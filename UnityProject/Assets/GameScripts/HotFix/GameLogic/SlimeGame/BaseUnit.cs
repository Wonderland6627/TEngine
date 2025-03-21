using System;
using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

public enum UnitType: int
{
    Player = 0,
    Enemy_1 = 1,
}

public partial class BaseUnit : UIWidget
{
    public UnitType unitType;
    
    public float moveDuration = 15f;
    public float arrivalThreshold = 0.1f;
    
    public BaseCastle target;
    private Vector2 moveDir;

    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void Init() 
    {
        arrivalThreshold = 1f;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        Execute();
    }

    private void Execute()
    {
        if (target == null)
        {
            return;
        }
        
        var uiRootRect = GameModule.UI.UIRootRect;
        float screenWidth = uiRootRect.rect.width;
        float distancePerFrame = screenWidth / moveDuration * Time.deltaTime;
        Vector2 currentPos = transform.localPosition;
        currentPos += moveDir * distancePerFrame;
        transform.localPosition = currentPos;
    }

    public void SetTarget(BaseCastle target)
    {
        this.target = target;
        moveDir = target.transform.position - transform.position;
        moveDir = moveDir.normalized;
    }

    private void OnTriggerTarget()
    {
        if (target == null)
        {
            return;
        }
        
        target.OnOccupyByUnit(unitType);
        
        Destroy();
    }

    private void OnTriggerEnemy(BaseUnit enemyUnit)
    {
        enemyUnit.Destroy();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // BaseCastle otherCastle = other.GetComponent<BaseCastle>();
        // if (otherCastle == target)
        // {
        //     OnTriggerTarget();
        // }
        
        // BaseUnit otherUnit = other.GetComponent<BaseUnit>();
        // if (otherUnit != null && otherUnit.unitType != unitType)
        // {
        //     OnTriggerEnemy(otherUnit);
        // }
    }
}

partial class BaseUnit
{
    #region 脚本工具生成的代码
    private Image m_imgUnit;
    protected override void ScriptGenerator()
    {
        m_imgUnit = FindChildComponent<Image>("m_imgUnit");
    }
    #endregion
}