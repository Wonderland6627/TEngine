using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TEngine;
using DG.Tweening;

namespace GameLogic
{
    partial class UITutorialTips : UIWidget
    {
        private Tweener m_tween;

        public void DoScale()
        {
            m_imgHand.transform.localScale = Vector3.one;
            m_tween = m_imgHand.transform.DOScale(0.8f, 0.5f).SetEase(Ease.InOutQuad)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void StopScale()
        {
            m_tween?.Kill();
            m_tween = null;
        }
    }

    partial class UITutorialTips
    {
        #region 脚本工具生成的代码
		private Image m_imgHand;
		protected override void ScriptGenerator()
		{
			m_imgHand = FindChildComponent<Image>("m_imgHand");
		}
		#endregion
    }
}
