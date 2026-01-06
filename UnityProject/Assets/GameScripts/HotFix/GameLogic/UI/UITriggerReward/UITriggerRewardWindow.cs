using UnityEngine;
using UnityEngine.UI;
using TEngine;
using UnityEngine.Events;

namespace GameLogic
{
	[Window(UILayer.UI, fullScreen: false)]
	partial class UITriggerRewardWindow
	{
		private UnityAction closeAction;
		
		protected override void OnCreate()
		{
			base.OnCreate();
			
			EventTriggerListener.Get(m_imgBG.gameObject).OnClick = go =>
			{
				Close();
			};
        }

        protected override void OnDestroy()
        {
			closeAction?.Invoke();
            base.OnDestroy();
        }
	}
}

