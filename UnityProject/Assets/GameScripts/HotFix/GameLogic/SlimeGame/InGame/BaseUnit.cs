using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameLogic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

public enum UnitType: int
{
    Player   = 0,
    Enemy_1  = 1,
    Enemy_2  = 2,
    Enemy_3  = 3,
    Enemy_4  = 4,
}

public partial class BaseUnit : BaseObject
{
    public UnitType unitType;

    public float moveDuration = 15f;
    public float arrivalThreshold = 0.1f;
    
    public BaseCastle target;
    private Vector2 moveDir;
    private float sqrArrivalThreshold = 0f; // 平方阈值

    protected override void OnCreate()
    {
        base.OnCreate();
        sqrArrivalThreshold = arrivalThreshold * arrivalThreshold;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public void Init() 
    {
        sqrArrivalThreshold = arrivalThreshold * arrivalThreshold;
    }

    protected override void OnGameUpdate()
    {
        base.OnGameUpdate();
        Move2Target();
        CheckTriggered();
    }

    private void Move2Target()
    {
        var uiRootRect = GameModule.UI.UIRootRect;
        float screenWidth = uiRootRect.rect.width;
        float speedCoe = World.Instance.GetFactionMoveSpeedCoe(unitType);
        float distancePerFrame = screenWidth / moveDuration * Time.deltaTime * speedCoe;
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
        if (target == null || target.transform == null)
        {
            return;
        }

        Vector2 unitPos = transform.position;
        Vector2 targetPos = target.transform.position;
        float sqrDistance = (unitPos - targetPos).sqrMagnitude;
        
        if (sqrDistance < sqrArrivalThreshold)
        {
            OnTriggerTarget();
        }
    }

    private void OnTriggerTarget()
    {
        target.OnTriggeredByUnit(unitType);
        Destroy();
    }

    public void Disappear()
    {
        World.Instance.Vibrate();
        Destroy();
    }

    public void SetUnitImagePath(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            return;
        }

        LoadUnitImageAsync(imagePath).Forget();
    }

    private async UniTaskVoid LoadUnitImageAsync(string imagePath)
    {
        var sprite = await GameModule.Resource.LoadAssetAsync<Sprite>(imagePath);
        if (sprite == null || m_imgUnit == null)
        {
            return;
        }

        m_imgUnit.sprite = sprite;
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