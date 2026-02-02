using Cysharp.Threading.Tasks;
using GameLogic.Network;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 金币管理（World 的部分类）
    /// 负责金币的增减业务逻辑和网络请求
    /// </summary>
    public partial class World
    {
        /// <summary>
        /// 增加金币（调用服务器接口）
        /// </summary>
        /// <param name="amount">增加的数量</param>
        /// <param name="source">金币来源（使用 Constants.CurrencySource 常量）</param>
        /// <param name="metadata">额外元数据</param>
        /// <returns>更新后的金币数量</returns>
        public async UniTask<int> AddCoin(int amount, string source, object metadata = null)
        {
            var request = new
            {
                currencyType = CurrencyTypes.COIN,
                amount = amount,
                source = source,
                metadata = metadata
            };
            
            var response = await NetManager.CallHttp<AddCurrencyResponse>("addCurrency", request);
            
            if (!response.IsSuccess || response.data == null)
            {
                Log.Error($"[World] Add coin failed: {response.ErrorMessage}");
                return GameData.Coin;
            }
            
            GameData.UpdateCoin(response.data.coin);
            return GameData.Coin;
        }

        /// <summary>
        /// 扣除金币（调用服务器接口）
        /// </summary>
        /// <param name="amount">扣除的数量</param>
        /// <param name="reason">扣除原因</param>
        /// <returns>更新后的金币数量，失败返回-1</returns>
        public async UniTask<int> DeductCoin(int amount, string reason)
        {
            var request = new
            {
                currencyType = CurrencyTypes.COIN,
                amount = amount,
                reason = reason
            };
            
            var response = await NetManager.CallHttp<DeductCurrencyResponse>("deductCurrency", request);
            
            if (!response.IsSuccess || response.data == null)
            {
                Log.Error($"[World] Deduct coin failed: {response.ErrorMessage}");
                return -1;
            }
            
            GameData.UpdateCoin(response.data.coin);
            return GameData.Coin;
        }
    }
}

