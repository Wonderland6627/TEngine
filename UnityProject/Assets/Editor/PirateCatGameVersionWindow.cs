using UnityEditor;
using UnityEngine;

/// <summary>
/// 微信小游戏打包工具窗口（分步骤操作）
/// </summary>
public class PirateCatGameVersionWindow : EditorWindow
{
    private string version = "v0.1.7.2";
    private bool hasResourceChanged = false;
    
    [MenuItem("PirateCat/打包工具")]
    private static void ShowWindow()
    {
        var window = GetWindow<PirateCatGameVersionWindow>();
        window.titleContent = new GUIContent("微信小游戏打包工具");
        window.minSize = new Vector2(450, 500);
        window.Show();
    }
    
    private void OnEnable()
    {
        // 加载当前版本号
        LoadCurrentVersion();
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        
        // 标题
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("微信小游戏打包工具", titleStyle);
        
        EditorGUILayout.Space(20);
        
        // ========== 步骤1: 修改版本号 ==========
        EditorGUILayout.LabelField("步骤1: 修改版本号", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("版本号:", GUILayout.Width(80));
        version = EditorGUILayout.TextField(version);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(version));
        if (GUILayout.Button("确认并更新版本号", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("确认更新", 
                $"确定要将版本号更新为 {version} 吗？\n\n这将更新以下文件：\n- YooAssetSettings.asset\n- TEngineGlobalSettings.asset\n- MiniGameConfig.asset", 
                "确定", "取消"))
            {
                PirateCatEditorTools.UpdateVersion(version);
                LoadCurrentVersion(); // 重新加载以显示更新后的值
            }
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        
        // ========== 步骤2: 构建 Bundle ==========
        EditorGUILayout.LabelField("步骤2: 构建 AssetBundle", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("是否修改了资源:", GUILayout.Width(120));
        hasResourceChanged = EditorGUILayout.Toggle(hasResourceChanged);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUI.BeginDisabledGroup(!hasResourceChanged || string.IsNullOrEmpty(version));
        if (GUILayout.Button("打开 AssetBundle Builder", GUILayout.Height(30)))
        {
            PirateCatEditorTools.BuildBundle(version);
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space(3);
        GUIStyle helpStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10,
            wordWrap = true,
            normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
        };
        if (!hasResourceChanged)
        {
            EditorGUILayout.LabelField("提示: 如果只修改了代码，可以跳过此步骤", helpStyle);
        }
        else
        {
            EditorGUILayout.LabelField("提示: 点击按钮将打开 AssetBundle Builder 窗口，请在窗口中完成构建", helpStyle);
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        
        // ========== 步骤3: 导出微信小游戏 ==========
        EditorGUILayout.LabelField("步骤3: 导出微信小游戏", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        if (GUILayout.Button("调用微信转换工具", GUILayout.Height(30)))
        {
            PirateCatEditorTools.CallWeChatTransformTool();
        }
        
        EditorGUILayout.Space(3);
        GUIStyle helpStyle2 = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10,
            wordWrap = true,
            normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
        };
        EditorGUILayout.LabelField("提示: 点击按钮后会打开微信转换工具面板，请在面板中完成转换", helpStyle2);
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        
        // ========== 步骤4: 更新 project.config.json 和复制 cloudfunctions ==========
        EditorGUILayout.LabelField("步骤4: 更新 project.config.json 和复制 cloudfunctions", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        if (GUILayout.Button("更新配置并复制云函数", GUILayout.Height(30)))
        {
            if (PirateCatEditorTools.UpdateProjectConfigJson())
            {
                EditorUtility.DisplayDialog("成功", 
                    "操作完成！\n\n" +
                    "✓ project.config.json 已更新，已添加 cloudfunctionRoot 配置\n" +
                    "✓ cloudfunctions 文件夹已复制到 WXExport/minigame/ 目录", 
                    "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "更新失败或文件不存在，请确保已完成微信小游戏转换（步骤3）", "确定");
            }
        }
        
        EditorGUILayout.Space(3);
        GUIStyle helpStyle4 = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10,
            wordWrap = true,
            normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
        };
        EditorGUILayout.LabelField("提示: 在完成微信转换后（步骤3），点击此按钮：", helpStyle4);
        EditorGUILayout.LabelField("  • 更新 project.config.json，添加 cloudfunctionRoot 配置", helpStyle4);
        EditorGUILayout.LabelField("  • 将项目根目录下的 cloudfunctions 文件夹复制到 WXExport/minigame/ 目录", helpStyle4);
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        
        // ========== 步骤5: 复制到备份目录 ==========
        EditorGUILayout.LabelField("步骤5: 复制到备份目录", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(version));
        if (GUILayout.Button("复制到 CDN_Backup", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("确认复制",
                $"确定要将文件复制到备份目录吗？\n\n版本号: {version}\n" +
                $"资源修改: {(hasResourceChanged ? "是（将复制 StreamingAssets 和 bin.txt）" : "否（只复制 bin.txt）")}",
                "确定", "取消"))
            {
                string backupPath = System.IO.Path.Combine("CDN_Backup", "MiniGame", version);
                string fullPath = System.IO.Path.GetFullPath(backupPath);
                if (System.IO.Directory.Exists(fullPath))
                {
                    EditorUtility.RevealInFinder(fullPath);
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", $"目录不存在: {fullPath}", "确定");
                }
                
                PirateCatEditorTools.CopyToBackupDirectory(version, hasResourceChanged);
            }
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space(3);
        GUIStyle helpStyle5 = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10,
            wordWrap = true,
            normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
        };
        EditorGUILayout.LabelField("提示: 文件将从 WXExport/webgl/ 复制到 CDN_Backup/MiniGame/{版本号}/", helpStyle5);
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(20);
        
        // ========== 当前配置信息 ==========
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("当前配置:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("YooAsset版本:", GUILayout.Width(120));
        EditorGUILayout.LabelField(GetCurrentYooAssetVersion());
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("CDN地址:", GUILayout.Width(120));
        EditorGUILayout.LabelField(GetCurrentCDNUrl(), EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// 加载当前版本号
    /// </summary>
    private void LoadCurrentVersion()
    {
        try
        {
            var settings = AssetDatabase.LoadAssetAtPath<YooAsset.YooAssetSettings>(
                "Assets/TEngine/AssetSetting/Resources/YooAssetSettings.asset");
            if (settings != null)
            {
                version = settings.BuildVersion;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PirateCatGameVersionWindow] Failed to load current version: {e.Message}");
        }
    }
    
    /// <summary>
    /// 获取当前 YooAsset 版本号
    /// </summary>
    private string GetCurrentYooAssetVersion()
    {
        try
        {
            var settings = AssetDatabase.LoadAssetAtPath<YooAsset.YooAssetSettings>(
                "Assets/TEngine/AssetSetting/Resources/YooAssetSettings.asset");
            return settings != null ? settings.BuildVersion : "未知";
        }
        catch
        {
            return "未知";
        }
    }
    
    /// <summary>
    /// 获取当前 CDN 地址
    /// </summary>
    private string GetCurrentCDNUrl()
    {
        try
        {
            var settings = AssetDatabase.LoadAssetAtPath<TEngineSettings>(
                "Assets/TEngine/ResRaw/Resources/TEngineGlobalSettings.asset");
            if (settings != null)
            {
                return settings.FrameworkGlobalSettings.ResourcesArea.InnerResourceSourceUrl;
            }
        }
        catch
        {
            // Ignore
        }
        return "未知";
    }
    
}