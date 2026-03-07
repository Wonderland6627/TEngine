using System;
using System.Collections.Generic;
using TEngine;
using Cysharp.Threading.Tasks;

namespace GameLogic.Network
{
    /// <summary>
    /// Mock数据生成器
    /// 用于在编辑器环境下生成模拟的云函数数据，方便验证逻辑
    /// </summary>
    public partial class MockData
    {
        public static readonly int MAX_MOCK_DATA_COUNT = 15;

        /// <summary>
        /// 预定义的模拟OpenID列表（最多15个）
        /// OpenID格式：ox + 26位字母数字组合，总长度28
        /// </summary>
        private static readonly string[] MockOpenIDs = new string[]
        {
            "ox0H165teb8ukZRaa9WiUoOWbzJs",
            "ox1A234bcdefghijklmnopqrstuvwx",
            "ox2B345cdefghijklmnopqrstuvwxy",
            "ox3C456defghijklmnopqrstuvwxyz",
            "ox4D567efghijklmnopqrstuvwxyzA",
            "ox5E678fghijklmnopqrstuvwxyzAB",
            "ox6F789ghijklmnopqrstuvwxyzABC",
            "ox7G890hijklmnopqrstuvwxyzABCD",
            "ox8H901ijklmnopqrstuvwxyzABCDE",
            "ox9I012jklmnopqrstuvwxyzABCDEF",
            "ox0J123klmnopqrstuvwxyzABCDEFG",
            "ox1K234lmnopqrstuvwxyzABCDEFGH",
            "ox2L345mnopqrstuvwxyzABCDEFGHI",
            "ox3M456nopqrstuvwxyzABCDEFGHIJ",
            "ox4N567opqrstuvwxyzABCDEFGHIJK"
        };

        /// <summary>
        /// 预定义的模拟昵称列表（最多15个）
        /// 包含各种字符类型用于测试：中文、英文、数字、特殊字符、emoji、空格等
        /// </summary>
        private static readonly string[] MockNickNames = new string[]
        {
            "玩家A",                                    // 纯中文
            "Player123",                                // 英文+数字
            "测试用户@#$",                              // 中文+特殊字符
            "Game Master",                              // 英文+空格
            "🎮游戏达人🎯",                            // 中文+emoji
            "Test_User-01",                             // 英文+下划线+连字符+数字
            "玩家B (VIP)",                              // 中文+空格+括号
            "Player@2024",                              // 英文+@+数字
            "测试123用户",                              // 中文+数字
            "Game🎲Player",                             // 英文+emoji
            "用户_测试_01",                             // 中文+下划线+数字
            "Player & Friend",                          // 英文+空格+&+空格
            "测试用户★☆",                              // 中文+特殊符号
            "Player123_Test",                           // 英文+数字+下划线
            "游戏玩家🎮2024"                            // 中文+emoji+数字
        };

        /// <summary>
        /// 预定义的模拟头像URL列表
        /// </summary>
        private static readonly string[] MockAvatarUrls = new string[]
        {
            "https://example.com/avatar1.jpg",
            "https://example.com/avatar2.jpg",
            "https://example.com/avatar3.jpg",
            "https://example.com/avatar4.jpg",
            "https://example.com/avatar5.jpg",
            "https://example.com/avatar6.jpg",
            "https://example.com/avatar7.jpg",
            "https://example.com/avatar8.jpg",
            "https://example.com/avatar9.jpg",
            "https://example.com/avatar10.jpg",
            "https://example.com/avatar11.jpg",
            "https://example.com/avatar12.jpg",
            "https://example.com/avatar13.jpg",
            "https://example.com/avatar14.jpg",
            "https://example.com/avatar15.jpg"
        };

        /// <summary>
        /// 生成模拟的用户游戏信息数据列表
        /// </summary>
        /// <param name="count">生成的数据数量，最大数量为MAX_MOCK_DATA_COUNT
        /// <returns>模拟的用户游戏信息数据列表</returns>
        public static List<UserGameInfoData> GetMockGameInfoData(int count, bool includeSelf = false)
        {
            List<UserGameInfoData> mockDataList = new List<UserGameInfoData>();
            
            // 限制最大数量为MAX_MOCK_DATA_COUNT
            int actualCount = Math.Min(count, MAX_MOCK_DATA_COUNT);
            actualCount = Math.Max(actualCount, 0); // 确保不为负数

            Random random = new Random();

            for (int i = 0; i < actualCount; i++)
            {
                UserGameInfoData mockData = new UserGameInfoData
                {
                    _id = $"mock_id_{i}_{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                    openID = MockOpenIDs[i],
                    nickName = MockNickNames[i],
                    avatarUrl = MockAvatarUrls[i],
                    progressLevelID = random.Next(1, 10) // 随机生成1-10的关卡等级
                };

                mockDataList.Add(mockData);
            }

            if (includeSelf)
            {
                mockDataList.Add(new UserGameInfoData
                {
                    _id = $"mock_id_{actualCount}_{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                    openID = World.Instance.GameData.UserInfo.openID,
                    nickName = World.Instance.GameData.UserInfo.nickName,
                    avatarUrl = World.Instance.GameData.UserInfo.avatarUrl,
                    progressLevelID = World.Instance.GameData.ProgressLevelID
                });
            }

            Log.Info($"[MockData] Generated {mockDataList.Count} mock UserGameInfoData items");
            return mockDataList;
        }
    }
}