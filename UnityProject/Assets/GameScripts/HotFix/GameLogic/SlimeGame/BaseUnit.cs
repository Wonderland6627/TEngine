using System;
using System.Collections;
using System.Collections.Generic;
using GameLogic;
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
        Move2Target();
        CheckTriggered();
    }

    private void Move2Target()
    {
        var uiRootRect = GameModule.UI.UIRootRect;
        float screenWidth = uiRootRect.rect.width;
        float distancePerFrame = screenWidth / moveDuration * Time.deltaTime * World.Instance.playerSlimeMoveSpeedCoe;
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

    private void CheckTriggered()
    {
        if (target == null)
        {
            return;
        }

        //check castle
        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance < arrivalThreshold)
        {
            OnTriggerTarget();
        }
    }

    private void OnTriggerTarget()
    {
        target.OnTriggeredByUnit(unitType);
        Destroy();
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