using System.Collections;
using System.Collections.Generic;
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
            GameEvent.AddEventListener<int>(SlimeEvent.OnCoinChanged, OnCoinChanged);
        }
        private void OnCoinChanged(int coin)
        {
            m_tmpCoin.text = coin.ToString();
        }

        private void OnAddCoin()
        {
            CurrencyManager.Instance.AddCoin(100, "debug_add_coin");
        }
        private void OnDeductCoin()
        {
            CurrencyManager.Instance.DeductCoin(100, "debug_deduct_coin");
        }

        protected override void OnDestroy()
        {
            GameEvent.RemoveEventListener<int>(SlimeEvent.OnCoinChanged, OnCoinChanged);
            base.OnDestroy();
        }
    }
}
