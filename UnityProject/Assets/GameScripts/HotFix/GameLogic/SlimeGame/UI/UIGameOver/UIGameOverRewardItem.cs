using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public partial class UIGameOverRewardItem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        public void SetData(ResourceType type, int amount)
        {
            m_txtCount.text = $"+{amount}";
            LoadIcon(type).Forget();
        }

        private async UniTaskVoid LoadIcon(ResourceType type)
        {
            string iconPath = ResourceDef.GetIconPath(type);
            if (string.IsNullOrEmpty(iconPath)) return;

            var sprite = await GameModule.Resource.LoadAssetAsync<Sprite>(iconPath);
            if (sprite != null && m_imgItem != null)
            {
                m_imgItem.sprite = sprite;
            }
        }
    }

#region 脚本工具生成的代码 === 复制开始 ===
	partial class UIGameOverRewardItem : UIWidget
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
