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
		private GameObject m_itemBottomTab;
		private GameObject m_itemBottomTab1;
		private GameObject m_itemBottomTab2;

		protected override void ScriptGenerator()
		{
			m_itemLevelTab = FindChild("m_itemLevelTab").gameObject;
			m_tfBottomTabContent = FindChild("m_tfBottomTabContent");
			m_itemBottomTab = FindChild("m_tfBottomTabContent/m_itemBottomTab").gameObject;
			m_itemBottomTab1 = FindChild("m_tfBottomTabContent/m_itemBottomTab1").gameObject;
			m_itemBottomTab2 = FindChild("m_tfBottomTabContent/m_itemBottomTab2").gameObject;
		}
		#endregion
	}
}