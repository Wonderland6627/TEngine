using System;
using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    partial class UILevelCell : UIWidget
    {
        public LevelConfig config;

        protected override void OnCreate()
        {
            base.OnCreate();
            Log.Debug("UILevelCell OnCreate");
            EventTriggerListener.Get(gameObject).OnClick = OnCellClick;
        }

        public void SetConfig(LevelConfig config)
        {
            this.config = config;
            m_textLevelTxt.text = config.levelId.ToString();
        }

        private void OnCellClick(GameObject go) 
        {
            GameModule.UI.ShowUIAsync<UIMainWindow>();
            Log.Debug($"{go.name}");
        }
    }

    partial class UILevelCell
    {
		#region 脚本工具生成的代码
		private Text m_textLevelTxt;
		protected override void ScriptGenerator()
		{
			m_textLevelTxt = FindChildComponent<Text>("m_textLevelTxt");
		}
		#endregion
    }
}
