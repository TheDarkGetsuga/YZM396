using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneSaveLogic : MonoBehaviour
{
    [Tooltip("Should this scene save the scene name to save data?")]
    public bool saveScene = true;

    [Tooltip("Should gold be saved when this scene starts?")]
    public bool saveGold = true;

    [Tooltip("Should weapons be loaded when this scene starts?")]
    public bool loadWeapons = true;

    [Tooltip("Optional override for scene name (usually auto-filled)")]
    public string customSceneName = "";

    void Start()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("SaveManager not found. SceneSaveLogic aborted.");
            return;
        }

        string sceneName = string.IsNullOrEmpty(customSceneName) ? SceneManager.GetActiveScene().name : customSceneName;

        if (saveScene)
        {
            SaveManager.Instance.SetScene(sceneName);
            Debug.Log($"[SceneSaveLogic] Scene name saved: {sceneName}");
        }

        if (saveGold)
        {
            SaveManager.Instance.SetGold(SaveManager.Instance.GetGold()); //log golds
            Debug.Log("[SceneSaveLogic] Gold saved.");
        }

        if (loadWeapons)
        {
            var swordSwing = FindAnyObjectByType<SwordSwing>();
            if (swordSwing != null)
            {
                swordSwing.LoadSwordsFromSave();
                Debug.Log("[SceneSaveLogic] Weapons loaded.");
            }
        }
    }
}
