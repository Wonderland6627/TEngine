using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    partial class UIFriendsRankPanel : UIWidget
    {
        override protected void OnCreate()
        {
            base.OnCreate();

            ShowFriendsRank();

            EventTriggerListener.Get(m_tfTapArea.gameObject).OnClick = go =>
            {
                Destroy();
            };
        }

        private void ShowFriendsRank()
        {
            World.Instance.ShowFriendsRank(m_rimgRankTex);
        }

        protected override void OnDestroy()
        {
            World.Instance.DestroyOpenDataRenderer();
            base.OnDestroy();
        }
    }

    [Window(UILayer.UI)]
	partial class UIFriendsRankPanel
	{
		#region 脚本工具生成的代码
		private Transform m_tfTapArea;
		private RawImage m_rimgRankTex;
		protected override void ScriptGenerator()
		{
			m_tfTapArea = FindChild("m_tfTapArea");
			m_rimgRankTex = FindChildComponent<RawImage>("m_rimgRankTex");
		}
		#endregion
	}
}
