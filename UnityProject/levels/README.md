# levels 目录说明

本目录是关卡配置的**编辑用暂存区**，不参与游戏打包和运行，游戏实际读取的是：

```
Assets/AssetRaw/Configs/jsons/levels.json
```

## 文件

- `level_1.json` ~ `level_N.json`：单关配置，一关一个文件，方便逐关编辑（尤其是让 AI 改关卡时）
- `levels.json`：由 `level_X.json` 合并而成，仅供参考，不是游戏读取的那份
- `split_levels.py` / `.bat`：把 `levels.json` 拆成单关文件
- `merge_levels.py` / `.bat`：把单关文件合并成 `levels.json`
- `rules.md`：关卡设计规则，供人和 AI 生成/修改关卡时参考

## 使用方式

1. 改关卡请直接编辑对应的 `level_X.json`，不要改 `levels.json`
2. 改完后运行 `merge_levels.bat` 生成新的 `levels.json`
3. 把生成的 `levels.json` 手动复制到 `Assets/AssetRaw/Configs/jsons/levels.json`（游戏真正读取的位置），再走正常的关卡发布流程
