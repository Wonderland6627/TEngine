using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using WeChatWASM;
using TEngine;
using System;

namespace GameLogic
{
    partial class World
    {
        private WXCustomAd _bannerAd;

        public WXCustomAd BannerAd => _bannerAd;

        private WXRewardedVideoAd _rewardedVideoAd;
        public WXRewardedVideoAd RewardedVideoAd => _rewardedVideoAd;
        
        public void InitAds()
        {
            var windowInfo = WX.GetWindowInfo();
            _bannerAd = WX.CreateCustomAd(new WXCreateCustomAdParam()
            {
                adUnitId = "adunit-2d2f49b16256da07",
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
                Log.Info($"[Ads] bannerad error response: {res.errCode}, {res.errMsg}");
            });
            _bannerAd.OnLoad((loadRsp)=> {
                Log.Info($"[Ads] bannerad loaded: {loadRsp.errMsg}");
            });

            _rewardedVideoAd = WX.CreateRewardedVideoAd(new WXCreateRewardedVideoAdParam()
            {
                adUnitId = "adunit-105af47a1ee46634",
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
            bool triggerReward = rsp.isEnded;
            OnRewardedVideoAdClosed(triggerReward);
        }
        
        public void ShowBannerAd()
        {
#if UNITY_EDITOR
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
        
        public void ShowRewardedVideoAd()
        {
#if UNITY_EDITOR
            return;
#endif
            if (_rewardedVideoAd == null)
            {
                Log.Error("[Ads] rewardedVideoAd is null");
                return;
            }
            _rewardedVideoAd.Show();
            Log.Info("[Ads] show rewardedVideoAd");
        }
    }
}
