# YooAsset CDN 版本目录结构支持

## 功能说明

已实现支持从 CDN 根目录读取版本文件，并根据版本号从对应版本目录读取 bundle 和清单文件的功能。

## CDN 结构

```
https://cdn.example.com/game/
├── Package.version          <-- 版本文件（根目录）
└── v1.2/                    <-- 版本目录
    ├── Package_v1.2.hash
    ├── Package_v1.2.json
    ├── Package_v1.2.bytes
    └── [bundle files...]
```

## 实现原理

### 1. 版本文件读取

- **位置**：CDN 根目录
- **文件名**：`Package.version`（简化格式）或 `PackageManifest_{PackageName}_{BuildVersion}.version`（标准格式）
- **URL 格式**：`{CDNBaseURL}/Package.version`

### 2. Bundle 和清单文件读取

- **位置**：版本目录
- **URL 格式**：`{CDNBaseURL}/{version}/{fileName}`
- **示例**：
  - Bundle: `https://cdn.example.com/game/v1.2/bundle_name.bundle`
  - 清单: `https://cdn.example.com/game/v1.2/PackageManifest_DefaultPackage_v1.2.bytes`

## 代码实现

### RemoteServices 类

`RemoteServices` 类已增强，支持：
1. 自动识别版本文件（`.version` 结尾）
2. 版本文件从根目录读取
3. Bundle 和清单文件从版本目录读取
4. 自动从 package 获取当前版本号

### 版本号更新流程

1. **初始化阶段**：
   - 创建 `RemoteServices` 时传递 `packageName`
   - 存储 `RemoteServices` 引用到 `RemoteServicesMap`

2. **版本读取阶段**：
   - `QueryRemotePackageVersionOperation` 调用 `GetRemoteMainURL("Package.version")`
   - `RemoteServices` 识别为版本文件，返回根目录路径：`{CDNBaseURL}/Package.version`

3. **版本更新阶段**：
   - `ProcedureUpdateVersion` 在版本更新成功后调用 `UpdateRemoteServicesVersion`
   - 更新 `RemoteServices` 的版本号

4. **Bundle 下载阶段**：
   - `RemoteServices.GetRemoteMainURL(bundleFileName)` 被调用
   - 识别为非版本文件，返回版本目录路径：`{CDNBaseURL}/{version}/{bundleFileName}`

## 配置说明

### HostServerURL 配置

在 `TEngineGlobalSettings.asset` 中配置 CDN 根目录：

```yaml
HostServerURL: https://cdn.example.com/game
FallbackHostServerURL: https://cdn.example.com/game
```

**注意**：URL 应该是 CDN 的根目录，不包含版本号。

### 版本文件命名

支持两种版本文件命名格式：
1. **简化格式**（推荐）：`Package.version`
2. **标准格式**：`PackageManifest_{PackageName}_{BuildVersion}.version`

`RemoteServices` 会自动识别这两种格式。

## 使用示例

### 1. 配置 CDN 地址

在 Unity Editor 中：
1. 打开 `Assets/TEngine/ResRaw/Resources/TEngineGlobalSettings.asset`
2. 设置 `HostServerURL` 为 CDN 根目录（如：`https://cdn.example.com/game`）

### 2. 上传资源到 CDN

按照以下结构上传资源：

```
game/
├── Package.version          (内容：v1.2)
└── v1.2/
    ├── Package_v1.2.hash
    ├── Package_v1.2.json
    ├── Package_v1.2.bytes
    └── [所有 bundle 文件]
```

### 3. 运行时行为

游戏运行时会：
1. 从 `{CDNBaseURL}/Package.version` 读取版本号（如：`v1.2`）
2. 根据版本号从 `{CDNBaseURL}/v1.2/` 目录下载 bundle 和清单文件

## 兼容性

### 向后兼容

如果 CDN 上还没有版本目录结构，`RemoteServices` 会：
- 版本文件仍然从根目录读取
- Bundle 文件如果版本号未设置，会尝试从根目录读取（兼容旧逻辑）

### 渐进式迁移

可以逐步迁移到新结构：
1. 先上传版本文件到根目录
2. 逐步将 bundle 移动到版本目录
3. 旧版本仍然可以从根目录读取

## 日志

`RemoteServices` 会输出以下日志：
- `[RemoteServices] Updated package version to: {version}` - 版本号更新
- `[RemoteServices] Package version not available, using root path for file: {fileName}` - 版本号未设置时的警告

## 注意事项

1. **版本文件格式**：`Package.version` 文件内容应该是纯文本版本号（如：`v1.2`），不包含其他内容
2. **URL 格式**：确保 CDN URL 以 `/` 结尾或不包含尾部斜杠，代码会自动处理
3. **版本号格式**：建议使用语义化版本号（如：`v1.2.3`），但任何字符串都可以
4. **大小写敏感**：CDN 路径是大小写敏感的，确保版本号大小写一致

## 故障排查

### 问题：无法读取版本文件

**可能原因**：
- CDN URL 配置错误
- 版本文件不存在或路径错误
- 网络连接问题

**解决方法**：
1. 检查 `HostServerURL` 配置
2. 确认 `Package.version` 文件存在于 CDN 根目录
3. 检查网络连接和 CDN 访问权限

### 问题：Bundle 下载失败

**可能原因**：
- 版本号未正确更新
- Bundle 文件不在版本目录中
- 版本目录路径错误

**解决方法**：
1. 检查日志中的版本号更新信息
2. 确认 bundle 文件在正确的版本目录中
3. 检查版本号格式是否正确

## 相关文件

- `Assets/TEngine/Runtime/Modules/ResourceModule/ResourceManager.Services.cs` - RemoteServices 实现
- `Assets/TEngine/Runtime/Modules/ResourceModule/ResourceManager.cs` - 版本号更新逻辑
- `Assets/GameScripts/Main/Procedure/ProcedureUpdateVersion.cs` - 版本更新流程

