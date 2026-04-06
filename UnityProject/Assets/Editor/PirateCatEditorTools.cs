using System.IO;
using UnityEditor;
using UnityEngine;
using YooAsset;

/// <summary>
/// 打包工具核心逻辑
/// </summary>
public static class PirateCatEditorTools
{
    // 配置文件路径
    private const string YooAssetSettingsPath = "Assets/TEngine/AssetSetting/Resources/YooAssetSettings.asset";
    private const string TEngineGlobalSettingsPath = "Assets/TEngine/ResRaw/Resources/TEngineGlobalSettings.asset";
    private const string MiniGameConfigPath = "Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset";
    
    // CDN配置
    private const string CDNBaseUrl = "https://a.unity.cn/client_api/v1/buckets/cde09f24-d39c-4845-a3e3-17344f4f2894/content/MiniGame/";
    
    // 备份目录
    private const string BackupBasePath = "CDN_Backup/MiniGame";
    
    /// <summary>
    /// 步骤1: 更新资源版本号（更新所有相关配置文件）
    /// </summary>
    /// <param name="resourceVersion">资源版本号（如 v0.1.7.3）</param>
    /// <param name="appVersion">App版本号（如 1.0.0），用于CDN地址拼接</param>
    /// <returns>是否成功</returns>
    public static bool UpdateVersion(string resourceVersion, string appVersion)
    {
        try
        {
            Debug.Log($"[PirateCatEditorTools] Step 1: Update resource version to {resourceVersion}");
            
            // 验证资源版本号格式
            if (string.IsNullOrEmpty(resourceVersion))
            {
                EditorUtility.DisplayDialog("错误", "资源版本号不能为空", "确定");
                return false;
            }
            
            // 验证App版本号格式
            if (string.IsNullOrEmpty(appVersion))
            {
                EditorUtility.DisplayDialog("错误", "App版本号不能为空", "确定");
                return false;
            }
            
            // 更新 YooAssetSettings.asset 的 BuildVersion（使用资源版本号）
            UpdateYooAssetVersion(resourceVersion);
            
            UpdateAppVersion(appVersion);
            // 更新 InnerResourceSourceUrl（使用App版本号拼接CDN地址）
            UpdateInnerResourceSourceUrl(appVersion);
            // 更新 MiniGameConfig.asset 的 CDN（使用App版本号拼接CDN地址）
            UpdateMiniGameConfigCDN(appVersion);
            
            Debug.Log($"[PirateCatEditorTools] Resource version updated successfully to {resourceVersion}, CDN uses app version {appVersion}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PirateCatEditorTools] Failed to update resource version: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("错误", $"更新资源版本号失败：{e.Message}", "确定");
            return false;
        }
    }
    
    /// <summary>
    /// 更新 App 版本号（更新 BuildSettings 中的 PlayerSettings.bundleVersion）
    /// </summary>
    /// <param name="appVersion">App版本号（如 1.0.0）</param>
    /// <returns>是否成功</returns>
    public static void UpdateAppVersion(string appVersion)
    {
        PlayerSettings.bundleVersion = appVersion;
        Debug.Log($"[PirateCatEditorTools] Update app version to {appVersion}");
    }
    
    /// <summary>
    /// 步骤2: 打开 AssetBundle Builder 窗口
    /// </summary>
    /// <returns>是否成功打开</returns>
    public static bool BuildBundle(string version)
    {
        try
        {
            Debug.Log($"[PirateCatEditorTools] Step 2: Open AssetBundle Builder window");
            
            // 打开 YooAsset 的 AssetBundle Builder 窗口
            YooAsset.Editor.AssetBundleBuilderWindow.OpenWindow();
            
            Debug.Log($"[PirateCatEditorTools] AssetBundle Builder window opened successfully");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PirateCatEditorTools] Failed to open AssetBundle Builder window: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("错误", $"打开 AssetBundle Builder 窗口失败：{e.Message}", "确定");
            return false;
        }
    }
    
    /// <summary>
    /// 步骤3: 调用微信小游戏转换工具
    /// </summary>
    /// <returns>是否成功调用</returns>
    public static bool CallWeChatTransformTool()
    {
        try
        {
            Debug.Log("[PirateCatEditorTools] Step 3: Call WeChat Transform Tool");
            
            // 通过菜单项调用
            EditorApplication.ExecuteMenuItem("微信小游戏/转换小游戏");
            
            Debug.Log("[PirateCatEditorTools] WeChat transform tool called successfully");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PirateCatEditorTools] Failed to call WeChat transform tool: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("错误", $"调用微信转换工具失败：{e.Message}", "确定");
            return false;
        }
    }
    
    /// <summary>
    /// 步骤4: 复制文件到 Backup 目录
    /// 按照 YooAsset 官方资源部署文档，备份目录结构与 CDN 扁平结构 1:1 映射
    /// </summary>
    /// <param name="appVersion">App版本号，对应 CDN 目录名</param>
    /// <param name="resourceVersion">资源版本号，对应 YooAsset 构建输出子目录</param>
    /// <param name="hasResourceChanged">是否修改了资源</param>
    /// <returns>是否成功</returns>
    public static bool CopyToBackupDirectory(string appVersion, string resourceVersion, bool hasResourceChanged)
    {
        try
        {
            Debug.Log($"[PirateCatEditorTools] Step 4: Copy files to backup directory (app={appVersion}, res={resourceVersion})");
            
            CopyToBackup(appVersion, resourceVersion, hasResourceChanged);
            
            string backupPath = Path.Combine(BackupBasePath, appVersion);
            Debug.Log($"[PirateCatEditorTools] Files copied to backup directory successfully: {backupPath}");
            EditorUtility.DisplayDialog("成功", $"文件已复制到备份目录：{backupPath}\n\n该目录内容可直接上传到 CDN 的 MiniGame/{appVersion}/ 路径下", "确定");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PirateCatEditorTools] Failed to copy files: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("错误", $"复制文件失败：{e.Message}", "确定");
            return false;
        }
    }
    
    /// <summary>
    /// 更新 YooAssetSettings 的版本号
    /// </summary>
    public static void UpdateYooAssetVersion(string version)
    {
        Debug.Log($"[PirateCatEditorTools] Update YooAssetSettings version to {version}");
        
        var settings = AssetDatabase.LoadAssetAtPath<YooAssetSettings>(YooAssetSettingsPath);
        if (settings == null)
        {
            throw new System.Exception($"Failed to load YooAssetSettings from {YooAssetSettingsPath}");
        }
        
        settings.BuildVersion = version;
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    /// <summary>
    /// 更新 InnerResourceSourceUrl（使用App版本号拼接CDN地址）
    /// </summary>
    /// <param name="appVersion">App版本号（如 1.0.0）</param>
    public static void UpdateInnerResourceSourceUrl(string appVersion)
    {
        Debug.Log($"[PirateCatEditorTools] Update InnerResourceSourceUrl with app version {appVersion}");
        
        var settings = AssetDatabase.LoadAssetAtPath<TEngineSettings>(TEngineGlobalSettingsPath);
        if (settings == null)
        {
            throw new System.Exception($"Failed to load TEngineGlobalSettings from {TEngineGlobalSettingsPath}");
        }
        
        string newUrl = $"{CDNBaseUrl}{appVersion}/";
        var resourcesArea = settings.FrameworkGlobalSettings.ResourcesArea;
        
        // 使用反射或序列化方式修改（这里需要根据实际类型调整）
        // 由于 ResourcesArea 是序列化字段，需要通过 SerializedObject 修改
        var serializedObject = new SerializedObject(settings);
        var resourcesAreaProperty = serializedObject.FindProperty("m_FrameworkGlobalSettings.m_ResourcesArea.m_InnerResourceSourceUrl");
        if (resourcesAreaProperty != null)
        {
            resourcesAreaProperty.stringValue = newUrl;
            serializedObject.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning("[PirateCatEditorTools] Cannot find InnerResourceSourceUrl property, trying alternative method");
            // 备用方法：直接修改 YAML 文件
            UpdateYAMLFile(TEngineGlobalSettingsPath, "m_InnerResourceSourceUrl", newUrl);
        }
        
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    /// <summary>
    /// 更新 MiniGameConfig 的 CDN 地址（使用App版本号拼接）
    /// </summary>
    /// <param name="appVersion">App版本号（如 1.0.0）</param>
    public static void UpdateMiniGameConfigCDN(string appVersion)
    {
        Debug.Log($"[PirateCatEditorTools] Update MiniGameConfig CDN with app version {appVersion}");
        
        string newUrl = $"{CDNBaseUrl}{appVersion}/";
        
        // 直接修改 YAML 文件，使用更精确的匹配
        if (!File.Exists(MiniGameConfigPath))
        {
            throw new System.Exception($"File not found: {MiniGameConfigPath}");
        }
        
        string content = File.ReadAllText(MiniGameConfigPath);
        string originalContent = content;
        
        // 先清理可能的格式错误：如果CDN行后面直接跟了URL但没有冒号，删除多余的行
        // 匹配模式：CDN行后面跟着一个只有URL的行（没有key）
        string cleanupPattern = @"(  CDN:\s+https://[^\r\n]+)\r?\n(\s{2,}https://[^\r\n]+)";
        content = System.Text.RegularExpressions.Regex.Replace(content, cleanupPattern, "$1");
        
        // 匹配 CDN: 后面跟URL的模式，确保只匹配一行
        // 注意：Unity YAML文件在 ProjectConf 下使用2个空格缩进
        // 使用非贪婪匹配，确保只匹配到行尾
        string pattern = @"(  CDN:\s+)(https://[^\r\n]+)";
        string replacement = $"$1{newUrl}";
        
        bool matched = false;
        if (System.Text.RegularExpressions.Regex.IsMatch(content, pattern))
        {
            content = System.Text.RegularExpressions.Regex.Replace(content, pattern, replacement);
            matched = true;
        }
        else
        {
            // 如果没找到匹配，尝试匹配空值的情况
            pattern = @"(  CDN:\s*)([^\r\n]*)";
            if (System.Text.RegularExpressions.Regex.IsMatch(content, pattern))
            {
                content = System.Text.RegularExpressions.Regex.Replace(content, pattern, replacement);
                matched = true;
            }
        }
        
        // 只有在内容发生变化时才写入文件
        if (content != originalContent)
        {
            File.WriteAllText(MiniGameConfigPath, content);
            AssetDatabase.Refresh();
            Debug.Log($"[PirateCatEditorTools] CDN updated successfully to {newUrl}");
        }
        else if (!matched)
        {
            Debug.LogWarning("[PirateCatEditorTools] CDN value was not updated, pattern may not match");
        }
    }
    
    /// <summary>
    /// 复制文件到 Backup 目录
    /// 按照 YooAsset 官方资源部署文档，所有 bundle/manifest 文件直接放在 APP 版本目录下（扁平结构）
    /// https://www.yooasset.com/docs/1.5.x/guide-editor/AssetBundleDeployer
    /// 
    /// Bundle 文件来源: Bundles/WebGL/DefaultPackage/{resourceVersion}/
    /// bin.txt 来源: WXExport/webgl/*.bin.txt
    /// 备份目标: CDN_Backup/MiniGame/{appVersion}/ （与 CDN 结构 1:1 映射）
    /// </summary>
    public static void CopyToBackup(string appVersion, string resourceVersion, bool hasResourceChanged)
    {
        Debug.Log($"[PirateCatEditorTools] Copy files to backup directory (app={appVersion}, res={resourceVersion})");
        
        string backupPath = Path.Combine(BackupBasePath, appVersion);
        
        if (!Directory.Exists(backupPath))
        {
            Directory.CreateDirectory(backupPath);
        }
        
        if (hasResourceChanged)
        {
            // 从 YooAsset 构建输出目录复制 bundle 文件（扁平结构，直接放在 APP 版本目录下）
            string bundleOutputRoot = YooAsset.Editor.AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            string bundleSourcePath = Path.Combine(bundleOutputRoot, "WebGL", "DefaultPackage", resourceVersion);
            
            if (!Directory.Exists(bundleSourcePath))
            {
                throw new System.Exception(
                    $"Bundle output directory not found: {bundleSourcePath}\n" +
                    $"Please ensure YooAsset build has completed with PackageVersion={resourceVersion}");
            }
            
            string[] bundleFiles = Directory.GetFiles(bundleSourcePath, "*", SearchOption.TopDirectoryOnly);
            int copiedCount = 0;
            foreach (string sourceFile in bundleFiles)
            {
                string fileName = Path.GetFileName(sourceFile);
                // 跳过 OutputCache 等非发布文件
                if (fileName.StartsWith("."))
                    continue;
                    
                string destPath = Path.Combine(backupPath, fileName);
                File.Copy(sourceFile, destPath, true);
                copiedCount++;
            }
            Debug.Log($"[PirateCatEditorTools] Copied {copiedCount} bundle files from {bundleSourcePath} to {backupPath}");
        }
        
        // 复制 bin.txt 文件（从 WXExport/webgl/ 文件夹下）
        string exportPath = GetMiniGameExportPath();
        string webglPath = Path.Combine(exportPath, "webgl");
        
        if (!Directory.Exists(webglPath))
        {
            throw new System.Exception($"WebGL folder not found: {webglPath}. Please ensure the WeChat transform tool has completed successfully.");
        }
        
        string[] binFiles = Directory.GetFiles(webglPath, "*.bin.txt", SearchOption.TopDirectoryOnly);
        if (binFiles.Length == 0)
        {
            Debug.LogWarning($"[PirateCatEditorTools] No .bin.txt files found in {webglPath}");
        }
        else
        {
            foreach (string binFile in binFiles)
            {
                string fileName = Path.GetFileName(binFile);
                string destPath = Path.Combine(backupPath, fileName);
                File.Copy(binFile, destPath, true);
                Debug.Log($"[PirateCatEditorTools] Copied {fileName} to {destPath}");
            }
        }
    }
    
    /// <summary>
    /// 获取微信小游戏导出路径
    /// </summary>
    private static string GetMiniGameExportPath()
    {
        // 从 MiniGameConfig.asset 读取 DST 字段
        if (!File.Exists(MiniGameConfigPath))
        {
            throw new System.Exception($"MiniGameConfig file not found: {MiniGameConfigPath}");
        }
        
        string content = File.ReadAllText(MiniGameConfigPath);
        var match = System.Text.RegularExpressions.Regex.Match(content, @"DST:\s*(.+)");
        if (match.Success && match.Groups.Count > 1)
        {
            string dstPath = match.Groups[1].Value.Trim();
            // 处理相对路径
            if (!Path.IsPathRooted(dstPath))
            {
                dstPath = Path.Combine(Application.dataPath, "..", dstPath);
            }
            return Path.GetFullPath(dstPath);
        }
        
        // 如果读取失败，使用默认路径
        return Path.Combine(Application.dataPath, "..", "WXExport");
    }
    
    /// <summary>
    /// 更新 YAML 文件中的字段值（通用方法，用于更新 InnerResourceSourceUrl）
    /// </summary>
    private static void UpdateYAMLFile(string filePath, string key, string value)
    {
        if (!File.Exists(filePath))
        {
            throw new System.Exception($"File not found: {filePath}");
        }
        
        string content = File.ReadAllText(filePath);
        
        // 匹配模式：key: 后面跟任意内容，但只匹配到行尾
        // 使用更精确的匹配，确保只替换正确的字段，并且不跨行
        string pattern;
        if (key.EndsWith(":"))
        {
            // 如果key已经包含冒号，直接使用
            pattern = $@"({key}\s+)([^\r\n]+)";
        }
        else
        {
            // 如果key不包含冒号，添加冒号
            pattern = $@"({key}:\s+)([^\r\n]+)";
        }
        
        // 转义value中的特殊字符（URL中的特殊字符）
        string escapedValue = System.Text.RegularExpressions.Regex.Escape(value);
        // 但我们需要保留URL中的字符，所以不使用Escape，而是确保value不包含换行
        string replacement = $"$1{value}";
        
        if (System.Text.RegularExpressions.Regex.IsMatch(content, pattern))
        {
            content = System.Text.RegularExpressions.Regex.Replace(content, pattern, replacement);
            File.WriteAllText(filePath, content);
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogWarning($"[PirateCatEditorTools] Pattern not found in file: {pattern}");
        }
    }
}