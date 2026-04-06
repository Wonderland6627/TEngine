using System;
using System.Collections.Generic;
using GameLogic;
using GameLogic.Network;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 用户游戏数据编辑器窗口（仅 Play Mode + 非正式服可用）
/// 通过 debugSetUserGameInfo 接口修改服务端数据，修改后自动刷新本地数据并触发事件
/// </summary>
public class UserGameInfoEditorWindow : OdinEditorWindow
{
    [MenuItem("PirateCat/用户数据编辑器")]
    private static void ShowWindow()
    {
        var window = GetWindow<UserGameInfoEditorWindow>();
        window.titleContent = new GUIContent("用户数据编辑器");
        window.minSize = new Vector2(450, 600);
        window.Show();
    }

    private bool _isBusy;
    private bool _hasData;
    private string _statusMessage = "点击 Fetch 加载用户数据";
    private UserGameInfoData _snapshotData;

    #region 只读字段

    [ShowInInspector, ReadOnly, Title("基本信息"), LabelText("_id")]
    public string ID { get; private set; } = "";

    [ShowInInspector, ReadOnly, LabelText("openID")]
    public string OpenID { get; private set; } = "";

    #endregion

    #region 可编辑字段

    [ShowInInspector, Title("游戏数据"), LabelText("通关关卡ID")]
    public int ProgressLevelID { get; set; }

    [ShowInInspector, Title("用户资料"), LabelText("昵称")]
    public string NickName { get; set; } = "";

    [ShowInInspector, LabelText("头像URL")]
    public string AvatarUrl { get; set; } = "";

    [ShowInInspector, Title("资源"), DictionaryDrawerSettings(KeyLabel = "类型", ValueLabel = "数量")]
    public Dictionary<ResourceType, int> Resources { get; set; } = new();

    [ShowInInspector, Title("物品"), DictionaryDrawerSettings(KeyLabel = "物品ID", ValueLabel = "数量")]
    public Dictionary<int, int> Goods { get; set; } = new();

    [ShowInInspector, Title("已领取关卡宝箱")]
    public List<int> ClaimedLevelChests { get; set; } = new();

    #endregion

    #region 生命周期

    protected override void OnEnable()
    {
        base.OnEnable();
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    protected override void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        base.OnDisable();
    }

    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingPlayMode) return;
        _hasData = false;
        _isBusy = false;
        _snapshotData = null;
        _statusMessage = "点击 Fetch 加载用户数据";
        Repaint();
    }

    #endregion

    #region OnGUI

    protected override void OnGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("请进入 Play Mode 后使用此工具", MessageType.Warning);
            return;
        }

        if (NetManager.Instance == null)
        {
            EditorGUILayout.HelpBox("NetManager 未初始化", MessageType.Error);
            return;
        }

        if (NetManager.Instance.CurrentServerType == ServerType.Production)
        {
            EditorGUILayout.HelpBox("不允许在正式服使用此工具", MessageType.Error);
            return;
        }

        DrawStatusBar();
        DrawActionButtons();
        EditorGUILayout.Space(4);

        base.OnGUI();
    }

    private void DrawStatusBar()
    {
        var serverType = NetManager.Instance.CurrentServerType;
        EditorGUILayout.HelpBox(
            $"服务器: {serverType}  |  {_statusMessage}",
            _hasData ? MessageType.Info : MessageType.None);
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = !_isBusy;
        if (GUILayout.Button("Fetch", GUILayout.Height(28)))
            FetchData();

        GUI.enabled = !_isBusy && _hasData;
        if (GUILayout.Button("Save & Sync", GUILayout.Height(28)))
            SaveAndSync();
        if (GUILayout.Button("Reset", GUILayout.Height(28)))
            ResetToSnapshot();

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    #endregion

    #region 操作

    private async void FetchData()
    {
        _isBusy = true;
        _statusMessage = "正在拉取...";
        Repaint();

        try
        {
            var response = await NetManager.Instance.CallHttp<UserGameInfoData>("getUserGameInfoV2");
            if (response.IsSuccess && response.data != null)
            {
                ApplyFromServerData(response.data);
                SaveSnapshot(response.data);
                _hasData = true;
                _statusMessage = $"拉取成功 ({DateTime.Now:HH:mm:ss})";
            }
            else
            {
                _statusMessage = $"拉取失败: {response.ErrorMessage}";
            }
        }
        catch (Exception e)
        {
            _statusMessage = $"拉取异常: {e.Message}";
        }
        finally
        {
            _isBusy = false;
            Repaint();
        }
    }

    private async void SaveAndSync()
    {
        _isBusy = true;
        _statusMessage = "正在保存...";
        Repaint();

        try
        {
            var payload = BuildPayload();
            var response = await NetManager.Instance.CallHttp<object>("debugSetUserGameInfo", payload);
            if (!response.IsSuccess)
            {
                _statusMessage = $"保存失败: {response.ErrorMessage}";
                return;
            }

            _statusMessage = "保存成功，正在同步本地数据...";
            Repaint();

            World.Instance.FetchUserGameInfo(success =>
            {
                _statusMessage = success
                    ? $"同步完成 ({DateTime.Now:HH:mm:ss})。部分改动（如关卡进度）可能需要重新 Play 才能完全生效。"
                    : "服务端已更新，但本地同步失败，建议重新进入 Play Mode。";
                Repaint();
            });
        }
        catch (Exception e)
        {
            _statusMessage = $"保存异常: {e.Message}";
        }
        finally
        {
            _isBusy = false;
            Repaint();
        }
    }

    private void ResetToSnapshot()
    {
        if (_snapshotData == null) return;
        ApplyFromServerData(_snapshotData);
        _statusMessage = "已恢复到上次拉取的数据";
        Repaint();
    }

    #endregion

    #region 数据转换

    /// <summary>
    /// 从服务端 DTO 填充编辑器字段
    /// </summary>
    private void ApplyFromServerData(UserGameInfoData data)
    {
        ID = data._id ?? "";
        OpenID = data.openID ?? "";
        ProgressLevelID = data.progressLevelID;
        NickName = data.nickName ?? "";
        AvatarUrl = data.avatarUrl ?? "";

        Resources = new Dictionary<ResourceType, int>();
        if (data.resources != null)
        {
            foreach (var kvp in data.resources)
            {
                if (int.TryParse(kvp.Key, out int typeInt) && Enum.IsDefined(typeof(ResourceType), typeInt))
                    Resources[(ResourceType)typeInt] = kvp.Value;
            }
        }

        Goods = new Dictionary<int, int>();
        if (data.goods != null)
        {
            foreach (var kvp in data.goods)
            {
                if (int.TryParse(kvp.Key, out int goodsId))
                    Goods[goodsId] = kvp.Value;
            }
        }

        ClaimedLevelChests = data.claimedLevelChests != null
            ? new List<int>(data.claimedLevelChests)
            : new List<int>();
    }

    /// <summary>
    /// 保存当前服务端数据快照，用于 Reset
    /// </summary>
    private void SaveSnapshot(UserGameInfoData data)
    {
        _snapshotData = new UserGameInfoData
        {
            _id = data._id,
            openID = data.openID,
            progressLevelID = data.progressLevelID,
            nickName = data.nickName,
            avatarUrl = data.avatarUrl,
            resources = data.resources != null ? new Dictionary<string, int>(data.resources) : null,
            goods = data.goods != null ? new Dictionary<string, int>(data.goods) : null,
            claimedLevelChests = data.claimedLevelChests != null ? new List<int>(data.claimedLevelChests) : null,
        };
    }

    /// <summary>
    /// 将编辑器字段构建为服务端请求 payload（不含只读字段 _id / openID）
    /// </summary>
    private Dictionary<string, object> BuildPayload()
    {
        var serverResources = new Dictionary<string, int>();
        foreach (var kvp in Resources)
            serverResources[((int)kvp.Key).ToString()] = kvp.Value;

        var serverGoods = new Dictionary<string, int>();
        foreach (var kvp in Goods)
            serverGoods[kvp.Key.ToString()] = kvp.Value;

        return new Dictionary<string, object>
        {
            { "progressLevelID", ProgressLevelID },
            { "nickName", NickName },
            { "avatarUrl", AvatarUrl },
            { "resources", serverResources },
            { "goods", serverGoods },
            { "claimedLevelChests", ClaimedLevelChests },
        };
    }

    #endregion
}
