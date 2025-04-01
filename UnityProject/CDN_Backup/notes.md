# CDN 备份说明

## 备份目录结构
CDN 备份目录按照平台和版本进行管理，具体结构如下：
```plaintext
CDN_Backup
└── MiniGame
    ├── v0.1
    ├── v0.2
    └── ...
```

### CDN结构1
1. YooAsset Builder打Bundle时，按顺序修改版本号（v0.1、v0.2等）
2. 修改InnerResourceSourceUrl的版本号：
   https://a.unity.cn/client_api/v1/buckets/cde09f24-d39c-4845-a3e3-17344f4f2894/release_by_badge/【版本号】/content/
3. 将InnerResourceSourceUrl复制到小游戏导出配置的CDN地址
4. 导出微信小游戏工程
5. 将导出工程中的StreamingAssets文件夹和bin.txt文件根据版本好复制到Backup目录下的对应版本号文件夹中
6. 将导出工程中的StreamingAssets文件夹和bin.txt文件上传到UOSCDN
7. 在UOSCDN中创建一个新的release，并将assign设置为修改后的版本号
8. 在Unity和微信小游戏开发者工具中测试

### CDN结构2 （新）
2. 修改InnerResourceSourceUrl的版本号：
   https://a.unity.cn/client_api/v1/buckets/cde09f24-d39c-4845-a3e3-17344f4f2894/content/MiniGame/【版本号】/
6. 将对应的文件夹上传到UOSCDN

UOSCDN: https://uos.unity.cn/services/bd2fcdf8-2152-4e5f-b1be-3f6b950c8034/asset/bucket/cde09f24-d39c-4845-a3e3-17344f4f2894

