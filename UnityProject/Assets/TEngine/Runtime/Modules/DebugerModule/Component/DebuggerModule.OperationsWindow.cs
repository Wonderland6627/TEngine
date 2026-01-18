using System;
using UnityEngine;

namespace TEngine
{
    public sealed partial class DebuggerModule : Module
    {
        private sealed class OperationsWindow : ScrollableDebuggerWindowBase
        {
            // 服务器切换回调
            private static Action<int> _onServerSwitch;
            private static Func<int> _getCurrentServer;
            private static Func<string[]> _getServerNames;

            /// <summary>
            /// 注册服务器切换功能
            /// </summary>
            /// <param name="getServerNames">获取服务器名称列表</param>
            /// <param name="getCurrentServer">获取当前服务器索引</param>
            /// <param name="onSwitch">切换服务器回调</param>
            public static void RegisterServerSwitch(Func<string[]> getServerNames, Func<int> getCurrentServer, Action<int> onSwitch)
            {
                _getServerNames = getServerNames;
                _getCurrentServer = getCurrentServer;
                _onServerSwitch = onSwitch;
            }

            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Operations</b>");
                GUILayout.BeginVertical("box");
                {
                    // 服务器切换
                    if (_getServerNames != null && _onServerSwitch != null)
                    {
                        GUILayout.Label("<b>Server Environment</b>");
                        string[] serverNames = _getServerNames();
                        int currentIndex = _getCurrentServer?.Invoke() ?? 0;
                        int newIndex = GUILayout.Toolbar(currentIndex, serverNames, GUILayout.Height(30f));
                        if (newIndex != currentIndex)
                        {
                            _onServerSwitch(newIndex);
                        }
                        GUILayout.Space(10f);
                    }

                    ObjectPoolModule objectPoolModule = ModuleSystem.GetModule<ObjectPoolModule>();
                    if (objectPoolModule != null)
                    {
                        if (GUILayout.Button("Object Pool Release", GUILayout.Height(30f)))
                        {
                            objectPoolModule.Release();
                        }

                        if (GUILayout.Button("Object Pool Release All Unused", GUILayout.Height(30f)))
                        {
                            objectPoolModule.ReleaseAllUnused();
                        }
                    }

                    ResourceModule resourceModule = ModuleSystem.GetModule<ResourceModule>();
                    if (resourceModule != null)
                    {
                        if (GUILayout.Button("Unload Unused Assets", GUILayout.Height(30f)))
                        {
                            resourceModule.ForceUnloadUnusedAssets(false);
                        }

                        if (GUILayout.Button("Unload Unused Assets and Garbage Collect", GUILayout.Height(30f)))
                        {
                            resourceModule.ForceUnloadUnusedAssets(true);
                        }
                    }

                    if (GUILayout.Button("Shutdown Game Framework (None)", GUILayout.Height(30f)))
                    {
                        ModuleSystem.Shutdown(ShutdownType.None);
                    }
                    if (GUILayout.Button("Shutdown Game Framework (Restart)", GUILayout.Height(30f)))
                    {
                        ModuleSystem.Shutdown(ShutdownType.Restart);
                    }
                    if (GUILayout.Button("Shutdown Game Framework (Quit)", GUILayout.Height(30f)))
                    {
                        ModuleSystem.Shutdown(ShutdownType.Quit);
                    }
                }
                GUILayout.EndVertical();
            }
        }
    }
}
