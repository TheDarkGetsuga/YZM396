using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    public GameObject transitionUIPrefab;

    private GameObject currentUIInstance;
    private TextMeshProUGUI quoteText;
    private Image blackPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject("SceneTransitionManager");
            obj.AddComponent<SceneTransitionManager>();
            DontDestroyOnLoad(obj);
        }
    }

    public void LoadSceneWithQuote(string sceneName, string quote)
    {
        StartCoroutine(DoSceneTransition(sceneName, quote));
    }

    private IEnumerator DoSceneTransition(string sceneName, string quote)
    {
        Debug.Log("Starting DoSceneTransition coroutine.");

        if (currentUIInstance == null)
        {
            if (transitionUIPrefab == null)
            {
                Debug.Log("transitionUIPrefab is null, loading from Resources...");
                transitionUIPrefab = Resources.Load<GameObject>("TransitionUIRoot");
                Debug.Log("TransitionUIPrefab loaded from Resources: " + (transitionUIPrefab != null));
            }

            if (transitionUIPrefab != null)
            {
                currentUIInstance = Instantiate(transitionUIPrefab);
                DontDestroyOnLoad(currentUIInstance);

                quoteText = currentUIInstance.GetComponentInChildren<TextMeshProUGUI>();
                blackPanel = currentUIInstance.GetComponentInChildren<Image>();

                if (quoteText == null)
                {
                    Debug.LogError("TextMeshProUGUI component not found in prefab children!");
                    yield break;
                }

                if (blackPanel == null)
                {
                    Debug.LogError("Black panel Image component not found in prefab children!");
                    yield break;
                }

                Debug.Log("TextMeshProUGUI found on object: " + quoteText.gameObject.name);
                Debug.Log("Black panel Image found on object: " + blackPanel.gameObject.name);
            }
            else
            {
                Debug.LogError("TransitionUIRoot prefab not assigned and not found in Resources!");
                SceneManager.LoadScene(sceneName);
                yield break;
            }
        }

        // Fade out music
        AudioManager audioManager = Object.FindAnyObjectByType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.FadeOutMusic(1f); // fades out over 1 second
        }

        // Set initial colors
        Color originalTextColor = quoteText.color;
        Color transparentTextColor = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, 0f);
        quoteText.color = transparentTextColor;

        Color originalPanelColor = blackPanel.color;
        Color transparentPanelColor = new Color(originalPanelColor.r, originalPanelColor.g, originalPanelColor.b, 0f);
        blackPanel.color = transparentPanelColor;

        quoteText.text = quote;

        // Fade in both text and panel
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            blackPanel.color = Color.Lerp(transparentPanelColor, originalPanelColor, t);
            quoteText.color = Color.Lerp(transparentTextColor, originalTextColor, t);
            yield return null;
        }

        Debug.Log("Quote displayed, holding on screen for 4 seconds.");
        yield return new WaitForSeconds(8f);

        // Fade out only the text
        t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            quoteText.color = Color.Lerp(transparentTextColor, originalTextColor, t);
            yield return null;
        }

        // Keep panel fully opaque
        blackPanel.color = originalPanelColor;

        Debug.Log("Loading scene asynchronously: " + sceneName);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log("Scene loaded, cleaning up transition UI.");
        if (currentUIInstance != null)
        {
            Destroy(currentUIInstance);
            currentUIInstance = null;
            quoteText = null;
            blackPanel = null;
        }
    }
}
