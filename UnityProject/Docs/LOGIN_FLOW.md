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
- **路由**：`d:\CustomProjects\Slime\piratecat_slime_express\routes\minigame.js`
- **控制器**：`d:\CustomProjects\Slime\piratecat_slime_express\controllers\minigameController.js`
- **认证服务**：`d:\CustomProjects\Slime\piratecat_slime_express\services\authService.js`
- **认证中间件**：`d:\CustomProjects\Slime\piratecat_slime_express\middlewares\auth.js`

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
- 但Editor环境没有微信SDK，无法获取code

**解决方案：使用测试模式**

#### 方式1：通过测试Token（推荐）
```
NetManager.CallHttp<UserGameInfoData>("getUserGameInfoV2")
  ├─ Header: x-test-token: {TEST_TOKEN}
  ├─ Header: x-test-openid: "test_user"
  ↓
Express服务端检测到测试模式
  ├─ isTestMode(req) = true
  ├─ authMiddleware 跳过token验证
  └─ req.user = { openid: "test_user", platform: "test", isTestMode: true }
```

#### 方式2：通过环境变量
- 设置 `ENABLE_TEST_MODE=true`
- 设置 `TEST_TOKEN=any` 或具体token值

### 2.3 Editor环境完整流程（应该实现）

```
Unity Editor启动
  ↓
World.AsyncInit()
  ↓
InitEditor()
  ├─ 设置测试用户信息（本地）
  └─ 可选：调用服务端获取/创建用户数据
  ↓
【可选】测试模式登录
NetManager.CallHttp<UserGameInfoData>("getUserGameInfoV2", null)
  ├─ Header: x-test-token: {TEST_TOKEN}      // 从环境变量读取
  ├─ Header: x-test-openid: "test_user"        // 或从配置读取
  ↓
Express服务端（测试模式）
  ├─ authMiddleware 检测到测试模式
  ├─ 跳过token验证
  └─ 返回/创建测试用户数据
```

### 2.4 代码位置

- **Editor初始化**：`Assets/GameScripts/HotFix/GameLogic/SlimeGame/World_GameData.cs` (第154-165行)
- **测试模式检测**：`d:\CustomProjects\Slime\piratecat_slime_express\middlewares\auth.js` (第11-21行)

---

## 三、服务端接口说明

### 3.1 登录接口

#### `POST /api/minigame/getCode2Session`
**功能**：通过微信code获取session信息和token

**请求参数**：
```json
{
  "code": "微信登录凭证code"
}
```

**响应格式**：
```json
{
  "code": 0,
  "data": {
    "openid": "用户openid",
    "token": "JWT token",
    "session_key": "微信session_key",
    "unionid": "unionid（如果有）",
    "appid": "微信appid",
    "platform": "wechat"
  },
  "msg": "get session success"
}
```

**流程**：
1. 验证code参数
2. 检测平台类型（默认微信）
3. 调用微信API：`https://api.weixin.qq.com/sns/jscode2session`
4. 生成JWT token（包含openid、platform、session_key）
5. 返回session信息

### 3.2 获取用户信息接口

#### `POST /api/minigame/getUserGameInfoV2`
**功能**：获取用户游戏信息（需要认证）

**认证方式**：
- **Bearer Token**：`Authorization: Bearer {token}`
- **测试模式**：`x-test-token: {TEST_TOKEN}` + `x-test-openid: {openid}`

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
1. ✅ 在Editor环境下，自动设置测试token header
2. ✅ 可选：调用服务端接口同步测试用户数据

**建议实现位置**：
- 在 `NetManager.CallHttp()` 中，检测到Editor环境时自动添加测试header
- 或在 `InitEditor()` 中调用服务端接口

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

## 六、测试模式配置

### 6.1 环境变量

**服务端需要配置**：
```bash
ENABLE_TEST_MODE=true
TEST_TOKEN=any                    # 或具体token值
TEST_OPENID=test_user             # 可选，默认值
NODE_ENV=development              # 或 production
```

### 6.2 测试模式检测逻辑

**服务端**：`middlewares/auth.js`
1. 检查 `ENABLE_TEST_MODE === 'true'` 或 `NODE_ENV === 'development'`
2. 检查请求header中的 `x-test-token`
3. 如果匹配，跳过token验证，使用测试openid

---

## 七、流程图总结

### 微信环境完整流程
```
启动 → InitWX → WX.Login() → getCode2Session → 保存token → getUserGameInfoV2 → 更新本地数据
```

### Editor环境完整流程
```
启动 → InitEditor → 设置测试用户（本地）→ [可选] getUserGameInfoV2（测试模式）→ 更新本地数据
```

---

## 八、注意事项

1. **微信环境**：必须先登录获取token，才能调用需要认证的接口
2. **Editor环境**：使用测试模式时，需要配置正确的环境变量
3. **Token过期**：客户端需要处理token过期的情况
4. **错误处理**：所有网络请求都应该有错误处理和重试机制
5. **数据同步**：本地数据和服务端数据需要保持同步

---

## 九、相关文件清单

### 客户端
- `Assets/GameScripts/HotFix/GameLogic/Network/NetManager.cs` - 网络管理器
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/World.cs` - 游戏世界初始化
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/World_GameData.cs` - 用户数据管理
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/SlimeGameData.cs` - 数据结构定义

### 服务端
- `d:\CustomProjects\Slime\piratecat_slime_express\routes\minigame.js` - 路由定义
- `d:\CustomProjects\Slime\piratecat_slime_express\controllers\minigameController.js` - 控制器
- `d:\CustomProjects\Slime\piratecat_slime_express\services\authService.js` - 认证服务
- `d:\CustomProjects\Slime\piratecat_slime_express\middlewares\auth.js` - 认证中间件
- `d:\CustomProjects\Slime\piratecat_slime_express\utils\tokenManager.js` - Token管理
- `d:\CustomProjects\Slime\piratecat_slime_express\utils\platformAuth.js` - 平台认证
