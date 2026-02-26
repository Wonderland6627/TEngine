using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public partial class UIResourceItem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }
    }

#region 脚本工具生成的代码 === 复制开始 ===
	partial class UIResourceItem : UIWidget
	{
		private Image m_imgResource;
		private Text m_txtResource;

		protected override void ScriptGenerator()
		{
			m_imgResource = FindChildComponent<Image>("m_imgResource");
			m_txtResource = FindChildComponent<Text>("m_txtResource");
		}
	}
#endregion === 复制结束 ===
}
