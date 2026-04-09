# 响应规范检查报告（文件名历史保留）

> 说明：文件名沿用历史命名，但当前检查对象已切换为 Express HTTP API。  
> 检查时间：2026-04-09

## 统一规范

业务接口统一响应格式：

```json
{
  "code": 0,
  "data": {},
  "msg": "success"
}
```

对应服务端封装：

- 成功：`middlewares/response.js -> success()`
- 失败：`middlewares/response.js -> error()`
- 异常统一出口：`middlewares/errorHandler.js`

---

## 检查范围

### 已检查业务接口（`/api/minigame/*`）

- `getCode2Session`
- `getUserWXContext`
- `getUserGameInfoV2`
- `setUserGameInfoV2`
- `getUserRankListV2`
- `getLevelsConfigV2`
- `updateResource`
- `getResources`
- `claimLevelReward`
- `claimDailyCheckin`
- `claimLevelChest`

### 结论

- 上述业务接口均通过 `success()` / `error()` 返回，符合 `{code,data,msg}` 规范。

---

## 特殊接口说明（非业务统一壳）

以下接口不是 `minigame` 业务接口，不强制套 `{code,data,msg}`：

- `GET /health`：返回 `{status, timestamp}`（用于健康检查）

以下接口已符合统一壳：

- `GET /api/version`
- `POST /api/time`

---

## 兼容性结论

- 客户端 `Response<T>` 解析逻辑与服务端业务接口当前实现一致。
- `health` 接口不应使用 `NetManager.CallHttp<T>()` 的业务响应模型解析。

---

## 建议

1. 未来新增 `minigame` 业务接口时，统一走 `success()` 返回
2. 若新增非业务基础接口（如监控/探针），在文档中标注“不走业务响应壳”
