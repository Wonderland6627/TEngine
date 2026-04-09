# 微信小游戏打包流程分析

## 打包流程步骤

### 步骤1: 修改 YooAssetSettings.asset 的 BuildVersion
- **文件路径**: `Assets/TEngine/AssetSetting/Resources/YooAssetSettings.asset`
- **需要修改**: `BuildVersion` 字段（当前值: `v0.1.7.2`）
- **脚本可行性**: ✅ **可以通过脚本执行**
  - 使用 `AssetDatabase.LoadAssetAtPath` 加载 ScriptableObject
  - 修改 `BuildVersion` 属性
  - 使用 `EditorUtility.SetDirty` 和 `AssetDatabase.SaveAssets` 保存

### 步骤2: 修改 InnerResourceSourceUrl 的版本号
- **文件路径**: `Assets/TEngine/ResRaw/Resources/TEngineGlobalSettings.asset`
- **需要修改**: `m_InnerResourceSourceUrl` 字段
- **URL模板**: `https://a.unity.cn/client_api/v1/buckets/cde09f24-d39c-4845-a3e3-17344f4f2894/content/MiniGame/【版本号】/`
- **脚本可行性**: ✅ **可以通过脚本执行**
  - 加载 `TEngineGlobalSettings` ScriptableObject
  - 修改 `ResourcesArea.InnerResourceSourceUrl` 属性
  - 保存修改

### 步骤3: 将 InnerResourceSourceUrl 复制到小游戏导出配置的 CDN 地址
- **文件路径**: `Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset`
- **需要修改**: `ProjectConf.CDN` 字段
- **脚本可行性**: ✅ **可以通过脚本执行**
  - 加载 `MiniGameConfig` ScriptableObject
  - 将步骤2的 URL 复制到 `ProjectConf.CDN`
  - 保存修改

### 步骤4: 导出微信小游戏工程
- **导出路径**: `D:/CustomProjects/TEngine/UnityProject/WXExport` (从 MiniGameConfig.asset 的 DST 字段获取)
- **脚本可行性**: ⚠️ **部分可通过脚本执行**
  - 可以使用 `BuildPipeline.BuildPlayer` 构建 WebGL 目标
  - 但微信小游戏可能需要特定的后处理步骤（可能需要调用微信SDK的导出方法）
  - 需要确认微信小游戏SDK是否提供导出API

### 步骤5: 复制 StreamingAssets 和 bin.txt 到 Backup 目录
- **源路径**: `WXExport/StreamingAssets` 和 `WXExport/*.bin.txt`
- **目标路径**: `CDN_Backup/MiniGame/【版本号】/`
- **脚本可行性**: ✅ **完全可以通过脚本执行**
  - 使用 `FileUtil.CopyFileOrDirectory` 或 `System.IO` 进行文件复制
  - 创建版本号目录
  - 复制 StreamingAssets 文件夹
  - 查找并复制所有 `.bin.txt` 文件

### 步骤6: 上传 StreamingAssets 和 bin.txt 到 UOSCDN
- **UOSCDN地址**: `https://uos.unity.cn/services/bd2fcdf8-2152-4e5f-b1be-3f6b950c8034/asset/bucket/cde09f24-d39c-4845-a3e3-17344f4f2894`
- **上传路径**: `content/MiniGame/【版本号】/`
- **脚本可行性**: ⚠️ **需要确认UOSCDN API**
  - 项目中有 `cn.unity.uos.launcher` 包
  - 需要查找 UOSCDN 的上传 API
  - 可能需要使用 Unity Cloud Services API 或 UOS SDK

### 步骤7: 在 UOSCDN 中创建新的 release 并 assign 版本号
- **脚本可行性**: ⚠️ **需要确认UOSCDN API**
  - 需要查找 UOSCDN 的 release 创建 API
  - 可能需要使用 Unity Cloud Services API

### 步骤8: 测试
- **脚本可行性**: ❌ **需要手动操作**
  - Unity 中测试
  - 微信小游戏开发者工具中测试

## 资源修改判断逻辑

根据用户需求，需要区分两种情况：

### 情况1: 修改了资源（需要重新 build 和上传 bundle）
- ✅ 执行步骤1: 修改 YooAssetSettings.asset 的 BuildVersion
- ✅ 执行步骤2: 修改 InnerResourceSourceUrl
- ✅ 执行步骤3: 更新 MiniGameConfig.asset 的 CDN
- ✅ 执行步骤4: 使用 YooAsset 构建 AssetBundle（调用 `ReleaseTools.BuildInternal`）
- ✅ 执行步骤5: 导出微信小游戏工程
- ✅ 执行步骤6: 复制到 Backup 目录
- ✅ 执行步骤7: 上传 StreamingAssets 和 bin.txt 到 UOSCDN
- ✅ 执行步骤8: 创建 release

### 情况2: 只修改了代码（只需要上传新的 bin.txt）
- ❌ 跳过步骤1-3（不修改版本号）
- ❌ 跳过步骤4（不构建 AssetBundle）
- ✅ 执行步骤5: 导出微信小游戏工程（只导出代码，不包含资源）
- ✅ 执行步骤6: 只复制 bin.txt 到 Backup 目录（不复制 StreamingAssets）
- ✅ 执行步骤7: 只上传 bin.txt 到 UOSCDN（不上传 StreamingAssets）
- ❌ 跳过步骤8（不创建新 release，或使用现有 release）

## 需要进一步确认的事项

1. **微信小游戏导出API**: 确认微信小游戏SDK是否提供程序化导出方法
2. **UOSCDN API**: 查找 UOSCDN 的上传和 release 管理 API
3. **bin.txt 文件位置**: 确认导出工程中 bin.txt 文件的确切位置和命名规则
4. **版本号格式**: 确认版本号格式（当前使用 `v0.1.7.2` 格式）

## 下一步行动

1. 先实现步骤1-3（修改配置文件）
2. 实现步骤5（文件复制到Backup）
3. 查找并实现步骤4（微信小游戏导出）
4. 查找并实现步骤6-7（UOSCDN上传和release管理）
5. 根据"是否修改资源"选项实现条件分支逻辑


