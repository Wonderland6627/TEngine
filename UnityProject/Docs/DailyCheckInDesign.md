# 每日宝箱赠送金币功能设计文档

## 1. 功能概述
在主界面设置一个“每日宝箱”按钮，玩家每日登录游戏点击即可领取一次金币奖励。机制简单直接，每日刷新，不可重复领取，不强制连续签到。

## 2. 数据存储 (Client PlayerPrefs)
使用 Unity 内置的 `PlayerPrefs` 进行本地持久化存储。

| Key | 类型 | 描述 |
| :--- | :--- | :--- |
| `DailyChest_LastClaimDate` | string | 上次成功领取宝箱的日期 (格式: `yyyy-MM-dd`, 例如 "2023-10-27")。 |

## 3. 核心逻辑

### A. 状态检查 (打开主界面或应用恢复焦点时触发)
1.  获取当前系统日期 `Today` (使用 `DateTime.Now.ToString("yyyy-MM-dd")`)。
2.  读取 `DailyChest_LastClaimDate`。
3.  **判定逻辑**:
    *   **可领取**: `DailyChest_LastClaimDate` 为空 或 `Today != DailyChest_LastClaimDate`。
        *   **表现**: 宝箱显示正常/特效状态 (如跳动)，可能有红点提示。
    *   **已领取**: `Today == DailyChest_LastClaimDate`。
        *   **表现**: 宝箱显示开启/置灰状态，提示“明日再来”。

### B. 领取动作 (点击“宝箱”按钮)
1.  **前置检查**: 再次确认是否为“可领取”状态 (防止跨天边缘情况)。
2.  **发放奖励**: 
    *   调用 `CurrencyManager` 增加固定或随机数量的金币 (例如 500 金币)。
    *   *播放金币飞入动画*。
3.  **保存数据**:
    *   更新 `DailyChest_LastClaimDate` 为 `Today`。
    *   调用 `PlayerPrefs.Save()`。
4.  **刷新 UI**: 
    *   宝箱变为“已开启”或“置灰”状态。
    *   隐藏红点。

## 4. 奖励配置
*   **奖励内容**: 固定 500 金币 (或配置为 200-800 随机)。
*   **反馈**: 领取成功后弹出通用奖励提示 (`UIRewardPopup` 或类似的 Toast)。

## 5. UI 设计
*   **位置**: 主界面 (`UIMainWindow`) 显眼位置 (如左上角或右上角)。
*   **交互**:
    *   **点击 (可领取)**: 播放开启音效 -> 飞金币特效 -> 增加数值 -> 按钮变灰。
    *   **点击 (已领取)**: 弹出轻提示 "今天已经领过啦，明天再来吧！"。

## 6. 代码集成可行性分析
*   **无需新增系统脚本**: 逻辑非常简单，可以直接写在 `UIMainWindow` 的逻辑中，或者新建一个极其轻量的 `DailyChestCtrl`。
*   **货币接入**: 
    *   调用 `CurrencyManager.Instance.AddCoin(amount, "DailyChest")`。
    *   如果需要纯本地且 `AddCoin` 走网络，可临时使用 `CurrencyManager.Instance.UpdateCoin(CurrencyManager.Instance.CurrentCoin + amount)` (需确认 `UpdateCoin` 访问权限)。

## 7. 实施步骤
1.  **资源准备**: 准备 "宝箱关闭" 和 "宝箱开启(或置灰)" 两张 Sprite。
2.  **UI 搭建**: 在 `UIMainWindow` Prefab 中添加一个 Button。
## 8. 安全性与防作弊 (防修改时间)
纯客户端逻辑如果完全依赖设备本地时间 (`DateTime.Now`)，用户可以通过修改手机系统时间来无限刷取奖励。为解决此问题，建议采用 **网络时间校准** 方案。

### 方案：网络时间校准 (推荐)
**核心思想**: 验证日期时不信任本地时间，而是向高可用的公共服务器发起请求，以服务器返回的时间为准。

1.  **实现原理**:
    *   在玩家点击宝箱或打开界面时，发起一个轻量级的 `UnityWebRequest.Head` 请求。
    *   目标 URL 可以是稳定的大型网站 (如 `https://www.baidu.com` 或 `https://www.qq.com`)，也可以是专门的时间 API。
    *   从 HTTP 响应头 (Response Headers) 中读取 `Date` 字段。
    *   将该 GMT 时间转换为本地日期，作为判断依据。

2.  **逻辑流程**:
    1.  用户点击宝箱。
    2.  显示 Loading 转圈。
    3.  发送 Head 请求 (消耗极小)。
    4.  **请求成功**: 
        *   解析时间得到 `NetToday`。
        *   对比 `NetToday` 与 `DailyChest_LastClaimDate`。
        *   如果不同，发放奖励并保存 `NetToday`。
    5.  **请求失败 (断网)**:
        *   提示“请连接网络以领取奖励”。
        *   (或者) 如果允许离线游玩，可以降级使用本地时间，但记录一个 `RiskyFlag`，下次联网时如果发现时间倒流则扣除金币 (实现较复杂，建议直接禁止离线领取)。

### 补充手段：时间单调性检查 (辅助)
如果不想每次都请求网络，可以配合本地记录：
*   每次保存数据时，额外记录 `LastKnownTimestamp` (时间戳)。
*   每次启动或领取时，检查 `CurrentTimestamp > LastKnownTimestamp`。
*   如果 `CurrentTimestamp` 变小了，说明用户回调了时间，直接锁定宝箱。