using Cysharp.Threading.Tasks;
using GameConfig;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public partial class UIRewardItem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        public void SetData(EItemType itemType, int itemId, int amount)
        {
            m_txtCount.text = $"+{amount}";
            LoadIcon(itemType, itemId).Forget();
        }

        private async UniTaskVoid LoadIcon(EItemType itemType, int itemId)
        {
            string iconPath = itemType switch
            {
                EItemType.RESOURCE => ResourceDef.GetIconPath((ResourceType)itemId),
                EItemType.GOODS => GoodsDef.GetIconPath(itemId),
                _ => null
            };
            if (string.IsNullOrEmpty(iconPath)) return;

            var sprite = await GameModule.Resource.LoadAssetAsync<Sprite>(iconPath);
            if (sprite != null && m_imgItem != null)
            {
                m_imgItem.sprite = sprite;
            }
        }
    }

#region 脚本工具生成的代码 === 复制开始 ===
	partial class UIRewardItem : UIWidget
	{
		private Image m_imgItem;
		private Text m_txtCount;

		protected override void ScriptGenerator()
		{
			m_imgItem = FindChildComponent<Image>("m_imgItem");
			m_txtCount = FindChildComponent<Text>("m_txtCount");
		}
	}
#endregion === 复制结束 ===
}
