using System;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public partial class UIResourceItem
    {
        private ResourceType _resourceType;
        private Func<int> _valueGetter;
        private Func<int, string> _formatter;

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        /// <summary>
        /// 初始化资源项，绑定指定 ResourceType 的变化事件
        /// </summary>
        public void Init(ResourceType resourceType, Func<int> valueGetter, Func<int, string> formatter = null)
        {
            _resourceType = resourceType;
            _valueGetter = valueGetter;
            _formatter = formatter;

            RefreshDisplay(_valueGetter());
            GameEvent.AddEventListener<ResourceChangedParam>(SlimeEvent.OnResourceChanged, OnResourceChanged);
        }

        private void OnResourceChanged(ResourceChangedParam param)
        {
            if (param.resourceType != _resourceType) return;
            RefreshDisplay(param.newValue);
        }

        private void RefreshDisplay(int value)
        {
            m_txtResource.text = _formatter != null ? _formatter(value) : value.ToString();
        }

        protected override void OnDestroy()
        {
            GameEvent.RemoveEventListener<ResourceChangedParam>(SlimeEvent.OnResourceChanged, OnResourceChanged);
            base.OnDestroy();
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
