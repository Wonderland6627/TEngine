using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TEngine;

namespace GameLogic
{
    [Window(UILayer.UI, fullScreen: false)]
    partial class UIDebugWindow
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            m_tmpCoin.text = World.Instance.GameData.Coin.ToString();

            EventTriggerListener.Get(m_imgClose.gameObject).OnClick = go =>
            {
                Close();
            };
            EventTriggerListener.Get(m_btnAddCoin.gameObject).OnClick = go =>
            {
                OnAddCoin();
            };
            EventTriggerListener.Get(m_btnDeductCoin.gameObject).OnClick = go =>
            {
                OnDeductCoin();
            };
            GameEvent.AddEventListener<ResourceChangedParam>(SlimeEvent.OnResourceChanged, OnResourceChanged);
        }

        private void OnResourceChanged(ResourceChangedParam param)
        {
            if (param.resourceType != ResourceType.Coin) return;
            m_tmpCoin.text = param.newValue.ToString();
        }

        private async void OnAddCoin()
        {
            await World.Instance.UpdateResource(ResourceType.Coin, 100, "debug_add_coin");
        }
        private async void OnDeductCoin()
        {
            await World.Instance.UpdateResource(ResourceType.Coin, -100, "debug_deduct_coin");
        }

        protected override void OnDestroy()
        {
            GameEvent.RemoveEventListener<ResourceChangedParam>(SlimeEvent.OnResourceChanged, OnResourceChanged);
            base.OnDestroy();
        }
    }
}
