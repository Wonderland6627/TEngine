using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public partial class World
    {
        private async void TryStartGameGuide()
        {
            // if (GameData.GuideFinish) return;
            if (playingLevelId != 1) return;

            GameModule.UI.ShowUIAsync<UIRuleTipsWindow>(true);

            Log.Info("[World] 1 castles.Count = " + castles.Count);
            await UniTask.WaitUntil(() => castles != null && castles.Count > 0);
            Log.Info("[World] 2 castles.Count = " + castles.Count);

            UIMainWindow mainWindow = await GameModule.UI.GetUIAsyncAwait<UIMainWindow>();
            if (mainWindow == null) return;
            mainWindow.ShowTutorialTips();

            Log.Info("[World] TryStartGameGuide");
        }

        private void ClearGameGuide()
        {
            Log.Info("[World] ClearGameGuide");
        }
    }
}
