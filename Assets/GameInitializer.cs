using UnityEngine;
using SweetSugar.Scripts.Integrations.Network;
using PlayFab;
using PlayFab.ClientModels;

public class GameInitializer : MonoBehaviour
{
    void Start()
    {
#if PLAYFAB
        Debug.Log("GameInitializer: Assigning PlayFabDataManager");
// NetworkManager.dataManager = (NetworkDataManager)new PlayFabDataManager();

        // Pull current level from PlayFab and override PlayerPrefs
        FetchProgressFromPlayFab();
#endif
    }

    void FetchProgressFromPlayFab()
    {
        var request = new GetPlayerStatisticsRequest
        {
            StatisticNames = new System.Collections.Generic.List<string> { "Level" }
        };

        PlayFabClientAPI.GetPlayerStatistics(request, result =>
        {
            foreach (var stat in result.Statistics)
            {
                if (stat.StatisticName == "Level")
                {
                    int levelFromPlayFab = stat.Value;
                    PlayerPrefs.SetInt("ReachedLevel", levelFromPlayFab);
                    Debug.Log($"GameInitializer: Synced ReachedLevel from PlayFab: {levelFromPlayFab}");
                }
            }
        }, error =>
        {
            Debug.LogError("GameInitializer: Failed to fetch level from PlayFab\n" + error.GenerateErrorReport());
        });
    }
}
