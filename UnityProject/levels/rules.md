# 关卡配置规则文档

本文档定义了关卡配置的规则和约束，用于指导AI生成和优化关卡配置。

## 一、数据结构说明

### 1.1 关卡根结构 (LevelConfig)
```json
{
    "levelId": int,           // 关卡ID，从1开始递增
    "castles": [...],         // 城堡列表
    "roads": [...],           // 道路列表
    "config": {...}           // 游戏参数配置
}
```

### 1.2 城堡结构 (Castle)
```json
{
    "id": int,                    // 城堡ID，从0开始，必须连续且唯一
    "castleType": int,             // 城堡类型：0=Nest(巢穴，可生产单位), 1=Tower(塔楼，不可生产) (目前只有Nest类型)
    "occupiedOnStart": bool,      // 是否在游戏开始时被占领
    "occupiedSlimeType": int,      // 占领单位类型：0=Player(玩家), 1=Enemy_1(敌人)
    "occupiedUnitCount": int,      // 初始占领单位数量
    "position": {
        "x": int,                 // X坐标（像素坐标）
        "y": int                  // Y坐标（像素坐标）
    }
}
```

### 1.3 道路结构 (Road)
```json
{
    "startCastleId": int,         // 起始城堡ID（必须存在于castles中）
    "endCastleId": int            // 目标城堡ID（必须存在于castles中）
}
```

### 1.4 游戏配置 (Config)
```json
{
    "playerSpawnInterval": float,      // 玩家单位生成间隔（秒）
    "playerAttackInterval": float,     // 玩家攻击间隔（秒）
    "enemy_1_SpawnInterval": float,   // 敌人1单位生成间隔（秒）
    "enemy_1_AttackInterval": float   // 敌人1攻击间隔（秒）
}
```

## 二、配置规则与约束

### 2.1 城堡规则

#### 2.1.1 城堡ID规则
- **必须从0开始，连续递增**，不能有间隔
- 例如：如果有5个城堡，ID必须是 0, 1, 2, 3, 4
- ID必须唯一，不能重复

#### 2.1.2 城堡类型规则
- `castleType = 0` (Nest/巢穴)：可以生产单位，通常用于玩家和敌人的起始基地
- `castleType = 1` (Tower/塔楼)：不能生产单位，通常作为战略要地或中间节点 (暂无此类型功能)
- 建议：至少有一个玩家城堡和一个敌人城堡使用Nest类型

#### 2.1.3 占领状态规则
- `occupiedOnStart = true`：游戏开始时已被占领
  - 必须设置 `occupiedSlimeType`（0或1）
  - 必须设置 `occupiedUnitCount > 0`
- `occupiedOnStart = false`：游戏开始时为空城堡
  - `occupiedSlimeType` 通常为0（但会被忽略）
  - `occupiedUnitCount` 通常为0（但会被忽略）

#### 2.1.4 单位类型规则
- `occupiedSlimeType = 0`：玩家单位（Player）
- `occupiedSlimeType = 1`：敌人单位（Enemy_1）
- 每个关卡**至少需要1个玩家城堡和1个敌人城堡**（occupiedOnStart=true）

#### 2.1.5 初始单位数量规则
- 玩家城堡：建议范围 10-30，根据关卡难度递增
- 敌人城堡：建议范围 5-35，根据关卡难度递增
- 空城堡：`occupiedUnitCount = 0`（当occupiedOnStart=false时）

#### 2.1.6 位置规则
- 使用像素坐标系统
- 城堡之间应保持**合理距离**（建议至少100像素），避免重叠
- 坐标可以是负数
- 坐标范围在x:(-400,400) y:(-600,600)

### 2.2 道路规则

#### 2.2.1 道路连接规则
- `startCastleId` 和 `endCastleId` **必须存在于castles数组中**
- 两个城堡ID不能相同（不能自己连接自己）
- 道路是双向，从startCastleId指向endCastleId，不要再重复指回来

#### 2.2.2 道路网络规则
- **所有城堡必须通过道路连接**，不能有孤立城堡
- 建议形成**连通图**，确保玩家可以从起始城堡到达所有可到达的城堡
- 道路应形成**合理的路径网络**，避免过于复杂或过于简单
- 建议：玩家城堡到敌人城堡之间应有**至少一条路径**
- 玩法特点：取胜的关键在于形成多打少，多打一

#### 2.2.3 道路数量建议
- 简单关卡（3-5个城堡）：2-5条道路
- 中等关卡（6-10个城堡）：5-12条道路
- 复杂关卡（11+个城堡）：12+条道路
- 道路数量应保证网络连通性，但避免过度连接导致混乱

### 2.3 游戏配置规则

#### 2.3.1 生成间隔 (SpawnInterval)
- 单位：秒
- 建议范围：0.5 - 2.0
- 数值越小，生成速度越快，游戏节奏越快
- 通常玩家和敌人的生成间隔可以相同，也可以不同以调整难度
- 目前设为固定 1

#### 2.3.2 攻击间隔 (AttackInterval)
- 单位：秒
- 建议范围：0.1 - 0.5
- 数值越小，攻击频率越高
- 通常设置为0.275左右
- 目前设为固定 0.275

#### 2.3.3 难度平衡
- 简单关卡：玩家优势明显（玩家单位数多）
- 中等关卡：双方相对平衡（玩家单位仍然多于敌人）
- 困难关卡：敌人优势明显（敌人单位数多）

## 三、关卡设计模式

- 由于生成速度和间隔一样，所以胜利的条件是形成以多打少
### 3.1 简单关卡模式（关卡1-3）
- 城堡数量：3-5个
- 道路结构：线性或简单分支
- 布局：玩家在底部，敌人在顶部，中间1-2个空城堡
- 示例：玩家城堡 → 空城堡 → 敌人城堡

### 3.2 中等关卡模式（关卡4-7）
- 城堡数量：5-8个
- 道路结构：树状或简单网络
- 布局：多个分支路径，玩家可选择不同路线
- 特点：有多个空城堡作为战略要点

### 3.3 复杂关卡模式（关卡8+）
- 城堡数量：8-16个或更多
- 道路结构：复杂网络，多条路径
- 布局：多层次的战略纵深
- 特点：多个敌人城堡，需要多线作战

## 四、验证检查清单

在生成或修改关卡配置后，请检查以下项目：

- [ ] 所有城堡ID从0开始连续递增，无间隔
- [ ] 每个城堡的position坐标唯一，不重叠
- [ ] 至少有一个玩家城堡（occupiedOnStart=true, occupiedSlimeType=0）
- [ ] 至少有一个敌人城堡（occupiedOnStart=true, occupiedSlimeType=1）
- [ ] 所有道路的startCastleId和endCastleId都存在于castles中
- [ ] 所有城堡都通过道路连接（无孤立城堡）
- [ ] 玩家城堡到敌人城堡至少有一条路径
- [ ] occupiedOnStart=true的城堡，occupiedUnitCount > 0
- [ ] 游戏配置参数在合理范围内
- [ ] JSON格式正确，可以正常解析

## 五、优化建议

### 5.1 难度曲线
- 随着关卡ID增加，逐步增加：
  - 城堡数量
  - 道路复杂度
  - 敌人初始单位数
  - 敌人生成速度（可选）

### 5.2 布局优化
- 避免城堡过于集中或过于分散
- 保持视觉上的平衡和美感
- 确保道路连接在视觉上清晰可辨

### 5.3 游戏性优化
- 提供多条路径选择，增加策略性
- 设置关键节点（空城堡），增加战术深度
- 平衡双方实力，确保游戏有挑战性但不过于困难

## 六、示例参考

### 示例1：简单线性关卡
```json
{
    "levelId": 1,
    "castles": [
        {"id": 0, "castleType": 0, "occupiedOnStart": true, "occupiedSlimeType": 0, "occupiedUnitCount": 15, "position": {"x": -170, "y": -340}},
        {"id": 1, "castleType": 0, "occupiedOnStart": false, "occupiedSlimeType": 0, "occupiedUnitCount": 0, "position": {"x": 0, "y": 0}},
        {"id": 2, "castleType": 0, "occupiedOnStart": true, "occupiedSlimeType": 1, "occupiedUnitCount": 5, "position": {"x": 170, "y": 340}}
    ],
    "roads": [
        {"startCastleId": 0, "endCastleId": 1},
        {"startCastleId": 1, "endCastleId": 2}
    ],
    "config": {
        "playerSpawnInterval": 1,
        "playerAttackInterval": 0.275,
        "enemy_1_SpawnInterval": 1,
        "enemy_1_AttackInterval": 0.275
    }
}
```

### 示例2：分支网络关卡
- 多个空城堡形成分支
- 玩家可以选择不同路径进攻
- 敌人城堡在多个分支的终点

## 七、注意事项

1. **JSON格式**：确保所有JSON格式正确，注意逗号、引号等
2. **数据类型**：严格按照定义使用int、float、bool类型
3. **向后兼容**：修改现有关卡时，保持核心结构不变
4. **测试验证**：生成配置后应在游戏中测试，确保可正常加载和运行
5. **关卡1特殊处理**：关卡1是教程关卡，应保持简单，且有教程提示性
6. **修改要求**：只修改level_N.json，不要修改levels.json

---

**最后更新**：基于当前代码结构和实际关卡数据生成
**用途**：作为AI生成和优化关卡配置的参考指南

