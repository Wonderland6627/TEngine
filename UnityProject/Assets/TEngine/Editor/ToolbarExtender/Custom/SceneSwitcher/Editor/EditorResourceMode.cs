using UnityEditor;
using UnityEngine;

namespace TEngine.Editor
{
    [InitializeOnLoad]
    public class EditorResourceMode
    {
        static class ToolbarStyles
        {
            public static readonly GUIStyle ToolBarButtonGuiStyle;

            static ToolbarStyles()
            {
                ToolBarButtonGuiStyle = new GUIStyle(ButtonStyleName)
                {
                    padding = new RectOffset(2, 8, 2, 2),
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
            }
        }

        static EditorResourceMode()
        {
            _playModeIndex = EditorPrefs.GetInt("EditorPlayMode", 0);
        }

        private const string ButtonStyleName = "Tab middle";

        private static readonly string[] _resourceModeNames =
        {
            "EditorMode (编辑器下的模拟模式)",
            "OfflinePlayMode (单机模式)",
            "HostPlayMode (联机运行模式)",
            "WebPlayMode (WebGL运行模式)"
        };

        /// <summary> 窄屏时与 _resourceModeNames 一一对应的短标签，避免下拉过宽。 </summary>
        private static readonly string[] _resourceModeNamesShort =
        {
            "EditorMode",
            "Offline",
            "Host",
            "Web"
        };

        private static int _playModeIndex = 0;
        public static int PlayModeIndex => _playModeIndex;

        /// <summary>
        /// 由 EditorToolbarRightCoordinator 调用；不在此注册 Toolbar。
        /// </summary>
        /// <param name="popupWidth">PopupWidth：下拉控件宽度（已含 DPI 缩放）。</param>
        /// <param name="useShortLabels">为 true 时使用短选项文案以适配窄宽度。</param>
        internal static void DrawToolbarSection(float popupWidth, bool useShortLabels)
        {
            string[] names = useShortLabels ? _resourceModeNamesShort : _resourceModeNames;

            // 资源模式
            int selectedIndex = EditorGUILayout.Popup(
                "", _playModeIndex, names, ToolbarStyles.ToolBarButtonGuiStyle, GUILayout.Width(popupWidth));
            // ReSharper disable once RedundantCheckBeforeAssignment
            if (selectedIndex != _playModeIndex)
            {
                Debug.Log($"更改编辑器资源运行模式 : {_resourceModeNames[selectedIndex]}");
                _playModeIndex = selectedIndex;
                EditorPrefs.SetInt("EditorPlayMode", selectedIndex);
            }
        }
    }
}
