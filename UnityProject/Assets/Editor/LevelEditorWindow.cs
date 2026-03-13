using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using GameLogic;
using Newtonsoft.Json;

/// <summary>
/// 关卡编辑器窗口 - 用于可视化编辑关卡配置
/// </summary>
public class LevelEditorWindow : EditorWindow
{
    // 分辨率设置
    private int resolutionWidth = 1080;
    private int resolutionHeight = 2340;
    
    // 关卡数据
    private List<LevelConfig> levels = new List<LevelConfig>();
    private int selectedLevelIndex = 0;
    private LevelConfig currentLevel = null;
    
    // 选中状态
    private int selectedCastleId = -1;
    private int selectedRoadIndex = -1;
    private bool isRoadLinkMode = false;
    private int roadLinkStartCastleId = -1;
    
    // 可视化面板相关
    private Vector2 scrollPosition = Vector2.zero;
    private float zoom = 1.0f;
    private Vector2 panOffset = Vector2.zero;
    
    // 文件路径
    private string jsonFilePath = "Assets/AssetRaw/Configs/jsons/levels.json";
    
    // UI布局
    private float detailPanelWidth = 300f;
    private Vector2 detailScrollPosition = Vector2.zero;
    
    [MenuItem("PirateCat/关卡编辑器")]
    private static void ShowWindow()
    {
        var window = GetWindow<LevelEditorWindow>();
        window.titleContent = new GUIContent("关卡编辑器");
        window.minSize = new Vector2(800, 600);
        window.Show();
    }
    
    private void OnEnable()
    {
        LoadJsonFile();
    }
    
    private void OnGUI()
    {
        wantsMouseMove = isRoadLinkMode;
        
        if (isRoadLinkMode && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
        {
            isRoadLinkMode = false;
            roadLinkStartCastleId = -1;
            Repaint();
            Event.current.Use();
        }
        
        DrawToolbar();
        
        EditorGUILayout.BeginHorizontal();
        
        // 可视化面板
        DrawVisualizationPanel();
        
        // 详情面板（一直显示）
        DrawDetailPanel();
        
        EditorGUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// 绘制工具栏
    /// </summary>
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        // 分辨率设置
        EditorGUILayout.LabelField("分辨率:", GUILayout.Width(60));
        EditorGUI.BeginChangeCheck();
        resolutionWidth = EditorGUILayout.IntField(resolutionWidth, GUILayout.Width(60));
        EditorGUILayout.LabelField("x", GUILayout.Width(15));
        resolutionHeight = EditorGUILayout.IntField(resolutionHeight, GUILayout.Width(60));
        if (EditorGUI.EndChangeCheck())
        {
            Repaint();
        }
        
        GUILayout.FlexibleSpace();
        
        // 右上角按钮组：加载关卡、切换关卡、保存
        // 加载按钮
        if (GUILayout.Button("加载关卡", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            LoadJsonFile();
        }
        
        // 关卡选择
        if (levels.Count > 0)
        {
            int newIndex = EditorGUILayout.Popup(selectedLevelIndex, GetLevelNames(), EditorStyles.toolbarPopup, GUILayout.Width(100));
            if (newIndex != selectedLevelIndex)
            {
                selectedLevelIndex = newIndex;
                currentLevel = levels[selectedLevelIndex];
                selectedCastleId = -1;
            }
        }
        
        // 保存按钮
        EditorGUI.BeginDisabledGroup(currentLevel == null);
        if (GUILayout.Button("保存", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            SaveJsonFile();
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.EndHorizontal();
    }
    
    /// <summary>
    /// 绘制可视化面板
    /// </summary>
    private void DrawVisualizationPanel()
    {
        Rect visualizationRect = EditorGUILayout.GetControlRect(false, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        
        if (currentLevel == null || currentLevel.castles == null)
        {
            EditorGUI.LabelField(visualizationRect, "请先加载关卡文件", EditorStyles.centeredGreyMiniLabel);
            return;
        }
        
        // 绘制背景
        EditorGUI.DrawRect(visualizationRect, new Color(0.2f, 0.2f, 0.2f));
        
        // 计算分辨率宽高比
        float aspectRatio = (float)resolutionWidth / resolutionHeight;
        
        // 计算可用的可视化区域（考虑详情面板）
        float availableWidth = visualizationRect.width;
        float availableHeight = visualizationRect.height;
        float padding = 20f;
        
        // 根据宽高比计算屏幕框的实际大小
        float screenFrameWidth, screenFrameHeight;
        if (availableWidth / availableHeight > aspectRatio)
        {
            // 高度限制
            screenFrameHeight = availableHeight - padding * 2;
            screenFrameWidth = screenFrameHeight * aspectRatio;
        }
        else
        {
            // 宽度限制
            screenFrameWidth = availableWidth - padding * 2;
            screenFrameHeight = screenFrameWidth / aspectRatio;
        }
        
        // 计算屏幕框的位置（居中）
        float screenFrameX = visualizationRect.x + (availableWidth - screenFrameWidth) / 2f;
        float screenFrameY = visualizationRect.y + (availableHeight - screenFrameHeight) / 2f;
        Rect screenFrameRect = new Rect(screenFrameX, screenFrameY, screenFrameWidth, screenFrameHeight);
        
        // 绘制屏幕框线（白色边框）
        Handles.BeginGUI();
        Handles.color = Color.white;
        
        // 绘制矩形框（四条边）
        Vector2 topLeft = new Vector2(screenFrameRect.x, screenFrameRect.y);
        Vector2 topRight = new Vector2(screenFrameRect.x + screenFrameRect.width, screenFrameRect.y);
        Vector2 bottomLeft = new Vector2(screenFrameRect.x, screenFrameRect.y + screenFrameRect.height);
        Vector2 bottomRight = new Vector2(screenFrameRect.x + screenFrameRect.width, screenFrameRect.y + screenFrameRect.height);
        
        Handles.DrawLine(topLeft, topRight);      // 上边
        Handles.DrawLine(topRight, bottomRight);  // 右边
        Handles.DrawLine(bottomRight, bottomLeft); // 下边
        Handles.DrawLine(bottomLeft, topLeft);    // 左边
        
        // 计算坐标转换（基于屏幕框范围）
        // 坐标范围：从-resolutionWidth/2到resolutionWidth/2，从-resolutionHeight/2到resolutionHeight/2
        float worldMinX = -resolutionWidth / 2f;
        float worldMaxX = resolutionWidth / 2f;
        float worldMinY = -resolutionHeight / 2f;
        float worldMaxY = resolutionHeight / 2f;
        
        float worldRangeX = worldMaxX - worldMinX;
        float worldRangeY = worldMaxY - worldMinY;
        
        // 防止除零错误
        if (worldRangeX <= 0) worldRangeX = 1f;
        if (worldRangeY <= 0) worldRangeY = 1f;
        
        // 绘制道路（支持选中高亮和点击选择）
        if (currentLevel.roads != null)
        {
            for (int i = 0; i < currentLevel.roads.Count; i++)
            {
                var road = currentLevel.roads[i];
                var startCastle = currentLevel.castles.Find(c => c.id == road.startCastleId);
                var endCastle = currentLevel.castles.Find(c => c.id == road.endCastleId);
                
                if (startCastle == null || endCastle == null || 
                    startCastle.position == null || endCastle.position == null)
                    continue;
                
                Vector2 startPos = WorldToScreenFrame(startCastle.position.x, startCastle.position.y, 
                    worldMinX, worldMaxX, worldMinY, worldMaxY, screenFrameRect);
                Vector2 endPos = WorldToScreenFrame(endCastle.position.x, endCastle.position.y,
                    worldMinX, worldMaxX, worldMinY, worldMaxY, screenFrameRect);
                
                bool isSelected = (i == selectedRoadIndex);
                Handles.color = isSelected ? Color.yellow : Color.gray;
                Handles.DrawLine(startPos, endPos);
                
                if (!isRoadLinkMode && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                {
                    float dist = PointToLineDistance(Event.current.mousePosition, startPos, endPos);
                    if (dist < 8f)
                    {
                        selectedRoadIndex = i;
                        selectedCastleId = -1;
                        Repaint();
                        Event.current.Use();
                    }
                }
            }
        }
        
        // 绘制城堡
        foreach (var castle in currentLevel.castles)
        {
            if (castle.position == null) continue;
            
            Vector2 screenPos = WorldToScreenFrame(castle.position.x, castle.position.y,
                worldMinX, worldMaxX, worldMinY, worldMaxY, screenFrameRect);
            
            // 确定颜色
            Color castleColor = Color.white;
            if (castle.occupiedOnStart)
            {
                castleColor = castle.occupiedSlimeType == 0 ? Color.blue : Color.red;
            }
            else
            {
                castleColor = Color.gray;
            }
            
            // 选中状态
            if (castle.id == selectedCastleId)
            {
                castleColor = Color.yellow;
            }
            
            // 道路连接模式起点高亮
            if (isRoadLinkMode && castle.id == roadLinkStartCastleId)
            {
                castleColor = Color.green;
            }
            
            // 绘制城堡圆圈
            Handles.color = castleColor;
            Handles.DrawSolidDisc(screenPos, Vector3.forward, 15f);
            
            // 绘制边框
            Handles.color = Color.black;
            Handles.DrawWireDisc(screenPos, Vector3.forward, 15f);
            
            // 绘制ID标签和单位数量
            int castleIndex = currentLevel.castles.IndexOf(castle);
            string labelText = $"[{castleIndex}] {castle.occupiedUnitCount}";
            Handles.Label(screenPos + Vector2.up * 20, labelText, EditorStyles.whiteLabel);
            
            // 检测点击
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Vector2 mousePos = Event.current.mousePosition;
                if (Vector2.Distance(mousePos, screenPos) < 15f)
                {
                    if (isRoadLinkMode)
                    {
                        if (roadLinkStartCastleId < 0)
                        {
                            roadLinkStartCastleId = castle.id;
                        }
                        else if (castle.id != roadLinkStartCastleId)
                        {
                            AddRoad(roadLinkStartCastleId, castle.id);
                            isRoadLinkMode = false;
                            roadLinkStartCastleId = -1;
                        }
                    }
                    else
                    {
                        selectedCastleId = castle.id;
                        selectedRoadIndex = -1;
                    }
                    Repaint();
                    Event.current.Use();
                }
            }
        }
        
        // 道路连接模式：绘制预览线
        if (isRoadLinkMode && roadLinkStartCastleId >= 0 && currentLevel.castles != null)
        {
            var linkStartCastle = currentLevel.castles.Find(c => c.id == roadLinkStartCastleId);
            if (linkStartCastle?.position != null)
            {
                Vector2 linkStartPos = WorldToScreenFrame(linkStartCastle.position.x, linkStartCastle.position.y,
                    worldMinX, worldMaxX, worldMinY, worldMaxY, screenFrameRect);
                Handles.color = Color.green;
                Handles.DrawDottedLine(linkStartPos, Event.current.mousePosition, 4f);
            }
        }
        
        Handles.EndGUI();
        
        // 连接模式提示
        if (isRoadLinkMode)
        {
            string hint = roadLinkStartCastleId < 0 
                ? "● 点击起始城堡" 
                : $"● 已选城堡 {roadLinkStartCastleId}，点击目标城堡 (ESC取消)";
            Rect hintRect = new Rect(visualizationRect.x + 10, visualizationRect.y + 10, 400, 20);
            GUIStyle hintStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.green } };
            EditorGUI.LabelField(hintRect, hint, hintStyle);
        }
        
        // 处理鼠标事件
        HandleMouseEvents(visualizationRect);
    }
    
    /// <summary>
    /// 绘制详情面板
    /// </summary>
    private void DrawDetailPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(detailPanelWidth));
        
        EditorGUILayout.LabelField("详情面板", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        if (currentLevel == null)
        {
            EditorGUILayout.HelpBox("请先加载关卡", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }
        
        detailScrollPosition = EditorGUILayout.BeginScrollView(detailScrollPosition);
        
        // 关卡配置
        EditorGUILayout.LabelField("关卡配置", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField($"关卡ID: {currentLevel.levelId}");
        EditorGUILayout.LabelField($"城堡数量: {currentLevel.castles?.Count ?? 0}");
        EditorGUILayout.LabelField($"道路数量: {currentLevel.roads?.Count ?? 0}");
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // 城堡操作按钮
        EditorGUILayout.LabelField("城堡操作", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        
        if (GUILayout.Button("添加城堡", GUILayout.Height(25)))
        {
            AddNewCastle();
        }
        
        EditorGUI.BeginDisabledGroup(selectedCastleId < 0);
        if (GUILayout.Button("删除城堡", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("确认删除", 
                $"确定要删除ID为 {selectedCastleId} 的城堡吗？\n这将同时删除所有相关的道路连接。", 
                "确定", "取消"))
            {
                DeleteCastle(selectedCastleId);
            }
        }
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // 道路操作
        EditorGUILayout.LabelField("道路操作", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        
        if (isRoadLinkMode)
        {
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("取消连接", GUILayout.Height(25)))
            {
                isRoadLinkMode = false;
                roadLinkStartCastleId = -1;
                Repaint();
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            if (GUILayout.Button("添加道路", GUILayout.Height(25)))
            {
                isRoadLinkMode = true;
                // 如果已选中城堡，自动设为起点
                roadLinkStartCastleId = selectedCastleId;
                selectedCastleId = -1;
                selectedRoadIndex = -1;
            }
            
            EditorGUI.BeginDisabledGroup(selectedRoadIndex < 0);
            if (GUILayout.Button("删除道路", GUILayout.Height(25)))
            {
                if (selectedRoadIndex >= 0 && currentLevel.roads != null && selectedRoadIndex < currentLevel.roads.Count)
                {
                    var road = currentLevel.roads[selectedRoadIndex];
                    if (EditorUtility.DisplayDialog("确认删除", 
                        $"确定要删除城堡 {road.startCastleId} ↔ 城堡 {road.endCastleId} 之间的道路吗？", 
                        "确定", "取消"))
                    {
                        DeleteRoad(selectedRoadIndex);
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // 选中城堡详情
        if (selectedCastleId >= 0)
        {
            var selectedCastle = currentLevel.castles?.Find(c => c.id == selectedCastleId);
            if (selectedCastle != null)
            {
                EditorGUILayout.LabelField("选中城堡", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUI.BeginChangeCheck();
                
                selectedCastle.id = EditorGUILayout.IntField("ID", selectedCastle.id);
                
                // 城堡类型（使用Enum）
                CastleType currentCastleType = (CastleType)selectedCastle.castleType;
                CastleType newCastleType = (CastleType)EditorGUILayout.EnumPopup("城堡类型", currentCastleType);
                selectedCastle.castleType = (int)newCastleType;
                
                selectedCastle.occupiedOnStart = EditorGUILayout.Toggle("开局占领", selectedCastle.occupiedOnStart);
                
                // 史莱姆类型（使用Enum）- 当开局占领为false时禁用
                EditorGUI.BeginDisabledGroup(!selectedCastle.occupiedOnStart);
                UnitType currentSlimeType = (UnitType)selectedCastle.occupiedSlimeType;
                UnitType newSlimeType = (UnitType)EditorGUILayout.EnumPopup("史莱姆类型", currentSlimeType);
                selectedCastle.occupiedSlimeType = (int)newSlimeType;
                EditorGUI.EndDisabledGroup();
                
                EditorGUI.BeginDisabledGroup(!selectedCastle.occupiedOnStart);
                selectedCastle.occupiedUnitCount = EditorGUILayout.IntField("单位数量", selectedCastle.occupiedUnitCount);
                EditorGUI.EndDisabledGroup();
                
                // 空城堡占领需求 - 只在开局占领为false时显示
                if (!selectedCastle.occupiedOnStart)
                {
                    selectedCastle.emptyCastleOccupyRequirement = EditorGUILayout.IntField("占领需求数量", selectedCastle.emptyCastleOccupyRequirement);
                }
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("位置", EditorStyles.boldLabel);
                
                if (selectedCastle.position == null)
                {
                    selectedCastle.position = new LevelConfig.Castle.Position();
                }
                
                selectedCastle.position.x = EditorGUILayout.IntField("X", selectedCastle.position.x);
                selectedCastle.position.y = EditorGUILayout.IntField("Y", selectedCastle.position.y);
                
                if (EditorGUI.EndChangeCheck())
                {
                    Repaint();
                }
                
                EditorGUILayout.EndVertical();
                
                // 关联道路
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("关联道路", EditorStyles.miniLabel);
                if (currentLevel.roads != null)
                {
                    bool hasRoad = false;
                    for (int i = 0; i < currentLevel.roads.Count; i++)
                    {
                        var road = currentLevel.roads[i];
                        if (road.startCastleId != selectedCastleId && road.endCastleId != selectedCastleId)
                            continue;
                        
                        hasRoad = true;
                        int otherCastleId = road.startCastleId == selectedCastleId 
                            ? road.endCastleId : road.startCastleId;
                        
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"  ↔ 城堡 {otherCastleId}", GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("×", GUILayout.Width(22), GUILayout.Height(18)))
                        {
                            if (EditorUtility.DisplayDialog("确认删除", 
                                $"确定要删除与城堡 {otherCastleId} 之间的道路吗？", 
                                "确定", "取消"))
                            {
                                DeleteRoad(i);
                                GUIUtility.ExitGUI();
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    if (!hasRoad)
                    {
                        EditorGUILayout.LabelField("  无关联道路", EditorStyles.miniLabel);
                    }
                }
            }
        }
        else if (selectedRoadIndex >= 0 && currentLevel?.roads != null && selectedRoadIndex < currentLevel.roads.Count)
        {
            var selectedRoad = currentLevel.roads[selectedRoadIndex];
            
            EditorGUILayout.LabelField("选中道路", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField($"起点城堡: {selectedRoad.startCastleId}");
            EditorGUILayout.LabelField($"终点城堡: {selectedRoad.endCastleId}");
            
            EditorGUILayout.Space(5);
            if (GUILayout.Button("删除此道路", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("确认删除", 
                    $"确定要删除城堡 {selectedRoad.startCastleId} ↔ 城堡 {selectedRoad.endCastleId} 之间的道路吗？", 
                    "确定", "取消"))
                {
                    DeleteRoad(selectedRoadIndex);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("点击城堡或道路以查看详情", MessageType.Info);
        }
        
        EditorGUILayout.Space(10);
        
        // 游戏配置
        if (currentLevel.config != null)
        {
            EditorGUILayout.LabelField("游戏配置", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUI.BeginChangeCheck();
            
            currentLevel.config.playerSpawnInterval = EditorGUILayout.FloatField("玩家生成间隔", currentLevel.config.playerSpawnInterval);
            currentLevel.config.playerAttackInterval = EditorGUILayout.FloatField("玩家攻击间隔", currentLevel.config.playerAttackInterval);
            currentLevel.config.enemy_1_SpawnInterval = EditorGUILayout.FloatField("敌人1生成间隔", currentLevel.config.enemy_1_SpawnInterval);
            currentLevel.config.enemy_1_AttackInterval = EditorGUILayout.FloatField("敌人1攻击间隔", currentLevel.config.enemy_1_AttackInterval);
            
            if (EditorGUI.EndChangeCheck())
            {
                Repaint();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    
    /// <summary>
    /// 世界坐标转屏幕坐标（基于屏幕框）
    /// </summary>
    private Vector2 WorldToScreenFrame(float worldX, float worldY, float minX, float maxX, float minY, float maxY, Rect screenFrameRect)
    {
        float rangeX = maxX - minX;
        float rangeY = maxY - minY;
        
        // 防止除零错误
        if (rangeX <= 0) rangeX = 1f;
        if (rangeY <= 0) rangeY = 1f;
        
        float normalizedX = (worldX - minX) / rangeX;
        float normalizedY = 1f - (worldY - minY) / rangeY; // Y轴翻转
        
        float screenX = screenFrameRect.x + normalizedX * screenFrameRect.width;
        float screenY = screenFrameRect.y + normalizedY * screenFrameRect.height;
        
        return new Vector2(screenX, screenY);
    }
    
    /// <summary>
    /// 处理鼠标事件
    /// </summary>
    private void HandleMouseEvents(Rect rect)
    {
        if (!rect.Contains(Event.current.mousePosition))
            return;
        
        Event e = Event.current;
        
        // 滚轮缩放
        if (e.type == EventType.ScrollWheel)
        {
            zoom += e.delta.y * 0.01f;
            zoom = Mathf.Clamp(zoom, 0.5f, 3f);
            Repaint();
            e.Use();
        }
    }
    
    
    /// <summary>
    /// 获取关卡名称数组
    /// </summary>
    private string[] GetLevelNames()
    {
        string[] names = new string[levels.Count];
        for (int i = 0; i < levels.Count; i++)
        {
            names[i] = $"关卡 {levels[i].levelId}";
        }
        return names;
    }
    
    /// <summary>
    /// 加载JSON文件
    /// </summary>
    private void LoadJsonFile()
    {
        string fullPath = Path.GetFullPath(jsonFilePath);
        
        if (!File.Exists(fullPath))
        {
            EditorUtility.DisplayDialog("错误", $"文件不存在: {fullPath}", "确定");
            return;
        }
        
        try
        {
            string json = File.ReadAllText(fullPath);
            levels = JsonConvert.DeserializeObject<List<LevelConfig>>(json);
            
            if (levels == null || levels.Count == 0)
            {
                EditorUtility.DisplayDialog("警告", "加载的关卡数据为空", "确定");
                return;
            }
            
            selectedLevelIndex = 0;
            currentLevel = levels[selectedLevelIndex];
            selectedCastleId = -1;
            
            Debug.Log($"成功加载 {levels.Count} 个关卡");
            Repaint();
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("错误", $"加载JSON文件失败: {e.Message}", "确定");
            Debug.LogError(e);
        }
    }
    
    /// <summary>
    /// 添加新城堡
    /// </summary>
    private void AddNewCastle()
    {
        if (currentLevel == null)
            return;
        
        if (currentLevel.castles == null)
        {
            currentLevel.castles = new List<LevelConfig.Castle>();
        }
        
        // 计算新的ID（使用当前最大ID+1）
        int newId = 0;
        foreach (var castle in currentLevel.castles)
        {
            if (castle.id >= newId)
            {
                newId = castle.id + 1;
            }
        }
        
        // 创建新城堡
        var newCastle = new LevelConfig.Castle
        {
            id = newId,
            castleType = 0,
            occupiedOnStart = false,
            occupiedSlimeType = 0,
            occupiedUnitCount = 0,
            emptyCastleOccupyRequirement = -10,
            position = new LevelConfig.Castle.Position
            {
                x = 0,
                y = 0
            }
        };
        
        currentLevel.castles.Add(newCastle);
        selectedCastleId = newId;
        
        Repaint();
    }
    
    /// <summary>
    /// 删除城堡
    /// </summary>
    private void DeleteCastle(int castleId)
    {
        if (currentLevel == null || currentLevel.castles == null)
            return;
        
        // 删除城堡
        var castleToRemove = currentLevel.castles.Find(c => c.id == castleId);
        if (castleToRemove != null)
        {
            currentLevel.castles.Remove(castleToRemove);
        }
        
        // 删除相关的道路
        if (currentLevel.roads != null)
        {
            currentLevel.roads.RemoveAll(road => 
                road.startCastleId == castleId || road.endCastleId == castleId);
        }
        
        // 清除选中状态
        selectedCastleId = -1;
        selectedRoadIndex = -1;
        
        Repaint();
    }
    
    /// <summary>
    /// 添加道路
    /// </summary>
    private void AddRoad(int startCastleId, int endCastleId)
    {
        if (currentLevel == null) return;
        
        if (currentLevel.roads == null)
            currentLevel.roads = new List<LevelConfig.Road>();
        
        if (HasRoad(startCastleId, endCastleId))
        {
            Debug.LogWarning($"Road between castle {startCastleId} and castle {endCastleId} already exists");
            return;
        }
        
        currentLevel.roads.Add(new LevelConfig.Road
        {
            startCastleId = startCastleId,
            endCastleId = endCastleId
        });
        
        Debug.Log($"Road added: castle {startCastleId} -> castle {endCastleId}");
        Repaint();
    }
    
    /// <summary>
    /// 删除道路
    /// </summary>
    private void DeleteRoad(int roadIndex)
    {
        if (currentLevel?.roads == null || roadIndex < 0 || roadIndex >= currentLevel.roads.Count)
            return;
        
        currentLevel.roads.RemoveAt(roadIndex);
        selectedRoadIndex = -1;
        Repaint();
    }
    
    /// <summary>
    /// 检查两个城堡之间是否已有道路（双向）
    /// </summary>
    private bool HasRoad(int castleId1, int castleId2)
    {
        if (currentLevel?.roads == null) return false;
        return currentLevel.roads.Exists(r =>
            (r.startCastleId == castleId1 && r.endCastleId == castleId2) ||
            (r.startCastleId == castleId2 && r.endCastleId == castleId1));
    }
    
    /// <summary>
    /// 点到线段距离
    /// </summary>
    private float PointToLineDistance(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 line = lineEnd - lineStart;
        float lenSq = line.sqrMagnitude;
        if (lenSq < 0.001f) return Vector2.Distance(point, lineStart);
        
        float t = Mathf.Clamp01(Vector2.Dot(point - lineStart, line) / lenSq);
        Vector2 projection = lineStart + t * line;
        return Vector2.Distance(point, projection);
    }
    
    /// <summary>
    /// 保存JSON文件
    /// </summary>
    private void SaveJsonFile()
    {
        if (levels == null || levels.Count == 0)
        {
            EditorUtility.DisplayDialog("错误", "没有可保存的关卡数据", "确定");
            return;
        }
        
        string fullPath = Path.GetFullPath(jsonFilePath);
        
        try
        {
            // 压缩JSON（无格式化）
            string json = JsonConvert.SerializeObject(levels, Formatting.None);
            File.WriteAllText(fullPath, json);
            
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("成功", "关卡配置已保存", "确定");
            Debug.Log($"成功保存关卡配置到: {fullPath}");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("错误", $"保存JSON文件失败: {e.Message}", "确定");
            Debug.LogError(e);
        }
    }
}

