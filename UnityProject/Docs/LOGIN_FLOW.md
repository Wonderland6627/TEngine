# 登录流程文档

## 概述

客户端支持微信小游戏和Unity Editor两种环境，使用统一的登录流程和Token认证机制。

## 登录流程

### 微信环境

```
启动
  → World.Login()
    → 检查Token有效性（NetManager.NeedLogin）
      ├─ Token有效 → 跳过登录
      └─ Token无效 → PlatformManager.Login()
          → WeChatPlatformAdapter.InitSDK()
            → WX.Login() 获取code
              → NetManager.CallHttp("getCode2Session", {code, platform:"wechat"})
                → 保存token
                  → World.FetchUserGameInfo()
```

### Editor环境

```
启动
  → World.Login()
    → 检查Token有效性（NetManager.NeedLogin）
      ├─ Token有效 → 跳过登录
      └─ Token无效 → PlatformManager.Login()
          → EditorPlatformAdapter.GetLoginCode() 返回"editor"
            → NetManager.CallHttp("getCode2Session", {code:"editor", platform:"Editor"})
              → 保存token
                → World.FetchUserGameInfo()
```

## 服务端接口

### getCode2Session
**功能**：通过平台code获取session信息和token

**请求**：
```json
{
  "code": "平台登录凭证",
  "platform": "wechat"  // wechat | Editor
}
```

**响应**：
```json
{
  "code": 0,
  "data": {
    "openid": "用户openid",
    "token": "JWT token",
    "platform": "wechat"
  }
}
```

### getUserGameInfoV2
**功能**：获取用户游戏信息（需要认证）

**认证**：`Authorization: Bearer {token}`

**响应**：
```json
{
  "code": 0,
  "data": {
    "openID": "用户openid",
    "nickName": "昵称",
    "progressLevelID": 1,
    "coin": 0,
    "energy": 100
  }
}
```

## Token管理

### Token存储
- 存储在`NetManager.AuthToken`属性
- 自动保存到`PlayerPrefs`（key: `NetManager_AuthToken`）
- 启动时自动加载

### Token使用
- HTTP请求自动添加Header：`Authorization: Bearer {token}`
- 代码位置：`NetManager.CallHttp()` → `SendHttpRequestInternal()`

### Token有效性检查
```csharp
// 启动时检查
if (NetManager.NeedLogin) {
    await World.Instance.Login();
} else {
    // Token有效，跳过登录
}

// 获取Token状态
TokenState state = NetManager.GetTokenState();
// None | Valid | Expiring | Expired | Invalid

// 获取剩余有效时间
long seconds = NetManager.GetTokenRemainingSeconds();
```

### Token过期处理
- **客户端预检**：启动时检查Token有效性，过期则重新登录
- **服务端验证**：请求时收到401/UNAUTHORIZED错误，触发重新登录
- **清除Token**：`NetManager.ClearToken()`

## 平台适配器

### 接口定义
```csharp
public interface IPlatformAdapter
{
    string PlatformName { get; }
    bool RequiresSDKInit { get; }
    UniTask<bool> InitSDK();
    UniTask<string> GetLoginCode();
}
```

### 已实现平台
- `WeChatPlatformAdapter` - 微信平台
- `EditorPlatformAdapter` - Unity Editor平台
- `DouYinPlatformAdapter` - 抖音平台（预留）
- `BilibiliPlatformAdapter` - B站平台（预留）

### 使用方式
```csharp
// 统一登录入口（推荐）
bool success = await PlatformManager.Login();

// 或指定平台登录（测试用）
bool success = await PlatformManager.LoginWithPlatform("Editor");
```

## 测试模式配置

### 服务端配置
**开发环境**：
```bash
NODE_ENV=development
ENABLE_TEST_MODE=true
TEST_OPENID=editor_test_user
```

**生产环境**：
```bash
NODE_ENV=production
ENABLE_TEST_MODE=false
```

### 客户端服务器选择
- Unity Editor：默认连接Dev服务器（localhost:3000）
- 真机/微信开发者工具：默认连接Production服务器（云托管域名）
- 可通过Debug面板手动切换服务器

## 错误码

### 服务端错误码
- `0` - SUCCESS：成功
- `-1` - ERROR：通用错误
- `-2` - UNAUTHORIZED：未授权（Token无效/过期）
- `-3` - NOT_FOUND：资源未找到
- `-4` - VALIDATION_ERROR：参数验证失败

### 客户端错误码
- `-100` - CLIENT_NO_NETWORK：无网络连接
- `-101` - CLIENT_TIMEOUT：请求超时
- `-102` - CLIENT_PARSE_ERROR：响应解析失败
- `-103` - CLIENT_UNKNOWN_API：未知API
- `-104` - CLIENT_REQUEST_FAILED：请求失败

代码位置：`Assets/GameScripts/HotFix/GameLogic/Network/CloudResponse.cs`

## 注意事项

1. **统一登录入口**：使用`World.Login()`或`PlatformManager.Login()`
2. **Token缓存复用**：启动时自动检查Token有效性，有效则跳过登录
3. **微信环境**：SDK初始化在登录时自动完成
4. **Editor环境**：
   - 客户端默认连接Dev服务器
   - 服务端必须设置`ENABLE_TEST_MODE=true`
   - code可以是任意值
5. **Token过期**：启动时检查，请求时收到401错误触发重新登录
6. **错误处理**：所有网络请求都有错误处理和重试机制（最多3次）
7. **数据同步**：本地数据和服务端数据保持同步

## 相关文件

### 客户端
- `Assets/GameScripts/HotFix/GameLogic/Network/NetManager.cs` - 网络管理器
- `Assets/GameScripts/HotFix/GameLogic/Network/PlatformManager.cs` - 平台管理器
- `Assets/GameScripts/HotFix/GameLogic/Network/IPlatformAdapter.cs` - 平台适配器接口
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/World_GameData.cs` - 登录流程

### 服务端
- `piratecat_slime_express/routes/minigame.js` - 路由定义
- `piratecat_slime_express/controllers/minigameController.js` - 控制器
- `piratecat_slime_express/services/authService.js` - 认证服务
- `piratecat_slime_express/middlewares/auth.js` - 认证中间件
- `piratecat_slime_express/utils/tokenManager.js` - Token管理
- `piratecat_slime_express/utils/platformAuth.js` - 平台认证
