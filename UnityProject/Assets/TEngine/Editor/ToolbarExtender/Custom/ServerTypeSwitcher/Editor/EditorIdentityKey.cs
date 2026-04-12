using System;
using UnityEditor;
using UnityEngine;

namespace TEngine.Editor
{
    /// <summary>
    /// 编辑器工具栏身份标识输入框
    /// 仅当ServerType为Local时显示，输入的key作为登录code发送给服务端以区分不同开发者
    /// </summary>
    [InitializeOnLoad]
    public class EditorIdentityKey
    {
        private const string PREF_KEY = "EditorIdentityKey";
        private const string KEY_SERVER_TYPE = "NetManager_ServerType";
        private const int SERVER_TYPE_LOCAL = 0;
        private const float POLL_INTERVAL = 1f;

        private static string _identityKey;
        private static double _lastPollTime;
        private static GUIStyle _labelStyle;
        private static GUIStyle _textFieldStyle;

        static EditorIdentityKey()
        {
            _identityKey = EditorPrefs.GetString(PREF_KEY, "");
            if (string.IsNullOrEmpty(_identityKey))
            {
                _identityKey = DateTime.Now.ToString("yyyyMMddHHmmss");
            }
        }

        /// <summary>
        /// Guard：仅 Local 服且由协调器结合窗口宽度决定是否绘制。
        /// </summary>
        internal static bool ShouldDrawIdentitySection()
        {
            return PlayerPrefs.GetInt(KEY_SERVER_TYPE, 0) == SERVER_TYPE_LOCAL;
        }

        /// <summary>
        /// 由 EditorToolbarRightCoordinator 调用；紧挨服务器下拉右侧绘制。
        /// </summary>
        internal static void DrawToolbarSection()
        {
            _labelStyle ??= new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                fontStyle = FontStyle.Bold
            };

            _textFieldStyle ??= new GUIStyle(EditorStyles.toolbarTextField)
            {
                alignment = TextAnchor.MiddleLeft
            };

            double now = EditorApplication.timeSinceStartup;
            if (now - _lastPollTime > POLL_INTERVAL)
            {
                _lastPollTime = now;
                _identityKey = EditorPrefs.GetString(PREF_KEY, "");
            }

            float labelW = EditorToolbarRightLayout.Scale(EditorToolbarRightLayout.IdentityLabelWidthDesign);
            float fieldW = EditorToolbarRightLayout.Scale(EditorToolbarRightLayout.IdentityFieldWidthDesign);

            GUILayout.Label("ID:", _labelStyle, GUILayout.Width(labelW));
            string newKey = EditorGUILayout.TextField(_identityKey, _textFieldStyle, GUILayout.Width(fieldW));

            if (newKey != _identityKey)
            {
                _identityKey = newKey;
                EditorPrefs.SetString(PREF_KEY, newKey);
                Debug.Log($"[EditorIdentityKey] Identity key changed: {newKey}");
            }
        }
    }
}
