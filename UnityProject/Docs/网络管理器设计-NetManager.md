# NetManager 设计文档（当前实现）

## 定位

`NetManager` 是客户端统一网络入口，负责：

- API 路由名到 URL 的映射
- HTTP 请求发送、超时与重试
- Bearer Token 自动携带
- 服务器时间同步
- 选服切换（Local/Dev/Production）

---

## 响应格式

客户端按统一结构解析：

```json
{
  "code": 0,
  "data": {},
  "msg": "success"
}
```

---

## API 路由映射

当前 `ApiRouteMap`：

- `getServerTime` -> `/api/time`
- `getCode2Session` -> `/api/minigame/getCode2Session`
- `getUserWXContext` -> `/api/minigame/getUserWXContext`
- `getUserGameInfoV2` -> `/api/minigame/getUserGameInfoV2`
- `setUserGameInfoV2` -> `/api/minigame/setUserGameInfoV2`
- `getUserRankListV2` -> `/api/minigame/getUserRankListV2`
- `getLevelsConfigV2` -> `/api/minigame/getLevelsConfigV2`
- `updateResource` -> `/api/minigame/updateResource`
- `getResources` -> `/api/minigame/getResources`
- `claimLevelReward` -> `/api/minigame/claimLevelReward`
- `claimDailyCheckin` -> `/api/minigame/claimDailyCheckin`
- `claimLevelChest` -> `/api/minigame/claimLevelChest`

---

## 核心调用方式

### 1) 获取完整响应

```csharp
var response = await NetManager.Instance.CallHttp<UserGameInfoData>("getUserGameInfoV2");
if (response.IsSuccess)
{
    var userInfo = response.data;
}
else if (response.IsUnauthorized)
{
    // 需要重新登录
    await World.Instance.Login();
}
```

### 2) 直接拿 data

```csharp
var serverTime = await NetManager.Instance.CallHttpData<ServerTimeData>("getServerTime");
```

### 3) 资源更新示例

```csharp
var response = await NetManager.Instance.CallHttp<UpdateResourceResponse>(
    "updateResource",
    new { resourceType = 1, change = 100, source = "level_reward" }
);
```

---

## 时间同步

### 机制

1. 调用 `getServerTime` 获取服务器 UTC 毫秒时间戳
2. 计算 `offset = serverTime - clientUtcNow`
3. 使用 `DateTime.UtcNow + offset` 得到服务器时间

### 暴露属性

- `ServerTime`：服务器 UTC 时间
- `ServerTimeLocal`：服务器本地时区时间（`ToLocalTime()`）
- `HasSynced`：是否已至少同步过一次

### 同步时机

- 初始化时
- 应用恢复前台时
- 每 300 秒周期同步

---

## Token 管理

- 存储字段：`AuthToken`
- 本地缓存：`PlayerPrefs(NetManager_AuthToken)`
- 状态判断：`GetTokenState()` / `NeedLogin`
- 辅助方法：`GetTokenRemainingSeconds()` / `ClearToken()`

---

## 选服策略

`ServerType`：

- `Local` -> `http://localhost:3000`
- `Dev` -> `https://express-slime-dev-216111-7-1352845565.sh.run.tcloudbase.com`
- `Production` -> `https://express-slime-216111-7-1352845565.sh.run.tcloudbase.com`

默认值：

- Editor 下默认 `Local`
- 非 Editor 下默认 `Production`

---

## 网络可靠性策略

- 请求超时：30 秒
- 自动重试：最多 3 次（仅连接类错误）
- 重试间隔：1 秒
- 无网络短路：直接返回 `CLIENT_NO_NETWORK`
- 未知 API 名：返回 `CLIENT_UNKNOWN_API`

---

## 错误码

服务端：

- `0` SUCCESS
- `-1` ERROR
- `-2` UNAUTHORIZED
- `-3` NOT_FOUND
- `-4` VALIDATION_ERROR

客户端：

- `-100` CLIENT_NO_NETWORK
- `-101` CLIENT_TIMEOUT
- `-102` CLIENT_PARSE_ERROR
- `-103` CLIENT_UNKNOWN_API
- `-104` CLIENT_REQUEST_FAILED

---

## 关键代码

- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/Network/NetManager.cs`
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/Network/CloudResponse.cs`
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/Constants/Constants.cs`
