using UnityEngine;
using UnityEngine.UI;
using TEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace GameLogic
{
    [Window(UILayer.UI, fullScreen: false)]
    partial class UIGetRewardWindow
    {
        private const string REWARD_ITEM_PATH = "Assets/AssetRaw/Prefabs/UI/Widget/UIReward/m_itemRewardItem.prefab";

        protected override void OnCreate()
        {
            base.OnCreate();
        }
    }
}
