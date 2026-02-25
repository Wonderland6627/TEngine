# NetManager 设计文档

## 响应格式

### 标准响应
```json
{
  "code": 0,        // 0=成功, 非0=失败
  "data": {},       // 业务数据
  "msg": "success"  // 消息描述
}
```

### 错误码

**服务端错误码**：
- `0` - SUCCESS：成功
- `-1` - ERROR：通用错误
- `-2` - UNAUTHORIZED：未授权
- `-3` - NOT_FOUND：资源未找到
- `-4` - VALIDATION_ERROR：参数验证失败

**客户端错误码**：
- `-100` - CLIENT_NO_NETWORK：无网络
- `-101` - CLIENT_TIMEOUT：超时
- `-102` - CLIENT_PARSE_ERROR：解析失败
- `-103` - CLIENT_UNKNOWN_API：未知API
- `-104` - CLIENT_REQUEST_FAILED：请求失败

## 核心功能

### HTTP接口调用

```csharp
// 方式1：获取完整响应（推荐）
var response = await NetManager.CallHttp<UserGameInfoData>("getUserGameInfoV2");
if (response.IsSuccess) {
    var userInfo = response.data;
} else {
    Log.Error($"Error: {response.ErrorMessage}");
    if (response.IsUnauthorized) {
        await World.Instance.Login();
    }
}

// 方式2：直接获取数据（简洁）
var userInfo = await NetManager.CallHttpData<UserGameInfoData>("getUserGameInfoV2");
```

**特性**：
- 请求超时：30秒
- 自动重试：连接错误时最多3次
- 网络状态检查：无网络时直接返回错误
- 自动携带Token：`Authorization: Bearer {token}`

### Token管理

```csharp
// 检查Token状态
if (NetManager.NeedLogin) {
    await World.Instance.Login();
} else {
    long remaining = NetManager.GetTokenRemainingSeconds();
    Log.Info($"Token有效，剩余：{remaining}秒");
}

// 清除Token
NetManager.ClearToken();
```

**Token状态**：
- `TokenState.None` - 无Token
- `TokenState.Valid` - 有效
- `TokenState.Expiring` - 即将过期（<24小时）
- `TokenState.Expired` - 已过期
- `TokenState.Invalid` - 无效

### 服务器配置

```csharp
// 服务器类型
public enum ServerType {
    Local,       // 本地服务器
    Dev,         // 测试服务器
    Production   // 正式服务器
}

// 切换服务器
NetManager.CurrentServerType = ServerType.Dev;
```

**默认服务器**：
- Unity Editor：Dev（localhost:3000）
- 真机/微信开发者工具：Production（云托管域名）
- 可通过Debug面板手动切换

## 使用示例

### 用户信息
```csharp
// 获取用户信息
var response = await NetManager.CallHttp<UserGameInfoData>("getUserGameInfoV2");
if (response.IsSuccess) {
    var userInfo = response.data;
}

// 更新用户信息
await NetManager.CallHttp<object>("setUserGameInfoV2", new {
    progressLevelID = 10,
    nickName = "玩家昵称"
});
```

### 排行榜
```csharp
var rankList = await NetManager.CallHttpData<List<UserGameInfoData>>("getUserRankListV2");
```

### 货币操作
```csharp
// 增加金币
await NetManager.CallHttp<object>("addCoin", new {
    amount = 100,
    source = "level_reward"
});

// 扣除金币
await NetManager.CallHttp<object>("deductCoin", new {
    amount = 50,
    reason = "refresh_reward"
});
```

### 体力操作
```csharp
// 扣除体力
await NetManager.CallHttp<object>("updateEnergy", new {
    change = -10,
    source = "level_play"
});
```

## 设计要点

### 核心优势
1. **简洁**：一行代码完成调用和解析
2. **统一**：所有接口使用相同方式调用
3. **类型安全**：泛型自动解析，编译期检查
4. **错误处理**：统一的错误处理和重试机制
5. **Token管理**：自动保存、加载、携带Token
6. **网络优化**：超时控制、自动重试、网络状态检查

### 核心流程

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

### 错误处理机制

**重试策略**：
- 只对连接错误重试
- 协议错误（404、500等）不重试
- 最多重试3次，间隔1秒

**错误码处理**：
- 服务端错误码：直接返回给业务层
- 客户端错误码：网络、解析等本地错误
- 401/UNAUTHORIZED：可触发重新登录

## 已实现功能

### 网络优化
- ✅ 请求超时：默认30秒
- ✅ 自动重试：连接错误时最多3次
- ✅ 网络状态检查：无网络时直接返回错误
- ✅ 内存优化：使用`using`确保资源释放

### Token管理
- ✅ Token缓存：自动保存到PlayerPrefs
- ✅ Token有效性检查：解析JWT exp字段
- ✅ Token过期处理：客户端预检+服务端验证

### 错误码系统
- ✅ 统一错误码：服务端（0, -1, -2, -3, -4）+ 客户端（-100~-199）
- ✅ 错误码辅助属性：`IsSuccess`、`IsUnauthorized`、`IsClientError`、`ErrorMessage`

## 相关文件

- `Assets/GameScripts/HotFix/GameLogic/Network/NetManager.cs` - 网络管理器
- `Assets/GameScripts/HotFix/GameLogic/Network/CloudResponse.cs` - 响应格式、错误码定义
- `Assets/GameScripts/HotFix/GameLogic/Network/PlatformManager.cs` - 平台管理器
