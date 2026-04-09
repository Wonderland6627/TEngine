# 云数据库表结构文档（当前实现）

> 本文档描述当前 Express 服务端 + CloudBase 文档库的实际字段结构。  
> 文件名沿用历史命名，但内容已按现状更新。

## 架构说明

- 客户端不再直连云函数执行业务逻辑
- 当前主链路：`Unity -> Express API -> CloudBase 文档数据库`
- 数据集合主要由服务端 DAO 层 `utils/cloudbaseDB.js` 读写

---

## 集合：`UserGameInfos`

### 关键字段

| 字段 | 类型 | 说明 |
|---|---|---|
| `_id` | String | 文档主键 |
| `openID` | String | 用户唯一标识（认证后写入） |
| `progressLevelID` | Number | 当前已通关最高关卡 |
| `nickName` | String | 玩家昵称（排行榜过滤依赖） |
| `avatarUrl` | String | 玩家头像 |
| `resources` | Object | 资源字典，key 为资源类型 ID（字符串） |
| `goods` | Object | 物品字典，key 为 goodsId（字符串） |
| `claimedLevelChests` | Number[] | 已领取的里程碑宝箱关卡 ID 列表 |
| `createdAt` | Date | 创建时间 |
| `updatedAt` | Date | 更新时间 |

### `resources` 约定

当前资源类型（与 `ResourceType` 对齐）：

- `"1"`：Coin
- `"2"`：Energy
- `"3"`：Diamond

示例：

```json
{
  "1": 500,
  "2": 120,
  "3": 0
}
```

### `goods` 约定

- 动态映射，按 `goodsId` 存储数量
- 示例：`{ "1001": 2, "1002": 1 }`

### 示例文档

```json
{
  "_id": "7f2b0e...",
  "openID": "editor_user_a",
  "progressLevelID": 12,
  "nickName": "测试玩家",
  "avatarUrl": "https://example.com/avatar.png",
  "resources": {
    "1": 820,
    "2": 130,
    "3": 0
  },
  "goods": {
    "1001": 2
  },
  "claimedLevelChests": [3, 6, 9],
  "createdAt": "2026-04-09T10:00:00.000Z",
  "updatedAt": "2026-04-09T10:30:00.000Z"
}
```

---

## 集合：`Levels`

由 `getLevelsConfigV2` 读取，默认通过固定 `_id` 读取一条配置文档。  
客户端当前使用数据结构：

- 顶层对象
- `configs.levels` 为关卡数组

---

## 关键业务读写关系

### 登录与用户初始化

- 接口：`getCode2Session` + `getUserGameInfoV2`
- `getUserGameInfoV2` 在用户不存在时自动创建新文档（含默认 `resources` 和 `goods`）

### 资源变更

- 接口：`updateResource`
- 写入路径：`resources.{resourceTypeId}`
- 使用原子 `inc`

### 通关奖励

- 接口：`claimLevelReward`
- 统一结算 coin / energy，并在首次通关时推进 `progressLevelID`

### 每日签到

- 接口：`claimDailyCheckin`
- 根据 `TbGlobalConfig.daily_checkin_reward_id` 解析奖励并原子发放

### 推关激励宝箱

- 接口：`claimLevelChest`
- 校验进度与重复领取后：
  - `claimedLevelChests` push
  - 资源/物品原子发放

---

## 索引建议

建议至少维护以下索引：

1. `openID`（唯一或高选择性查询）
2. `progressLevelID`（排行榜排序）
3. `nickName`（排行榜过滤非空时可辅助）

---

## 字段命名注意

- 用户主键字段使用 `openID`（大写 ID）
- 客户端/服务端交互时，资源和物品映射统一用字符串键（JSON 对象键）
