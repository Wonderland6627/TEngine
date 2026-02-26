using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;
namespace GameLogic
{
    public partial class UIResourcesBar
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }
    }

#region 脚本工具生成的代码 === 复制开始 ===
	partial class UIResourcesBar : UIWidget
	{
		private GameObject m_itemResourceCoin;
		private GameObject m_itemResourceEnergy;

		protected override void ScriptGenerator()
		{
			m_itemResourceCoin = FindChild("m_itemResourceCoin").gameObject;
			m_itemResourceEnergy = FindChild("m_itemResourceEnergy").gameObject;
		}
	}
#endregion === 复制结束 ===
}
