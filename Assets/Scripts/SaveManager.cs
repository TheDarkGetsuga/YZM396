using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    public SaveData currentSave;
    private string currentCharacterName;
    private string saveFolderPath;
    private string saveFilePath => Path.Combine(saveFolderPath, currentCharacterName + "_save.json");

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFolderPath = Application.persistentDataPath + "/saves";

            if (!Directory.Exists(saveFolderPath))
                Directory.CreateDirectory(saveFolderPath);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this when starting a new game
    public void StartNewGame(string characterName)
    {
        currentCharacterName = characterName;
        currentSave = new SaveData
        {
            characterName = characterName,
            gold = 0,
            sceneName = "StartScene",
            obtainedSwordNames = new List<string>()
        };

        SaveGame();
    }

    // Call this to load an existing save
    public bool LoadGame(string characterName)
    {
        currentCharacterName = characterName;

        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            currentSave = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"Game Loaded for {characterName}");
            return true;
        }
        else
        {
            Debug.LogWarning($"No save file found for {characterName}");
            return false;
        }
    }

    public void SaveGame()
    {
        if (string.IsNullOrEmpty(currentCharacterName))
        {
            Debug.LogWarning("No character selected. Cannot save.");
            return;
        }

        string json = JsonUtility.ToJson(currentSave, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Game Saved: {saveFilePath}");
    }

    public void SetSwordObtained(string swordName)
    {
        if (!currentSave.obtainedSwordNames.Contains(swordName))
        {
            currentSave.obtainedSwordNames.Add(swordName);
            SaveGame();
        }
    }

    public bool HasSword(string swordName)
    {
        return currentSave.obtainedSwordNames.Contains(swordName);
    }

    public void SetScene(string sceneName)
    {
        currentSave.sceneName = sceneName;
        SaveGame();
    }
    public void SetGold(int gold)
    {
        currentSave.gold = gold;
        SaveGame();
    }

    public int GetGold()
    {
        return currentSave.gold;
    }

    public string GetSceneName()
    {
        return currentSave.sceneName;
    }

    public List<string> GetObtainedSwordNames()
    {
        return currentSave.obtainedSwordNames;
    }

    public string GetCurrentCharacterName()
    {
        return currentCharacterName;
    }
}
