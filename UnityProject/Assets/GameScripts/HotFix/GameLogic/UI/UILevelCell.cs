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
            EventTriggerListener.Get(gameObject).OnClick = OnCellClick;
        }

        public void SetConfig(LevelConfig config)
        {
            this.config = config;
            m_textLevelTxt.text = config.levelId.ToString();
            bool isLocked = config.levelId > World.Instance.gameData.UnlockedLevelId;
            SetLockState(isLocked);
        }

        private void SetLockState(bool isLock)
        {
            gameObject.GetComponent<Image>().color = isLock ? Color.gray : Color.white;
            EventTriggerListener.Get(gameObject).enabled = !isLock;
        }

        private void OnCellClick(GameObject go) 
        {
            int levelId = config.levelId;
            World.Instance.StartGame(levelId);
            GameModule.UI.ShowUIAsync<UIMainWindow>();
            Log.Debug($"[UILevelCell] OnCellClick [{levelId}]");
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
