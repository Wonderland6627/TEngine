using UnityEditor;
using UnityEngine;

namespace TEngine.Editor
{
    public static class LubanTools
    {
        private static string ConfigRoot => Application.dataPath + @"/../../Configs/GameConfig";

        [MenuItem("TEngine/Tools/Luban 转表 (JSON)")]
        public static void BuildLubanJson()
        {
            Application.OpenURL(ConfigRoot + @"/gen_code_bin_to_project.bat");
        }

        [MenuItem("TEngine/Tools/Luban 转表 (Binary)")]
        public static void BuildLubanBin()
        {
            Application.OpenURL(ConfigRoot + @"/gen_code_bin_to_project_lazyload.bat");
        }

        [MenuItem("TEngine/Tools/Luban 转表服务端 (JSON)")]
        public static void BuildLubanServerJson()
        {
            Application.OpenURL(ConfigRoot + @"/gen_code_bin_to_server.bat");
        }

        [MenuItem("TEngine/Tools/打开表格目录")]
        public static void OpenConfigFolder()
        {
            OpenFolderHelper.Execute(ConfigRoot);
        }
    }
}