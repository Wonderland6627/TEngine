using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 游戏配置
    /// 统一管理游戏内的各种参数配置
    /// </summary>
    public class VisibleGameConfig : ScriptableObject
    {
        [Header("消失效果配置")]
        [Tooltip("连锁消除间隔时间曲线\nX轴：0=第一个单位，1=最后一个单位\nY轴：间隔时间（秒），建议范围0.02-0.15")]
        public AnimationCurve disappearIntervalCurve = AnimationCurve.EaseInOut(0f, 0.15f, 1f, 0.02f);

        // 后续可以在这里添加更多游戏配置项
        // [Header("其他配置")]
        // public float someOtherValue = 1.0f;
    }
}
