using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	partial class UIGameWindow: UIWindow
	{
		#region 脚本工具生成的代码
		private Image m_img_bg;
		private Transform m_tfRoadContainer;
		private Transform m_tfCastleContainer;
		private Transform m_tfUnitContainer;
		private Button m_btnBack;
		private Button m_btnSettings;
		private Button m_btnRule;
		private GameObject m_itemTutorialTips;

		protected override void ScriptGenerator()
		{
			m_img_bg = FindChildComponent<Image>("Content/m_img_bg");
			m_tfRoadContainer = FindChild("Content/m_tfRoadContainer");
			m_tfCastleContainer = FindChild("Content/m_tfCastleContainer");
			m_tfUnitContainer = FindChild("Content/m_tfUnitContainer");
			m_btnBack = FindChildComponent<Button>("Content/m_btnBack");
			m_btnSettings = FindChildComponent<Button>("Content/m_btnSettings");
			m_btnRule = FindChildComponent<Button>("Content/m_btnRule");
			m_itemTutorialTips = FindChild("Content/m_itemTutorialTips").gameObject;
		}
		#endregion
	}
}