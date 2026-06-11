using UnityEngine;

namespace GameLogic
{
    public static class FactionUtil
    {
        public const int MaxFactionCount = 5;

        public static bool IsPlayer(UnitType type) => type == UnitType.Player;

        public static bool IsEnemy(UnitType a, UnitType b) => a != b;

        /// ---@summary 获取阵营对应的颜色
        public static Color GetFactionColor(UnitType type) => type switch
        {
            UnitType.Player  => new Color32(0, 120, 255, 255),
            UnitType.Enemy_1 => new Color32(220, 50, 50, 255),
            UnitType.Enemy_2 => new Color32(50, 200, 80, 255),
            UnitType.Enemy_3 => new Color32(150, 60, 220, 255),
            UnitType.Enemy_4 => new Color32(240, 150, 30, 255),
            _ => Color.white,
        };

        /// ---@summary 获取阵营简称（用于调试）
        public static string GetFactionFlag(UnitType type) => type switch
        {
            UnitType.Player  => "P",
            UnitType.Enemy_1 => "E1",
            UnitType.Enemy_2 => "E2",
            UnitType.Enemy_3 => "E3",
            UnitType.Enemy_4 => "E4",
            _ => "?",
        };

        /// ---@summary 获取所有已定义的阵营类型数组
        public static readonly UnitType[] AllFactions = new[]
        {
            UnitType.Player,
            UnitType.Enemy_1,
            UnitType.Enemy_2,
            UnitType.Enemy_3,
            UnitType.Enemy_4,
        };
    }
}
