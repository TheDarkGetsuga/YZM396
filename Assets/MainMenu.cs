using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    public SceneLoaderTrigger sceneLoaderTrigger;

    [Header("Panels")]
    public GameObject titlePanel;
    public GameObject mainMenuPanel;
    public GameObject newGamePanel;
    public GameObject loadGamePanel;

    [Header("Slot UI References")]
    public Transform newGameSlotParent;
    public Transform loadGameSlotParent;
    public GameObject slotButtonPrefab;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip menuEnterSound;
    public AudioClip menuBackSound;

    private string[] slotNames = { "Slot1", "Slot2", "Slot3", "Slot4", "Slot5" };
    private string SaveFolderPath => Path.Combine(Application.persistentDataPath, "saves");

    void Start()
    {
        Screen.SetResolution(3840, 2160, FullScreenMode.FullScreenWindow, 60);

        if (!Directory.Exists(SaveFolderPath))
            Directory.CreateDirectory(SaveFolderPath);

        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(false);
        ShowMainMenu();

        AddHoverSoundsToMainMenuButtons();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (newGamePanel.activeSelf || loadGamePanel.activeSelf)
            {
                PlayBackSound();
                ShowMainMenu();
            }
        }
    }

    public void Play()
    {
        ShowNewGamePanel();
    }

    public void ShowNewGamePanel()
    {
        PlayEnterSound();
        newGamePanel.SetActive(true);
        loadGamePanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        titlePanel.SetActive(false);
        PopulateSlotButtons(newGameSlotParent, isLoad: false);
    }

    public void ShowLoadGamePanel()
    {
        PlayEnterSound();
        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        titlePanel.SetActive(false);
        PopulateSlotButtons(loadGameSlotParent, isLoad: true);
    }

    private void ShowMainMenu()
    {
        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        titlePanel.SetActive(true);
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

            string displayName = "Empty";
            if (saveExists)
            {
                string json = File.ReadAllText(savePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);
                displayName = GetLevelDisplayName(saveData.sceneName);
            }

            text.text = displayName;

            Button button = buttonObj.GetComponent<Button>();
            if (isLoad)
                button.onClick.AddListener(() => LoadSlot(slot));
            else
                button.onClick.AddListener(() => StartNewGameInSlot(slot));

            AddHoverSound(buttonObj);
        }
    }

    string GetLevelDisplayName(string sceneName)
    {
        if (sceneName == "Level1") return "Chapter I - The Original Sin";
        else if (sceneName == "Level2") return "Chapter II - Feast of Throns";
        else if (sceneName == "Level3") return "Chapter III - The Thirteenth Seat Beneath The Spire";
        else if (sceneName == "Level4") return "Chapter IV - When the Bells Fell Silent";
        else if (sceneName == "Level5") return "Chapter V - Where Worms Do Not Die";
        else if (sceneName == "Level6") return "Chapter VI - Blood in the Water";
        else if (sceneName == "Level7") return "Chapter VII - To Drown is to Cleanse";
        else if (sceneName == "Level8") return "Chapter VIII - His Name, Etched in Filth";
        else if (sceneName == "Level9") return "Chapter IX - No Heaven for Heretics";
        else if (sceneName == "Level10") return "Chapter X - Covenant of Ash";
        else if (sceneName == "Level11") return "Chapter XI - His Throat Was a Chapel";
        else if (sceneName == "Level12") return "Chapter XII - God is a Mirror";
        else if (sceneName == "Level13") return "Chapter XIII - The Throne of Thorns";
        else return "Unknown Area";
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

    private void PlayHoverSound()
    {
        if (audioSource != null && hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    private void PlayEnterSound()
    {
        if (audioSource != null && menuEnterSound != null)
            audioSource.PlayOneShot(menuEnterSound);
    }

    private void PlayBackSound()
    {
        if (audioSource != null && menuBackSound != null)
            audioSource.PlayOneShot(menuBackSound);
    }

    private void AddHoverSound(GameObject buttonObj)
    {
        EventTrigger trigger = buttonObj.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = buttonObj.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        entry.callback.AddListener((_) => PlayHoverSound());
        trigger.triggers.Add(entry);
    }

    private void AddHoverSoundsToMainMenuButtons()
    {
        Button[] buttons = mainMenuPanel.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            AddHoverSound(button.gameObject);
        }
    }

    [System.Serializable]
    private class SaveData
    {
        public string sceneName;
        // Add other fields as needed
    }
}
