using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SceneLoaderTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad = "Level2";

    [TextArea(2, 5)]
    public string quote;

    [Header("Boss Death Activation")]
    public bool bossDeathActivation = false;
    public List<GameObject> bossEnemies;

    private bool hasTriggered = false;

    private void Start()
    {
        if (bossDeathActivation && bossEnemies != null && bossEnemies.Count > 0)
        {
            StartCoroutine(WaitForAllBossesDead());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            TriggerSceneLoad();
        }
    }

    private IEnumerator WaitForAllBossesDead()
    {
        yield return new WaitUntil(() => bossEnemies.TrueForAll(boss => boss == null));
        yield return new WaitForSeconds(1f);
        if (!hasTriggered)
        {
            hasTriggered = true;
            TriggerSceneLoad();
        }
    }

    public void TriggerSceneLoad()
    {
        Debug.Log("Triggering scene load. Quote provided: " + !string.IsNullOrWhiteSpace(quote));
        SceneTransitionManager.EnsureExists();

        if (!string.IsNullOrWhiteSpace(quote))
        {
            if (SceneTransitionManager.Instance != null)
            {
                Debug.Log("Calling LoadSceneWithQuote.");
                SceneTransitionManager.Instance.LoadSceneWithQuote(sceneToLoad, quote);
            }
            else
            {
                Debug.LogWarning("SceneTransitionManager.Instance is null, loading scene directly.");
                SceneManager.LoadScene(sceneToLoad);
            }
        }
        else
        {
            Debug.Log("No quote provided, loading scene directly.");
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
