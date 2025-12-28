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
        for (int i = 0; i < m_Units.Count; i++)
        {
            for (int j = i + 1; j < m_Units.Count; j++)
            {
                BaseUnit unit1 = m_Units[i];
                BaseUnit unit2 = m_Units[j];
                if (unit1 == null || unit1.transform == null
                 || unit2 == null || unit2.transform == null)
                {
                    continue;
                }
                if (unit1.unitType == unit2.unitType)
                {
                    continue;
                }
                float distance = Vector2.Distance(unit1.transform.position, unit2.transform.position);
                if (distance < 0.1f)
                {
                    m_Units.Remove(unit1);
                    m_Units.Remove(unit2);
                    unit1.Destroy();
                    unit2.Destroy();
                }  
            }  
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