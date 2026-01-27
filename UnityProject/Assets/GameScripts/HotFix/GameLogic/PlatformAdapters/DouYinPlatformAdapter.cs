using Cysharp.Threading.Tasks;
using TEngine;

namespace GameLogic.Network
{
    /// <summary>
    /// 抖音平台适配器（预留接口，待实现）
    /// </summary>
    public class DouYinPlatformAdapter : IPlatformAdapter
    {
        public string PlatformName => "douyin";
        
        public bool RequiresSDKInit => true;
        
        public UniTask<bool> InitSDK()
        {
            // TODO: 实现抖音SDK初始化
            Log.Warning("[DouYinPlatformAdapter] DouYin SDK initialization not implemented yet");
            return UniTask.FromResult(false);
        }
        
        public UniTask<string> GetLoginCode()
        {
            // TODO: 实现抖音登录获取code
            Log.Warning("[DouYinPlatformAdapter] DouYin login not implemented yet");
            return UniTask.FromResult<string>(null);
        }
    }
}

