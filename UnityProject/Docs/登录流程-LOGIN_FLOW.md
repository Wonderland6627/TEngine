# 登录流程文档（当前实现）

## 概述

客户端通过 `World.Login()` 走统一登录入口，底层由 `PlatformManager` 和 `NetManager` 完成：

1. 检查本地 Token 状态（`NetManager.NeedLogin`）
2. 无 Token / 过期 / 无效时触发平台登录
3. 调用服务端 `getCode2Session` 获取并缓存新 Token
4. 调用 `getUserWXContext` 同步 openid（登录后补充）
5. 后续业务接口统一走 Bearer Token 鉴权

---

## 客户端登录时序

### 1) 启动后判断是否需要登录

```csharp
if (!NetManager.Instance.NeedLogin)
{
    // 复用本地缓存 token
    return true;
}
```

### 2) 进入平台登录

`World.Login()` -> `PlatformManager.Login()`：

- 根据平台选择适配器（Editor/WeChat）
- 如需要先初始化 SDK（微信）
- 获取平台登录 code
- 调用 `getCode2Session`

### 3) 保存 Token

`PlatformManager.LoginWithCode()` 在登录成功后执行：

- `NetManager.Instance.AuthToken = response.data.token`
- Token 持久化到 `PlayerPrefs`（Key: `NetManager_AuthToken`）

### 4) 登录后补充用户上下文

`World.UpdateUserInfoFromLogin()` 调用 `getUserWXContext`：

- 从服务端返回中读取 `openid`
- 写入 `GameData.UserInfo.openID`

---

## 平台差异

### WeChat

- 平台名：`wechat`
- 适配器：`WeChatPlatformAdapter`
- 需要 `WX.InitSDK()` 和 `WX.cloud.Init()`
- 通过 `WX.Login()` 获取 `code`

### Editor

- 平台名：`Editor`（客户端发送），服务端会转小写识别为 `editor`
- 适配器：`EditorPlatformAdapter`
- `EditorPrefs("EditorIdentityKey")` 作为登录 code（默认 `default`）
- 服务端要求 `ENABLE_TEST_MODE=true` 才允许 `editor` 平台登录

### DouYin / Bilibili

- 客户端和服务端均有占位适配器
- 当前未实现真实登录流程

---

## Token 管理

### Token 状态

- `None`：无 Token
- `Valid`：有效
- `Expiring`：即将过期（< 24 小时）
- `Expired`：已过期
- `Invalid`：解析失败

### 判定逻辑

- `NetManager.GetTokenState()` 解析 JWT `exp` 字段
- `NetManager.NeedLogin` 在 `None / Expired / Invalid` 时返回 `true`

### 请求携带

所有 HTTP 接口自动加头：

`Authorization: Bearer {token}`

---

## 服务器选择（与登录联动）

`NetManager` 默认选服：

- Unity Editor：`Local`（`http://localhost:3000`）
- 非 Editor 包：`Production`

可在调试面板切换：

- `Local`
- `Dev`
- `Production`

---

## 相关接口

### 公开接口

- `POST /api/minigame/getCode2Session`

### 需要 Token 的接口

- `POST /api/minigame/getUserWXContext`
- `POST /api/minigame/getUserGameInfoV2`
- `POST /api/minigame/setUserGameInfoV2`

---

## 错误码

服务端响应码（与客户端常量一致）：

- `0` 成功
- `-1` 通用错误
- `-2` 未授权
- `-3` 未找到
- `-4` 参数错误

客户端本地错误码：

- `-100` 无网络
- `-101` 超时
- `-102` 解析失败
- `-103` 未知 API
- `-104` 请求失败

---

## 关键代码位置

### 客户端

- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/Manager/World/World_GameData.cs`
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/PlatformAdapters/PlatformManager.cs`
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/PlatformAdapters/IPlatformAdapter.cs`
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/Network/NetManager.cs`

### 服务端

- `piratecat_slime_express/routes/minigame.js`
- `piratecat_slime_express/controllers/minigameController.js`
- `piratecat_slime_express/services/authService.js`
- `piratecat_slime_express/middlewares/auth.js`
- `piratecat_slime_express/utils/tokenManager.js`
- `piratecat_slime_express/utils/platformAuth.js`
