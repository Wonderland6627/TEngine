using Cysharp.Threading.Tasks;
using TEngine;

namespace GameLogic.Network
{
    /// <summary>
    /// B站平台适配器（预留接口，待实现）
    /// </summary>
    public class BilibiliPlatformAdapter : IPlatformAdapter
    {
        public string PlatformName => "bilibili";
        
        public bool RequiresSDKInit => true;
        
        public UniTask<bool> InitSDK()
        {
            // TODO: 实现B站SDK初始化
            Log.Warning("[BilibiliPlatformAdapter] Bilibili SDK initialization not implemented yet");
            return UniTask.FromResult(false);
        }
        
        public UniTask<string> GetLoginCode()
        {
            // TODO: 实现B站登录获取code
            Log.Warning("[BilibiliPlatformAdapter] Bilibili login not implemented yet");
            return UniTask.FromResult<string>(null);
        }
    }
}

