using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType: int
{
    Player = 0,
    Enemy_1 = 1,
}

public class BaseUnit : MonoBehaviour
{
    public UnitType unitType;
    
    public float moveDuration = 15f;
    public float moveSpeed = 1.0f;
    public Vector2 moveDir;
    public float arrivalThreshold = 0.1f;
    
    public BaseCastle target;

    void Start()
    {
        arrivalThreshold = 1f;
    }

    void Update()
    {
        Execute();
    }

    private void Execute()
    {
        if (target == null)
        {
            return;
        }
        
        float distancePerFrame = Screen.width / moveDuration * Time.deltaTime * moveSpeed;
        transform.Translate(moveDir * distancePerFrame);
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
        Destroy(gameObject);
    }

    private void OnTriggerEnemy(BaseUnit enemyUnit)
    {
        Destroy(enemyUnit.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseCastle otherCastle = other.GetComponent<BaseCastle>();
        if (otherCastle == target)
        {
            OnTriggerTarget();
        }
        
        BaseUnit otherUnit = other.GetComponent<BaseUnit>();
        if (otherUnit != null && otherUnit.unitType != unitType)
        {
            OnTriggerEnemy(otherUnit);
        }
    }
}
