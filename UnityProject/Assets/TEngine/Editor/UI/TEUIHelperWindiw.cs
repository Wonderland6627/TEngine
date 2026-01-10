using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
#region
//作者:Saber
#endregion
namespace TEngine.Editor.UI
{

    public class TEUIToolWindow : OdinEditorWindow
    {
        public enum Type
        {
            normal,
            unitask,
            listener,
            all
        }
        [ReadOnly, LabelText("选中物体")]
        public Transform root;
        [LabelText("生成类型")]
        public Type type;
        [LabelText("命名空间")]
        public string nameSpace = "";
        [LabelText("类名"), OnValueChanged("OnClassNameChanged")]
        public string className;
        [LabelText("生成位置"), FolderPath, OnValueChanged("OnSavePathChanged")]
        public string savePath= "Assets/Scripts/UIScriptsAuto";
        
        // 获取用于路径的类名（去掉Window后缀）
        private string GetClassNameForPath()
        {
            if (string.IsNullOrEmpty(className))
                return className;
            
            // 如果类名以"Window"结尾，则去掉这个后缀
            if (className.EndsWith("Window"))
            {
                return className.Substring(0, className.Length - 6); // "Window"长度为6
            }
            
            return className;
        }
        
        // 真正的文件生成路径（只读，自动拼接生成位置和类名）
        [ReadOnly, LabelText("文件生成路径")]
        [ShowInInspector]
        private string ActualSavePath
        {
            get
            {
                if (string.IsNullOrEmpty(className))
                    return savePath;
                var classNameForPath = GetClassNameForPath();
                return $"{savePath}/{classNameForPath}";
            }
        }

        [ReadOnly, LabelText("生成的脚本"),HorizontalGroup(GroupID ="BS")]
        public TextAsset buildScript;

        protected override void OnDisable()
        {
            base.OnDisable();
            Save();

        }
        void Save()
        {
            EditorPrefs.SetString($"Last_UIScriptsAutoNameSpace", nameSpace);
            EditorPrefs.SetInt($"Last_UIScriptsAutoType", (int)type);
            EditorPrefs.SetString($"Last_UIScriptsAutoSavePath", savePath);
        }
        void Load()
        {
            nameSpace = EditorPrefs.GetString($"Last_UIScriptsAutoNameSpace", nameSpace);
            type = (Type)EditorPrefs.GetInt($"Last_UIScriptsAutoType", (int)type);
            var newSavePath = EditorPrefs.GetString($"Last_UIScriptsAutoSavePath", savePath);
            if (string.IsNullOrEmpty(newSavePath)==false)
            {
                savePath = newSavePath;
                OnSavePathChange();
            }
        }
        void OnSavePathChange()
        {
            var originPath = $"{ActualSavePath}/{className}_Auto.cs";
            var path = Path.GetFullPath(originPath);
            if (File.Exists(path))
                buildScript = AssetDatabase.LoadAssetAtPath<TextAsset>(originPath);
        }
        
        void OnSavePathChanged()
        {
            OnSavePathChange();
        }
        
        void OnClassNameChanged()
        {
            OnSavePathChange();
        }
        [Button(SdfIconType.Trash, Name = ""), HorizontalGroup(GroupID = "BS", Width = 50)]
        void Delete()
        {
            AssetDatabase.DeleteAsset($"{ActualSavePath}/{className}.cs");
            buildScript = null;
        }
        [Sirenix.OdinInspector.PropertySpace(30)]
        [Button(Icon = SdfIconType.Gem, Name = "生成"), HorizontalGroup(GroupID = "AA" /*, Width = 150*/,MinWidth =200,MarginLeft =0.3f,MarginRight =0.3f)]
        public void Build()
        {
            var str = TEUIHelper.Generate(type == Type.listener || type == Type.all, type == Type.unitask || type == Type.all, root, nameSpace, className);
            if (string.IsNullOrEmpty(str))
            {
                Debug.LogError("出错啦，请检查是否选中root为空" + root == null);
                return;
            }
            var originPath = $"{ActualSavePath}/{className}_Auto.cs";
            var path = Path.GetFullPath(originPath);
            MakeSure(path);
            File.WriteAllText(path, str);
            AssetDatabase.Refresh();
            buildScript= AssetDatabase.LoadAssetAtPath<TextAsset>(originPath);
            Debug.Log($"保存完毕:Cs->{className}.cs ,位置:{path}");
        }

        public void OnInit(Transform root)
        {
            this.root = root;
            this.className = this.root.name;
            Load();
        }
        [MenuItem("GameObject/ScriptGenerator/BuildWindow", priority = 40)]
        static void OpenWindow()
        {
            var root = Selection.activeTransform;
            if (root != null && (PrefabUtility.IsPartOfAnyPrefab(root) || (PrefabStageUtility.GetCurrentPrefabStage() != null && PrefabStageUtility.GetCurrentPrefabStage().IsPartOfPrefabContents(root.gameObject))))
            {
                var window = GetWindow<TEUIToolWindow>();
                // 设置窗口大小
                window.minSize = new Vector2(600, 250);
                window.OnInit(root);

            }
            else
                Debug.LogError("仅支持预制体，请选中预制体");
        }
        static void OpenTopWindow()
        {
            var window = EditorWindow.GetWindow<TEUIToolWindow>();
            //Debug.Log("windiw" + window.name + "_aaa");
            window.position = new Rect(0, 0, 600, 800);
        }

        public static void MakeSure(string path)
        {
            path = path.Replace("\\", "/");
            var extion = Path.GetExtension(path);

            var dire = System.IO.Path.GetDirectoryName(path);
            if (System.IO.Directory.Exists(dire) == false)
                System.IO.Directory.CreateDirectory(dire);

            if (string.IsNullOrEmpty(extion) == false)
            {
                if (System.IO.File.Exists(path) == false)
                    System.IO.File.Create(path).Dispose();
            }
        }
    }
}

