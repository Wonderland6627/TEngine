using Cysharp.Threading.Tasks;

namespace GameLogic.Network
{
    /// <summary>
    /// Editor平台适配器（测试模式）
    /// </summary>
    public class EditorPlatformAdapter : IPlatformAdapter
    {
        public string PlatformName => "Editor";
        
        public bool RequiresSDKInit => false;
        
        public UniTask<bool> InitSDK()
        {
            return UniTask.FromResult(true);
        }
        
        public UniTask<string> GetLoginCode()
        {
            // Editor环境下code可以是任意值
            return UniTask.FromResult("editor");
        }
    }
}

