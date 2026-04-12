using UnityEditor;

namespace TEngine.Editor
{
    /// <summary>
    /// 右侧工具栏自定义 GUI 的布局常量（以「设计像素」为基准，经 Scale 适配 Editor 缩放/DPI）。
    /// </summary>
    internal static class EditorToolbarRightLayout
    {
        /// <summary> GroupSpacing：相邻控件组之间的水平间距（设计像素）。 </summary>
        public const float GroupSpacingDesign = 6f;

        /// <summary> ServerPopupWidth：服务器类型下拉框宽度（设计像素）。 </summary>
        public const float ServerPopupWidthDesign = 180f;

        /// <summary> ResourcePopupWidthFull：资源模式下拉在「宽屏」下的宽度，可容纳长中文选项（设计像素）。 </summary>
        public const float ResourcePopupWidthFullDesign = 230f;

        /// <summary> ResourcePopupWidthNarrow：中等宽度下收缩后的下拉宽度（设计像素）。 </summary>
        public const float ResourcePopupWidthNarrowDesign = 130f;

        /// <summary> IdentityLabelWidth：「ID:」标签宽度（设计像素）。 </summary>
        public const float IdentityLabelWidthDesign = 22f;

        /// <summary> IdentityFieldWidth：身份测试 ID 输入框宽度（设计像素）。 </summary>
        public const float IdentityFieldWidthDesign = 100f;

        /// <summary>
        /// MinToolbarWidthForIdentity：视图宽度低于此值时不绘制身份区（Guard：优先保留服务器下拉）。
        /// </summary>
        public const float MinToolbarWidthForIdentity = 780f;

        /// <summary>
        /// MinToolbarWidthForResource：视图宽度低于此值时不绘制资源模式（Guard：先隐藏最低优先级的资源模式）。
        /// </summary>
        public const float MinToolbarWidthForResource = 1020f;

        /// <summary>
        /// MinToolbarWidthForResourceFull：高于此值使用完整资源模式文案与较宽下拉；否则用窄宽度+短标签。
        /// </summary>
        public const float MinToolbarWidthForResourceFull = 1280f;

        /// <summary>
        /// 将设计像素转换为当前 EditorGUIUtility.pixelsPerPoint 下的实际像素。
        /// </summary>
        public static float Scale(float designPixels)
        {
            return designPixels * EditorGUIUtility.pixelsPerPoint;
        }
    }
}
