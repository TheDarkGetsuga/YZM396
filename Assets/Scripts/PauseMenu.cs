using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private Canvas pauseCanvas;
    private bool isPaused = false;

    void Awake()
    {
        pauseCanvas = GetComponent<Canvas>();
        if (pauseCanvas == null)
            Debug.LogError("PauseMenu script must be attached to a Canvas!");

        pauseCanvas.enabled = false; // Hide at start

        // Find the Return to Main Menu button and hook it up
        Button returnButton = GetComponentInChildren<Button>();
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(ReturnToMainMenu);
        }
        else
        {
            Debug.LogError("No Button found inside PauseMenu Canvas!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void PauseGame()
    {
        pauseCanvas.enabled = true;
        Time.timeScale = 0f; // Pause game time
        isPaused = true;
    }

    void ResumeGame()
    {
        pauseCanvas.enabled = false;
        Time.timeScale = 1f; // Resume game time
        isPaused = false;
    }

    void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Make sure time is normal before loading scene
        SceneManager.LoadScene("Menu");
    }
}
