using Cysharp.Threading.Tasks;

namespace GameLogic.Network
{
    /// <summary>
    /// 平台适配器接口
    /// 每个平台需要实现此接口来提供平台相关功能（登录、SDK初始化等）
    /// 未来可扩展：分享、支付、广告等功能
    /// </summary>
    public interface IPlatformAdapter
    {
        /// <summary>
        /// 平台名称（与后端保持一致）
        /// </summary>
        string PlatformName { get; }
        
        /// <summary>
        /// 获取登录凭证code
        /// </summary>
        /// <returns>登录凭证code，失败返回null</returns>
        UniTask<string> GetLoginCode();
        
        /// <summary>
        /// 是否需要初始化SDK（如微信需要先初始化）
        /// </summary>
        bool RequiresSDKInit { get; }
        
        /// <summary>
        /// 初始化平台SDK（如果需要）
        /// </summary>
        UniTask<bool> InitSDK();
    }
}

