# 客户端与服务端常量同步规则（当前实现）

## 核心原则

1. **服务端为主源**：共享常量以 `piratecat_slime_express/config/constants.js` 为准  
2. **客户端镜像定义**：客户端在 `Assets/GameScripts/HotFix/GameLogic/SlimeGame/Constants/Constants.cs` 对齐
3. **改动必须双向同步**：修改共享常量后，客户端与服务端必须同批更新

---

## 当前共享常量映射

### 1) 响应码

- 服务端：`RESPONSE_CODE`
- 客户端：`ResponseCode`

对应值：

- `SUCCESS = 0`
- `ERROR = -1`
- `UNAUTHORIZED = -2`
- `NOT_FOUND = -3`
- `VALIDATION_ERROR = -4`

客户端额外保留本地错误码 `-100 ~ -199`，不要求服务端同步。

### 2) 资源类型

- 服务端：`RESOURCE_TYPE`
- 客户端：`ResourceType`

对应值：

- `COIN = 1`
- `ENERGY = 2`
- `DIAMOND = 3`

### 3) 资源来源

- 服务端：`RESOURCE_SOURCE`
- 客户端：`ResourceSource`

当前已对齐字段：

- `daily_checkin`
- `level_reward`
- `first_clear`
- `star_reward`
- `daily_task`
- `achievement`
- `daily_login`
- `ad_reward`
- `chest_reward`
- `level_consume`

---

## 变更检查清单

修改共享常量后，请按顺序检查：

1. 服务端 `constants.js` 是否已更新
2. 客户端 `Constants.cs` 是否值完全一致
3. API 文档是否同步（`piratecat_slime_express/docs/接口参考-API_REFERENCE.md`）
4. 关键调用点是否仍使用正确常量值

---

## 注意事项

- 客户端字符串常量需要与服务端完全一致（大小写敏感）
- 客户端 `enum` 数值变更属于协议变更，需和服务端一起发版
- 新增资源类型时，需要同时更新：
  - 服务端 `RESOURCE_CONFIG` 构建逻辑
  - 客户端 `ResourceType`、UI 展示与本地持久化逻辑
