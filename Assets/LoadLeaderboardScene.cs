using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLeaderboardScene : MonoBehaviour
{
    public void LoadLeaderboard()
    {
        SceneManager.LoadScene("leaderboard");
    }
}
