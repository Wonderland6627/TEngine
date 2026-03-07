using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GameConfig;
using Luban;
using SimpleJSON;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Luban 配置查看器。
/// 自动检测当前生成代码模式（cs-bin / cs-simple-json），加载对应格式的数据文件。
/// 使用前需先运行 "Luban 转表" 生成代码和数据。
/// </summary>
public class LubanConfigViewer : OdinMenuEditorWindow
{
    private static string BytesDir => Path.Combine(Application.dataPath, "AssetRaw/Configs/bytes");
    private static string JsonsDir => Path.Combine(Application.dataPath, "AssetRaw/Configs/jsons");

    private Tables _tables;
    private string _error;
    private readonly Dictionary<string, TableViewData> _tableViews = new Dictionary<string, TableViewData>();

    private Vector2 _rightScrollPos;
    private string _searchKey = "";
    private int _pageSize = 50;
    private int _currentPage;

    [MenuItem("PirateCat/配置查看器")]
    private static void ShowWindow()
    {
        var window = GetWindow<LubanConfigViewer>();
        window.titleContent = new GUIContent("Luban 配置查看器");
        window.minSize = new Vector2(800, 500);
        window.Show();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        tree.Selection.SupportsMultiSelect = false;

        _error = null;
        _tableViews.Clear();

        if (!TryLoadTables(out var error))
        {
            _error = error;
            return tree;
        }

        foreach (var prop in typeof(Tables).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead) continue;

            var tableInstance = prop.GetValue(_tables);
            if (tableInstance == null) continue;

            var view = BuildTableView(prop.Name, tableInstance);
            if (view == null) continue;

            _tableViews[prop.Name] = view;
            tree.Add($"{prop.Name}  ({view.Count})", view);
        }

        return tree;
    }

    #region 工具栏

    protected override void OnBeginDrawEditors()
    {
        if (MenuTree == null) return;

        SirenixEditorGUI.BeginHorizontalToolbar();
        {
            if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                ForceMenuTreeRebuild();
            }

            GUILayout.FlexibleSpace();

            var selected = MenuTree.Selection?.SelectedValue as TableViewData;
            if (selected != null)
            {
                GUILayout.Label("搜索主键:", EditorStyles.toolbarButton, GUILayout.Width(60));
                var newSearch = EditorGUILayout.TextField(_searchKey, EditorStyles.toolbarSearchField,
                    GUILayout.Width(150));
                if (newSearch != _searchKey)
                {
                    _searchKey = newSearch;
                    _currentPage = 0;
                }
            }
        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }

    #endregion

    #region 右侧面板绘制

    protected override void DrawEditor(int index)
    {
        if (!string.IsNullOrEmpty(_error))
        {
            EditorGUILayout.HelpBox(_error, MessageType.Error);
            return;
        }

        if (!(MenuTree?.Selection?.SelectedValue is TableViewData view))
        {
            EditorGUILayout.HelpBox("请从左侧选择一张配置表。", MessageType.Info);
            return;
        }

        var filtered = GetFilteredRecords(view);

        DrawTableHeader(view, filtered.Count);
        DrawPagination(filtered.Count);

        _rightScrollPos = EditorGUILayout.BeginScrollView(_rightScrollPos);
        {
            int start = _currentPage * _pageSize;
            int end = Mathf.Min(start + _pageSize, filtered.Count);

            for (int i = start; i < end; i++)
            {
                DrawRecord(filtered[i], i, view);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawTableHeader(TableViewData view, int filteredCount)
    {
        SirenixEditorGUI.BeginBox();
        {
            EditorGUILayout.LabelField(view.Name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                filteredCount == view.Count
                    ? $"共 {view.Count} 条记录"
                    : $"筛选 {filteredCount} / {view.Count} 条记录",
                EditorStyles.miniLabel);
        }
        SirenixEditorGUI.EndBox();
    }

    private void DrawPagination(int totalCount)
    {
        if (totalCount <= _pageSize) return;

        int totalPages = Mathf.CeilToInt((float)totalCount / _pageSize);
        _currentPage = Mathf.Clamp(_currentPage, 0, totalPages - 1);

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUI.BeginDisabledGroup(_currentPage <= 0);
            if (GUILayout.Button("◀ 上一页", GUILayout.Width(80)))
                _currentPage--;
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"第 {_currentPage + 1} / {totalPages} 页", GUILayout.Width(100));
            GUILayout.FlexibleSpace();

            EditorGUI.BeginDisabledGroup(_currentPage >= totalPages - 1);
            if (GUILayout.Button("下一页 ▶", GUILayout.Width(80)))
                _currentPage++;
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawRecord(RecordViewData record, int index, TableViewData view)
    {
        string title = view.PrimaryKeyField != null
            ? $"#{index}  {view.PrimaryKeyField.Name} = {record.PrimaryKeyValue}"
            : $"#{index}";

        SirenixEditorGUI.BeginBox();
        {
            SirenixEditorGUI.BeginBoxHeader();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            SirenixEditorGUI.EndBoxHeader();

            foreach (var field in record.Fields)
            {
                DrawFieldValue(field.Name, field.Value, field.FieldType, 0);
            }
        }
        SirenixEditorGUI.EndBox();
        EditorGUILayout.Space(2);
    }

    private void DrawFieldValue(string label, object value, Type type, int depth)
    {
        if (depth > 5) return;

        if (value == null)
        {
            EditorGUILayout.LabelField(label, "null");
            return;
        }

        if (IsPrimitive(type))
        {
            EditorGUILayout.LabelField(label, value.ToString());
            return;
        }

        if (value is BeanBase bean)
        {
            DrawBeanFoldout(label, bean, depth);
            return;
        }

        if (value is IList list)
        {
            DrawListFoldout(label, list, depth);
            return;
        }

        EditorGUILayout.LabelField(label, value.ToString());
    }

    private void DrawBeanFoldout(string label, BeanBase bean, int depth)
    {
        int id = GUIUtility.GetControlID(FocusType.Passive);
        string key = $"bean_{id}_{label}_{depth}";
        bool foldout = GetFoldout(key);

        foldout = EditorGUILayout.Foldout(foldout, $"{label}  ({bean.GetType().Name})", true);
        SetFoldout(key, foldout);

        if (!foldout) return;

        EditorGUI.indentLevel++;
        foreach (var f in bean.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (IsRefField(f)) continue;
            DrawFieldValue(f.Name, f.GetValue(bean), f.FieldType, depth + 1);
        }
        EditorGUI.indentLevel--;
    }

    private void DrawListFoldout(string label, IList list, int depth)
    {
        int id = GUIUtility.GetControlID(FocusType.Passive);
        string key = $"list_{id}_{label}_{depth}";
        bool foldout = GetFoldout(key);

        foldout = EditorGUILayout.Foldout(foldout, $"{label}  [{list.Count}]", true);
        SetFoldout(key, foldout);

        if (!foldout) return;

        EditorGUI.indentLevel++;
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item is BeanBase itemBean)
                DrawBeanFoldout($"[{i}]", itemBean, depth + 1);
            else
                DrawFieldValue($"[{i}]", item, item?.GetType() ?? typeof(object), depth + 1);
        }
        EditorGUI.indentLevel--;
    }

    #endregion

    #region 数据加载

    private bool TryLoadTables(out string error)
    {
        error = null;

        var ctor = typeof(Tables).GetConstructors();
        if (ctor.Length == 0)
        {
            error = "Tables 类没有公开构造函数，请先运行 \"Luban 转表\"。";
            return false;
        }

        var param = ctor[0].GetParameters();
        if (param.Length == 0)
        {
            error = "Tables 构造函数无参数，无法识别加载模式。";
            return false;
        }

        var loaderReturnType = param[0].ParameterType.GetGenericArguments()[1];
        bool isBinaryMode = loaderReturnType == typeof(ByteBuf);

        string dataDir = isBinaryMode ? BytesDir : JsonsDir;
        string ext = isBinaryMode ? "*.bytes" : "*.json";
        string modeName = isBinaryMode ? "Binary" : "JSON";

        if (!Directory.Exists(dataDir))
        {
            error = $"数据目录不存在: {dataDir}\n当前模式: {modeName}\n请先运行 \"Luban 转表\" 生成配置。";
            return false;
        }

        if (Directory.GetFiles(dataDir, ext).Length == 0)
        {
            error = $"数据目录为空: {dataDir}\n当前模式: {modeName}\n请先运行 \"Luban 转表\" 生成配置。";
            return false;
        }

        try
        {
            object loader;
            if (isBinaryMode)
            {
                Func<string, ByteBuf> fn = file =>
                {
                    string path = Path.Combine(dataDir, file + ".bytes");
                    if (!File.Exists(path))
                        throw new FileNotFoundException($"Config file not found: {path}");
                    return new ByteBuf(File.ReadAllBytes(path));
                };
                loader = fn;
            }
            else
            {
                Func<string, JSONNode> fn = file =>
                {
                    string path = Path.Combine(dataDir, file + ".json");
                    if (!File.Exists(path))
                        throw new FileNotFoundException($"Config file not found: {path}");
                    return JSON.Parse(File.ReadAllText(path));
                };
                loader = fn;
            }

            _tables = (Tables)ctor[0].Invoke(new object[] { loader });
        }
        catch (Exception e)
        {
            var inner = e.InnerException ?? e;
            error = $"加载配置表失败 ({modeName} 模式): {inner.Message}";
            return false;
        }

        return true;
    }

    private static TableViewData BuildTableView(string propName, object tableInstance)
    {
        var tableType = tableInstance.GetType();
        var dataListProp = tableType.GetProperty("DataList");
        if (dataListProp == null) return null;

        if (!(dataListProp.GetValue(tableInstance) is IList dataList)) return null;

        Type recordType = null;
        if (dataListProp.PropertyType.IsGenericType)
            recordType = dataListProp.PropertyType.GetGenericArguments()[0];

        var fields = recordType?.GetFields(BindingFlags.Public | BindingFlags.Instance);
        FieldInfo primaryKey = null;
        if (fields != null && fields.Length > 0 && !IsRefField(fields[0]))
            primaryKey = fields[0];

        var records = new List<RecordViewData>(dataList.Count);
        foreach (var record in dataList)
        {
            var fieldValues = new List<FieldViewData>();
            if (fields != null)
            {
                foreach (var f in fields)
                {
                    if (IsRefField(f)) continue;
                    fieldValues.Add(new FieldViewData
                    {
                        Name = f.Name,
                        Value = f.GetValue(record),
                        FieldType = f.FieldType
                    });
                }
            }

            records.Add(new RecordViewData
            {
                Instance = record,
                Fields = fieldValues,
                PrimaryKeyValue = primaryKey?.GetValue(record)
            });
        }

        return new TableViewData
        {
            Name = propName,
            Count = dataList.Count,
            Records = records,
            PrimaryKeyField = primaryKey
        };
    }

    #endregion

    #region 筛选

    private List<RecordViewData> GetFilteredRecords(TableViewData view)
    {
        if (string.IsNullOrEmpty(_searchKey)) return view.Records;

        var result = new List<RecordViewData>();
        foreach (var r in view.Records)
        {
            if (r.PrimaryKeyValue != null && r.PrimaryKeyValue.ToString().Contains(_searchKey))
                result.Add(r);
        }
        return result;
    }

    #endregion

    #region 辅助方法

    private static bool IsPrimitive(Type type)
    {
        if (type == null) return true;
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying != null) type = underlying;
        return type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type.IsEnum;
    }

    private static bool IsRefField(FieldInfo f)
    {
        return f.Name.EndsWith("_Ref") || f.Name == "__ID__";
    }

    private readonly Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();

    private bool GetFoldout(string key)
    {
        return _foldoutStates.TryGetValue(key, out var v) && v;
    }

    private void SetFoldout(string key, bool value)
    {
        _foldoutStates[key] = value;
    }

    #endregion

    #region 视图数据结构

    private class TableViewData
    {
        public string Name;
        public int Count;
        public List<RecordViewData> Records;
        public FieldInfo PrimaryKeyField;
    }

    private class RecordViewData
    {
        public object Instance;
        public List<FieldViewData> Fields;
        public object PrimaryKeyValue;
    }

    private class FieldViewData
    {
        public string Name;
        public object Value;
        public Type FieldType;
    }

    #endregion
}
