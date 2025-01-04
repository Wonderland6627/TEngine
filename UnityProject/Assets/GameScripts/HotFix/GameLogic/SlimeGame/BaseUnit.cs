using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlimeType
{
    Red,
    Blue,
}

public class BaseUnit : MonoBehaviour
{
    public SlimeType slimeType;
    
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
        
        if (Vector2.Distance(transform.position, target.transform.position) < arrivalThreshold)
        {
            OnReachTarget();
        }
    }

    public void SetTarget(BaseCastle target)
    {
        this.target = target;
        moveDir = target.transform.position - transform.position;
        moveDir = moveDir.normalized;
    }

    private void OnReachTarget()
    {
        if (target == null)
        {
            return;
        }
        
        target.OnOccupyByUnit(slimeType);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseUnit otherUnit = other.GetComponent<BaseUnit>();
        if (otherUnit == null)
        {
            return;
        }
        if (otherUnit.slimeType == slimeType)
        {
            return;
        }
        
        Destroy(otherUnit.gameObject);
    }
}
