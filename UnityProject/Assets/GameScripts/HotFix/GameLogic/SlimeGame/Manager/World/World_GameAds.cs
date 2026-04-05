using WeChatWASM;
using TEngine;

namespace GameLogic
{
    partial class World
    {
        private WXCustomAd _bannerAd;

        public WXCustomAd BannerAd => _bannerAd;

        private WXRewardedVideoAd _rewardedVideoAd;
        public WXRewardedVideoAd RewardedVideoAd => _rewardedVideoAd;
        
        // 当前广告事件参数，如果为null则表示没有广告在播放
        private AdsEventParam _currentAdsEventParam = null;
        
        public void InitAds()
        {
            const string BANNER_AD_UNIT_ID = "adunit-2d2f49b16256da07";
            const string REWARDED_VIDEO_AD_UNIT_ID = "adunit-105af47a1ee46634";

            var windowInfo = WX.GetWindowInfo();
            _bannerAd = WX.CreateCustomAd(new WXCreateCustomAdParam()
            {
                adUnitId = BANNER_AD_UNIT_ID,
                adIntervals = 30,
                style = new CustomStyle()
                {
                    left = 0,
                    top = (int)(windowInfo.windowHeight - 150),
                    width = (int)windowInfo.windowWidth,
                },
            });
            _bannerAd.OnError((res)=>
            {
                Log.Info($"[Ads] bannerAd error response: {res.errCode}, {res.errMsg}");
            });
            _bannerAd.OnLoad((loadRsp)=> {
                Log.Info($"[Ads] bannerAd loaded: {loadRsp.errMsg}");
            });

            _rewardedVideoAd = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam()
            {
                adUnitId = REWARDED_VIDEO_AD_UNIT_ID,
                multiton = false,
            });
            _rewardedVideoAd.Load(success =>
            {
                Log.Info("[Ads] rewardedVideoAd loaded");
            }, failed =>
            {
                Log.Error($"[Ads] rewardedVideoAd load failed: {failed.errCode}, {failed.errMsg}");
            });
            _rewardedVideoAd.OnClose(OnRewardedVideoAdClose);

            Log.Info("[Ads] init ads success");
        }
        
        public void OnRewardedVideoAdClose(WXRewardedVideoAdOnCloseResponse rsp)
        {
            if (rsp == null)
            {
                Log.Error("[Ads] rewardedVideoAd close response is null");
                return;
            }

            Log.Info($"[Ads] rewardedVideoAd close: {rsp.isEnded}, {rsp.callbackId}, {rsp.errMsg}");
            
            // 检查是否有广告事件参数
            if (_currentAdsEventParam == null)
            {
                Log.Error("[Ads] No current ads event param found");
                return;
            }

            _currentAdsEventParam.isCompleted = rsp.isEnded;
            
            GameEvent.Send(SlimeEvent.OnAdsResultReceived, _currentAdsEventParam);
            Log.Info($"[Ads] send ads result event: {_currentAdsEventParam.adsType}, {_currentAdsEventParam.isCompleted}, {_currentAdsEventParam.userData}");
            
            ClearAdsState();
        }
        
        public void ShowAds(AdsType adsType, object userData = null)
        {
            _currentAdsEventParam = new AdsEventParam
            {
                adsType = adsType,
                userData = userData
            };
            ShowRewardedVideoAd();
        }

        private void ClearAdsState()
        {
            _currentAdsEventParam = null;
        }
        
        public void ShowBannerAd()
        {
#if UNITY_EDITOR
            Log.Info("[Ads] show bannerad in editor");
            return;
#endif

            if (_bannerAd == null)
            {
                Log.Error("[Ads] bannerad is null");
                return;
            }
            _bannerAd.Show();
            Log.Info("[Ads] show bannerad");
        }

        public void HideBannerAd()
        {
#if UNITY_EDITOR
            Log.Info("[Ads] hide bannerad in editor");
            return;
#endif

            if (_bannerAd == null)
            {
                Log.Error("[Ads] bannerad is null");
                return;
            }
            _bannerAd.Hide();
            Log.Info("[Ads] hide bannerad");
        }
        
        private void ShowRewardedVideoAd()
        {
#if UNITY_EDITOR
            SimulateAdCompleteInEditor();
            return;
#endif
            if (_rewardedVideoAd == null)
            {
                Log.Error("[Ads] rewardedVideoAd is null");
                return;
            }
            _rewardedVideoAd.Show();
            Log.Info($"[Ads] show rewardedVideoAd, with userData: {_currentAdsEventParam != null}");
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor下模拟广告观看完成，直接发送成功事件，使所有广告消费者正常工作
        /// </summary>
        private void SimulateAdCompleteInEditor()
        {
            if (_currentAdsEventParam == null)
            {
                Log.Error("[Ads] No current ads event param found in editor");
                return;
            }

            _currentAdsEventParam.isCompleted = true;

            Log.Info($"[Ads] Editor simulate ad complete: {_currentAdsEventParam.adsType}, userData={_currentAdsEventParam.userData}");
            GameEvent.Send(SlimeEvent.OnAdsResultReceived, _currentAdsEventParam);
            ClearAdsState();
        }
#endif
    }
}
