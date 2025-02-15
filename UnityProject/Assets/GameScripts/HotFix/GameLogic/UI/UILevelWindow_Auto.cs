using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
	[Window(UILayer.UI)]
	partial class UILevelWindow : UIWindow
	{
		protected override void OnCreate()
		{
			base.OnCreate();

			var levels = World.Instance.GetAllLevels();
			for (int i = 0; i < levels.Count; i++) 
			{
				UILevelCell levelCell = CreateWidgetByPrefab<UILevelCell>(m_itemLevelCell, m_rectContent);
				levelCell.SetConfig(levels[i]);
			}
		}
	}
	
	partial class UILevelWindow
	{
		#region 脚本工具生成的代码
		private GameObject m_itemLevelCell;
		private Transform m_tfLevelContainer;
		private ScrollRect m_scrollRectLevelScroll;
		private RectTransform m_rectContent;
		protected override void ScriptGenerator()
		{
			m_itemLevelCell = FindChild("bg/m_itemLevelCell").gameObject;
			m_tfLevelContainer = FindChild("bg/m_tfLevelContainer");
			m_scrollRectLevelScroll = FindChildComponent<ScrollRect>("bg/m_tfLevelContainer/m_scrollRectLevelScroll");
			m_rectContent = FindChildComponent<RectTransform>("bg/m_tfLevelContainer/m_scrollRectLevelScroll/Viewport/m_rectContent");
		}
		#endregion
	}

	
}