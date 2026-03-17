using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
    partial class UIDailyChestBtn
    {
        private const string ICON_UNCLAIMED = "Assets/AssetRaw/UIRaw/Atlas/UIMain/icon_gift01.png";
        private const string ICON_CLAIMED = "Assets/AssetRaw/UIRaw/Atlas/UIMain/icon_gift01_opened.png";

        protected override void OnCreate()
        {
            base.OnCreate();
            EventTriggerListener.Get(gameObject).OnClick = go => OnClickClaim();
        }

        public void RefreshState()
        {
            bool claimed = DailyChestManager.Instance.IsTodayClaimed();
            m_rectRedPoint.gameObject.SetActive(!claimed);
            SetIcon(claimed ? ICON_CLAIMED : ICON_UNCLAIMED).Forget();
        }

        private void OnClickClaim()
        {
            if (DailyChestManager.Instance.IsTodayClaimed()) return;

            DailyChestManager.Instance.TryClaimToday().ContinueWith(rewardParam =>
            {
                if (rewardParam != null) 
                {
                    RefreshState();
                    GameModule.UI.ShowUIAsync<UIGetRewardWindow>(rewardParam);
                }
            }).Forget();
        }

        private async UniTaskVoid SetIcon(string path)
        {
            var sprite = await GameModule.Resource.LoadAssetAsync<Sprite>(path);
            if (sprite != null && m_imgIcon != null)
            {
                m_imgIcon.sprite = sprite;
            }
        }
    }

#region 脚本工具生成的代码 === 复制开始 ===
	partial class UIDailyChestBtn : UIWidget
	{
		private Image m_imgIcon;
		private Text m_txtDesc;
		private RectTransform m_rectRedPoint;

		protected override void ScriptGenerator()
		{
			m_imgIcon = FindChildComponent<Image>("m_imgIcon");
			m_txtDesc = FindChildComponent<Text>("m_txtDesc");
			m_rectRedPoint = FindChildComponent<RectTransform>("m_rectRedPoint");
		}
	}
#endregion === 复制结束 ===
}
