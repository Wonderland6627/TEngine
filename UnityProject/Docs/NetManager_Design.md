# NetManager 设计思路（简洁版）

## 一、云函数返回值规范

### 标准响应格式
```json
{
  "code": 0,        // 0=成功, 非0=失败
  "data": {},       // 业务数据（任意类型）
  "msg": "success"  // 消息描述
}
```

### 错误码规范
- `code = 0`: 成功
- `code < 0`: 业务错误（如 -1, -2...）
- 网络错误：通过 fail 回调处理

---

## 二、NetManager 核心设计（伪代码）

### 1. 响应包装类
```csharp
// 统一响应格式
class CloudResponse<T> {
    int code;
    T data;
    string msg;
    
    bool IsSuccess => code == 0;
}
```

### 2. NetManager 核心类
```csharp
class NetManager {
    // 通用调用方法
    static async UniTask<CloudResponse<T>> Call<T>(
        string functionName,
        object params = null
    ) {
        var tcs = new UniTaskCompletionSource<CloudResponse<T>>();
        
        WX.cloud.CallFunction(new CallFunctionParam() {
            name = functionName,
            data = params,
            success = (res) => {
                // 自动解析响应
                var response = Json.ToObject<CloudResponse<T>>(res.result);
                tcs.TrySetResult(response);
            },
            fail = (err) => {
                // 网络错误处理
                Log.Error($"Call {functionName} failed: {err}");
                tcs.TrySetException(new Exception(err.ToJson()));
            }
        });
        
        return await tcs.Task;
    }
    
    // 便捷方法：只返回数据（成功时）
    static async UniTask<T> CallData<T>(
        string functionName,
        object params = null
    ) {
        var response = await Call<CloudResponse<T>>(functionName, params);
        return response.IsSuccess ? response.data : default(T);
    }
}
```

---

## 三、使用示例

### 示例1：获取用户信息
```csharp
// 方式1：获取完整响应
var response = await NetManager.Call<CloudResponse<UserInfo>>("getUserGameInfo");
if (response.IsSuccess) {
    var userInfo = response.data;
    // 使用 userInfo
} else {
    Log.Error($"Error: {response.msg}");
}

// 方式2：直接获取数据（更简洁）
var userInfo = await NetManager.CallData<UserInfo>("getUserGameInfo");
if (userInfo != null) {
    // 使用 userInfo
}
```

### 示例2：获取排行榜列表
```csharp
var rankList = await NetManager.CallData<List<RankInfo>>("getUserRankList");
if (rankList != null) {
    // 使用 rankList
}
```

### 示例3：设置数据（无返回值）
```csharp
var response = await NetManager.Call<CloudResponse<object>>(
    "setUserGameInfo", 
    new { progressLevelID = 10 }
);
if (response.IsSuccess) {
    Log.Info("设置成功");
}
```

---

## 四、设计要点

### 优点
1. **简洁**：一行代码完成调用和解析
2. **统一**：所有云函数调用使用相同接口
3. **类型安全**：泛型自动解析，编译期检查
4. **错误处理**：统一的错误处理机制

### 核心流程
```
调用云函数 
  → 自动解析响应 
    → 返回 CloudResponse<T>
      → 业务代码判断 IsSuccess
        → 使用 data
```

---

## 五、扩展建议（可选）

### 1. 添加重试机制
```csharp
static async UniTask<CloudResponse<T>> CallWithRetry<T>(
    string functionName,
    object params = null,
    int retryCount = 3
) {
    for (int i = 0; i < retryCount; i++) {
        try {
            return await Call<T>(functionName, params);
        } catch {
            if (i == retryCount - 1) throw;
            await UniTask.Delay(1000);
        }
    }
}
```

### 2. 添加缓存（可选）
```csharp
static Dictionary<string, object> cache = new();

static async UniTask<T> CallWithCache<T>(
    string functionName,
    object params = null,
    float cacheTime = 60f
) {
    string key = $"{functionName}_{params?.ToJson()}";
    if (cache.ContainsKey(key)) {
        return (T)cache[key];
    }
    
    var data = await CallData<T>(functionName, params);
    cache[key] = data;
    // 定时清除缓存...
    return data;
}
```

---

## 六、总结

**核心思想**：
- 统一响应格式：`{code, data, msg}`
- 统一调用接口：`NetManager.Call<T>()`
- 自动解析：无需手动 JSON 解析
- 类型安全：泛型自动转换

**使用原则**：
- 简单调用用 `CallData<T>()`
- 需要错误信息用 `Call<CloudResponse<T>>()`
- 保持代码简洁，避免过度设计

