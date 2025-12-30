using UnityEngine;
using UnityEditor;

namespace GameLogic.Editor
{
    /// <summary>
    /// 游戏配置编辑器工具
    /// </summary>
    public class GameConfigEditor
    {
        [MenuItem("PirateCat/创建游戏配置")]
        public static void CreateGameConfig()
        {
            // 检查配置是否已存在
            string configPath = "Assets/AssetRaw/Configs/GameConfig.asset";
            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(configPath);
            
            if (config != null)
            {
                EditorUtility.DisplayDialog("提示", "游戏配置文件已存在！", "确定");
                Selection.activeObject = config;
                return;
            }
            
            // 创建配置实例
            config = ScriptableObject.CreateInstance<GameConfig>();
            
            // 确保目录存在
            string directory = System.IO.Path.GetDirectoryName(configPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            // 保存配置
            AssetDatabase.CreateAsset(config, configPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // 选中新创建的配置
            Selection.activeObject = config;
            EditorUtility.DisplayDialog("成功", "游戏配置已创建！\n路径: " + configPath, "确定");
            
            Debug.Log($"[GameConfigEditor] Created config at: {configPath}");
        }
    }
}
