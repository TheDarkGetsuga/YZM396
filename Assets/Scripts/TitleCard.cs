using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleCardController : MonoBehaviour
{
    public float fadeInDuration = 1f;
    public float displayDuration = 2f;
    public float fadeOutDuration = 1f;
    public string titleText = "Title Card";

    private TextMeshProUGUI text;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        text.text = titleText;
        StartCoroutine(PlayTitleCard());
    }

    IEnumerator PlayTitleCard()
    {
        yield return Fade(0f, 1f, fadeInDuration);
        yield return new WaitForSeconds(displayDuration);
        yield return Fade(1f, 0f, fadeOutDuration);
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
