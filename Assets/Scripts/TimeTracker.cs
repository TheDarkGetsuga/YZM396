using UnityEngine;

public class TimeTracker : MonoBehaviour
{
    private float elapsedTime = 0f;
    public static TimeTracker Instance;

    void Awake()
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

    void Update()
    {
        elapsedTime += Time.deltaTime;
    }

    void OnDestroy()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.currentSave != null)
        {
            SaveManager.Instance.currentSave.totalPlayTime += elapsedTime;
            SaveManager.Instance.SaveGame();
        }
    }
    public float GetLiveTotalPlayTime()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.currentSave != null)
        {
            return SaveManager.Instance.currentSave.totalPlayTime + elapsedTime;
        }

        return elapsedTime;
    }
}
