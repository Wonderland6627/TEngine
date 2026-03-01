using System;
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

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
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }

        static void OnToolbarGUI()
        {
            if (PlayerPrefs.GetInt(KEY_SERVER_TYPE, 0) != SERVER_TYPE_LOCAL) return;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                GUILayout.Space(-150);
                GUILayout.FlexibleSpace();

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

                GUILayout.Label("ID:", _labelStyle, GUILayout.Width(20));
                string newKey = EditorGUILayout.TextField(_identityKey, _textFieldStyle, GUILayout.Width(80));

                if (newKey != _identityKey)
                {
                    _identityKey = newKey;
                    EditorPrefs.SetString(PREF_KEY, newKey);
                    Debug.Log($"[EditorIdentityKey] Identity key changed: {newKey}");
                }

                GUILayout.FlexibleSpace();
                GUILayout.Space(400);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
