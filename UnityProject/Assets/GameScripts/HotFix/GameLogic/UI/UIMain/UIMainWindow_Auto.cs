using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TEngine;

namespace GameLogic
{
	partial class UIMainWindow: UIWindow
	{
		#region 脚本工具生成的代码
		private GameObject m_itemLevelView;
		private RectTransform m_rectResourcesBar;
		private GameObject m_itemResourceCoin;
		private GameObject m_itemResourceEnergy;
		private Text m_txtVersion;
		private Transform m_tfBottomTabContent;
		private GameObject m_itemBottomTab_Atlas;
		private GameObject m_itemBottomTab_Level;
		private GameObject m_itemBottomTab_Lottery;

		protected override void ScriptGenerator()
		{
			m_itemLevelView = FindChild("m_itemLevelView").gameObject;
			m_rectResourcesBar = FindChildComponent<RectTransform>("m_rectResourcesBar");
			m_itemResourceCoin = FindChild("m_rectResourcesBar/m_itemResourceCoin").gameObject;
			m_itemResourceEnergy = FindChild("m_rectResourcesBar/m_itemResourceEnergy").gameObject;
			m_txtVersion = FindChildComponent<Text>("m_txtVersion");
			m_tfBottomTabContent = FindChild("m_tfBottomTabContent");
			m_itemBottomTab_Atlas = FindChild("m_tfBottomTabContent/m_itemBottomTab_Atlas").gameObject;
			m_itemBottomTab_Level = FindChild("m_tfBottomTabContent/m_itemBottomTab_Level").gameObject;
			m_itemBottomTab_Lottery = FindChild("m_tfBottomTabContent/m_itemBottomTab_Lottery").gameObject;
		}
		#endregion
	}
}