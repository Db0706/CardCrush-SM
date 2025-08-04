using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeUI : MonoBehaviour
{
    public float fadeDuration = 1.5f; // Time for full fade-in/out
    public bool startVisible = false; // Should the image start visible?

    private Image imageComponent;
    private bool isFadingIn = true;
    private Coroutine fadeRoutine;

    void Start()
    {
        imageComponent = GetComponent<Image>();

        if (imageComponent == null)
        {
            Debug.LogError("No Image component found on " + gameObject.name);
            return;
        }

        // Set initial visibility
        float initialAlpha = startVisible ? 1f : 0f;
        Color color = imageComponent.color;
        color.a = initialAlpha;
        imageComponent.color = color;

        // Start the fade loop
        fadeRoutine = StartCoroutine(FadeLoop());
    }

    private IEnumerator FadeLoop()
    {
        while (true)
        {
            float elapsedTime = 0f;
            float startAlpha = isFadingIn ? 0f : 1f;
            float endAlpha = isFadingIn ? 1f : 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);

                Color color = imageComponent.color;
                color.a = alpha;
                imageComponent.color = color;

                yield return null;
            }

            isFadingIn = !isFadingIn; // Toggle fade direction
            yield return new WaitForSeconds(0.5f); // Pause before switching
        }
    }
}
