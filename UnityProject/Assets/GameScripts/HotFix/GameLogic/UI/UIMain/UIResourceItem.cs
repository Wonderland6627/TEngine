using System;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public partial class UIResourceItem
    {
        private string _bindEvent;
        private Func<int> _valueGetter;
        private Func<int, string> _formatter;

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        /// <summary>
        /// 初始化资源项，注入数据绑定
        /// </summary>
        /// <param name="eventName">监听的数据变化事件</param>
        /// <param name="valueGetter">获取当前值的回调</param>
        /// <param name="formatter">数值格式化（可选，默认 ToString）</param>
        public void Init(string eventName, Func<int> valueGetter, Func<int, string> formatter = null)
        {
            _bindEvent = eventName;
            _valueGetter = valueGetter;
            _formatter = formatter;

            RefreshDisplay(_valueGetter());
            GameEvent.AddEventListener<int>(_bindEvent, RefreshDisplay);
        }

        private void RefreshDisplay(int value)
        {
            m_txtResource.text = _formatter != null ? _formatter(value) : value.ToString();
        }

        protected override void OnDestroy()
        {
            if (_bindEvent != null)
                GameEvent.RemoveEventListener<int>(_bindEvent, RefreshDisplay);
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
