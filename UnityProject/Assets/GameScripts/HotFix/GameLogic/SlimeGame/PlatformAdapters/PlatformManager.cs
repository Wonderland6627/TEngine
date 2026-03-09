using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TEngine;
using GameLogic.Network;

namespace GameLogic.Network
{
    /// <summary>
    /// 平台管理器
    /// 统一管理各平台的适配器（登录、SDK初始化等）
    /// 未来可扩展：分享、支付、广告等功能
    /// </summary>
    public static class PlatformManager
    {
        private static readonly Dictionary<string, IPlatformAdapter> _adapters = new Dictionary<string, IPlatformAdapter>();
        private static IPlatformAdapter _currentAdapter = null;
        
        /// <summary>
        /// 初始化平台管理器（注册所有平台适配器）
        /// </summary>
        public static void Initialize()
        {
            RegisterAdapter(new EditorPlatformAdapter());
            RegisterAdapter(new WeChatPlatformAdapter());
            RegisterAdapter(new DouYinPlatformAdapter());
            RegisterAdapter(new BilibiliPlatformAdapter());
            
            Log.Info($"[PlatformManager] Initialized with {_adapters.Count} platform adapters");
        }
        
        /// <summary>
        /// 注册平台适配器
        /// </summary>
        public static void RegisterAdapter(IPlatformAdapter adapter)
        {
            if (adapter == null)
            {
                Log.Error("[PlatformManager] Cannot register null adapter");
                return;
            }
            
            _adapters[adapter.PlatformName] = adapter;
            Log.Info($"[PlatformManager] Registered platform adapter: {adapter.PlatformName}");
        }
        
        /// <summary>
        /// 获取平台适配器
        /// </summary>
        public static IPlatformAdapter GetAdapter(string platformName)
        {
            if (_adapters.TryGetValue(platformName, out var adapter))
            {
                return adapter;
            }
            
            Log.Error($"[PlatformManager] Platform adapter not found: {platformName}");
            return null;
        }
        
        /// <summary>
        /// 设置当前使用的平台（根据编译条件自动选择）
        /// </summary>
        public static void SetCurrentPlatform()
        {
#if UNITY_EDITOR
            _currentAdapter = GetAdapter("Editor");
#else
            // 运行时可以根据配置或环境变量选择平台
            // 目前默认使用微信
            _currentAdapter = GetAdapter("wechat");
#endif
            
            if (_currentAdapter != null)
            {
                Log.Info($"[PlatformManager] Current platform set to: {_currentAdapter.PlatformName}");
            }
        }
        
        /// <summary>
        /// 获取当前平台适配器
        /// </summary>
        public static IPlatformAdapter GetCurrentAdapter()
        {
            if (_currentAdapter == null)
            {
                SetCurrentPlatform();
            }
            return _currentAdapter;
        }
        
        /// <summary>
        /// 执行登录流程（统一入口）
        /// </summary>
        public static async UniTask<bool> Login()
        {
            var adapter = GetCurrentAdapter();
            if (adapter == null)
            {
                Log.Error("[PlatformManager] No platform adapter available");
                return false;
            }
            
            Log.Info($"[PlatformManager] Starting login for platform: {adapter.PlatformName}");
            
            // 步骤1: 初始化SDK（如果需要）
            if (adapter.RequiresSDKInit)
            {
                bool initSuccess = await adapter.InitSDK();
                if (!initSuccess)
                {
                    Log.Error($"[PlatformManager] SDK initialization failed for {adapter.PlatformName}");
                    return false;
                }
            }
            
            // 步骤2: 获取登录凭证code
            string code = await adapter.GetLoginCode();
            if (string.IsNullOrEmpty(code))
            {
                Log.Error($"[PlatformManager] Failed to get login code for {adapter.PlatformName}");
                return false;
            }
            
            // 步骤3: 使用code登录（公共逻辑）
            return await LoginWithCode(code, adapter.PlatformName);
        }
        
        /// <summary>
        /// 使用code登录（公共逻辑）
        /// </summary>
        private static async UniTask<bool> LoginWithCode(string code, string platform)
        {
            var loginRequest = new Dictionary<string, object>
            {
                { "code", code },
                { "platform", platform }
            };
            
            var response = await NetManager.Instance.CallHttp<SessionData>("getCode2Session", loginRequest);
            if (!response.IsSuccess || response.data == null)
            {
                Log.Error($"[PlatformManager] LoginWithCode failed ({platform}): {response.ErrorMessage}");
                return false;
            }

            // 保存token
            NetManager.Instance.AuthToken = response.data.token;
            Log.Info($"[PlatformManager] LoginWithCode success ({platform}): openid={response.data.openid}, token saved");
            
            // 更新本地用户信息（如果需要，可以在外部处理）
            // 这里只负责登录和保存token
            
            return true;
        }
        
        /// <summary>
        /// 手动指定平台登录（用于测试或切换平台）
        /// </summary>
        public static async UniTask<bool> LoginWithPlatform(string platformName)
        {
            var adapter = GetAdapter(platformName);
            if (adapter == null)
            {
                return false;
            }
            
            var originalAdapter = _currentAdapter;
            _currentAdapter = adapter;
            
            try
            {
                return await Login();
            }
            finally
            {
                _currentAdapter = originalAdapter;
            }
        }
    }
}

