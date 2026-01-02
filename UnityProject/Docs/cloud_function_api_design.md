# 云函数数据交换方案设计

## 问题分析

当前代码存在的问题：
1. **重复的JSON解析代码**：每次调用云函数都需要手动解析 `code`、`data`、`msg`
2. **格式兼容逻辑分散**：新旧格式兼容代码在每个地方重复
3. **错误处理不统一**：每个云函数调用的错误处理方式不一致
4. **类型安全性差**：使用 `JObject` 和 `Dictionary`，容易出错
5. **代码可维护性差**：业务逻辑和解析逻辑混在一起

## 方案对比

### 方案一：统一响应包装器 + 泛型解析器（推荐⭐⭐⭐⭐⭐）

**核心思想**：定义标准的响应格式，使用泛型自动解析

**优点**：
- ✅ 类型安全，编译期检查
- ✅ 代码简洁，减少重复
- ✅ 易于扩展和维护
- ✅ 支持新旧格式兼容的统一处理
- ✅ 错误处理统一

**缺点**：
- ⚠️ 需要定义响应模型类
- ⚠️ 需要一定的学习成本

**实现示例**：
```csharp
// 1. 定义标准响应格式
public class CloudResponse<T>
{
    public int code { get; set; }
    public T data { get; set; }
    public string msg { get; set; }
    
    public bool IsSuccess => code == 0;
}

// 2. 定义数据模型
public class UserGameInfoData
{
    public string _id { get; set; }
    public string openid { get; set; }
    public int progressLevelID { get; set; }
    public string nickName { get; set; }
    public string avatarUrl { get; set; }
    public string openID { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
    
    // 兼容旧格式的字段（可选）
    [JsonProperty("userGameInfo")]
    public UserGameInfoLegacy userGameInfo { get; set; }
}

// 3. 使用方式
var response = await CloudFunctionService.CallAsync<CloudResponse<UserGameInfoData>>(
    "getUserGameInfo", 
    null
);

if (response.IsSuccess)
{
    var userData = response.data;
    // 自动处理新旧格式兼容
    int levelID = userData.progressLevelID; // 优先新格式
}
```

---

### 方案二：云函数服务层抽象（推荐⭐⭐⭐⭐）

**核心思想**：创建 `CloudFunctionService` 封装所有云函数调用

**优点**：
- ✅ 统一管理所有云函数
- ✅ 可以统一处理错误、日志、重试等
- ✅ 易于添加缓存、请求队列等功能
- ✅ 代码组织清晰

**缺点**：
- ⚠️ 需要维护服务类
- ⚠️ 可能增加代码量

**实现示例**：
```csharp
public class CloudFunctionService
{
    // 统一调用入口
    public static async UniTask<CloudResponse<T>> CallAsync<T>(
        string functionName, 
        object parameters = null,
        System.Action<T> onSuccess = null,
        System.Action<string> onError = null)
    {
        // 统一处理：调用、解析、错误处理、格式兼容
    }
    
    // 便捷方法
    public static async UniTask<UserGameInfoData> GetUserGameInfoAsync()
    {
        var response = await CallAsync<CloudResponse<UserGameInfoData>>("getUserGameInfo");
        return response.IsSuccess ? response.data : null;
    }
}

// 使用方式
var userInfo = await CloudFunctionService.GetUserGameInfoAsync();
if (userInfo != null)
{
    GameData.SetProgressLevelID(userInfo.progressLevelID);
}
```

---

### 方案三：请求/响应协议定义（推荐⭐⭐⭐）

**核心思想**：定义请求和响应的接口协议，使用特性标记

**优点**：
- ✅ 协议清晰，易于文档化
- ✅ 可以自动生成代码
- ✅ 支持版本管理

**缺点**：
- ⚠️ 实现复杂度较高
- ⚠️ 可能过度设计

**实现示例**：
```csharp
[CloudFunction("getUserGameInfo")]
public class GetUserGameInfoRequest : ICloudRequest
{
    // 请求参数（如果有）
}

[CloudFunctionResponse]
public class GetUserGameInfoResponse : ICloudResponse<UserGameInfoData>
{
    public int code { get; set; }
    public UserGameInfoData data { get; set; }
    public string msg { get; set; }
}

// 使用方式
var response = await CloudFunctionClient.CallAsync<GetUserGameInfoRequest, GetUserGameInfoResponse>(
    new GetUserGameInfoRequest()
);
```

---

### 方案四：混合方案（推荐⭐⭐⭐⭐⭐）

**核心思想**：结合方案一和方案二，提供最佳实践

**架构**：
```
CloudFunctionService (服务层)
    ↓
CloudResponse<T> (响应包装)
    ↓
Data Models (数据模型)
    ↓
Format Compatibility (格式兼容层)
```

**完整实现结构**：
```
CloudFunction/
├── Core/
│   ├── CloudResponse.cs          // 标准响应格式
│   ├── CloudFunctionService.cs   // 服务层
│   └── FormatCompatibility.cs    // 格式兼容处理
├── Models/
│   ├── UserGameInfoData.cs       // 用户游戏信息
│   ├── PlayerRankInfoData.cs    // 排行榜信息
│   └── ...
└── Extensions/
    └── CloudResponseExtensions.cs // 扩展方法
```

---

## 推荐方案：混合方案（方案四）

### 实现结构

#### 1. 核心响应类
```csharp
namespace GameLogic.CloudFunction
{
    /// <summary>
    /// 云函数标准响应格式
    /// </summary>
    public class CloudResponse<T>
    {
        public int code { get; set; }
        public T data { get; set; }
        public string msg { get; set; }
        
        public bool IsSuccess => code == 0;
        
        public string ErrorMessage => IsSuccess ? null : msg;
    }
}
```

#### 2. 数据模型（支持新旧格式兼容）
```csharp
namespace GameLogic.CloudFunction.Models
{
    /// <summary>
    /// 用户游戏信息数据模型（自动兼容新旧格式）
    /// </summary>
    public class UserGameInfoData
    {
        public string _id { get; set; }
        public string openid { get; set; }
        
        // 新格式字段（优先）
        public int? progressLevelID { get; set; }
        public string nickName { get; set; }
        public string avatarUrl { get; set; }
        public string openId { get; set; }
        
        // 旧格式兼容（用于反序列化）
        [JsonProperty("userGameInfo")]
        public UserGameInfoLegacy LegacyData { get; set; }
        
        // 兼容属性：优先使用新格式，如果没有则使用旧格式
        [JsonIgnore]
        public int ProgressLevelID => progressLevelID ?? LegacyData?.progressLevelID ?? 0;
        
        [JsonIgnore]
        public string NickName => nickName ?? LegacyData?.nickName ?? "";
        
        [JsonIgnore]
        public string AvatarUrl => avatarUrl ?? LegacyData?.avatarUrl ?? "";
        
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
    
    [System.Serializable]
    public class UserGameInfoLegacy
    {
        public int? progressLevelID { get; set; }
        public string nickName { get; set; }
        public string avatarUrl { get; set; }
    }
}
```

#### 3. 云函数服务层
```csharp
namespace GameLogic.CloudFunction
{
    public static class CloudFunctionService
    {
        /// <summary>
        /// 调用云函数（通用方法）
        /// </summary>
        public static async UniTask<CloudResponse<T>> CallAsync<T>(
            string functionName,
            object parameters = null)
        {
            var tcs = new UniTaskCompletionSource<CloudResponse<T>>();
            
            WX.cloud.CallFunction(new CallFunctionParam()
            {
                name = functionName,
                data = parameters,
                success = (res) =>
                {
                    try
                    {
                        var response = Utility.Json.ToObject<CloudResponse<T>>(res.result);
                        tcs.TrySetResult(response);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"[CloudFunction] Parse response failed: {e}");
                        tcs.TrySetException(e);
                    }
                },
                fail = (err) =>
                {
                    Log.Error($"[CloudFunction] Call {functionName} failed: {err.ToJson()}");
                    tcs.TrySetException(new Exception(err.ToJson()));
                }
            });
            
            return await tcs.Task;
        }
        
        /// <summary>
        /// 获取用户游戏信息
        /// </summary>
        public static async UniTask<UserGameInfoData> GetUserGameInfoAsync()
        {
            var response = await CallAsync<CloudResponse<UserGameInfoData>>("getUserGameInfo");
            if (response.IsSuccess)
            {
                return response.data;
            }
            Log.Error($"[CloudFunction] GetUserGameInfo failed: {response.ErrorMessage}");
            return null;
        }
        
        /// <summary>
        /// 设置用户游戏信息
        /// </summary>
        public static async UniTask<bool> SetUserGameInfoAsync(
            int progressLevelID, 
            string nickName = "", 
            string avatarUrl = "")
        {
            var parameters = new Dictionary<string, object>
            {
                { "progressLevelID", progressLevelID }
            };
            if (!string.IsNullOrEmpty(nickName))
                parameters.Add("nickName", nickName);
            if (!string.IsNullOrEmpty(avatarUrl))
                parameters.Add("avatarUrl", avatarUrl);
                
            var response = await CallAsync<CloudResponse<object>>("setUserGameInfo", parameters);
            return response.IsSuccess;
        }
        
        /// <summary>
        /// 获取排行榜
        /// </summary>
        public static async UniTask<List<PlayerRankInfoData>> GetUserRankListAsync()
        {
            var response = await CallAsync<CloudResponse<List<UserGameInfoData>>>("getUserRankList");
            if (response.IsSuccess && response.data != null)
            {
                return response.data
                    .Where(u => !string.IsNullOrEmpty(u.NickName) && !string.IsNullOrEmpty(u.AvatarUrl))
                    .Select((u, index) => new PlayerRankInfoData
                    {
                        playerRank = index + 1,
                        openid = u.openid,
                        progressLevelID = u.ProgressLevelID,
                        nickName = u.NickName,
                        avatarURL = u.AvatarUrl
                    })
                    .OrderByDescending(r => r.progressLevelID)
                    .ToList();
            }
            return new List<PlayerRankInfoData>();
        }
    }
}
```

#### 4. 使用示例
```csharp
// 旧代码（繁琐）
public void GetUserGameInfo()
{
    WX.cloud.CallFunction(new CallFunctionParam()
    {
        name = "getUserGameInfo",
        success = (res) =>
        {
            JObject resultJson = JObject.Parse(res.result);
            if (resultJson.TryGetValue("code", out var code))
            {
                int codeInt = code.ToObject<int>();
                if (codeInt != 0) return;
            }
            // ... 大量解析代码
        }
    });
}

// 新代码（简洁）
public async UniTask GetUserGameInfo()
{
    var userInfo = await CloudFunctionService.GetUserGameInfoAsync();
    if (userInfo != null)
    {
        GameData.SetProgressLevelID(userInfo.ProgressLevelID); // 自动兼容新旧格式
    }
}
```

---

## 方案对比总结

| 特性 | 方案一 | 方案二 | 方案三 | 方案四（推荐） |
|------|--------|--------|--------|----------------|
| 类型安全 | ✅ | ✅ | ✅ | ✅ |
| 代码简洁 | ✅✅ | ✅✅ | ✅ | ✅✅ |
| 易于维护 | ✅✅ | ✅✅ | ✅ | ✅✅ |
| 格式兼容 | ✅ | ✅ | ✅ | ✅✅ |
| 扩展性 | ✅ | ✅✅ | ✅✅ | ✅✅ |
| 实现复杂度 | 低 | 中 | 高 | 中 |
| 学习成本 | 低 | 低 | 中 | 低 |

---

## 建议

**推荐使用方案四（混合方案）**，原因：
1. ✅ 结合了方案一和方案二的优点
2. ✅ 类型安全，减少运行时错误
3. ✅ 代码简洁，减少重复
4. ✅ 统一处理格式兼容问题
5. ✅ 易于扩展和维护
6. ✅ 实现复杂度适中

**实施步骤**：
1. 创建 `CloudFunction` 目录结构
2. 实现核心类（`CloudResponse`、`CloudFunctionService`）
3. 定义数据模型（支持新旧格式兼容）
4. 逐步迁移现有代码
5. 添加单元测试

---

## 下一步

请选择您偏好的方案，我可以立即开始实现。如果选择方案四，我可以：
1. 创建完整的代码结构
2. 实现所有核心类
3. 迁移现有的云函数调用代码
4. 添加详细的注释和文档



