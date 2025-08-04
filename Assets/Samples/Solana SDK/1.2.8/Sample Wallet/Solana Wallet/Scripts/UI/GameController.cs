using UnityEngine;

// ReSharper disable once CheckNamespace
public class GameController : MonoBehaviour
{
    private const string RepoUrl = "https://ddgaming.io";
    
    public void OpenSDKRepo()
    {
        Application.OpenURL(RepoUrl);
    }
    
}
