using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public class DataKey
    {
        public const string DateCreated = "DateCreated";
        public const string LastLogin = "LastLogin";
        public const string HeartCount = "numOfHearts";
        public const string BonusBlockCount = "bonusBlocks";
        public const string LastCheckpoint = "LastCheckpoint";
        public const string LastLevelCompleted = "LastLevelCompleted";
        public const string IsAudioEnabled = "IsAudioEnabled";
    }

    private static PlayerData _loadedPlayerData = new PlayerData();
    private static string playerDataPath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, "playerData");
        }
    }
    
    private const string playerDataFilename = "playerdata.bpd";

    public Dictionary<string, object> data = new Dictionary<string, object>();

    public static void Load()
    {
        string directory = playerDataPath;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        string path = Path.Combine(directory, playerDataFilename);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            _loadedPlayerData = JsonConvert.DeserializeObject<PlayerData>(json);
        }
        else
        {
            _loadedPlayerData = new PlayerData();
            SetData(DataKey.DateCreated, DateTime.UtcNow.ToShortDateString());
            SetData(DataKey.HeartCount, 3);
            SetData(DataKey.BonusBlockCount, 0);
            SetData(DataKey.IsAudioEnabled, true);
        }
        
        SetData(DataKey.LastLogin, DateTime.UtcNow.ToShortDateString());
        Save();
        Debug.Log($"Loaded player data at path: {path}");
    }
    
    public static void Save()
    {
        string directory = playerDataPath;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        string path = Path.Combine(directory, playerDataFilename);
        string json = JsonConvert.SerializeObject(_loadedPlayerData);
        
        File.WriteAllText(path, json);
    }

    public static void ResetPlayerData()
    {
        SetData(DataKey.HeartCount, 3);
        SetData(DataKey.BonusBlockCount, 0);
        SetData(DataKey.IsAudioEnabled, true);
        SetData(DataKey.LastLogin, DateTime.UtcNow.ToShortDateString());
        SetData(DataKey.LastLevelCompleted, 0);
    }

    public static T GetData<T>(string key, object defaultValue)
    {
        if (_loadedPlayerData.data.ContainsKey(key))
        {
            object value = _loadedPlayerData.data[key];
            if (typeof(T) == typeof(int))
            {
                value = Convert.ToInt32(value);
                return (T)value;
            }
            else if (typeof(T) == typeof(bool))
            {
                value = Convert.ToBoolean(value);
                return (T) value;
            }
            else
            {
                return (T)_loadedPlayerData.data[key];
            }
        }
        else
        {
            _loadedPlayerData.data.Add(key, defaultValue);
        }

        return (T)defaultValue;
    }

    public static void SetData(string key, object data)
    {
        if (_loadedPlayerData.data.ContainsKey(key))
        {
            _loadedPlayerData.data[key] = data;
        }
        else
        {
            _loadedPlayerData.data.Add(key, data);
        }
    }

    public static void DeleteData(string key)
    {
        if (_loadedPlayerData.data.ContainsKey(key))
        {
            _loadedPlayerData.data.Remove(key);
        }
    }
}
