# 每日签到（每日宝箱）设计文档（当前实现）

## 功能概述

主界面每日宝箱当前走“服务端结算 + 客户端本地去重”模式：

1. 客户端先同步服务器时间
2. 客户端判断今天是否已领取（本地 `PlayerPrefs` 日期）
3. 若未领取，调用 `claimDailyCheckin`
4. 服务端按配置结算奖励并返回资源/物品增量
5. 客户端同步资源与物品，并记录今日已领

---

## 客户端实现

核心类：`DailyChestManager`

关键点：

- 本地 key：`DailyChest_LastClaimDate`
- 日期来源：`NetManager.ServerTime.ToLocalTime().Date`
- 入口方法：`TryClaimToday()`

流程伪代码：

```csharp
await NetManager.Instance.SyncServerTime();
if (IsTodayClaimed()) return null;

var response = await NetManager.Instance.CallHttp<ClaimDailyCheckinResponse>("claimDailyCheckin");
if (!response.IsSuccess) return null;

World.Instance.SyncResources(response.data.resources);
World.Instance.SyncGoods(response.data.goods);
PlayerPrefs.SetString("DailyChest_LastClaimDate", GetServerDate());
```

---

## 服务端实现

路由：

- `POST /api/minigame/claimDailyCheckin`

控制器：

- `controllers/dailyCheckinController.js`

服务层：

- `services/dailyCheckinService.js`

结算逻辑：

1. 从 `tbglobalconfig` 读取 `daily_checkin_reward_id`
2. 通过 `rewardService.resolveReward` 解析奖励条目
3. 资源走 `resourceService.batchUpdateResources`
4. 物品走 `goodsService.batchAddGoods`
5. 返回 `rewards + resources + goods`

---

## 响应结构

`ClaimDailyCheckinResponse`：

```json
{
  "rewards": [
    { "itemType": 1, "itemId": 1, "amount": 100, "source": "daily_checkin" }
  ],
  "resources": {
    "1": 500,
    "2": 120
  },
  "goods": {
    "1001": 1
  }
}
```

---

## 设计取舍

- **服务端统一结算**：奖励规则不暴露在客户端，避免作弊和多端不一致
- **客户端本地日期去重**：减少重复请求，提升体验
- **服务器时间校准**：避免直接依赖设备本地时钟

---

## 相关文件

### 客户端

- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/Manager/DailyChestManager.cs`
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/Network/NetManager.cs`
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/Manager/World/World_GameData.cs`
- `Assets/GameScripts/HotFix/GameLogic/SlimeGame/Network/Models/CurrencyResponseData.cs`

### 服务端

- `piratecat_slime_express/routes/minigame.js`
- `piratecat_slime_express/controllers/dailyCheckinController.js`
- `piratecat_slime_express/services/dailyCheckinService.js`
- `piratecat_slime_express/services/rewardService.js`
