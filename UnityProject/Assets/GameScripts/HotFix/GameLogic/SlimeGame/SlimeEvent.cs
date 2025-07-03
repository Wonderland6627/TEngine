using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public class SlimeEvent
    {
        public static string OnUserInfoUpdate = "OnUserInfoUpdate";
        
        public static string OnGameOver = "OnGameOver";

        public static string OnRewardSelect = "OnRewardSelect";

        public static string OnGetWXFont = "OnGetWXFont";

        public static string OnKnownTutorial = "OnKnownTutorial";
    }
    
    public class GameOverParam
    {
        public double duration;
        public UnitType winUnitType;

        public bool IsWin()
        {
            return winUnitType == UnitType.Player;  
        }

        public int GetStarCount()
        {
            if (!IsWin()) return 0;
            return 3;
        }
    }
}
