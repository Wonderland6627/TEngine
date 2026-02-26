using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace TEngine.Editor
{
    /// <summary>
    /// 编辑器工具栏服务器类型切换器
    /// 通过PlayerPrefs与运行时NetManager共享KEY_SERVER_TYPE
    /// </summary>
    [InitializeOnLoad]
    public class EditorServerType
    {
        private const string KEY_SERVER_TYPE = "NetManager_ServerType";
        private const float POLL_INTERVAL = 1f;

        private static readonly string[] ServerTypeNames =
        {
            "Server: Local (本地服)",
            "Server: Dev (测试服)",
            "Server: Production (正式服)"
        };

        private static int _serverTypeIndex;
        private static double _lastPollTime;
        static GUIStyle _popupStyle;

        static EditorServerType()
        {
            _serverTypeIndex = PlayerPrefs.GetInt(KEY_SERVER_TYPE, 0);
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }

        static void OnToolbarGUI()
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                GUILayout.Space(-375);
                GUILayout.FlexibleSpace();

                _popupStyle ??= new GUIStyle("Tab middle")
                {
                    padding = new RectOffset(2, 8, 2, 2),
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };

                double now = EditorApplication.timeSinceStartup;
                if (now - _lastPollTime > POLL_INTERVAL)
                {
                    _lastPollTime = now;
                    _serverTypeIndex = PlayerPrefs.GetInt(KEY_SERVER_TYPE, _serverTypeIndex);
                }

                int selectedIndex = EditorGUILayout.Popup(
                    "", _serverTypeIndex, ServerTypeNames, _popupStyle, GUILayout.Width(180));

                if (selectedIndex != _serverTypeIndex)
                {
                    Debug.Log($"[EditorServerType] Server switched: {ServerTypeNames[selectedIndex]}");
                    _serverTypeIndex = selectedIndex;
                    PlayerPrefs.SetInt(KEY_SERVER_TYPE, selectedIndex);
                    PlayerPrefs.Save();
                }

                GUILayout.FlexibleSpace();
                GUILayout.Space(400);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
