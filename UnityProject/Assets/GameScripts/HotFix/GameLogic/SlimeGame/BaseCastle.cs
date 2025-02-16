using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CastleType: int
{
    Nest = 0, //巢穴
    Tower = 1, //塔楼
}

public class BaseCastle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image playerCastleImage;
    public Image enemy_1_CastleImage;
    public Image freeImage;
    public Text countText;

    public Transform unitContainer;
    
    public CastleType castleType;
    public bool isOccupiedOnStart = true; //初始不为空塔
    public bool isOccupied => occupiedUnitCount > 0; //是否被任何单位占领
    public UnitType occupiedUnitType; //占领单位类型
    public int occupiedUnitCount; //占领单位数量

    public float unitSpawnInterval = 1f;

    public Vector2 startDragPos;
    public Vector2 dragDir;
    
    private Coroutine currentAttackCoroutine;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        playerCastleImage = transform.Find("Image_Blue").GetComponent<Image>();
        enemy_1_CastleImage = transform.Find("Image_Red").GetComponent<Image>();
        freeImage = transform.Find("Image_Free").GetComponent<Image>();
        countText = transform.Find("CountTxt").GetComponent<Text>();
        if (!isOccupiedOnStart)
        {
            occupiedUnitCount = -10;
        }

        UpdateCountText();
        UpdateCastleImage();
        
        InvokeRepeating("SpawnUnit", 1f, unitSpawnInterval);
    }

    private void UpdateCastleImage(bool animate = false)
    {
        playerCastleImage.gameObject.SetActive(false);
        enemy_1_CastleImage.gameObject.SetActive(false);
        freeImage.gameObject.SetActive(false);
        
        if (!isOccupied)
        {
            freeImage.gameObject.SetActive(true);
            return;
        }
        bool isPlayer = occupiedUnitType == UnitType.Player;
        playerCastleImage.gameObject.SetActive(isPlayer);
        enemy_1_CastleImage.gameObject.SetActive(!isPlayer);
    }

    private void UpdateCountText()
    {
        countText.text = $"{Mathf.Abs(occupiedUnitCount)}";
    }

    private void SpawnUnit()
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
        countText.text = $"{occupiedUnitCount}";
    }
    
    public void OnOccupyByUnit(UnitType unitType)
    {
        if (occupiedUnitCount == 0)
        {
            occupiedUnitType = unitType;
            StopCurrentAttack();
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag())
        {
            return;
        }
        startDragPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CanDrag())
        {
            return;
        }
        dragDir = eventData.position - startDragPos;
        dragDir.Normalize();
    }

    public void OnEndDrag(PointerEventData eventData)
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

        StopCurrentAttack();
        currentAttackCoroutine = StartCoroutine(SendSlime(occupiedUnitCount, target));
        
        // Debug.Log($"[{GetType().Name}] attack [{target.name}]");
    }

    private void StopCurrentAttack()
    {
        if (currentAttackCoroutine == null)
        {
            return;
        }
        StopCoroutine(currentAttackCoroutine);
        currentAttackCoroutine = null;
    }

    private IEnumerator SendSlime(int count, BaseCastle target)
    {
        var wait = new WaitForSeconds(0.5f);
        for (int i = 0; i < count; i++)
        {        
            yield return wait;
            World.Instance.CreateUnit(this, occupiedUnitType, target, unitContainer);
            occupiedUnitCount--;
            countText.text = $"{occupiedUnitCount}";
        }
    }
}
