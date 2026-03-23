using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
    partial class UILevelChestBtn
    {
        private const string ICON_CHEST = "Assets/AssetRaw/UIRaw/Atlas/Common/icon_chest.png";
        private const string ICON_CHEST_OPENED = "Assets/AssetRaw/UIRaw/Atlas/Common/icon_chest_opened.png";

        private LevelChestDisplayInfo _displayInfo;

        protected override void OnCreate()
        {
            base.OnCreate();
            EventTriggerListener.Get(gameObject).OnClick = go => OnClickClaim();
        }

        /// <summary>
        /// 刷新宝箱按钮状态
        /// </summary>
        public void RefreshState(LevelChestDisplayInfo displayInfo)
        {
            _displayInfo = displayInfo;
            
            if (displayInfo == null || displayInfo.config == null)
            {
                m_imgIcon.gameObject.SetActive(false);
                m_txtDesc.gameObject.SetActive(false);
                m_rectRedPoint.gameObject.SetActive(false);
                return;
            }

            m_imgIcon.gameObject.SetActive(true);
            m_txtDesc.gameObject.SetActive(true);
            m_txtDesc.text = displayInfo.config.LevelId.ToString();

            // 根据状态设置图标和红点
            switch (displayInfo.state)
            {
                case LevelChestState.Locked:
                    // 未解锁：不显示红点，显示 locked 图标
                    SetIcon(ICON_CHEST).Forget();
                    m_rectRedPoint.gameObject.SetActive(false);
                    break;

                case LevelChestState.Claimable:
                    // 可领取：显示红点，显示 icon_chest
                    SetIcon(ICON_CHEST).Forget();
                    m_rectRedPoint.gameObject.SetActive(true);
                    break;

                case LevelChestState.Claimed:
                    // 已领取：不显示红点，显示 icon_chest_opened
                    SetIcon(ICON_CHEST_OPENED).Forget();
                    m_rectRedPoint.gameObject.SetActive(false);
                    break;
            }

            Log.Info($"[UILevelChestBtn] RefreshState: level={displayInfo.config.LevelId}, state={displayInfo.state}");
        }

        private void OnClickClaim()
        {
            if (_displayInfo == null || _displayInfo.config == null) return;
            if (_displayInfo.state != LevelChestState.Claimable) return;

            int chestLevelId = _displayInfo.config.LevelId;
            Log.Info($"[UILevelChestBtn] OnClickClaim: chestLevelId={chestLevelId}");

            LevelChestManager.Instance.ClaimChest(chestLevelId).ContinueWith(success =>
            {
                if (success)
                {
                    // 刷新状态
                    RefreshState(new LevelChestDisplayInfo
                    {
                        config = _displayInfo.config,
                        state = LevelChestState.Claimed
                    });

                    // 显示奖励弹窗
                    var rewardParam = LevelChestManager.Instance.GetRewardPreview(chestLevelId);
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
	partial class UILevelChestBtn : UIWidget
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
