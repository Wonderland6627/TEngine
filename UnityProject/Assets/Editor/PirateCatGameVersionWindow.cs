using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

/// <summary>
/// 微信小游戏打包工具窗口（分步骤操作）
/// </summary>
public class PirateCatGameVersionWindow : EditorWindow
{
    private static readonly string[] BaselineModeLabels =
    {
        "自动（账本 → tag → 工作区）",
        "上次发布账本",
        "最近 publish tag",
        "指定 commit/tag",
        "仅当前工作区",
    };

    private string resourceVersion = "v0.1.7.2"; // 资源版本号（YooAsset 热更清单）
    private string appVersion = "1.0.0"; // App版本号（CDN 目录分区）
    private string wechatVersion = string.Empty; // 微信小游戏后台代码包版本（提审后补记，与工程无关）
    private bool hasResourceChanged = false;
    private string versionSuggestion = string.Empty;
    private MessageType versionSuggestionType = MessageType.Info;
    private VersionBaselineMode baselineMode = VersionBaselineMode.Auto;
    private string customBaselineRef = string.Empty;
    private string lastPublishSummary = string.Empty;
    private Vector2 suggestionScroll;
    private Vector2 windowScroll;
    private GUIStyle suggestionRichStyle;
    
    [MenuItem("PirateCat/打包工具")]
    private static void ShowWindow()
    {
        var window = GetWindow<PirateCatGameVersionWindow>();
        window.titleContent = new GUIContent("微信小游戏打包工具");
        window.minSize = new Vector2(450, 620);
        window.Show();
    }
    
    private void OnEnable()
    {
        // 加载当前版本号
        LoadCurrentVersions();
        RefreshLastPublishSummary();
    }
    
    private void OnGUI()
    {
        windowScroll = EditorGUILayout.BeginScrollView(windowScroll);
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("打开Bundle目录", EditorStyles.toolbarButton))
        {
            OpenDirectory(Path.GetFullPath(AssetBundleBuilderHelper.GetDefaultBuildOutputRoot()), "Bundle目录");
        }
        if (GUILayout.Button("打开CDN备份目录", EditorStyles.toolbarButton))
        {
            OpenDirectory(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "CDN_Backup")), "CDN备份目录");
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

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

        // ========== 智能建议 ==========
        EditorGUILayout.LabelField("版本建议（Windows）", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("对比基准:", GUILayout.Width(70));
        baselineMode = (VersionBaselineMode)EditorGUILayout.Popup((int)baselineMode, BaselineModeLabels);
        EditorGUILayout.EndHorizontal();

        if (baselineMode == VersionBaselineMode.CustomRef)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("commit/tag:", GUILayout.Width(70));
            customBaselineRef = EditorGUILayout.TextField(customBaselineRef);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("账本:", GUILayout.Width(70));
        EditorGUILayout.LabelField(string.IsNullOrEmpty(lastPublishSummary) ? "未加载" : lastPublishSummary, EditorStyles.wordWrappedLabel);
        if (GUILayout.Button("刷新", GUILayout.Width(48)))
        {
            RefreshLastPublishSummary();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("分析相对基准的变更并给出建议", GUILayout.Height(28)))
        {
            if (PirateCatEditorTools.TryGetVersionRecommendationWindows(
                    baselineMode,
                    customBaselineRef,
                    out VersionRecommendationResult result,
                    out string error))
            {
                versionSuggestion = result.Text;
                versionSuggestionType = string.Equals(result.BaselineSource, "workdir", System.StringComparison.Ordinal)
                    ? MessageType.Warning
                    : MessageType.Info;
                hasResourceChanged = result.HasResourceChanged;
            }
            else
            {
                versionSuggestion = error;
                versionSuggestionType = MessageType.Warning;
            }
        }

        if (!string.IsNullOrEmpty(versionSuggestion))
        {
            if (versionSuggestionType == MessageType.Warning)
            {
                EditorGUILayout.HelpBox("当前基准可能不可靠（已回退到工作区或分析告警），请核对下方详情。", MessageType.Warning);
            }

            EnsureSuggestionStyle();
            EditorGUILayout.Space(2);
            // 用支持富文本的 Label 渲染，凸显最有用的建议
            suggestionScroll = EditorGUILayout.BeginScrollView(
                suggestionScroll,
                GUILayout.MinHeight(260),
                GUILayout.MaxHeight(420));
            GUILayout.Label(
                FormatSuggestionRichText(versionSuggestion),
                suggestionRichStyle,
                GUILayout.ExpandWidth(true));
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(15);
        
        // ========== 步骤1: 修改版本号 ==========
        EditorGUILayout.LabelField("步骤1: 修改版本号", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // App版本号
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("App版本号:", GUILayout.Width(100));
        appVersion = EditorGUILayout.TextField(appVersion);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // 资源版本号
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("资源版本号:", GUILayout.Width(100));
        resourceVersion = EditorGUILayout.TextField(resourceVersion);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(resourceVersion) || string.IsNullOrEmpty(appVersion));
        if (GUILayout.Button("确认并更新版本号", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("确认更新", 
                $"确定要更新版本号吗？\n\nApp版本号: {appVersion}\n资源版本号: {resourceVersion}\n\n这将更新以下文件：\n- BuildSettings (PlayerSettings.bundleVersion)\n- YooAssetSettings.asset\n- TEngineGlobalSettings.asset\n- MiniGameConfig.asset", 
                "确定", "取消"))
            {
                bool success = PirateCatEditorTools.UpdateVersion(resourceVersion, appVersion);
                if (success)
                {
                    EditorUtility.DisplayDialog("成功", 
                        $"版本号更新成功！\n\nApp版本号: {appVersion}\n资源版本号: {resourceVersion}", 
                        "确定");
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "更新失败", "确定");
                }
                
                LoadCurrentVersions(); // 重新加载以显示更新后的值
            }
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "三个版本号的分工:\n" +
            "1) 资源版本号(YooAsset热更清单) -> 每次打出并上传的新 Bundle 都递增（含测试轮次）。\n" +
            "2) App版本号(CDN目录分区) -> 默认不变，仅在要隔离/并行旧资源时才新开目录。\n" +
            "3) 微信代码包版本(后台线上版本) -> 仅在提审代码包时递增，与资源号无强制关系；在步骤6中后台发布后补记。",
            MessageType.None);
        
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
        EditorGUI.BeginDisabledGroup(!hasResourceChanged || string.IsNullOrEmpty(resourceVersion));
        if (GUILayout.Button("打开 AssetBundle Builder", GUILayout.Height(30)))
        {
            PirateCatEditorTools.BuildBundle(resourceVersion);
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
        
        // ========== 步骤4: 复制到备份目录（测试/发布均用，不写账本） ==========
        EditorGUILayout.LabelField("步骤4: 复制到备份目录（测试上传）", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(resourceVersion) || string.IsNullOrEmpty(appVersion));
        if (GUILayout.Button("复制到 CDN_Backup", GUILayout.Height(30)))
        {
            string bundleInfo = hasResourceChanged
                ? $"是（将从 Bundles/WebGL/DefaultPackage/{resourceVersion}/ 复制 bundle + manifest + bin.txt）"
                : "否（只复制 bin.txt，要求目标 App 版本目录已存在可用资源）";
            string codeOnlyWarning = hasResourceChanged
                ? string.Empty
                : "\n\n警告：仅代码模式建议保持 App 版本号不变；若切到新 App 版本目录，请先确保该目录已有对应资源文件。";
            
            if (EditorUtility.DisplayDialog("确认复制",
                $"确定要将文件复制到备份目录吗？\n\n" +
                $"App版本号: {appVersion}\n" +
                $"资源版本号: {resourceVersion}\n" +
                $"资源修改: {bundleInfo}\n\n" +
                $"备份目标: CDN_Backup/MiniGame/{appVersion}/\n" +
                $"该目录内容可直接上传到 CDN\n\n" +
                $"注意：此操作不会写入发布账本，可放心用于真机/开发者工具测试。" +
                $"{codeOnlyWarning}",
                "确定", "取消"))
            {
                PirateCatEditorTools.CopyToBackupDirectory(appVersion, resourceVersion, hasResourceChanged);
                
                string backupPath = System.IO.Path.Combine("CDN_Backup", "MiniGame", appVersion);
                string fullPath = System.IO.Path.GetFullPath(backupPath);
                if (System.IO.Directory.Exists(fullPath))
                {
                    EditorUtility.RevealInFinder(fullPath);
                }
            }
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space(3);
        GUIStyle helpStyle4 = new GUIStyle(GUI.skin.label)
        {
            fontSize = 10,
            wordWrap = true,
            normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
        };
        EditorGUILayout.LabelField(
            $"提示: Bundle/Manifest 从 Bundles/WebGL/DefaultPackage/{{资源版本号}}/ 复制，bin.txt 从 WXExport/webgl/ 复制\n" +
            $"测试发现问题 -> 修改后升资源版本号重新打 Bundle，重复步骤2~4，不影响账本\n" +
            $"备份到 CDN_Backup/MiniGame/{{App版本号}}/ （与 CDN 扁平结构一致，可直接上传）", helpStyle4);
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);

        // ========== 步骤5: 确认发布基线（测试通过后） ==========
        EditorGUILayout.LabelField("步骤5: 确认发布基线（测试通过后）", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(resourceVersion) || string.IsNullOrEmpty(appVersion));
        if (GUILayout.Button("确认发布基线（写入账本 + 打 tag）", GUILayout.Height(30)))
        {
            string publishMode = hasResourceChanged ? "full" : "code_only";
            if (EditorUtility.DisplayDialog("确认发布基线",
                $"确定把当前状态记录为线上发布基线吗？\n\n" +
                $"App版本号: {appVersion}\n" +
                $"资源版本号: {resourceVersion}\n" +
                $"发布模式: {publishMode}\n" +
                $"commit: 当前 HEAD\n\n" +
                $"微信代码包版本将沿用账本旧值（若本次有提审，请在步骤6中补记新版本）。\n\n" +
                $"仅在测试验证通过、确认此版本就是线上版本时执行。",
                "确定", "取消"))
            {
                string recordSummary = PirateCatEditorTools.RecordPublishBaseline(appVersion, resourceVersion, publishMode, string.Empty);
                RefreshLastPublishSummary();
                EditorUtility.DisplayDialog("发布基线", recordSummary, "确定");
            }
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField(
            "提示: 测试轮次不要点这里；只有确定「这版就是线上版本」时才写基线。\n" +
            "之后的版本建议将以此基线为对比起点。", helpStyle4);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);

        // ========== 步骤6: 微信后台发布后补记版本 ==========
        EditorGUILayout.LabelField("步骤6: 微信后台发布后补记版本（可选）", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("微信代码包版本:", "微信小游戏后台本次发布的版本号，仅记录进账本，与资源版本号无强制关系"), GUILayout.Width(100));
        wechatVersion = EditorGUILayout.TextField(wechatVersion);
        if (GUILayout.Button("补记进账本", GUILayout.Width(90), GUILayout.Height(20)))
        {
            if (PirateCatEditorTools.TryUpdateLedgerWechatVersion(wechatVersion, out string message))
            {
                RefreshLastPublishSummary();
                EditorUtility.DisplayDialog("成功", message, "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("失败", message, "确定");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField(
            "提示: 仅当本次重新导出并在微信后台提审/发布了新代码包时才需要。\n" +
            "纯资源热更不需要动微信版本，跳过此步骤。", helpStyle4);

        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(20);
        
        // ========== 当前配置信息 ==========
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("当前配置:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("App版本号:", GUILayout.Width(120));
        EditorGUILayout.LabelField(GetCurrentAppVersion());
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("资源版本号:", GUILayout.Width(120));
        EditorGUILayout.LabelField(GetCurrentYooAssetVersion());
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("CDN地址:", GUILayout.Width(120));
        EditorGUILayout.LabelField(GetCurrentCDNUrl(), EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        EditorGUILayout.EndScrollView();
    }

    private static void OpenDirectory(string fullPath, string label)
    {
        if (Directory.Exists(fullPath))
        {
            EditorUtility.RevealInFinder(fullPath);
        }
        else
        {
            EditorUtility.DisplayDialog("提示", $"{label}不存在: {fullPath}", "确定");
        }
    }

    private void EnsureSuggestionStyle()
    {
        if (suggestionRichStyle != null)
        {
            return;
        }

        suggestionRichStyle = new GUIStyle(EditorStyles.label)
        {
            richText = true,
            wordWrap = true,
            fontSize = 12,
            padding = new RectOffset(8, 8, 6, 6),
        };
    }

    /// <summary>
    /// 给建议文本的关键行加颜色/加粗，凸显最有用的结论。
    /// </summary>
    private static string FormatSuggestionRichText(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return raw;
        }

        string[] lines = raw.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
        var builder = new StringBuilder(raw.Length + 256);
        foreach (string line in lines)
        {
            builder.Append(HighlightSuggestionLine(line));
            builder.Append('\n');
        }

        return builder.ToString().TrimEnd('\n');
    }

    private static string HighlightSuggestionLine(string line)
    {
        string trimmed = line.TrimStart(' ', '-', '\t');

        // 告警：最需要注意，红色加粗
        if (trimmed.StartsWith("警告"))
        {
            return Colorize(line, "#E06C75", bold: true);
        }

        // 资源版本号结论：需要更新=橙黄醒目，不更新=绿色安心
        if (trimmed.StartsWith("资源版本号：建议更新"))
        {
            return Colorize(line, "#E5B567", bold: true);
        }
        if (trimmed.StartsWith("资源版本号：建议不更新"))
        {
            return Colorize(line, "#98C379", bold: true);
        }

        // 操作流程：给出具体动作，蓝色加粗
        if (trimmed.StartsWith("流程："))
        {
            return Colorize(line, "#61AFEF", bold: true);
        }

        // App 版本号需留意的情况
        if (trimmed.StartsWith("App版本号：检测到"))
        {
            return Colorize(line, "#E5B567", bold: true);
        }

        // 段落标题
        if (trimmed.StartsWith("自动分析结果") || trimmed.StartsWith("推荐："))
        {
            return Bold(line);
        }

        return line;
    }

    private static string Colorize(string text, string hexColor, bool bold)
    {
        string inner = bold ? $"<b>{text}</b>" : text;
        return $"<color={hexColor}>{inner}</color>";
    }

    private static string Bold(string text)
    {
        return $"<b>{text}</b>";
    }

    private void RefreshLastPublishSummary()
    {
        PirateCatEditorTools.TryGetLastPublishSummary(out lastPublishSummary);

        // 微信代码包版本与工程无关，默认沿用账本里上次发布的值，方便直接微调递增
        if (string.IsNullOrEmpty(wechatVersion))
        {
            wechatVersion = PirateCatEditorTools.GetLastPublishWechatVersion();
        }
    }
    
    /// <summary>
    /// 加载当前版本号（资源版本号和App版本号）
    /// </summary>
    private void LoadCurrentVersions()
    {
        try
        {
            // 加载资源版本号
            var settings = AssetDatabase.LoadAssetAtPath<YooAsset.YooAssetSettings>(
                "Assets/TEngine/AssetSetting/Resources/YooAssetSettings.asset");
            if (settings != null)
            {
                resourceVersion = settings.BuildVersion;
            }
            
            // 加载App版本号
            appVersion = PlayerSettings.bundleVersion;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[PirateCatGameVersionWindow] Failed to load current versions: {e.Message}");
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
    /// 获取当前 App 版本号
    /// </summary>
    private string GetCurrentAppVersion()
    {
        try
        {
            return PlayerSettings.bundleVersion;
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
