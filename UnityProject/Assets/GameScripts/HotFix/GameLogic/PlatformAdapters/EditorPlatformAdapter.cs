using Cysharp.Threading.Tasks;

namespace GameLogic.Network
{
    /// <summary>
    /// Editor平台适配器（测试模式）
    /// </summary>
    public class EditorPlatformAdapter : IPlatformAdapter
    {
        private const string PREF_KEY = "EditorIdentityKey";
        
        public string PlatformName => "Editor";
        
        public bool RequiresSDKInit => false;
        
        public UniTask<bool> InitSDK()
        {
            return UniTask.FromResult(true);
        }
        
        public UniTask<string> GetLoginCode()
        {
#if UNITY_EDITOR
            var editorCode = UnityEditor.EditorPrefs.GetString(PREF_KEY, "");
            if (string.IsNullOrEmpty(editorCode)) editorCode = "default";
            return UniTask.FromResult(editorCode);
#endif
            return UniTask.FromResult("default");
        }
    }
}

