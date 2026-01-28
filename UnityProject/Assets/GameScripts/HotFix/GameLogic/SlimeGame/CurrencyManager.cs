using System;
using Cysharp.Threading.Tasks;
using GameLogic.Network;
using GameLogic.Network.Models;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 货币管理器
    /// </summary>
    public class CurrencyManager : Singleton<CurrencyManager>
    {
        private int _currentCoin = 0;
        
        /// <summary>
        /// 当前金币数量
        /// </summary>
        public int CurrentCoin => _currentCoin;
        
        /// <summary>
        /// 初始化货币数据（从用户信息中加载）
        /// </summary>
        /// <param name="userGameInfo">用户游戏信息</param>
        public void Initialize(UserGameInfoData userGameInfo)
        {
            if (userGameInfo != null)
            {
                _currentCoin = userGameInfo.coin;
                Log.Info($"[CurrencyManager] Initialize coin: {_currentCoin}");
            }
        }
        
        /// <summary>
        /// 更新金币数量（从服务器数据同步）
        /// </summary>
        /// <param name="coin">金币数量</param>
        public void UpdateCoin(int coin)
        {
            _currentCoin = coin;
            Log.Info($"[CurrencyManager] Update coin: {_currentCoin}");
            // 触发事件通知（UI层可以监听）
            GameEvent.Send(SlimeEvent.OnCoinChanged, coin);
        }
        
        /// <summary>
        /// 增加货币（通用方法）
        /// </summary>
        /// <param name="currencyType">货币类型（使用 CurrencyType 常量）</param>
        /// <param name="amount">增加的数量</param>
        /// <param name="source">货币来源（使用 CurrencySource 常量）</param>
        /// <param name="metadata">额外元数据</param>
        /// <returns>更新后的货币数量</returns>
        public async UniTask<int> AddCurrency(string currencyType, int amount, string source, object metadata = null)
        {
            var request = new
            {
                currencyType = currencyType,
                amount = amount,
                source = source,
                metadata = metadata
            };
            
            var response = await NetManager.CallHttp<AddCurrencyResponse>("addCurrency", request);
            if (response.IsSuccess && response.data != null)
            {
                // 更新本地货币
                if (currencyType == CurrencyType.COIN)
                {
                    UpdateCoin(response.data.coin);
                    return response.data.coin;
                }
                // 未来可以扩展其他货币类型
            }
            else
            {
                Log.Error($"[CurrencyManager] Add currency failed: {response.msg}");
            }
            
            return _currentCoin;
        }
        
        /// <summary>
        /// 扣除货币（通用方法）
        /// </summary>
        /// <param name="currencyType">货币类型（使用 CurrencyType 常量）</param>
        /// <param name="amount">扣除的数量</param>
        /// <param name="reason">扣除原因</param>
        /// <returns>更新后的货币数量，失败返回-1</returns>
        public async UniTask<int> DeductCurrency(string currencyType, int amount, string reason)
        {
            var request = new
            {
                currencyType = currencyType,
                amount = amount,
                reason = reason
            };
            
            var response = await NetManager.CallHttp<DeductCurrencyResponse>("deductCurrency", request);
            if (response.IsSuccess && response.data != null)
            {
                // 更新本地货币
                if (currencyType == CurrencyType.COIN)
                {
                    UpdateCoin(response.data.coin);
                    return response.data.coin;
                }
                // 未来可以扩展其他货币类型
            }
            else
            {
                Log.Error($"[CurrencyManager] Deduct currency failed: {response.msg}");
                return -1;  // 失败返回-1
            }
            
            return _currentCoin;
        }
        
        /// <summary>
        /// 增加金币（便捷方法）
        /// </summary>
        /// <param name="amount">增加的数量</param>
        /// <param name="source">金币来源（使用 CurrencySource 常量）</param>
        /// <param name="metadata">额外元数据</param>
        /// <returns>更新后的金币数量</returns>
        public async UniTask<int> AddCoin(int amount, string source, object metadata = null)
        {
            return await AddCurrency(CurrencyType.COIN, amount, source, metadata);
        }
        
        /// <summary>
        /// 扣除金币（便捷方法）
        /// </summary>
        /// <param name="amount">扣除的数量</param>
        /// <param name="reason">扣除原因</param>
        /// <returns>更新后的金币数量，失败返回-1</returns>
        public async UniTask<int> DeductCoin(int amount, string reason)
        {
            return await DeductCurrency(CurrencyType.COIN, amount, reason);
        }
        
        /// <summary>
        /// 从用户信息同步货币数据
        /// </summary>
        /// <param name="userGameInfo">用户游戏信息</param>
        public async UniTask SyncFromUserInfo(UserGameInfoData userGameInfo)
        {
            if (userGameInfo != null)
            {
                // 同步金币
                if (userGameInfo.coin != _currentCoin)
                {
                    UpdateCoin(userGameInfo.coin);
                }
            }
        }
    }
}

