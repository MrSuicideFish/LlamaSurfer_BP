using System;
using GoogleMobileAds.Api;

public class AdRequestInfo
{
    public Action OnAdComplete;
    public Action OnAdShow;
    public Action OnAdShowFailed;
    public Action OnRewardGranted;
}