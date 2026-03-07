using System.Collections.Generic;

namespace GameLogic
{
    /// <summary>
    /// 资源静态信息（客户端维护，用于 UI 显示）
    /// </summary>
    public class ResourceInfo
    {
        public string Name { get; }
        public string IconPath { get; }

        public ResourceInfo(string name, string iconPath)
        {
            Name = name;
            IconPath = iconPath;
        }
    }

    /// <summary>
    /// 资源定义（图标路径、名称等客户端显示信息）
    /// </summary>
    public static class ResourceDef
    {
        public static readonly Dictionary<ResourceType, ResourceInfo> Infos = new()
        {
            { ResourceType.Coin, new ResourceInfo("金币", "Assets/AssetRaw/UIRaw/Atlas/Common/icon_coin.png") },
            { ResourceType.Energy, new ResourceInfo("体力", "Assets/AssetRaw/UIRaw/Atlas/Common/icon_energy.png") },
            { ResourceType.Diamond, new ResourceInfo("钻石", "Assets/AssetRaw/UIRaw/Atlas/Common/icon_diamond.png") },
        };

        public static ResourceInfo GetInfo(ResourceType type)
        {
            return Infos.TryGetValue(type, out var info) ? info : null;
        }

        public static string GetName(ResourceType type)
        {
            return GetInfo(type)?.Name ?? type.ToString();
        }

        public static string GetIconPath(ResourceType type)
        {
            return GetInfo(type)?.IconPath ?? "";
        }
    }
}
