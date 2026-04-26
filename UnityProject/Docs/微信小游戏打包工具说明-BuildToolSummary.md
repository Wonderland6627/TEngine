# 微信小游戏打包工具说明

## 1. 工具能力概览

### 编辑器窗口

- 文件：`Assets/Editor/PirateCatGameVersionWindow.cs`
- 菜单：`PirateCat/打包工具`
- 功能：
  - 维护 `App版本号` 与 `资源版本号`
  - 标记“是否修改了资源”
  - 打开 YooAsset 构建窗口
  - 调用微信小游戏转换工具
  - 将发布文件复制到 `CDN_Backup`

### 核心脚本

- 文件：`Assets/Editor/PirateCatEditorTools.cs`
- 主要行为：
  - 更新 `YooAssetSettings.BuildVersion`
  - 更新 `PlayerSettings.bundleVersion`
  - 同步更新 `InnerResourceSourceUrl` 和 `MiniGameConfig.CDN`
  - 复制 `Bundles/WebGL/DefaultPackage/{资源版本}/` 与 `WXExport/webgl/*.bin.txt` 到备份目录

## 2. 实际执行流程

1. **步骤1：确认并更新版本号（可选）**
   - 更新资源版本：`YooAssetSettings.asset`
   - 更新 App 版本：`ProjectSettings.bundleVersion`
   - 更新 CDN 根地址：`TEngineGlobalSettings.asset` + `MiniGameConfig.asset`
2. **步骤2：构建 AssetBundle（手动）**
   - 工具只打开 Builder 窗口，构建需要在窗口里手动点击执行。
3. **步骤3：导出微信小游戏（手动）**
   - 调用“微信小游戏/转换小游戏”菜单，导出结果在 `WXExport/`。
4. **步骤4：复制发布文件到备份目录**
   - 资源变更：复制 `Bundles/WebGL/DefaultPackage/{资源版本}/` 的 bundle + manifest 相关文件，以及 `WXExport/webgl/*.bin.txt`。
   - 仅代码变更：只复制 `WXExport/webgl/*.bin.txt`。
   - 目标目录：`CDN_Backup/MiniGame/{App版本}/`。

## 3. 路径与版本规则

- `App版本号` 决定 CDN 目录层级：`.../MiniGame/{App版本}/`
- `资源版本号` 决定清单版本：
  - `PackageManifest_DefaultPackage.version`（内容为资源版本）
  - `PackageManifest_DefaultPackage_{资源版本}.hash`
  - `PackageManifest_DefaultPackage_{资源版本}.bytes`
- 运行时请求路径基于 CDN 根目录扁平结构，不再依赖上传 `StreamingAssets` 子目录。

## 4. 常见场景建议

### 修改了资源

- 需要：更新资源版本、构建 Bundle、复制 Bundle+Manifest+bin.txt、上传 CDN。

### 只修改了代码

- 建议：保持 `App版本号` 不变，仅替换 `bin.txt`。
- 若改了 `App版本号`，请确保新目录下已有对应资源文件，否则会出现清单或 Bundle 缺失。

## 5. 当前限制

- 工具不包含 UOSCDN 自动上传与 release 管理。
- 微信转换步骤仍依赖手动确认。

