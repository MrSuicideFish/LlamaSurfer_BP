using System;
using System.Collections.Generic;
using System.Text;
using Firebase.Analytics;
using MoreMountains.Tools;
using UnityEngine;

public class Analytics
{
    private class AnalyticsEvent
    {
        public const string ApplicationStart = "app_start";
        
        // gameplay
        public const string LevelStart = "level_start";
        public const string LevelComplete = "level_complete";
        public const string LevelFail = "level_fail";
        public const string CheckpointReached = "checkpoint_reached";
        
        // resources
        public const string HeartRequested = "heart_requested";
        public const string HeartGranted = "heart_granted";
        public const string BonusBlockRequested = "bonus_block_requested";
        public const string BonusBlockGranted = "bonus_block_granted";
        
        // ads
        public const string BannerShow = "banner_show";
        public const string BannerHide = "banner_hide";
        public const string BannerClicked = "banner_clicked";
        
        public const string InterstitialShow = "interstitial_show";
        public const string InterstitialHide = "interstitial_hide";
        public const string InterstitialClicked = "interstitial_clicked";
    }

    private class AnalyticsParam
    {
        public const string DateCreated = "date_created";
        public const string HeartCount = "heart_count";
        public const string BonusBlockCount = "bonus_blocks";
        public const string LevelNum = "level_num";
        public const string Checkpoint = "checkpoint";
        public const string StartCheckpoint = "start_checkpoint";
        public const string FailCheckpoint = "fail_checkpoint";
        public const string PointsCollected = "points_collected";
        public const string TotalPoints = "total_points";
    }

    private class Param
    {
        public string _name;
        public object _value;
        
        private Firebase.Analytics.Parameter data;
        
        public Param(string paramName, string paramValue)
        {
            data = new Parameter(paramName, paramValue);
            _name = paramName;
            _value = paramValue;
        }

        public Param(string paramName, long paramValue)
        {
            data = new Parameter(paramName, paramValue);
            _name = paramName;
            _value = paramValue;
        }

        public Param(string paramName, double paramValue)
        {
            data = new Parameter(paramName, paramValue);
            _name = paramName;
            _value = paramValue;
        }

        public Firebase.Analytics.Parameter GetData()
        {
            return data;
        }
    }
    
    public static void FireAppStart()
    {
        LogEvent(AnalyticsEvent.ApplicationStart);
    }

    public static void LevelStart(int levelNum, int startCheckpoint = 0)
    {
        List<Param> eventParams = new List<Param>()
        {
            new Param(AnalyticsParam.LevelNum, levelNum),
            new Param(AnalyticsParam.StartCheckpoint, startCheckpoint)
        };

        LogEvent(AnalyticsEvent.LevelStart, eventParams);
    }

    public static void LevelComplete(int levelNum, int pointsCollected, int totalPoints)
    {
        List<Param> eventParams = new List<Param>()
        {
            new Param(AnalyticsParam.LevelNum, levelNum),
            new Param(AnalyticsParam.PointsCollected, pointsCollected),
            new Param(AnalyticsParam.TotalPoints, totalPoints)
        };
        LogEvent(AnalyticsEvent.LevelComplete, eventParams);
    }

    public static void LevelFailed(int levelNum, int startCheckpoint, int failCheckpoint, int pointsCollected, int totalPoints)
    {
        List<Param> eventParams = new List<Param>()
        {
            new Param(AnalyticsParam.LevelNum, levelNum),
            new Param(AnalyticsParam.StartCheckpoint, startCheckpoint),
            new Param(AnalyticsParam.FailCheckpoint, failCheckpoint),
            new Param(AnalyticsParam.PointsCollected, pointsCollected),
            new Param(AnalyticsParam.TotalPoints, totalPoints)
        };
        LogEvent(AnalyticsEvent.LevelFail, eventParams);
    }

    public static void CheckpointReached(int levelNum, int checkpoint)
    {
        List<Param> eventParams = new List<Param>()
        {
            new Param(AnalyticsParam.LevelNum, levelNum),
            new Param(AnalyticsParam.Checkpoint, checkpoint)
        };
        LogEvent(AnalyticsEvent.CheckpointReached, eventParams);
    }

    private static void LogEvent(string name, List<Param> parameters = null)
    {
        if (name.Equals(string.Empty))
        {
            return;
        }
        
        if (parameters == null)
        {
            parameters = new List<Param>();
        }

        // Add default parameters
        string dateCreatedVal = PlayerData.GetData<string>(PlayerData.DataKey.DateCreated, "");
        parameters.Add(new Param(AnalyticsParam.DateCreated, dateCreatedVal));

        int heartCountVal = PlayerData.GetData<int>(PlayerData.DataKey.HeartCount, 0);
        parameters.Add(new Param(AnalyticsParam.HeartCount, heartCountVal));

        int blockCountVal = PlayerData.GetData<int>(PlayerData.DataKey.BonusBlockCount, 0);
        parameters.Add(new Param(AnalyticsParam.BonusBlockCount, blockCountVal));
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Logged Event '{name}'");
        sb.AppendLine("Parameters:");
        
        List<Parameter> firebaseParams = new List<Parameter>();
        foreach (Param p in parameters)
        {
            firebaseParams.Add(p.GetData());
            sb.AppendLine($"\"{p._name}\": \"{p._value}\"");
        }

        FirebaseAnalytics.LogEvent(name, firebaseParams.ToArray());
        Debug.Log(sb.ToString());
    }
}