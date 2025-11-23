using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;
using TEngine.Editor;
using Newtonsoft.Json.Linq;

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
    /// 步骤1: 更新版本号（更新所有相关配置文件）
    /// </summary>
    /// <param name="version">版本号（如 v0.1.7.3）</param>
    /// <returns>是否成功</returns>
    public static bool UpdateVersion(string version)
    {
        try
        {
            Debug.Log($"[PirateCatEditorTools] Step 1: Update version to {version}");
            
            // 验证版本号格式
            if (string.IsNullOrEmpty(version) || !version.StartsWith("v"))
            {
                EditorUtility.DisplayDialog("错误", "版本号必须以 'v' 开头（如 v0.1.7.3）", "确定");
                return false;
            }
            
            // 更新 YooAssetSettings.asset 的 BuildVersion
            UpdateYooAssetVersion(version);
            
            // 更新 InnerResourceSourceUrl
            UpdateInnerResourceSourceUrl(version);
            
            // 更新 MiniGameConfig.asset 的 CDN
            UpdateMiniGameConfigCDN(version);
            
            Debug.Log($"[PirateCatEditorTools] Version updated successfully to {version}");
            EditorUtility.DisplayDialog("成功", $"版本号已更新为 {version}", "确定");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PirateCatEditorTools] Failed to update version: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("错误", $"更新版本号失败：{e.Message}", "确定");
            return false;
        }
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
    /// 步骤3.5: 更新 project.config.json 文件，添加 cloudfunctionRoot 配置
    /// </summary>
    /// <returns>是否成功</returns>
    public static bool UpdateProjectConfigJson()
    {
        try
        {
            Debug.Log("[PirateCatEditorTools] Step 3.5: Update project.config.json");
            
            string exportPath = GetMiniGameExportPath();
            string projectConfigPath = Path.Combine(exportPath, "minigame", "project.config.json");
            
            // 检查文件是否存在
            if (!File.Exists(projectConfigPath))
            {
                Debug.LogWarning($"[PirateCatEditorTools] project.config.json not found at {projectConfigPath}, skipping update");
                return false;
            }
            
            // 读取 JSON 文件
            string jsonContent = File.ReadAllText(projectConfigPath);
            
            // 使用 Newtonsoft.Json 解析 JSON
            JObject jsonObj = JObject.Parse(jsonContent);
            
            // 检查是否已经存在 cloudfunctionRoot
            bool alreadyExists = jsonObj["cloudfunctionRoot"] != null;
            
            // 设置或更新 cloudfunctionRoot 配置
            jsonObj["cloudfunctionRoot"] = "cloudfunctions/";
            
            if (alreadyExists)
            {
                Debug.Log("[PirateCatEditorTools] Updated existing cloudfunctionRoot in project.config.json");
            }
            else
            {
                Debug.Log("[PirateCatEditorTools] Added cloudfunctionRoot to project.config.json");
            }
            
            // 格式化 JSON（保持缩进）
            string formattedJson = jsonObj.ToString(Newtonsoft.Json.Formatting.Indented);
            
            // 写入文件
            File.WriteAllText(projectConfigPath, formattedJson);
            Debug.Log($"[PirateCatEditorTools] project.config.json updated successfully at {projectConfigPath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PirateCatEditorTools] Failed to update project.config.json: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }
    
    /// <summary>
    /// 步骤4: 复制文件到 Backup 目录
    /// </summary>
    /// <param name="version">版本号</param>
    /// <param name="hasResourceChanged">是否修改了资源</param>
    /// <returns>是否成功</returns>
    public static bool CopyToBackupDirectory(string version, bool hasResourceChanged)
    {
        try
        {
            Debug.Log($"[PirateCatEditorTools] Step 4: Copy files to backup directory");
            
            // 在复制文件之前，先更新 project.config.json
            UpdateProjectConfigJson();
            
            CopyToBackup(version, hasResourceChanged);
            
            Debug.Log($"[PirateCatEditorTools] Files copied to backup directory successfully");
            EditorUtility.DisplayDialog("成功", $"文件已复制到备份目录：{version}", "确定");
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
    /// 更新 InnerResourceSourceUrl
    /// </summary>
    public static void UpdateInnerResourceSourceUrl(string version)
    {
        Debug.Log($"[PirateCatEditorTools] Update InnerResourceSourceUrl version to {version}");
        
        var settings = AssetDatabase.LoadAssetAtPath<TEngineSettings>(TEngineGlobalSettingsPath);
        if (settings == null)
        {
            throw new System.Exception($"Failed to load TEngineGlobalSettings from {TEngineGlobalSettingsPath}");
        }
        
        string newUrl = $"{CDNBaseUrl}{version}/";
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
    /// 更新 MiniGameConfig 的 CDN 地址
    /// </summary>
    public static void UpdateMiniGameConfigCDN(string version)
    {
        Debug.Log($"[PirateCatEditorTools] Update MiniGameConfig CDN version to {version}");
        
        string newUrl = $"{CDNBaseUrl}{version}/";
        
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
    /// 注意：导出后的WXExport文件夹中会有webgl和minigame两个文件夹，需要的文件在webgl文件夹下
    /// </summary>
    public static void CopyToBackup(string version, bool hasResourceChanged)
    {
        Debug.Log($"[PirateCatEditorTools] Copy files to backup directory");
        
        string exportPath = GetMiniGameExportPath();
        // 文件在 webgl 文件夹下
        string webglPath = Path.Combine(exportPath, "webgl");
        string backupPath = Path.Combine(BackupBasePath, version);
        
        // 检查 webgl 文件夹是否存在
        if (!Directory.Exists(webglPath))
        {
            throw new System.Exception($"WebGL folder not found: {webglPath}. Please ensure the WeChat transform tool has completed successfully.");
        }
        
        // 创建备份目录
        if (!Directory.Exists(backupPath))
        {
            Directory.CreateDirectory(backupPath);
        }
        
        if (hasResourceChanged)
        {
            // 复制 StreamingAssets 文件夹（从 webgl 文件夹下）
            string streamingAssetsSource = Path.Combine(webglPath, "StreamingAssets");
            string streamingAssetsDest = Path.Combine(backupPath, "StreamingAssets");
            
            if (Directory.Exists(streamingAssetsSource))
            {
                if (Directory.Exists(streamingAssetsDest))
                {
                    Directory.Delete(streamingAssetsDest, true);
                }
                FileUtil.CopyFileOrDirectory(streamingAssetsSource, streamingAssetsDest);
                Debug.Log($"[PirateCatEditorTools] Copied StreamingAssets from {streamingAssetsSource} to {streamingAssetsDest}");
            }
            else
            {
                Debug.LogWarning($"[PirateCatEditorTools] StreamingAssets folder not found at {streamingAssetsSource}");
            }
        }
        
        // 复制 bin.txt 文件（从 webgl 文件夹下）
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
                Debug.Log($"[PirateCatEditorTools] Copied {fileName} from {binFile} to {destPath}");
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