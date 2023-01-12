#define USE_DEBUG_ADS
using System;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdsManager
{
    private static AdsManager _inst;

    private static AdsManager Inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = new AdsManager();
            }

            return _inst;
        }
    }
    
#if USE_DEBUG_ADS
    private const string BannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
    private const string InterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";
    private const string RewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";
#else
    private const string BannerAdUnitId = "ca-app-pub-9008022025116492/4724395833";
    private const string InterstitialAdUnitId = "ca-app-pub-9008022025116492/1136504617";
    private const string RewardedAdUnitId = "ca-app-pub-9008022025116492/2098232494";
#endif


    private BannerView _bannerView;
    private BannerView BannerView
    {
        get
        {
            if (_bannerView == null)
            {
                _bannerView = new BannerView(BannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
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

    private AdRequestInfo _request;
    public static bool IsInitialized { get; set; }

    public static void Initialize()
    {
        Debug.Log("Initializing Ads");
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log($"Init Mobile Ads: {initStatus}");
            IsInitialized = true;
            LoadInterstitial();
            LoadRewarded();
            ShowBanner();
        });
    }

    public static void Shutdown()
    {
        Inst.interstitial.Destroy();
        Inst.BannerView.Destroy();
        _inst = null;
    }

    public static void ShowBanner()
    {
        if (IsInitialized)
        {
            AdRequest request = new AdRequest.Builder().Build();
            Inst.BannerView.LoadAd(request);
        }
    }

    public static void HideBanner()
    {
        if (IsInitialized)
        {
            Inst.BannerView.Hide();
        }
    }

    public static void LoadInterstitial()
    {
        if (IsInitialized)
        {
            AdRequest request = new AdRequest.Builder().Build();
            Inst.interstitial.LoadAd(request);
        }
    }

    public static void ShowInterstitial(AdRequestInfo request)
    {
        if (IsInitialized)
        {
            if (Inst.interstitial.IsLoaded())
            {
                Inst._request = request;
                Inst.interstitial.Show();
            }
        }
        else
        {
            request.OnAdComplete?.Invoke();
        }
    }

    public static void LoadRewarded()
    {
        if (IsInitialized)
        {
            AdRequest request = new AdRequest.Builder().Build();
            Inst.Rewarded.LoadAd(request);
        }
    }

    public static void ShowRewarded(AdRequestInfo request)
    {
        if (IsInitialized)
        {
            if (Inst.Rewarded.IsLoaded())
            {
                Inst._request = request;
                Inst.Rewarded.Show();
            }
        }
        else
        {
            request.OnAdComplete?.Invoke();
        }
    }

    public static bool IsInterstitialLoaded()
    {
        if (Inst._interstitial == null) return false;
        return Inst._interstitial.IsLoaded();
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
        if (_request != null)
        {
            _request.OnAdShowFailed?.Invoke();
        }
    }

    private void OnAdShow(object sender, EventArgs args)
    {
        Debug.Log($"On ad Show - Sender:{sender}\nArgs:{args}");
        if (_request != null)
        {
            _request.OnAdShow?.Invoke();
        }
    }

    private void OnAdClose(object sender, EventArgs args)
    {
        Debug.Log($"On ad Close - Sender:{sender}\nArgs:{args}");
        if (_request != null)
        {
            _request.OnAdComplete?.Invoke();
        }
    }
    private void OnRewardedAdGranted(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        if (_request != null)
        {
            _request.OnRewardGranted?.Invoke();
        }
        Debug.Log($"On reward granted - Sender:{sender}\nRewardType:{type} ({amount})");
    }
}