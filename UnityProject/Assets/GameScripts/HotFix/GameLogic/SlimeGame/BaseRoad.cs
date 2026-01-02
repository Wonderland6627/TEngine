using UnityEngine;
using UnityEngine.UI;
using TEngine;
using System.Collections.Generic;
using GameLogic;

public partial class BaseRoad : BaseObject
{
    public Road data;

    private List<BaseUnit> m_Units = new List<BaseUnit>();
    public List<BaseUnit> Units => m_Units;

    private HashSet<BaseUnit> unitsToDestroy = new HashSet<BaseUnit>();
    
    const float sqrTriggerDistance = 0.01f; // 0.1f * 0.1f

    protected override void OnCreate()
    {
        base.OnCreate();
        m_imgRoad.pixelsPerUnitMultiplier = UnityEngine.Random.Range(2, 3.3f);
    }

    protected override void OnGameUpdate()
    {
        base.OnGameUpdate();
        CheckUnitsTriggered();
    }

    protected override void OnDestroy()
    {
        for (int i = 0; i < m_Units.Count; i++)
        {
            m_Units[i].Destroy();
        }
        m_Units.Clear();
        base.OnDestroy();
    }

    private void CheckUnitsTriggered()
    {
        if (m_Units.Count < 2)
        {
            return;
        }

        unitsToDestroy.Clear();
        for (int i = 0; i < m_Units.Count; i++)
        {
            BaseUnit unit1 = m_Units[i];
            if (unit1 == null || unit1.transform == null || unitsToDestroy.Contains(unit1))
            {
                continue;
            }
            
            for (int j = i + 1; j < m_Units.Count; j++)
            {
                BaseUnit unit2 = m_Units[j];
                if (unit2 == null || unit2.transform == null || unitsToDestroy.Contains(unit2))
                {
                    continue;
                }
                
                if (unit1.unitType == unit2.unitType)
                {
                    continue;
                }
                
                Vector2 pos1 = unit1.transform.position;
                Vector2 pos2 = unit2.transform.position;
                float sqrDistance = (pos1 - pos2).sqrMagnitude;
                
                if (sqrDistance < sqrTriggerDistance)
                {
                    unitsToDestroy.Add(unit1);
                    unitsToDestroy.Add(unit2);
                    break; // unit1已标记销毁，跳出内层循环
                }
            }
        }
        
        foreach (BaseUnit unit in unitsToDestroy)
        {
            m_Units.Remove(unit);
            unit.Destroy();
        }
    }

    public void RegisterUnit(BaseUnit unit)
    {
        if (m_Units.Contains(unit))
        {
            return;
        }
        m_Units.Add(unit);
    }
}

partial class BaseRoad
{
	#region 脚本工具生成的代码
	private Image m_imgRoad;
	protected override void ScriptGenerator()
	{
		m_imgRoad = FindChildComponent<Image>("m_imgRoad");
	}
	#endregion
}