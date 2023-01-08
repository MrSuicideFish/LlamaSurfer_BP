using System;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdsManager
{
    private static AdsManager _inst;
    
    private const string BannerAdUnitId = "ca-app-pub-9008022025116492/4724395833";
    private const string InterstitialAdUnitId = "ca-app-pub-9008022025116492/1136504617";
    private const string RewardedAdUnitId = "ca-app-pub-9008022025116492/2098232494";

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
    private bool _isInitialized;

    public static void Initialize()
    {
        Debug.Log("Initializing Ads");
        _inst = new AdsManager();
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log($"Init Mobile Ads: {initStatus}");
            _inst._isInitialized = true;
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
        if (_inst._isInitialized)
        {
            AdRequest request = new AdRequest.Builder().Build();
            _inst.BannerView.LoadAd(request);
        }
    }

    public static void HideBanner()
    {
        if (_inst._isInitialized)
        {
            _inst.BannerView.Hide();
        }
    }

    public static void LoadInterstitial()
    {
        if (_inst._isInitialized)
        {
            AdRequest request = new AdRequest.Builder().Build();
            _inst.interstitial.LoadAd(request);
        }
    }

    public static void ShowInterstitial(AdRequestInfo request)
    {
        if (_inst._isInitialized)
        {
            if (_inst.interstitial.IsLoaded())
            {
                _inst._request = request;
                _inst.interstitial.Show();
            }
        }
        else
        {
            request.OnAdComplete?.Invoke();
        }
    }

    public static void LoadRewarded()
    {
        if (_inst._isInitialized)
        {
            AdRequest request = new AdRequest.Builder().Build();
            _inst.Rewarded.LoadAd(request);
        }
    }

    public static void ShowRewarded(AdRequestInfo request)
    {
        if (_inst._isInitialized)
        {
            if (_inst.Rewarded.IsLoaded())
            {
                _inst._request = request;
                _inst.Rewarded.Show();
            }
        }
        else
        {
            request.OnAdComplete?.Invoke();
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
        Debug.Log($"On reward granted - Sender:{sender}\nRewardType:{type} ({amount})");
    }
}