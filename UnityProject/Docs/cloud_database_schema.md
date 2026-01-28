# 微信小游戏云数据库表结构文档

## 通信流程

### 1. 初始化流程
```
客户端 → WX.InitSDK() → WX.cloud.Init() → 初始化云环境
```

### 2. 用户登录流程
```
客户端 → WX.Login() → 获取 code
客户端 → getUserWXContext() → 获取 openid、appid、unionid
```

### 3. 获取用户游戏信息流程
```
客户端 → getUserGameInfo() 
云函数 → 查询 UserGameInfos 集合（where openid = OPENID）
       → 如果不存在，创建空记录
       → 返回用户游戏数据
客户端 → 解析数据，更新本地 ProgressLevelID
```

### 4. 更新用户游戏信息流程
```
客户端 → setUserGameInfo(progressLevelID, nickName, avatarUrl)
云函数 → 查询 UserGameInfos 集合（where openID = OPENID）
       → 如果不存在，创建新记录
       → 如果存在，更新顶层字段（progressLevelID, nickName, avatarUrl等）和 updatedAt
       → 返回更新结果
```

### 5. 获取排行榜流程
```
客户端 → getUserRankList()
云函数 → 查询 UserGameInfos 集合
       → 按 progressLevelID 降序排序（字段在顶层）
       → 限制返回前 100 条记录
       → 返回排行榜数据
客户端 → 解析数据，生成 PlayerRankInfo 列表
```

## 云数据库表结构

### 集合名称：UserGameInfos

**注意**：`userGameInfo` 字段已弃用，所有游戏相关字段现在都直接在文档顶层。

| 字段名 | 类型 | 说明 | 是否必填 | 备注 |
|--------|------|------|----------|------|
| `_id` | String | 文档唯一标识 | 是 | 数据库自动生成 |
| `openID` | String | 用户唯一标识 | 是 | 通过认证获取，用于查询和更新用户数据 |
| `progressLevelID` | Number | 用户当前进度关卡ID | 否 | 用于排行榜排序，默认值为 0 |
| `nickName` | String | 用户昵称 | 否 | 通过客户端传入，用于排行榜显示 |
| `avatarUrl` | String | 用户头像URL | 否 | 通过客户端传入，用于排行榜显示 |
| `coin` | Number | 用户金币数量 | 否 | 默认值为 0 |
| `createdAt` | Date | 记录创建时间 | 是 | 首次创建时自动设置 |
| `updatedAt` | Date | 记录更新时间 | 是 | 每次更新时自动更新 |

**已弃用字段**：
- `userGameInfo` - 此字段已弃用，其内容已拆分为顶层字段
- `openid` - 已统一为 `openID`（注意大小写）

## 数据示例

### 完整记录示例
```json
{
  "_id": "073a77ac681a0c320284cee70fe9ff04",
  "openID": "ox0H160OiHbng6giS50wOp6YZ7R4",
  "progressLevelID": 2,
  "nickName": "Indey",
  "avatarUrl": "https://thirdwx.qlogo.cn/mmopen/vi_32/...",
  "coin": 150,
  "createdAt": "2025-05-06T13:18:42.514Z",
  "updatedAt": "2025-05-06T13:50:00.139Z"
}
```

### 空记录示例（新用户）
```json
{
  "_id": "2b83cb16681b842a02941e9307bb451f",
  "openID": "ox0H168lFD1nmJ_mBG7VR1lc3QwI",
  "progressLevelID": 0,
  "nickName": "",
  "avatarUrl": "",
  "coin": 0,
  "createdAt": "2025-05-07T16:02:50.522Z",
  "updatedAt": "2025-05-07T16:02:50.522Z"
}
```

## 云函数说明

### getUserGameInfo
- **功能**：获取当前用户的游戏信息
- **参数**：无（自动从微信上下文获取 openid）
- **返回**：用户游戏数据，如果不存在则创建空记录

### setUserGameInfo
- **功能**：设置/更新当前用户的游戏信息
- **参数**：
  - `progressLevelID` (Number): 进度关卡ID
  - `nickName` (String, 可选): 用户昵称
  - `avatarUrl` (String, 可选): 用户头像URL
- **返回**：更新结果

### getUserRankList
- **功能**：获取排行榜数据
- **参数**：无
- **返回**：前100名用户数据，按 `progressLevelID` 降序排序（字段在文档顶层）

### getCode2Session
- **功能**：获取微信上下文信息
- **参数**：`code` (String, 可选)
- **返回**：`openid`、`appid`、`unionid`

## 索引建议

为了提高查询性能，建议为以下字段创建索引：

1. **openID**：单字段索引（用于快速查询用户数据）
2. **progressLevelID**：单字段索引（用于排行榜排序）
3. **coin**：单字段索引（可选，用于金币排行榜）

## 注意事项

1. `openID` 是用户唯一标识，所有查询和更新操作都基于此字段（注意大小写）
2. **`userGameInfo` 字段已弃用**，所有游戏相关字段（`progressLevelID`、`nickName`、`avatarUrl`、`coin` 等）现在都直接在文档顶层
3. 新用户首次调用 `getUserGameInfo` 时会自动创建空记录
4. 排行榜查询限制为前100名，如需更多数据需要修改云函数
5. `progressLevelID` 是排行榜排序的关键字段，确保该字段存在且为数字类型
6. 字段命名统一使用驼峰命名法（如 `progressLevelID`、`openID`）

