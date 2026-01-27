# 登录流程梳理文档

## 概述

本文档梳理了微信小游戏环境和Unity Editor环境下的登录流程，包括客户端和服务端的交互。

---

## 一、微信环境登录流程

### 1.1 完整登录流程（已实现）

```
Unity客户端启动
  ↓
World.AsyncInit()
  ↓
World.Login()               // 统一登录入口
  ├─ 检查Token有效性（NetManager.NeedLogin）
  │   ├─ Token有效 → 跳过登录，直接使用
  │   └─ Token无效/过期 → 执行登录流程
  ↓
PlatformManager.Login()     // 平台登录管理器
  ├─ 获取当前平台适配器（WeChatPlatformAdapter）
  ├─ 初始化SDK（如果需要）
  │   └─ WeChatPlatformAdapter.InitSDK()
  │       ├─ WX.InitSDK()          // 初始化微信SDK
  │       └─ WX.cloud.Init()        // 初始化云环境
  ├─ 获取登录凭证code
  │   └─ WeChatPlatformAdapter.GetLoginCode()
  │       └─ WX.Login()             // 获取微信登录凭证code
  └─ 使用code登录
      └─ PlatformManager.LoginWithCode(code, "wechat")
          ↓
          NetManager.CallHttp<SessionData>("getCode2Session", { code, platform: "wechat" })
          ↓
          Express服务端: /api/minigame/getCode2Session
          ├─ validateCode           // 验证code参数
          ├─ authService.getSession(code, 'wechat')
          │   ├─ WeChatAuth.code2Session(code)  // 调用微信API: jscode2session
          │   └─ generateToken()    // 生成JWT token（7天有效期）
          └─ 返回: { openid, token, session_key, unionid, appid }
          ↓
          客户端保存token
          ├─ NetManager.AuthToken = response.data.token
          └─ 自动保存到 PlayerPrefs
  ↓
更新用户信息
  └─ World.UpdateUserInfoFromLogin()
      └─ NetManager.CallHttp<WXContextData>("getUserWXContext")
  ↓
获取用户游戏信息
  └─ World.FetchUserGameInfo()
      └─ NetManager.CallHttp<UserGameInfoData>("getUserGameInfoV2")
          ├─ Header: Authorization: Bearer {token}
          ↓
          Express服务端: /api/minigame/getUserGameInfoV2
          ├─ authMiddleware         // 验证token，提取openid
          └─ userService.getUserGameInfo(openid)
             ├─ 查询数据库
             ├─ 如果不存在，创建空记录
             └─ 返回用户游戏数据
          ↓
          客户端更新本地数据
          ├─ GameData.UserInfo.openID = userInfo.openID
          ├─ GameData.UserInfo.nickName = userInfo.nickName
          └─ GameData.SetProgressLevelID(userInfo.progressLevelID)
```

### 1.2 Token有效性检查

**启动时检查**：
- `NetManager.NeedLogin` 检查Token状态
- 解析JWT的 `exp` 字段判断是否过期
- Token有效时跳过登录，直接使用缓存的Token

**Token状态**：
- `TokenState.None` - 无Token
- `TokenState.Valid` - 有效
- `TokenState.Expiring` - 即将过期（剩余<24小时）
- `TokenState.Expired` - 已过期
- `TokenState.Invalid` - 无效（解析失败）

### 1.3 代码位置

#### 客户端代码
- **统一登录入口**：`Assets/GameScripts/HotFix/GameLogic/SlimeGame/World_GameData.cs` - `Login()`
- **平台管理器**：`Assets/GameScripts/HotFix/GameLogic/Network/PlatformManager.cs`
- **微信适配器**：`Assets/GameScripts/HotFix/GameLogic/Network/PlatformAdapters/WeChatPlatformAdapter.cs`
- **网络管理**：`Assets/GameScripts/HotFix/GameLogic/Network/NetManager.cs`
- **用户数据**：`Assets/GameScripts/HotFix/GameLogic/SlimeGame/World_GameData.cs`

#### 服务端代码
- **路由**：`piratecat_slime_express/routes/minigame.js`
- **控制器**：`piratecat_slime_express/controllers/minigameController.js`
- **认证服务**：`piratecat_slime_express/services/authService.js`
- **认证中间件**：`piratecat_slime_express/middlewares/auth.js`

---

## 二、Editor环境登录流程

### 2.1 完整登录流程（已实现）

```
Unity Editor启动
  ↓
World.AsyncInit()
  ↓
World.Login()               // 统一登录入口
  ├─ 检查Token有效性（NetManager.NeedLogin）
  │   ├─ Token有效 → 跳过登录，直接使用
  │   └─ Token无效/过期 → 执行登录流程
  ↓
PlatformManager.Login()     // 平台登录管理器
  ├─ 获取当前平台适配器（EditorPlatformAdapter）
  ├─ 初始化SDK（Editor不需要）
  ├─ 获取登录凭证code
  │   └─ EditorPlatformAdapter.GetLoginCode()
  │       └─ 返回固定值 "editor"
  └─ 使用code登录
      └─ PlatformManager.LoginWithCode("editor", "Editor")
          ↓
          NetManager.CallHttp<SessionData>("getCode2Session", { code: "editor", platform: "Editor" })
          ↓
          Express服务端: /api/minigame/getCode2Session
          ├─ PlatformAuthFactory.detectPlatform(req) → "editor"
          ├─ 检查 ENABLE_TEST_MODE === 'true'
          ├─ EditorAuth.code2Session(code)
          │   └─ 返回测试openid（从TEST_OPENID环境变量或自动生成）
          ├─ generateToken() 生成JWT token（7天有效期）
          └─ 返回: { openid, token, platform: "editor", ... }
          ↓
          客户端保存token
          ├─ NetManager.AuthToken = response.data.token
          └─ 自动保存到 PlayerPrefs
  ↓
更新用户信息
  └─ World.UpdateUserInfoFromLogin()
  ↓
获取用户游戏信息
  └─ World.FetchUserGameInfo()
      └─ NetManager.CallHttp<UserGameInfoData>("getUserGameInfoV2")
          ├─ Header: Authorization: Bearer {token}
          ↓
          Express服务端: /api/minigame/getUserGameInfoV2
          ├─ authMiddleware 验证token，提取openid
          └─ userService.getUserGameInfo(openid)
             ├─ 查询数据库
             ├─ 如果不存在，创建空记录
             └─ 返回用户游戏数据
          ↓
          客户端更新本地数据
          ├─ GameData.UserInfo.openID = userInfo.openID
          ├─ GameData.UserInfo.nickName = userInfo.nickName
          └─ GameData.SetProgressLevelID(userInfo.progressLevelID)
```

### 2.2 Editor平台特点

- **无需SDK初始化**：Editor环境不需要初始化微信SDK
- **固定code**：code可以是任意值，通常使用 "editor"
- **测试模式**：服务端需要设置 `ENABLE_TEST_MODE=true`

### 2.3 代码位置

- **统一登录入口**：`Assets/GameScripts/HotFix/GameLogic/SlimeGame/World_GameData.cs` - `Login()`
- **Editor适配器**：`Assets/GameScripts/HotFix/GameLogic/Network/PlatformAdapters/EditorPlatformAdapter.cs`
- **平台管理器**：`Assets/GameScripts/HotFix/GameLogic/Network/PlatformManager.cs`
- **平台认证**：`piratecat_slime_express/utils/platformAuth.js` (EditorAuth类)
- **平台检测**：`piratecat_slime_express/utils/platformAuth.js` (detectPlatform方法)
- **认证中间件**：`piratecat_slime_express/middlewares/auth.js` (统一token验证)

---

## 三、服务端接口说明

### 3.1 登录接口

#### `POST /api/minigame/getCode2Session`
**功能**：通过平台code获取session信息和token（支持多平台）

**请求参数**：
```json
{
  "code": "平台登录凭证code",
  "platform": "wechat"  // 可选，默认"wechat"。Editor环境使用"Editor"
}
```

**平台类型**：
- `wechat`：微信平台（默认），需要真实微信code
- `Editor`：Unity编辑器平台（测试模式），code可以是任意值，需要 `ENABLE_TEST_MODE=true`
- `douyin`：抖音平台（预留，待实现）
- `bilibili`：B站平台（预留，待实现）

**请求方式**：
- **Body参数**：`{ "code": "...", "platform": "..." }`
- **Header参数**：`x-platform: Editor`（可选，优先级低于body）
- **Query参数**：`?platform=Editor`（可选，优先级最低）

**响应格式**：
```json
{
  "code": 0,
  "data": {
    "openid": "用户openid",
    "token": "JWT token",
    "session_key": "session_key",
    "unionid": "unionid（如果有）",
    "appid": "微信appid（仅微信平台）",
    "platform": "wechat"  // 或 "editor"
  },
  "msg": "success"
}
```

**流程**：
1. 检测平台类型（从header、body或query获取，默认微信）
2. 如果是Editor平台，检查 `ENABLE_TEST_MODE === 'true'`
3. 根据平台类型调用对应的认证实例：
   - 微信平台：调用微信API `https://api.weixin.qq.com/sns/jscode2session`
   - Editor平台：使用测试openid（从 `TEST_OPENID` 环境变量或自动生成）
4. 生成JWT token（包含openid、platform、session_key）
5. 返回session信息

### 3.2 获取用户信息接口

#### `POST /api/minigame/getUserGameInfoV2`
**功能**：获取用户游戏信息（需要认证）

**认证方式**：
- **Bearer Token**：`Authorization: Bearer {token}`（标准方式）
- **自定义Header**：`x-auth-token: {token}`（备选方式）
- **Body/Query**：`token: {token}`（备选方式）

**注意**：Editor环境需要先通过 `getCode2Session` 接口（platform="Editor"）获取token

**响应格式**：
```json
{
  "code": 0,
  "data": {
    "openID": "用户openid",
    "nickName": "用户昵称",
    "avatarUrl": "头像URL",
    "progressLevelID": 1,
    "createdAt": "2024-01-01T00:00:00.000Z",
    "updatedAt": "2024-01-01T00:00:00.000Z"
  },
  "msg": "get user game info success"
}
```

**业务逻辑**：
- 如果用户不存在，自动创建空记录
- 返回用户游戏数据

### 3.3 其他接口

#### `POST /api/minigame/getUserWXContext`
- 需要认证
- 返回微信上下文信息

#### `POST /api/minigame/setUserGameInfoV2`
- 需要认证
- 更新用户游戏信息

#### `POST /api/minigame/getUserRankListV2`
- 可选认证
- 获取排行榜

#### `POST /api/minigame/getLevelsConfigV2`
- 可选认证
- 获取关卡配置

---

## 四、平台适配器架构

### 4.1 架构设计

**设计模式**：适配器模式（Adapter Pattern）

**核心组件**：
- `IPlatformAdapter` - 平台适配器接口
- `PlatformManager` - 平台管理器（统一入口）
- `*PlatformAdapter` - 各平台具体实现

**文件结构**：
```
GameLogic/Network/
├── IPlatformAdapter.cs                    # 平台适配器接口
├── PlatformManager.cs                     # 平台管理器
└── PlatformAdapters/                      # 平台适配器实现
    ├── EditorPlatformAdapter.cs          # Editor平台
    ├── WeChatPlatformAdapter.cs          # 微信平台
    ├── DouYinPlatformAdapter.cs          # 抖音平台（预留）
    └── BilibiliPlatformAdapter.cs        # B站平台（预留）
```

### 4.2 平台适配器接口

```csharp
public interface IPlatformAdapter
{
    string PlatformName { get; }              // 平台名称
    bool RequiresSDKInit { get; }             // 是否需要SDK初始化
    UniTask<bool> InitSDK();                   // 初始化SDK
    UniTask<string> GetLoginCode();            // 获取登录凭证code
}
```

### 4.3 使用方式

**统一登录入口**：
```csharp
// 自动根据平台选择适配器并登录
bool success = await PlatformManager.Login();

// 或指定平台登录（测试用）
bool success = await PlatformManager.LoginWithPlatform("Editor");
```

**扩展新平台**：
1. 实现 `IPlatformAdapter` 接口
2. 在 `PlatformManager.Initialize()` 中注册
3. 完成

### 4.4 未来扩展

平台适配器可扩展的功能：
- ✅ 登录（已实现）
- ✅ SDK初始化（已实现）
- 🔄 分享功能（待实现）
- 🔄 支付功能（待实现）
- 🔄 广告功能（待实现）
- 🔄 用户信息获取（待实现）

---

## 五、Token管理

### 5.1 Token存储

**客户端**：
- 存储在 `NetManager.AuthToken` 属性中
- 自动保存到 `PlayerPrefs`（key: `NetManager_AuthToken`）
- 自动从 `PlayerPrefs` 加载

**代码位置**：`Assets/GameScripts/HotFix/GameLogic/Network/NetManager.cs`

### 5.2 Token使用

**自动携带**：
- HTTP请求自动在Header中添加：`Authorization: Bearer {token}`
- 代码位置：`NetManager.CallHttp()` - `SendHttpRequestInternal()`

### 5.3 Token有效性检查

**客户端预检**：
- 启动时检查Token有效性（解析JWT的 `exp` 字段）
- `NetManager.NeedLogin` - 判断是否需要登录
- `NetManager.GetTokenRemainingSeconds()` - 获取剩余有效时间

**Token状态**：
- `TokenState.None` - 无Token
- `TokenState.Valid` - 有效（剩余时间 >= 24小时）
- `TokenState.Expiring` - 即将过期（剩余时间 < 24小时）
- `TokenState.Expired` - 已过期
- `TokenState.Invalid` - 无效（解析失败）

**优势**：
- ✅ 避免无效请求：启动时检查，Token有效则跳过登录
- ✅ 提升用户体验：减少不必要的登录流程
- ✅ 安全可靠：客户端检查只是优化，服务端验证是最终保障

### 5.4 Token过期处理

**服务端**：
- Token有效期：7天（JWT标准格式）
- 过期后返回 `code = -2` (UNAUTHORIZED)

**客户端**：
- 启动时检查Token有效性，过期则重新登录
- 请求时收到401/UNAUTHORIZED错误，触发重新登录流程
- 使用 `NetManager.ClearToken()` 清除Token

### 5.5 错误码定义

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

## 六、测试模式配置（Editor平台）

### 6.1 环境变量

**服务端需要配置**：
```bash
ENABLE_TEST_MODE=true             # 必须设置为true，才能使用Editor平台
TEST_OPENID=editor_test_user      # 可选，不设置则自动生成（格式：editor_test_{timestamp}）
NODE_ENV=development              # 或 production
```

**注意**：
- `ENABLE_TEST_MODE=true` 是使用Editor平台的必要条件
- 生产环境应设置 `ENABLE_TEST_MODE=false`，禁用Editor平台

### 6.2 Editor平台测试模式逻辑

**服务端**：`utils/platformAuth.js`
1. 客户端调用 `getCode2Session` 时，传递 `platform="Editor"`（通过header或body）
2. 服务端检测到 `platform === "editor"` 时：
   - 检查 `ENABLE_TEST_MODE === 'true'`，否则抛出错误
   - 使用 `EditorAuth.code2Session(code)` 处理
   - code可以是任意值，不做验证
   - 返回测试openid（从 `TEST_OPENID` 环境变量或自动生成）
   - 生成标准JWT token
3. 客户端使用获取的token，通过标准的 `Authorization: Bearer {token}` header访问其他接口

### 6.3 客户端调用示例

**Unity Editor环境**（使用平台管理器）：
```csharp
// 方式1：统一登录入口（推荐）
bool success = await World.Instance.Login();

// 方式2：直接使用平台管理器
bool success = await PlatformManager.Login();

// 方式3：指定平台登录（测试用）
bool success = await PlatformManager.LoginWithPlatform("Editor");

// 使用token访问接口（自动携带Authorization header）
var userInfo = await NetManager.CallHttp<UserGameInfoData>("getUserGameInfoV2");
```

**手动登录**（不推荐，仅用于特殊场景）：
```csharp
// 1. 获取token（Editor平台）
var sessionRequest = new { 
    platform = "Editor", 
    code = "editor_test_code"  // 任意值
};
var sessionResponse = await NetManager.CallHttp<SessionData>("getCode2Session", sessionRequest);

// 2. 保存token
NetManager.AuthToken = sessionResponse.data.token;

// 3. 使用token访问接口（自动携带Authorization header）
var userInfo = await NetManager.CallHttp<UserGameInfoData>("getUserGameInfoV2");
```

---

## 七、流程图总结

### 微信环境完整流程
```
启动 
  → World.Login() 
    → 检查Token有效性
      ├─ Token有效 → 跳过登录
      └─ Token无效 → PlatformManager.Login()
          → WeChatPlatformAdapter.InitSDK() 
            → WX.Login() 
              → getCode2Session 
                → 保存token 
                  → getUserGameInfoV2 
                    → 更新本地数据
```

### Editor环境完整流程
```
启动 
  → World.Login() 
    → 检查Token有效性
      ├─ Token有效 → 跳过登录
      └─ Token无效 → PlatformManager.Login()
          → EditorPlatformAdapter.GetLoginCode() 
            → getCode2Session(platform="Editor") 
              → 保存token 
                → getUserGameInfoV2 
                  → 更新本地数据
```

### 多平台统一流程
```
启动 
  → PlatformManager.Initialize()      // 注册所有平台适配器
  → PlatformManager.SetCurrentPlatform()  // 根据编译条件选择平台
  → World.Login()                      // 统一登录入口
    → PlatformManager.Login()          // 平台管理器
      → IPlatformAdapter.InitSDK()    // 初始化SDK（如果需要）
      → IPlatformAdapter.GetLoginCode()  // 获取登录凭证
      → LoginWithCode()                // 使用code换取token
        → 保存token
          → 后续业务逻辑
```

---

## 八、注意事项

1. **统一登录入口**：使用 `World.Login()` 或 `PlatformManager.Login()`，自动处理Token检查和平台选择
2. **Token缓存复用**：启动时自动检查Token有效性，有效则跳过登录流程
3. **微信环境**：
   - SDK初始化在登录时自动完成（`WeChatPlatformAdapter.InitSDK()`）
   - 必须先调用 `WX.Login()` 获取code，然后调用 `getCode2Session` 获取token
4. **Editor环境**：
   - 服务端必须设置 `ENABLE_TEST_MODE=true` 才能使用Editor平台
   - code可以是任意值，不需要真实微信code
5. **Token过期**：
   - 启动时检查Token有效性，过期则自动重新登录
   - 请求时收到401/UNAUTHORIZED错误，触发重新登录流程
6. **错误处理**：
   - 所有网络请求都有错误处理和重试机制（最多重试3次）
   - 请求超时时间：30秒
   - 网络状态检查：无网络时直接返回错误
7. **数据同步**：本地数据和服务端数据需要保持同步
8. **平台扩展**：添加新平台只需实现 `IPlatformAdapter` 接口并注册

---

## 九、相关文件清单

### 客户端

**网络层**：
- `Assets/GameScripts/HotFix/GameLogic/Network/NetManager.cs` - 网络管理器（HTTP请求、Token管理）
- `Assets/GameScripts/HotFix/GameLogic/Network/CloudResponse.cs` - 响应格式定义、错误码定义
- `Assets/GameScripts/HotFix/GameLogic/Network/IPlatformAdapter.cs` - 平台适配器接口
- `Assets/GameScripts/HotFix/GameLogic/Network/PlatformManager.cs` - 平台管理器（统一入口）
- `Assets/GameScripts/HotFix/GameLogic/Network/PlatformAdapters/EditorPlatformAdapter.cs` - Editor平台适配器
- `Assets/GameScripts/HotFix/GameLogic/Network/PlatformAdapters/WeChatPlatformAdapter.cs` - 微信平台适配器
- `Assets/GameScripts/HotFix/GameLogic/Network/PlatformAdapters/DouYinPlatformAdapter.cs` - 抖音平台适配器（预留）
- `Assets/GameScripts/HotFix/GameLogic/Network/PlatformAdapters/BilibiliPlatformAdapter.cs` - B站平台适配器（预留）

**业务层**：
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/World.cs` - 游戏世界初始化
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/World_GameData.cs` - 用户数据管理、登录流程
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/SlimeGameData.cs` - 数据结构定义

### 服务端
- `piratecat_slime_express/routes/minigame.js` - 路由定义
- `piratecat_slime_express/controllers/minigameController.js` - 控制器
- `piratecat_slime_express/services/authService.js` - 认证服务
- `piratecat_slime_express/middlewares/auth.js` - 认证中间件（统一token验证）
- `piratecat_slime_express/utils/tokenManager.js` - Token管理（JWT生成和验证）
- `piratecat_slime_express/utils/platformAuth.js` - 平台认证（支持微信、Editor等平台）
