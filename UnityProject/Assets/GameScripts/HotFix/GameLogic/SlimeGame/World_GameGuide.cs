using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public partial class World
    {
        private void TryStartGameGuide()
        {
            if (GameData.GuideFinish) return;
            if (playingLevelId > 1) return;

            Log.Info("[World] TryStartGameGuide");
        }

        private void ClearGameGuide()
        {
            Log.Info("[World] ClearGameGuide");
        }
    }
}
