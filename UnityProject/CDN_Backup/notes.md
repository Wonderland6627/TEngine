# CDN 备份说明（当前流程）

## 目录结构

当前采用 App 版本目录管理（扁平结构）：

```plaintext
CDN_Backup
└── MiniGame
    ├── v1.1
    │   ├── PackageManifest_DefaultPackage.version
    │   ├── PackageManifest_DefaultPackage_v0.x.x.x.hash
    │   ├── PackageManifest_DefaultPackage_v0.x.x.x.bytes
    │   ├── *.bundle
    │   └── *.bin.txt
    └── ...
```

## 版本含义

- `App版本号`：决定目录层级（`MiniGame/{App版本}/`）。
- `资源版本号`：写入 `PackageManifest_DefaultPackage.version` 内容，并用于 hash/bytes 文件名。

## 标准流程

1. 资源有改动时，先更新 `YooAssetSettings.BuildVersion`（资源版本）。
2. 更新 `InnerResourceSourceUrl` 和 `MiniGameConfig.CDN` 到 `.../MiniGame/{App版本}/`。
3. 执行 YooAsset 构建，生成 `Bundles/WebGL/DefaultPackage/{资源版本}/`。
4. 导出微信小游戏工程，生成 `WXExport/webgl/*.bin.txt`。
5. 复制发布文件到 `CDN_Backup/MiniGame/{App版本}/`：
   - 资源变更：复制 bundle + manifest + bin.txt。
   - 仅代码变更：只复制 bin.txt（建议 App 版本保持不变）。
6. 上传备份目录内容到 UOSCDN 对应路径：`content/MiniGame/{App版本}/`。
7. 在 Unity 与微信开发者工具验证版本更新与资源加载。

## 关键说明

- 不再按 `StreamingAssets/...` 子目录上传，运行时请求已对齐 CDN 根目录扁平结构。
- 若“仅代码变更”但切换了 App 版本目录，新目录需提前具备旧资源文件，否则会缺失清单或 bundle。

UOSCDN: https://uos.unity.cn/services/bd2fcdf8-2152-4e5f-b1be-3f6b950c8034/asset/bucket/cde09f24-d39c-4845-a3e3-17344f4f2894
微信小游戏控制台: https://mp.weixin.qq.com/wxamp/home/guide?lang=zh_CN&token=28368697
CloudBase: https://tcb.cloud.tencent.com/dev?envId=slimecloudservice-6enxmrfbc5bddc#/db/doc/model/UserGameInfos?sourceType=internal_flexdb