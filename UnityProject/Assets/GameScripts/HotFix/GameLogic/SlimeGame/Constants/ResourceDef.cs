namespace GameLogic
{
    /// <summary>
    /// 资源静态信息查询（数据来源：Luban TbResource 表）
    /// </summary>
    public static class ResourceDef
    {
        public static GameConfig.Resource GetInfo(ResourceType type)
        {
            return ConfigSystem.Instance.Tables.TbResource.GetOrDefault((int)type);
        }

        public static string GetName(ResourceType type)
        {
            return GetInfo(type)?.Name ?? type.ToString();
        }

        public static string GetIconPath(ResourceType type)
        {
            return GetInfo(type)?.IconPath ?? "";
        }

        public static int GetDefaultValue(ResourceType type)
        {
            return GetInfo(type)?.DefaultValue ?? 0;
        }
    }
}
