# 登录流程梳理文档

## 概述

本文档梳理了微信小游戏环境和Unity Editor环境下的登录流程，包括客户端和服务端的交互。

---

## 一、微信环境登录流程

### 1.1 当前实现流程

#### 初始化阶段
```
Unity客户端启动
  ↓
World.AsyncInit()
  ↓
InitWX() 
  ├─ WX.InitSDK()          // 初始化微信SDK
  └─ WX.cloud.Init()        // 初始化云环境
  ↓
FetchUserGameInfo()        // 获取用户游戏信息
  ↓
NetManager.CallHttp<UserGameInfoData>("getUserGameInfoV2")
  ↓
Express服务端: /api/minigame/getUserGameInfoV2
  ├─ authMiddleware         // 验证token（需要先登录获取token）
  └─ userService.getUserGameInfo(openid)
```

#### 问题分析
**⚠️ 当前流程存在问题：**
1. **缺少登录步骤**：没有调用 `WX.Login()` 获取 `code`
2. **缺少token获取**：没有调用 `getCode2Session` 接口获取 `token`
3. **认证失败**：`getUserGameInfoV2` 需要 `authMiddleware`，但没有token会导致认证失败

### 1.2 完整的微信登录流程（应该实现）

```
Unity客户端启动
  ↓
World.AsyncInit()
  ↓
InitWX() 
  ├─ WX.InitSDK()          // 初始化微信SDK
  └─ WX.cloud.Init()        // 初始化云环境
  ↓
【登录步骤 - 当前缺失】
WX.Login()                  // 获取微信登录凭证code
  ↓
NetManager.CallHttp<SessionData>("getCode2Session", { code })
  ↓
Express服务端: /api/minigame/getCode2Session
  ├─ validateCode           // 验证code参数
  ├─ authService.getSession(code, 'wechat')
  │   ├─ WeChatAuth.code2Session(code)  // 调用微信API: jscode2session
  │   └─ generateToken()    // 生成JWT token
  └─ 返回: { openid, token, session_key, unionid, appid }
  ↓
客户端保存token
  ├─ NetManager.AuthToken = response.data.token
  └─ NetManager.AuthToken 自动保存到 PlayerPrefs
  ↓
获取用户游戏信息
  ↓
NetManager.CallHttp<UserGameInfoData>("getUserGameInfoV2")
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

### 1.3 代码位置

#### 客户端代码
- **初始化**：`Assets/GameScripts/HotFix/GameLogic/SlimeGame/World.cs` (第37-61行)
- **网络管理**：`Assets/GameScripts/HotFix/GameLogic/Network/NetManager.cs`
- **用户数据**：`Assets/GameScripts/HotFix/GameLogic/SlimeGame/World_GameData.cs`

#### 服务端代码
- **路由**：`piratecat_slime_express/routes/minigame.js`
- **控制器**：`piratecat_slime_express/controllers/minigameController.js`
- **认证服务**：`piratecat_slime_express/services/authService.js`
- **认证中间件**：`piratecat_slime_express/middlewares/auth.js`

---

## 二、Editor环境登录流程

### 2.1 当前实现流程

```
Unity Editor启动
  ↓
World.AsyncInit()
  ↓
InitEditor()                // 仅Editor环境
  ├─ 检查本地是否有openID
  └─ 如果没有，创建测试用户信息
     ├─ openID = "test"
     ├─ nickName = "EditorPlayer"
     └─ avatarUrl = "https://..."
  ↓
LoadConfig()                // 加载配置
  ↓
显示菜单界面
```

### 2.2 Editor环境下的HTTP请求

**当前问题：**
- Editor环境下调用 `getUserGameInfoV2` 时，需要token认证
- 但Editor环境没有微信SDK，无法获取真实的微信code

**解决方案：使用Editor平台测试模式**

#### Editor平台测试模式（最新实现）
```
Unity Editor启动
  ↓
World.AsyncInit()
  ↓
InitEditor()
  ├─ 设置测试用户信息（本地）
  └─ 调用服务端获取/创建用户数据
  ↓
【测试模式登录】
NetManager.CallHttp<SessionData>("getCode2Session", { 
  platform: "Editor", 
  code: "any_code_here"  // 任意值即可
})
  ├─ Header: x-platform: "Editor" 或 body.platform: "Editor"
  ↓
Express服务端: /api/minigame/getCode2Session
  ├─ 检测到 platform="Editor"
  ├─ 检查 ENABLE_TEST_MODE=true（环境变量）
  ├─ EditorAuth.code2Session(code)  // 使用测试openid
  │   └─ openid = TEST_OPENID 或自动生成
  ├─ generateToken()  // 生成JWT token
  └─ 返回: { openid, token, session_key, platform: "editor" }
  ↓
客户端保存token
  ├─ NetManager.AuthToken = response.data.token
  └─ NetManager.AuthToken 自动保存到 PlayerPrefs
  ↓
获取用户游戏信息
  ↓
NetManager.CallHttp<UserGameInfoData>("getUserGameInfoV2")
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

### 2.3 Editor环境完整流程（应该实现）

```
Unity Editor启动
  ↓
World.AsyncInit()
  ↓
InitEditor()
  ├─ 设置测试用户信息（本地，可选）
  └─ 调用服务端登录流程
  ↓
【测试模式登录 - 必须步骤】
NetManager.CallHttp<SessionData>("getCode2Session", {
  platform: "Editor",
  code: "editor_test_code"  // 任意值
})
  ├─ Header: x-platform: "Editor" 或 body.platform: "Editor"
  ↓
Express服务端（Editor平台）
  ├─ PlatformAuthFactory.detectPlatform(req) → "editor"
  ├─ 检查 ENABLE_TEST_MODE === 'true'
  ├─ EditorAuth.code2Session(code)
  │   └─ 返回测试openid（从TEST_OPENID环境变量或自动生成）
  ├─ generateToken() 生成JWT token
  └─ 返回: { openid, token, platform: "editor", ... }
  ↓
客户端保存token
  ├─ NetManager.AuthToken = response.data.token
  └─ 自动保存到 PlayerPrefs
  ↓
【获取用户游戏信息】
NetManager.CallHttp<UserGameInfoData>("getUserGameInfoV2")
  ├─ Header: Authorization: Bearer {token}
  ↓
Express服务端
  ├─ authMiddleware 验证token
  └─ 返回/创建用户数据
```

### 2.4 代码位置

- **Editor初始化**：`Assets/GameScripts/HotFix/GameLogic/SlimeGame/World_GameData.cs` (第154-165行)
- **平台认证**：`piratecat_slime_express/utils/platformAuth.js` (EditorAuth类，第110-131行)
- **平台检测**：`piratecat_slime_express/utils/platformAuth.js` (detectPlatform方法，第164-195行)
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

## 四、需要补充的实现

### 4.1 微信环境

**缺失的步骤**：
1. ✅ 调用 `WX.Login()` 获取code
2. ✅ 调用 `getCode2Session` 接口获取token
3. ✅ 保存token到 `NetManager.AuthToken`
4. ✅ 在后续请求中自动携带token

**建议实现位置**：
- 在 `World.AsyncInit()` 中，`InitWX()` 成功后
- 在 `FetchUserGameInfo()` 之前，先执行登录流程

### 4.2 Editor环境

**缺失的步骤**：
1. ✅ 在Editor环境下，调用 `getCode2Session` 接口（platform="Editor"）获取token
2. ✅ 保存token到 `NetManager.AuthToken`
3. ✅ 在后续请求中自动携带token（Authorization header）
4. ✅ 调用 `getUserGameInfoV2` 获取/同步用户数据

**建议实现位置**：
- 在 `World.AsyncInit()` 中，`InitEditor()` 成功后
- 在 `FetchUserGameInfo()` 之前，先执行Editor平台登录流程
- 检测到Editor环境时，自动使用 `platform="Editor"` 调用 `getCode2Session`

---

## 五、Token管理

### 5.1 Token存储

**客户端**：
- 存储在 `NetManager.AuthToken` 属性中
- 自动保存到 `PlayerPrefs`（key: `NetManager_AuthToken`）
- 自动从 `PlayerPrefs` 加载

**代码位置**：`Assets/GameScripts/HotFix/GameLogic/Network/NetManager.cs` (第30-53行)

### 5.2 Token使用

**自动携带**：
- HTTP请求自动在Header中添加：`Authorization: Bearer {token}`
- 代码位置：`NetManager.CallHttp()` (第190-193行)

### 5.3 Token过期

**服务端**：
- Token有效期：7天（JWT标准格式）
- 过期后需要重新登录

**客户端**：
- 检测到401错误时，清除本地token
- 重新执行登录流程

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

**Unity Editor环境**：
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
启动 → InitWX → WX.Login() → getCode2Session → 保存token → getUserGameInfoV2 → 更新本地数据
```

### Editor环境完整流程
```
启动 → InitEditor → getCode2Session(platform="Editor") → 保存token → getUserGameInfoV2 → 更新本地数据
```

---

## 八、注意事项

1. **微信环境**：必须先调用 `WX.Login()` 获取code，然后调用 `getCode2Session` 获取token，才能调用需要认证的接口
2. **Editor环境**：
   - 必须先调用 `getCode2Session`（platform="Editor"）获取token
   - 服务端必须设置 `ENABLE_TEST_MODE=true` 才能使用Editor平台
   - code可以是任意值，不需要真实微信code
3. **Token过期**：客户端需要处理token过期的情况（401错误），重新执行登录流程
4. **错误处理**：所有网络请求都应该有错误处理和重试机制
5. **数据同步**：本地数据和服务端数据需要保持同步
6. **平台标识**：Editor环境通过 `platform="Editor"` 参数标识，可以通过header（`x-platform`）或body传递

---

## 九、相关文件清单

### 客户端
- `Assets/GameScripts/HotFix/GameLogic/Network/NetManager.cs` - 网络管理器
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/World.cs` - 游戏世界初始化
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/World_GameData.cs` - 用户数据管理
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/SlimeGameData.cs` - 数据结构定义

### 服务端
- `piratecat_slime_express/routes/minigame.js` - 路由定义
- `piratecat_slime_express/controllers/minigameController.js` - 控制器
- `piratecat_slime_express/services/authService.js` - 认证服务
- `piratecat_slime_express/middlewares/auth.js` - 认证中间件（统一token验证）
- `piratecat_slime_express/utils/tokenManager.js` - Token管理（JWT生成和验证）
- `piratecat_slime_express/utils/platformAuth.js` - 平台认证（支持微信、Editor等平台）
