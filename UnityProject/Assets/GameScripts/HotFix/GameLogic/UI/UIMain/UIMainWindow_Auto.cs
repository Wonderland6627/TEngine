using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	partial class UIMainWindow: UIWindow
	{
		#region 脚本工具生成的代码
		private GameObject m_itemLevelTab;
		private Transform m_tfBottomTabContent;
		private GameObject m_itemBottomTab_Atlas;
		private GameObject m_itemBottomTab_Level;
		private GameObject m_itemBottomTab_Lottery;

		protected override void ScriptGenerator()
		{
			m_itemLevelTab = FindChild("m_itemLevelTab").gameObject;
			m_tfBottomTabContent = FindChild("m_tfBottomTabContent");
			m_itemBottomTab_Atlas = FindChild("m_tfBottomTabContent/m_itemBottomTab_Atlas").gameObject;
			m_itemBottomTab_Level = FindChild("m_tfBottomTabContent/m_itemBottomTab_Level").gameObject;
			m_itemBottomTab_Lottery = FindChild("m_tfBottomTabContent/m_itemBottomTab_Lottery").gameObject;
		}
		#endregion
	}
}