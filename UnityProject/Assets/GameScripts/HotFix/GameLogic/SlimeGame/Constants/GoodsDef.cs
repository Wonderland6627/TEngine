namespace GameLogic
{
    /// <summary>
    /// 物品静态信息查询（数据来源：Luban TbGoods 表）
    /// </summary>
    public static class GoodsDef
    {
        public static GameConfig.Goods GetInfo(int goodsId)
        {
            return ConfigSystem.Instance.Tables.TbGoods.GetOrDefault(goodsId);
        }

        public static string GetName(int goodsId)
        {
            return GetInfo(goodsId)?.Name ?? $"goods({goodsId})";
        }

        public static string GetIconPath(int goodsId)
        {
            return GetInfo(goodsId)?.IconPath ?? "";
        }
    }
}
