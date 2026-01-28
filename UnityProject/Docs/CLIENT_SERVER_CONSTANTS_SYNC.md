# 客户端与服务端常量同步规则

## 📌 重要规则

**所有与服务端共享的常量定义必须保持一致！**

## 服务端常量定义位置

服务端所有常量定义在：`piratecat_slime_express/config/constants.js`

## 客户端常量定义位置

客户端所有与服务端共享的常量应统一在：`Assets/GameScripts/HotFix/GameLogic/Network/Constants.cs`

## 同步规则

1. **服务端是唯一数据源**：所有常量值以服务端 `constants.js` 为准
2. **客户端必须同步**：修改服务端常量后，必须同步更新客户端 `Constants.cs`
3. **命名规范**：
   - 服务端使用 `UPPER_SNAKE_CASE`（如 `CURRENCY_TYPES`）
   - 客户端使用 `PascalCase` 类名 + `UPPER_SNAKE_CASE` 常量名（如 `CurrencyTypes.COIN`）
4. **注释要求**：客户端常量必须添加注释说明对应服务端的字段路径

## 当前需要同步的常量

- `RESPONSE_CODE` → `ResponseCode`
- `CURRENCY_TYPES` → `CurrencyTypes`
- `CURRENCY_SOURCE` → `CurrencySource`

## 注意事项

- 客户端可以有自己的本地错误码（如 `CLIENT_NO_NETWORK`），但服务端响应码必须一致
- 修改常量时，必须同时检查服务端和客户端代码，确保同步更新

