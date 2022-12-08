using System;
using System.IO;
using Newtonsoft.Json;

[Serializable]
public class PlayerData
{
    private static PlayerData _loadedPlayerData;
    private const string playerDataPath = "data/playerdata.json";
    
    public static void Load()
    {
        
    }
    
    public static void Save()
    {
        
    }
}
