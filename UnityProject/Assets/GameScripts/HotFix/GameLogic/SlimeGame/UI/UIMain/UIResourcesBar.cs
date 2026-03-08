using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public partial class UIResourcesBar
    {
        private UIResourceItem _coinItem;
        private UIResourceItem _energyItem;

        protected override void OnCreate()
        {
            base.OnCreate();

            _coinItem = CreateWidget<UIResourceItem>(m_itemResourceCoin);
            _coinItem.Init(
                ResourceType.Coin,
                () => World.Instance.GameData.Coin
            );

            _energyItem = CreateWidget<UIResourceItem>(m_itemResourceEnergy);
            _energyItem.Init(
                ResourceType.Energy,
                () => World.Instance.GameData.Energy,
                (value) => $"{value}/{ConfigSystem.Instance.Tables.TbGlobalConfig.EnergyMax}"
            );
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
