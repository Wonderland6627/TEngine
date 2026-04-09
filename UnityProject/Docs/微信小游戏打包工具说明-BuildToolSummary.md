# 微信小游戏打包工具总结

## 已完成功能

### 1. 编辑器窗口 (`PirateCatGameVersionWindow.cs`)
- ✅ 版本号输入框
- ✅ 资源修改选项（复选框）
- ✅ 打包按钮
- ✅ 当前配置信息显示
- ✅ 菜单项：`PirateCat/打包工具`

### 2. 打包工具核心逻辑 (`PirateCatEditorTools.cs`)

#### 已实现的功能：
- ✅ **步骤1**: 修改 YooAssetSettings.asset 的 BuildVersion
- ✅ **步骤2**: 修改 InnerResourceSourceUrl（通过 SerializedObject 或 YAML）
- ✅ **步骤3**: 更新 MiniGameConfig.asset 的 CDN 地址
- ✅ **步骤4**: 构建 AssetBundle（使用 YooAsset API）
- ✅ **步骤5**: 导出微信小游戏工程（使用微信转换工具面板）
- ✅ **步骤6**: 复制文件到 Backup 目录
  - 从 `WXExport/webgl/` 文件夹下查找文件
  - 根据"是否修改资源"选项决定是否复制 StreamingAssets
  - 始终复制 bin.txt 文件

#### 已移除的功能（待后续实现）：
- ❌ **步骤7**: 上传到 UOSCDN（暂时不实现，等流程跑通后再设计）
- ❌ **步骤8**: 创建 UOSCDN release（已忽略，参考 notes.md 第7步）

## 使用方式

1. 打开 Unity 编辑器
2. 菜单栏选择 `PirateCat/打包工具`
3. 在窗口中：
   - 输入版本号（如 `v0.1.7.3`）
   - 选择是否修改了资源
   - 点击"开始打包"按钮

## 打包流程说明

### 情况1: 修改了资源（hasResourceChanged = true）
1. 修改 YooAssetSettings.asset 的 BuildVersion
2. 修改 InnerResourceSourceUrl 的版本号
3. 更新 MiniGameConfig.asset 的 CDN 地址
4. 构建 AssetBundle
5. 导出微信小游戏工程（使用微信转换工具面板）
6. 从 `WXExport/webgl/` 复制 StreamingAssets 和 bin.txt 到 Backup 目录

### 情况2: 只修改了代码（hasResourceChanged = false）
1. 跳过版本号修改
2. 跳过 AssetBundle 构建
3. 导出微信小游戏工程（使用微信转换工具面板）
4. 从 `WXExport/webgl/` 只复制 bin.txt 到 Backup 目录（不复制 StreamingAssets）

## 配置文件路径

- YooAssetSettings: `Assets/TEngine/AssetSetting/Resources/YooAssetSettings.asset`
- TEngineGlobalSettings: `Assets/TEngine/ResRaw/Resources/TEngineGlobalSettings.asset`
- MiniGameConfig: `Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset`

## 备份目录结构

```
CDN_Backup/
└── MiniGame/
    ├── v0.1.7.2/
    │   ├── StreamingAssets/
    │   └── *.bin.txt
    └── ...
```

## 注意事项

1. **版本号格式**: 必须以 `v` 开头（如 `v0.1.7.3`）
2. **导出路径**: 从 MiniGameConfig.asset 的 DST 字段读取
3. **文件位置**: 导出后的文件在 `WXExport/webgl/` 文件夹下（不是直接在 `WXExport/` 下）
4. **微信转换工具**: 如果无法自动调用，会提示用户手动操作
5. **YAML 文件修改**: 使用正则表达式匹配和替换，需要确保格式正确
6. **UOSCDN 上传**: 暂时不实现，等流程跑通后再设计

## 导出流程说明

1. 工具会尝试通过反射自动调用微信转换工具面板
2. 如果反射失败，会尝试通过菜单项调用
3. 如果都失败，会弹出对话框提示用户手动操作
4. 用户手动导出完成后，点击"确定"继续后续的备份流程

## 文件结构

导出后的目录结构：
```
WXExport/
├── webgl/          # WebGL 构建文件（包含 StreamingAssets 和 bin.txt）
│   ├── StreamingAssets/
│   └── *.bin.txt
└── minigame/       # 微信小游戏文件
```

备份目录结构：
```
CDN_Backup/
└── MiniGame/
    ├── v0.1.7.2/
    │   ├── StreamingAssets/
    │   └── *.bin.txt
    └── ...
```

## 下一步工作

1. 测试完整打包流程
2. 优化微信转换工具的自动调用（如果SDK提供API）
3. 等流程跑通后，再设计 UOSCDN 上传功能
4. 添加错误处理和日志记录
5. 添加进度显示

## 已知问题

1. 微信转换工具可能无法自动调用，需要用户手动操作
2. YAML 文件修改可能不够稳定，建议后续改为使用 ScriptableObject 或序列化方式
3. UOSCDN 上传功能暂时不实现

