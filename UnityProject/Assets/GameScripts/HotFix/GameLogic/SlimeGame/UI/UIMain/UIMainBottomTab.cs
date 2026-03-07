using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public partial class UIMainBottomTab
    {
        public UIWidget bindedWidget;
        public System.Action<UIMainBottomTab> onClick;

        protected override void OnCreate()
        {
            base.OnCreate();
            EventTriggerListener.Get(gameObject).OnClick = go => onClick?.Invoke(this);
        }

        public void SetSelected(bool selected)
        {
            m_imgBg.gameObject.SetActive(!selected);
            if (bindedWidget != null)
            {
                bindedWidget.Visible = selected;
            }
        }
    }
    
#region 脚本工具生成的代码 === 复制开始 ===
	partial class UIMainBottomTab : UIWidget
	{
		private Image m_imgBg;
		private Image m_imgIcon;
		private Text m_txtName;

		protected override void ScriptGenerator()
		{
			m_imgBg = FindChildComponent<Image>("m_imgBg");
			m_imgIcon = FindChildComponent<Image>("m_imgIcon");
			m_txtName = FindChildComponent<Text>("m_txtName");
		}
	}
#endregion === 复制结束 ===
}
