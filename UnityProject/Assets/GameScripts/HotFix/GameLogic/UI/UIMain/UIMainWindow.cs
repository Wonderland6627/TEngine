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

        protected override void OnCreate()
        {
            base.OnCreate();

            levelView = CreateWidget<UILevelView>(m_itemLevelView);
            
            m_txtVersion.text = $"Version: {GameModule.Resource.GetPackageVersion()} {World.Instance.GameData.UserInfo.nickName}";

            GameEvent.AddEventListener<UserInfo>(SlimeEvent.OnUserInfoUpdate, OnUserInfoUpdate);
            
            GameModule.Audio.Play(TEngine.AudioType.Music, "BGM", true, 0.5f, true);
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
