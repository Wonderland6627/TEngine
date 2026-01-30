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
        
        public static string OnAdsResultReceived = "OnAdsResultReceived";
        
        public static string OnCoinChanged = "OnCoinChanged";  // 金币变化事件
        
        public static string OnEnergyChanged = "OnEnergyChanged";  // 体力值变化事件
        
        public static string OnEnergyNotEnough = "OnEnergyNotEnough";  // 体力值不足事件
    }

    // 广告类型枚举
    public enum AdsType
    {
        Reward,     // 锦囊奖励广告
        RefreshRewardsList,    // 刷新按钮广告
    }
    
    // 广告事件参数
    public class AdsEventParam
    {
        public bool isCompleted; // 是否完成观看
        public AdsType adsType; // 广告类型
        public object userData; // 用户自定义数据
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
