using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace TEngine.Editor
{
    /// <summary>
    /// 统一注册 Toolbar 右侧 GUI，固定绘制顺序为：资源模式 → 服务器类型 → 身份 ID（仅 Local），并做窄屏降级。
    /// </summary>
    [InitializeOnLoad]
    internal static class EditorToolbarRightCoordinator
    {
        static EditorToolbarRightCoordinator()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }

        static void OnToolbarGUI()
        {
            float viewWidth = EditorGUIUtility.currentViewWidth;
            float group = EditorToolbarRightLayout.Scale(EditorToolbarRightLayout.GroupSpacingDesign);

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            GUILayout.Space(group);

            bool drawResource = viewWidth >= EditorToolbarRightLayout.MinToolbarWidthForResource;
            if (drawResource)
            {
                bool useFullResourceLayout = viewWidth >= EditorToolbarRightLayout.MinToolbarWidthForResourceFull;
                float popupW = EditorToolbarRightLayout.Scale(
                    useFullResourceLayout
                        ? EditorToolbarRightLayout.ResourcePopupWidthFullDesign
                        : EditorToolbarRightLayout.ResourcePopupWidthNarrowDesign);
                EditorResourceMode.DrawToolbarSection(popupW, useShortLabels: !useFullResourceLayout);
                GUILayout.Space(group);
            }

            EditorServerType.DrawToolbarSection();

            bool drawIdentity = viewWidth >= EditorToolbarRightLayout.MinToolbarWidthForIdentity
                                && EditorIdentityKey.ShouldDrawIdentitySection();
            if (drawIdentity)
            {
                GUILayout.Space(group);
                EditorIdentityKey.DrawToolbarSection();
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}
