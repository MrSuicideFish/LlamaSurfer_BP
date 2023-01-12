using System.Collections;
using UnityEngine.UI;

public class HardDeathView : GameScreenView
{
   public void WatchAdForHeart()
   {
      AdRequestInfo request = new AdRequestInfo();
      request.OnRewardGranted += () =>
      {
         int heartCount = PlayerData.GetData<int>(PlayerData.DataKey.HeartCount, 3);
         if (heartCount < 3)
         {
            PlayerData.SetData(PlayerData.DataKey.HeartCount, heartCount + 1);
         }

         LevelLoader.RestartLevel();
      };
      AdsManager.ShowRewarded(request);
   }
}