using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditsScroller : MonoBehaviour
{
    public RectTransform creditsText;
    public float scrollSpeed = 50f;
    public float fadeDuration = 2f;
    public float delayBeforeFade = 2f;
    public Image fadeImage;
    public string menuSceneName = "Menu";

    public float skipDelay = 3f;
    private bool fading = false;
    private float startY;
    public float endY;
    private float timer = 0f;

    void Start()
    {
        startY = creditsText.anchoredPosition.y;
        float screenHeight = Screen.height;
    }

    void Update()
    {
        if (fading) return;

        timer += Time.deltaTime;

        creditsText.anchoredPosition += Vector2.up * -scrollSpeed * Time.deltaTime; //this is such a caveman code way of doing it but fuck it, it works

        if (creditsText.anchoredPosition.y >= endY)
        {
            StartCoroutine(FadeToBlackAndReturn());
            fading = true;
        }
        if (timer >= skipDelay && Input.anyKeyDown)
        {
            StartCoroutine(FadeToBlackAndReturn());
            fading = true;
        }
    }

    private IEnumerator FadeToBlackAndReturn() //BLEACH REFERENCE!?!? https://youtu.be/YKLijZfJVzs?si=AWDxtwmUC4J0YCG4
    {
        yield return new WaitForSeconds(delayBeforeFade);

        float elapsed = 0f;
        Color color = fadeImage.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, t);
            yield return null;
        }

        SceneManager.LoadScene(menuSceneName);
    }
}
