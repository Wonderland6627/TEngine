using Cysharp.Threading.Tasks;
using TEngine;
using WeChatWASM;

namespace GameLogic.Network
{
    /// <summary>
    /// 微信平台适配器
    /// </summary>
    public class WeChatPlatformAdapter : IPlatformAdapter
    {
        private static string WXCloudENV = "slimecloudservice-6enxmrfbc5bddc";
        
        public string PlatformName => "wechat";
        
        public bool RequiresSDKInit => true;
        
        public async UniTask<bool> InitSDK()
        {
            var tcs = new UniTaskCompletionSource<bool>();
            
            Log.Info("[WeChatPlatformAdapter] InitSDK: WX.InitSDK");
            WX.InitSDK((code) =>
            {
                Log.Info($"[WeChatPlatformAdapter] WX.InitSDK callback code: {code}");
                
                WX.cloud.Init(new ICloudConfig()
                {
                    env = WXCloudENV,
                    traceUser = true
                });
                tcs.TrySetResult(true);
            });
            
            return await tcs.Task;
        }
        
        public async UniTask<string> GetLoginCode()
        {
            var tcs = new UniTaskCompletionSource<string>();
            
            WX.Login(new LoginOption()
            {
                success = (res) =>
                {
                    Log.Info($"[WeChatPlatformAdapter] WX.Login success: code={res.code}");
                    tcs.TrySetResult(res.code);
                },
                fail = (err) =>
                {
                    Log.Error($"[WeChatPlatformAdapter] WX.Login failed: {err.ToJson()}");
                    tcs.TrySetResult(null);
                }
            });
            
            return await tcs.Task;
        }
    }
}

