# NetManager 设计文档

## 一、响应格式规范

### 标准响应格式
```json
{
  "code": 0,        // 0=成功, 非0=失败
  "data": {},       // 业务数据（任意类型）
  "msg": "success"  // 消息描述
}
```

### 错误码定义

**服务端错误码**（`ResponseCode`）：
- `0` - SUCCESS：成功
- `-1` - ERROR：通用错误
- `-2` - UNAUTHORIZED：未授权（Token无效/过期）
- `-3` - NOT_FOUND：资源未找到
- `-4` - VALIDATION_ERROR：参数验证失败

**客户端错误码**（`ResponseCode.CLIENT_*`）：
- `-100` - CLIENT_NO_NETWORK：无网络连接
- `-101` - CLIENT_TIMEOUT：请求超时
- `-102` - CLIENT_PARSE_ERROR：响应解析失败
- `-103` - CLIENT_UNKNOWN_API：未知API
- `-104` - CLIENT_REQUEST_FAILED：请求失败（重试后仍失败）

**代码位置**：`Assets/GameScripts/HotFix/GameLogic/Network/CloudResponse.cs`

---

## 二、NetManager 核心功能

### 2.1 云函数调用

**方法**：`Call<T>()` / `CallData<T>()`

**功能**：
- 调用微信云函数
- 自动解析响应
- 统一错误处理

**代码位置**：`Assets/GameScripts/HotFix/GameLogic/Network/NetManager.cs`

### 2.2 HTTP接口调用

**方法**：`CallHttp<T>()` / `CallHttpData<T>()`

**功能**：
- 调用Express服务端HTTP接口
- 自动添加Authorization header（Bearer Token）
- 支持请求超时（30秒）
- 支持自动重试（最多3次）
- 网络状态检查

**特性**：
- ✅ 请求超时配置：30秒
- ✅ 自动重试机制：连接错误时最多重试3次
- ✅ 网络状态检查：无网络时直接返回错误
- ✅ 内存优化：使用 `using` 确保资源释放

**代码位置**：`Assets/GameScripts/HotFix/GameLogic/Network/NetManager.cs` - `CallHttp<T>()`

### 2.3 Token管理

**属性**：`AuthToken`

**功能**：
- 自动保存到 `PlayerPrefs`
- 自动从 `PlayerPrefs` 加载
- HTTP请求自动携带（`Authorization: Bearer {token}`）

**Token有效性检查**：
- `NeedLogin` - 判断是否需要登录
- `GetTokenState()` - 获取Token详细状态
- `GetTokenRemainingSeconds()` - 获取剩余有效时间（秒）

**代码位置**：`Assets/GameScripts/HotFix/GameLogic/Network/NetManager.cs` - Token相关方法

### 2.4 服务器配置

**功能**：
- 支持多服务器环境（Dev/Production）
- 调试器服务器切换功能
- 服务器地址自动切换

**代码位置**：`Assets/GameScripts/HotFix/GameLogic/Network/NetManager.cs` - `ServerType` / `CurrentServerType`

---

## 三、使用示例

### 3.1 HTTP接口调用（推荐）

**获取用户信息**：
```csharp
// 方式1：获取完整响应（推荐，可获取错误信息）
var response = await NetManager.CallHttp<UserGameInfoData>("getUserGameInfoV2");
if (response.IsSuccess) {
    var userInfo = response.data;
    // 使用 userInfo
} else {
    Log.Error($"Error: {response.ErrorMessage}");
    
    // 检查是否为未授权错误
    if (response.IsUnauthorized) {
        // Token过期，重新登录
        await World.Instance.Login();
    }
}

// 方式2：直接获取数据（简洁，但无法获取错误信息）
var userInfo = await NetManager.CallHttpData<UserGameInfoData>("getUserGameInfoV2");
if (userInfo != null) {
    // 使用 userInfo
}
```

**设置用户信息**：
```csharp
var response = await NetManager.CallHttp<object>("setUserGameInfoV2", new {
    progressLevelID = 10,
    nickName = "玩家昵称"
});
if (response.IsSuccess) {
    Log.Info("设置成功");
}
```

**获取排行榜**：
```csharp
var rankList = await NetManager.CallHttpData<List<UserGameInfoData>>("getUserRankListV2");
if (rankList != null) {
    // 使用 rankList
}
```

### 3.2 云函数调用（微信云开发）

**调用云函数**：
```csharp
var response = await NetManager.Call<CloudResponse<UserInfo>>("getUserGameInfo");
if (response.IsSuccess) {
    var userInfo = response.data;
    // 使用 userInfo
}

// 或使用便捷方法
var userInfo = await NetManager.CallData<UserInfo>("getUserGameInfo");
```

### 3.3 Token管理

**检查Token状态**：
```csharp
// 启动时检查
if (NetManager.NeedLogin) {
    await World.Instance.Login();
} else {
    long remaining = NetManager.GetTokenRemainingSeconds();
    Log.Info($"Token有效，剩余时间：{remaining}秒");
}

// 清除Token（登出时）
NetManager.ClearToken();
```

### 3.4 服务器切换（调试用）

```csharp
// 切换到开发服务器
NetManager.CurrentServerType = ServerType.Dev;

// 切换到生产服务器
NetManager.CurrentServerType = ServerType.Production;
```

---

## 四、设计要点

### 4.1 核心优势

1. **简洁**：一行代码完成调用和解析
2. **统一**：所有接口调用使用相同接口
3. **类型安全**：泛型自动解析，编译期检查
4. **错误处理**：统一的错误处理和重试机制
5. **Token管理**：自动保存、加载、携带Token
6. **网络优化**：超时控制、自动重试、网络状态检查

### 4.2 核心流程

**HTTP请求流程**：
```
CallHttp<T>()
  → 网络状态检查
    → API路由查找
      → SendHttpRequestWithRetry()
        → SendHttpRequestInternal()
          ├─ 设置超时（30秒）
          ├─ 添加Authorization header
          ├─ 发送请求
          └─ 解析响应
            → 返回 Response<T>
```

**Token管理流程**：
```
启动
  → NetManager.Initialize()
    → 从PlayerPrefs加载Token
      → 解析JWT exp字段
        → 判断Token状态
          ├─ 有效 → 直接使用
          └─ 无效/过期 → 触发登录
```

### 4.3 错误处理机制

**重试策略**：
- 只对连接错误（ConnectionError）重试
- 协议错误（404、500等）不重试
- 最多重试3次，每次间隔1秒

**错误码处理**：
- 服务端错误码：直接返回给业务层
- 客户端错误码：网络、解析等本地错误
- 401/UNAUTHORIZED：可触发重新登录流程

---

## 五、已实现的功能

### 5.1 网络优化

**✅ 请求超时**：
- 默认30秒超时
- 超时后自动返回错误

**✅ 自动重试**：
- 连接错误时自动重试
- 最多重试3次，间隔1秒
- 协议错误不重试

**✅ 网络状态检查**：
- 请求前检查网络连接
- 无网络时直接返回错误（code = -100）

**✅ 内存优化**：
- 使用 `using` 确保 UploadHandler/DownloadHandler 释放
- 避免内存泄漏

### 5.2 Token管理

**✅ Token缓存**：
- 自动保存到 PlayerPrefs
- 启动时自动加载

**✅ Token有效性检查**：
- 解析JWT exp字段
- 启动时检查，有效则跳过登录

**✅ Token过期处理**：
- 客户端预检（优化）
- 服务端验证（安全保障）

### 5.3 错误码系统

**✅ 统一错误码**：
- 服务端错误码（0, -1, -2, -3, -4）
- 客户端错误码（-100 ~ -199）
- 避免错误码冲突

**✅ 错误码辅助属性**：
- `IsSuccess` - 是否成功
- `IsUnauthorized` - 是否未授权
- `IsClientError` - 是否客户端错误
- `ErrorMessage` - 错误消息

## 六、未来扩展建议（可选）

### 6.1 请求队列（可选）
```csharp
// 限制并发请求数量
private static readonly SemaphoreSlim _requestSemaphore = new SemaphoreSlim(5, 5);

public static async UniTask<Response<T>> CallHttp<T>(string apiName, object parameters = null)
{
    await _requestSemaphore.WaitAsync();
    try {
        // 执行请求
    } finally {
        _requestSemaphore.Release();
    }
}
```

### 6.2 请求缓存（可选）
```csharp
// 对GET请求或特定接口添加缓存
private static readonly Dictionary<string, CacheEntry> _cache = new();

public static async UniTask<T> CallHttpDataWithCache<T>(
    string apiName, 
    object parameters = null,
    float cacheTime = 60f
) where T : class
{
    string cacheKey = $"{apiName}_{parameters?.ToJson()}";
    if (_cache.TryGetValue(cacheKey, out var entry) && !entry.IsExpired) {
        return entry.Data as T;
    }
    
    var data = await CallHttpData<T>(apiName, parameters);
    if (data != null) {
        _cache[cacheKey] = new CacheEntry(data, cacheTime);
    }
    return data;
}
```

### 6.3 请求拦截器（可选）
```csharp
// 统一处理401错误，自动重新登录
public static async UniTask<Response<T>> CallHttp<T>(string apiName, object parameters = null)
{
    var response = await SendHttpRequestInternal<T>(...);
    
    if (response.IsUnauthorized) {
        // 自动重新登录
        await PlatformManager.Login();
        // 重试原请求
        return await SendHttpRequestInternal<T>(...);
    }
    
    return response;
}
```

---

## 七、总结

### 7.1 核心思想

- **统一响应格式**：`{code, data, msg}`
- **统一调用接口**：`NetManager.CallHttp<T>()` / `Call<T>()`
- **自动解析**：无需手动 JSON 解析
- **类型安全**：泛型自动转换
- **Token管理**：自动保存、加载、携带
- **网络优化**：超时、重试、状态检查

### 7.2 使用原则

- **HTTP接口**：优先使用 `CallHttp<T>()`（Express服务端）
- **云函数**：使用 `Call<T>()`（微信云开发）
- **简单调用**：使用 `CallHttpData<T>()` / `CallData<T>()`
- **需要错误信息**：使用 `CallHttp<Response<T>>()` / `Call<Response<T>>()`
- **Token检查**：启动时使用 `NetManager.NeedLogin` 判断
- **保持简洁**：避免过度设计

### 7.3 相关文件

- `Assets/GameScripts/HotFix/GameLogic/Network/NetManager.cs` - 网络管理器
- `Assets/GameScripts/HotFix/GameLogic/Network/CloudResponse.cs` - 响应格式、错误码定义
- `Assets/GameScripts/HotFix/GameLogic/Network/PlatformManager.cs` - 平台管理器
- `Assets/GameScripts/HotFix/GameLogic/Network/IPlatformAdapter.cs` - 平台适配器接口

