using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public SceneLoaderTrigger sceneLoaderTrigger;

    [Header("Panels")]
    public GameObject newGamePanel;
    public GameObject loadGamePanel;

    [Header("Slot UI References")]
    public Transform newGameSlotParent;
    public Transform loadGameSlotParent;
    public GameObject slotButtonPrefab;

    private string[] slotNames = { "Slot1", "Slot2", "Slot3", "Slot4", "Slot5" };
    private string SaveFolderPath => Path.Combine(Application.persistentDataPath, "saves");

    void Start()
    {
        if (!Directory.Exists(SaveFolderPath))
            Directory.CreateDirectory(SaveFolderPath);

        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(false);
    }

    public void Play()
    {
        ShowNewGamePanel();
    }

    public void ShowNewGamePanel()
    {
        newGamePanel.SetActive(true);
        loadGamePanel.SetActive(false);
        PopulateSlotButtons(newGameSlotParent, isLoad: false);
    }

    public void ShowLoadGamePanel()
    {
        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(true);
        PopulateSlotButtons(loadGameSlotParent, isLoad: true);
    }

    void PopulateSlotButtons(Transform parent, bool isLoad)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }

        foreach (string slot in slotNames)
        {
            GameObject buttonObj = Instantiate(slotButtonPrefab, parent);
            TMP_Text text = buttonObj.GetComponentInChildren<TMP_Text>();

            string savePath = Path.Combine(SaveFolderPath, slot + "_save.json");
            bool saveExists = File.Exists(savePath);

            text.text = saveExists ? $"{slot} (Saved)" : $"{slot} (Empty)";

            Button button = buttonObj.GetComponent<Button>();
            if (isLoad)
                button.onClick.AddListener(() => LoadSlot(slot));
            else
                button.onClick.AddListener(() => StartNewGameInSlot(slot));
        }
    }

    public void StartNewGameInSlot(string slot)
    {
        SaveManager.Instance.StartNewGame(slot);

        if (sceneLoaderTrigger != null)
            sceneLoaderTrigger.TriggerSceneLoad();
        else
            SceneManager.LoadScene("Level1");
    }

    public void LoadSlot(string slot)
    {
        bool success = SaveManager.Instance.LoadGame(slot);
        if (!success)
        {
            Debug.LogWarning("Save file does not exist.");
            return;
        }
        SceneManager.LoadScene(SaveManager.Instance.GetSceneName());
        
    }

    public void Quit()
    {
        Application.Quit();
    }
}
