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
    "occupiedSlimeType": int,      // 占领单位类型：0=Player, 1=Enemy_1, 2=Enemy_2, 3=Enemy_3, 4=Enemy_4
    "occupiedUnitCount": int,      // 初始占领单位数量
    "emptyCastleOccupyRequirement": int, // 空城堡需要多少个单位数量才能被占领，默认为-10
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
    "playerSpawnInterval": float,      // 玩家单位生成间隔（秒）- 旧字段，向后兼容
    "playerAttackInterval": float,     // 玩家攻击间隔（秒）- 旧字段，向后兼容
    "enemy_1_SpawnInterval": float,   // 敌人1单位生成间隔（秒）- 旧字段，向后兼容
    "enemy_1_AttackInterval": float,  // 敌人1攻击间隔（秒）- 旧字段，向后兼容
    "factions": {                      // 按阵营配置（新字段，优先使用）
        "0": { "spawnInterval": float, "attackInterval": float },
        "1": { "spawnInterval": float, "attackInterval": float },
        "2": { "spawnInterval": float, "attackInterval": float },
        "3": { "spawnInterval": float, "attackInterval": float },
        "4": { "spawnInterval": float, "attackInterval": float }
    }
}
```

**配置读取优先级**：若 `factions` 字典中存在对应阵营的配置，则优先使用；否则 fallback 到旧字段。

## 二、阵营系统说明

### 2.1 阵营类型
| 类型值 | 枚举名 | 颜色 | 说明 |
|--------|--------|------|------|
| 0 | Player | 蓝色 | 玩家阵营，可拖拽操控 |
| 1 | Enemy_1 | 红色 | AI敌人1 |
| 2 | Enemy_2 | 绿色 | AI敌人2 |
| 3 | Enemy_3 | 紫色 | AI敌人3 |
| 4 | Enemy_4 | 橙色 | AI敌人4 |

### 2.2 阵营对抗规则
- 所有阵营两两互相对立，不同阵营的单位在路上相遇会互相抵消
- 非己方单位进入城堡会减少城堡单位数，己方单位会增加
- 每个AI阵营独立决策，会攻击所有非己方阵营（包括其他AI）
- 玩家失去所有城堡立即判负，最后只剩1个阵营时游戏结束

## 三、配置规则与约束

### 3.1 城堡规则

#### 3.1.1 城堡ID规则
- **必须从0开始，连续递增**，不能有间隔
- ID必须唯一，不能重复

#### 3.1.2 占领状态规则
- `occupiedOnStart = true`：游戏开始时已被占领
  - 必须设置 `occupiedSlimeType`（0-4）
  - 必须设置 `occupiedUnitCount > 0`
- `occupiedOnStart = false`：游戏开始时为空城堡
  - `occupiedSlimeType` 通常为0（但会被忽略）
  - `occupiedUnitCount` 通常为0

#### 3.1.3 单位类型规则
- `occupiedSlimeType = 0`：玩家单位（Player）
- `occupiedSlimeType = 1`：敌人1（Enemy_1，红色）
- `occupiedSlimeType = 2`：敌人2（Enemy_2，绿色）
- `occupiedSlimeType = 3`：敌人3（Enemy_3，紫色）
- `occupiedSlimeType = 4`：敌人4（Enemy_4，橙色）
- 每个关卡**至少需要1个玩家城堡和1个敌人城堡**（occupiedOnStart=true）

#### 3.1.4 初始单位数量规则
- 玩家城堡：建议范围 10-30，根据关卡难度递增
- 敌人城堡：建议范围 5-35，根据关卡难度递增
- 空城堡：`occupiedUnitCount = 0`

#### 3.1.5 位置规则
- 使用像素坐标系统
- 城堡之间应保持**合理距离**（建议至少100像素），避免重叠
- 坐标范围在x:(-400,400) y:(-600,600)

### 3.2 道路规则

#### 3.2.1 道路连接规则
- `startCastleId` 和 `endCastleId` **必须存在于castles数组中**
- 两个城堡ID不能相同
- 道路是双向的，不要重复定义反向道路

#### 3.2.2 道路网络规则
- **所有城堡必须通过道路连接**，不能有孤立城堡
- 必须形成**连通图**
- 玩家城堡到所有敌人城堡之间应有路径可达
- 玩法特点：取胜的关键在于形成多打少

### 3.3 游戏配置规则

#### 3.3.1 多阵营配置
- 关卡1-2：使用旧字段即可（只有Player和Enemy_1）
- 关卡3+：建议使用 `factions` 字典配置所有参与的阵营
- `factions` 中的 key 为阵营类型的 int 值（"0"、"1"、"2"、"3"、"4"）

#### 3.3.2 生成间隔 (SpawnInterval)
- 单位：秒，建议范围：0.5 - 2.0
- 目前设为固定 1

#### 3.3.3 攻击间隔 (AttackInterval)
- 单位：秒，建议范围：0.1 - 0.5
- 目前设为固定 0.275

## 四、关卡设计模式

### 4.1 简单关卡（关卡1-3）
- 城堡数量：3-6个
- 阵营数：2（Player vs Enemy_1）
- 道路结构：线性或简单分支

### 4.2 三方对战关卡（关卡4-6）
- 城堡数量：7-11个
- 阵营数：3（Player + Enemy_1 + Enemy_2）
- 特点：引入"鹬蚌相争"玩法，AI阵营间也会互攻

### 4.3 四方混战关卡（关卡7-8）
- 城堡数量：10-12个
- 阵营数：4（Player + Enemy_1 + Enemy_2 + Enemy_3）
- 特点：复杂路网，多线作战

### 4.4 大逃杀关卡（关卡9-10）
- 城堡数量：13-14个
- 阵营数：4-5（最多5阵营全部参战）
- 特点：终极挑战，需要利用AI互攻的策略

## 五、胜负判定规则

- 玩家失去所有已占领城堡 -> 立即判负
- 只剩1个阵营拥有城堡（且无空城堡） -> 该阵营胜利
- 若最后赢家是玩家 -> 显示胜利界面
- 若最后赢家不是玩家 -> 显示失败界面

## 六、验证检查清单

在生成或修改关卡配置后，请检查以下项目：

- [ ] 所有城堡ID从0开始连续递增，无间隔
- [ ] 每个城堡的position坐标唯一，不重叠
- [ ] 至少有一个玩家城堡（occupiedOnStart=true, occupiedSlimeType=0）
- [ ] 至少有一个敌人城堡（occupiedOnStart=true, occupiedSlimeType=1/2/3/4）
- [ ] 所有道路的startCastleId和endCastleId都存在于castles中
- [ ] 所有城堡都通过道路连接（无孤立城堡）
- [ ] 玩家城堡到敌人城堡至少有一条路径
- [ ] occupiedOnStart=true的城堡，occupiedUnitCount > 0
- [ ] 游戏配置参数在合理范围内
- [ ] 多阵营关卡中，factions字典包含所有参与阵营的配置
- [ ] JSON格式正确，可以正常解析

## 七、注意事项

1. **JSON格式**：确保所有JSON格式正确，注意逗号、引号等
2. **数据类型**：严格按照定义使用int、float、bool类型
3. **向后兼容**：关卡1-2保持旧格式，通过 fallback 机制兼容
4. **测试验证**：生成配置后应在游戏中测试
5. **关卡1特殊处理**：关卡1是教程关卡，应保持简单
6. **修改要求**：只修改level_N.json，不要修改levels.json

---

**最后更新**：多阵营大逃杀系统改造 - 支持2-5个互相对立阵营
**用途**：作为AI生成和优化关卡配置的参考指南
