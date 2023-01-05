#define USE_TEST_ADS
using System;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdsManager
{
    private static AdsManager _inst;
    
#if USE_TEST_ADS
    private const string BannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
    private const string InterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
    private const string RewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
#else
    private const string BannerAdUnitId = "ca-app-pub-9518312041316421/3691356089";
    private const string InterstitialAdUnitId = "ca-app-pub-9518312041316421/5142386163";
    private const string RewardedAdUnitId = "ca-app-pub-9518312041316421/6866531372";
#endif

    private BannerView _bannerView;
    private BannerView BannerView
    {
        get
        {
            if (_bannerView == null)
            {
                _bannerView = new BannerView(BannerAdUnitId, AdSize.Banner, AdPosition.Top);
                _bannerView.OnAdLoaded += _inst.OnAdLoaded;
                _bannerView.OnAdFailedToLoad += _inst.OnAdLoadFailed;
                _bannerView.OnAdClosed += _inst.OnAdClose;
                _bannerView.OnAdOpening += _inst.OnAdShow;
            }
            
            return _bannerView;
        }
    }

    private InterstitialAd _interstitial;
    private InterstitialAd interstitial
    {
        get
        {
            if (_interstitial == null)
            {
                _interstitial = new InterstitialAd(InterstitialAdUnitId);
                _interstitial.OnAdLoaded += _inst.OnAdLoaded;
                _interstitial.OnAdFailedToLoad += _inst.OnAdLoadFailed;
                _interstitial.OnAdClosed += _inst.OnAdClose;
                _interstitial.OnAdOpening += _inst.OnAdShow;
            }
            return _interstitial;
        }
    }

    private RewardedAd _rewarded;
    private RewardedAd Rewarded
    {
        get
        {
            if (_rewarded == null)
            {
                _rewarded = new RewardedAd(RewardedAdUnitId);
                _rewarded.OnAdLoaded += _inst.OnAdLoaded;
                _rewarded.OnAdFailedToLoad += _inst.OnAdLoadFailed;
                _rewarded.OnAdFailedToShow += _inst.OnAdShowFailed;
                _rewarded.OnAdClosed += _inst.OnAdClose;
                _rewarded.OnAdOpening += _inst.OnAdShow;
                _rewarded.OnUserEarnedReward += _inst.OnRewardedAdGranted;
            }
            return _rewarded;
        }
    }

    public static void Initialize()
    {
        _inst = new AdsManager();
        MobileAds.Initialize(initStatus =>
        {
            LoadInterstitial();
            LoadRewarded();
            ShowBanner();
        });
    }

    public static void Shutdown()
    {
        _inst.interstitial.Destroy();
        _inst.BannerView.Destroy();
        _inst = null;
    }

    public static void ShowBanner()
    {
        AdRequest request = new AdRequest.Builder().Build();
        _inst.BannerView.LoadAd(request);
    }

    public static void HideBanner()
    {
        _inst.BannerView.Hide();
    }

    private static void LoadInterstitial()
    {
        AdRequest request = new AdRequest.Builder().Build();
        _inst.interstitial.LoadAd(request);
    }

    public static void ShowInterstitial()
    {
        if (_inst.interstitial.IsLoaded())
        {
            _inst.interstitial.Show();
        }
    }

    public static void LoadRewarded()
    {
        AdRequest request = new AdRequest.Builder().Build();
        _inst.Rewarded.LoadAd(request);
    }

    public static void ShowRewarded()
    {
        if (_inst.Rewarded.IsLoaded())
        {
            _inst.Rewarded.Show();
        }
    }

    private void OnAdLoaded(object sender, EventArgs args)
    {
        Debug.Log($"On ad loaded - Sender:{sender}\nArgs:{args}");
    }

    private void OnAdLoadFailed(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log($"Ad Failed To Load - Sender:{sender}\nArgs:{args}");
    }
    
    private void OnAdShowFailed(object sender, AdErrorEventArgs args)
    {

        Debug.Log($"Ad Failed To Show - Sender:{sender}\nArgs:{args}");
    }

    private void OnAdShow(object sender, EventArgs args)
    {
        Debug.Log($"On ad Show - Sender:{sender}\nArgs:{args}");
    }

    private void OnAdClose(object sender, EventArgs args)
    {
        Debug.Log($"On ad Close - Sender:{sender}\nArgs:{args}");
    }
    private void OnRewardedAdGranted(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        Debug.Log($"On reward granted - Sender:{sender}\nRewardType:{type} ({amount})");
    }
}