using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace GameLogic
{
    [Window(UILayer.UI, fullScreen: true)]
    partial class UIMainWindow
    {
        private UILevelView levelView;
        private UIResourcesBar resourcesBar;
        private UIMainBottomTab currentSelectedTab;

        protected override void OnCreate()
        {
            base.OnCreate();

            resourcesBar = CreateWidget<UIResourcesBar>(m_rectResourcesBar.gameObject);
            levelView = CreateWidget<UILevelView>(m_itemLevelView);
            
            InitBottomTabs();
            
            m_txtVersion.text = $"Version: {GameModule.Resource.GetPackageVersion()} {World.Instance.GameData.UserInfo.nickName}";
            GameEvent.AddEventListener<UserInfo>(SlimeEvent.OnUserInfoUpdate, OnUserInfoUpdate);
            GameModule.Audio.Play(TEngine.AudioType.Music, "BGM", true, 0.5f, true);
        }

        private void InitBottomTabs()
        {
            var atlasTab = CreateWidget<UIMainBottomTab>(m_itemBottomTab_Atlas);
            var levelTab = CreateWidget<UIMainBottomTab>(m_itemBottomTab_Level);
            var lotteryTab = CreateWidget<UIMainBottomTab>(m_itemBottomTab_Lottery);

            levelTab.bindedWidget = levelView;
            levelTab.onClick = SwitchBottomTab;
            atlasTab.onClick = SwitchBottomTab;
            lotteryTab.onClick = SwitchBottomTab;

            SwitchBottomTab(levelTab);
        }

        private void SwitchBottomTab(UIMainBottomTab tab)
        {
            if (tab == currentSelectedTab) return;

            if (currentSelectedTab != null)
            {
                currentSelectedTab.SetSelected(false);
            }

            currentSelectedTab = tab;
            currentSelectedTab.SetSelected(true);
        }

        private void OnUserInfoUpdate(UserInfo userInfo)
        {
            Log.Info($"[UIMainWindow] trigger OnUserInfoUpdate: {GameModule.Resource.GetPackageVersion()} {World.Instance.GameData.UserInfo.nickName}");
            if (m_txtVersion != null)
            {
                m_txtVersion.text = $"Version: {GameModule.Resource.GetPackageVersion()} {World.Instance.GameData.UserInfo.nickName}";
            }
        }

        protected override void OnDestroy()
        {
            GameEvent.RemoveEventListener<UserInfo>(SlimeEvent.OnUserInfoUpdate, OnUserInfoUpdate);
            base.OnDestroy();
        }
    }
}
