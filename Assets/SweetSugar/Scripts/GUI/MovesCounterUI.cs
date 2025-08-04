using TMPro;
using UnityEngine;
using SweetSugar.Scripts.Core;

public class MovesCounterUI : MonoBehaviour
{
    public TextMeshProUGUI movesText;

    private void Start()
    {
        if (movesText == null)
            movesText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (movesText == null || LevelManager.THIS == null || LevelManager.THIS.levelData == null) return;

        int moves = Mathf.Clamp(LevelManager.THIS.levelData.limit, 0, 999);

        // Optional: only update if value changed
        if (movesText.text != moves.ToString())
        {
            movesText.text = moves.ToString();

            if (moves <= 5)
            {
                movesText.color = new Color(1f, 0.4f, 0.8f); // bright pink
                movesText.outlineColor = Color.black;
            }
            else
            {
                movesText.color = new Color(0.1f, 0.1f, 0.3f); // dark blue
                movesText.outlineColor = Color.white;
            }

            movesText.ForceMeshUpdate();
        }
    }
}
