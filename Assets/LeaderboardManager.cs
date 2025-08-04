using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Top 3 Cards")]
    public GameObject player1Card;
    public GameObject player2Card;
    public GameObject player3Card;

    [Header("Rank Entry Prefab")]
    public GameObject rankEntryPrefab;
    public Transform rankListContainer;

    [Header("Config")]
    public string statisticName = "HighScore";
    public int maxResultsCount = 20;

    void Start()
    {
        GetLeaderboard();
    }

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = statisticName,
            StartPosition = 0,
            MaxResultsCount = maxResultsCount
        };

        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardReceived, OnError);
    }

    void OnLeaderboardReceived(GetLeaderboardResult result)
    {
        ClearLeaderboard();

        for (int i = 0; i < result.Leaderboard.Count; i++)
        {
            var entry = result.Leaderboard[i];
            string displayName = entry.DisplayName ?? "Player";
            int score = entry.StatValue;
            int rank = entry.Position + 1;

            if (i == 0) FillTopCard(player1Card, displayName, score, rank);
            else if (i == 1) FillTopCard(player2Card, displayName, score, rank);
            else if (i == 2) FillTopCard(player3Card, displayName, score, rank);
            else InstantiateRankEntry(displayName, score, rank);
        }
    }

    void FillTopCard(GameObject card, string name, int score, int rank)
    {
        card.transform.Find("PlayerName").GetComponent<TMP_Text>().text = name;
        card.transform.Find("PlayerScore").GetComponent<TMP_Text>().text = score.ToString();
        card.transform.Find("Rank").GetComponent<TMP_Text>().text = "#" + rank;
    }

    void InstantiateRankEntry(string name, int score, int rank)
    {
        GameObject entry = Instantiate(rankEntryPrefab, rankListContainer);
        entry.transform.Find("PlayerName").GetComponent<TMP_Text>().text = name;
        entry.transform.Find("PlayerScore").GetComponent<TMP_Text>().text = score.ToString();
        entry.transform.Find("Rank").GetComponent<TMP_Text>().text = "#" + rank;
    }

    void ClearLeaderboard()
    {
        foreach (Transform child in rankListContainer)
        {
            Destroy(child.gameObject);
        }
    }

    void OnError(PlayFabError error)
    {
        Debug.LogError("PlayFab Leaderboard Error: " + error.GenerateErrorReport());
    }
}
