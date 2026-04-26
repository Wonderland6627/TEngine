# 微信小游戏打包流程分析（已对齐当前代码）

## 1. YooAsset 运行时真实流程（WebPlayMode）

1. `ResourceModule` 启动时，将远端根地址设置为 `SettingsUtils.GetResDownLoadPath()`（来自 `TEngineGlobalSettings.asset` 的资源URL）。
2. 进入更新流程后，先请求远端版本文件：`PackageManifest_DefaultPackage.version`。
3. 再请求远端清单哈希：`PackageManifest_DefaultPackage_{资源版本}.hash`。
4. 然后请求远端二进制清单：`PackageManifest_DefaultPackage_{资源版本}.bytes`。
5. 根据清单创建下载器，按需下载 bundle。

> 说明：微信小游戏环境下，初始化阶段已跳过 `StreamingAssets/package/...` 的内置版本探测，避免非致命 404 噪音日志。

## 2. 版本与路径映射（当前工程）

- `App版本号`：`ProjectSettings/ProjectSettings.asset` 的 `bundleVersion`（当前为 `v1.1`）。
- `资源版本号`：`Assets/TEngine/AssetSetting/Resources/YooAssetSettings.asset` 的 `BuildVersion`（当前为 `v0.1.7.8`）。
- `远端根地址`：`Assets/TEngine/ResRaw/Resources/TEngineGlobalSettings.asset` 的 `m_InnerResourceSourceUrl`（当前指向 `.../MiniGame/v1.1/`）。
- `小游戏导出CDN地址`：`Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset` 的 `ProjectConf.CDN`（当前与 `m_InnerResourceSourceUrl` 一致）。
- `资源包名`：`DefaultPackage`（来自 `ResourceModule` 与 YooAsset 配置）。

## 3. 打包工具当前真实流程（PirateCat）

1. **更新版本号（可选）**
   - 更新 `YooAssetSettings.BuildVersion`（资源版本）。
   - 更新 `PlayerSettings.bundleVersion`（App版本）。
   - 同步更新 `InnerResourceSourceUrl` 与 `MiniGameConfig.CDN` 到 `.../MiniGame/{App版本}/`。
2. **构建资源（手动）**
   - 工具仅打开 `AssetBundle Builder` 窗口，构建动作由开发者在窗口中确认执行。
3. **导出微信小游戏（手动）**
   - 工具调用“微信小游戏/转换小游戏”菜单。
4. **复制到备份目录**
   - 当“修改了资源”时：从 `Bundles/WebGL/DefaultPackage/{资源版本}/` 复制 bundle/manifest 等文件。
   - 始终从 `WXExport/webgl/` 复制 `*.bin.txt`。
   - 目标目录：`CDN_Backup/MiniGame/{App版本}/`（与 CDN 扁平目录对齐）。

## 4. 已确认的错误与修正

- 旧文档中“上传 `StreamingAssets` 目录”的描述已过时。
- 当前正确做法是上传 `CDN_Backup/MiniGame/{App版本}/` 下的扁平文件集合，而不是 `StreamingAssets/...` 子目录。
- 若仅改代码（不改资源），建议保持 `App版本号` 不变，仅替换 `bin.txt`；若改了 `App版本号`，需确保新目录下已有对应资源文件，否则会出现远端资源缺失。

## 5. 发布前核对清单

- `YooAssetSettings.BuildVersion` 与本次资源构建版本一致。
- `InnerResourceSourceUrl` 与 `MiniGameConfig.CDN` 一致，且都指向本次 `App版本` 目录。
- CDN 目录至少存在：
  - `PackageManifest_DefaultPackage.version`
  - `PackageManifest_DefaultPackage_{资源版本}.hash`
  - `PackageManifest_DefaultPackage_{资源版本}.bytes`
  - 本次清单引用到的 bundle 文件
  - 对应 `*.bin.txt`
