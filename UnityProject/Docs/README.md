# 客户端文档索引

## 游戏设计文档

- **[故事背景与玩法介绍.md](./故事背景与玩法介绍.md)** - 世界观、种族设定、核心玩法
- **[史莱姆图鉴系统策划案.md](./史莱姆图鉴系统策划案.md)** - 图鉴系统完整策划方案

## 核心文档

- **[LOGIN_FLOW.md](./LOGIN_FLOW.md)** - 登录流程完整说明
- **[NetManager_Design.md](./NetManager_Design.md)** - 网络管理器设计

## 系统设计文档

- **[数据集中管理设计.md](./数据集中管理设计.md)** - 数据管理架构
- **[体力值系统设计.md](./体力值系统设计.md)** - 体力系统设计
- **[金币与体力系统需求设计.md](./金币与体力系统需求设计.md)** - 货币体力需求
- **[DailyCheckInDesign.md](./DailyCheckInDesign.md)** - 每日签到设计
- **[锦囊系统梳理文档.md](./锦囊系统梳理文档.md)** - 锦囊系统

## 技术文档

- **[CLIENT_SERVER_CONSTANTS_SYNC.md](./CLIENT_SERVER_CONSTANTS_SYNC.md)** - 客户端服务端常量同步
- **[服务器时间与时区处理.md](./服务器时间与时区处理.md)** - 时间时区处理
- **[序列化库日期处理分析.md](./序列化库日期处理分析.md)** - 日期序列化分析
- **[微信云数据库日期格式分析.md](./微信云数据库日期格式分析.md)** - 云数据库日期格式

## 数据库文档

- **[cloud_database_schema.md](./cloud_database_schema.md)** - 数据库Schema设计
- **[cloud_function_compliance_check.md](./cloud_function_compliance_check.md)** - 云函数合规检查

## 构建文档

- **[build_tool_summary.md](./build_tool_summary.md)** - 构建工具总结
- **[build_workflow_analysis.md](./build_workflow_analysis.md)** - 构建流程分析

## 文档说明

### 必读文档
1. **故事背景与玩法介绍.md** - 了解游戏世界观与核心玩法
2. **LOGIN_FLOW.md** - 了解登录流程和Token管理
3. **NetManager_Design.md** - 了解网络请求机制

### 系统开发参考
- 新功能开发前先查阅故事背景与玩法介绍，确保设计符合世界观
- 开发新功能前查看对应的系统设计文档
- 数据库操作参考cloud_database_schema.md

### 技术细节
- 时间处理相关问题参考时区处理文档
- 常量同步参考CLIENT_SERVER_CONSTANTS_SYNC.md

## 更新记录

### 2024-02 文档重构
- ✅ 删除过时的cloud_function_api_design.md（已改用HTTP API）
- ✅ 更新LOGIN_FLOW.md，删除云函数相关内容
- ✅ 精简NetManager_Design.md，聚焦核心功能
- ✅ 所有文档已更新以反映最新实现

### 文档精简说明
- 删除了云函数相关的过时内容
- 聚焦HTTP API和Token认证机制
- 保留系统设计文档供开发参考
- 技术细节文档保持不变
