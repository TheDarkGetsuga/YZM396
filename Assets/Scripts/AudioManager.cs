using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] levelMusicClips; // Array of music clips for each scene
    private AudioSource audioSource;

    private static AudioManager instance;

    void Awake()
    {
        // Ensure there is only one instance of AudioManager (persistent across scenes)
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Don't destroy this object when changing scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicates
        }

        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Play the appropriate music based on the current scene
        PlayMusicForCurrentScene();
    }

    void OnLevelWasLoaded(int level)
    {
        // Play the music when a new level is loaded
        PlayMusicForCurrentScene();
    }

    private void PlayMusicForCurrentScene()
    {
        // Get the scene's name
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Level1")
        {
            audioSource.clip = levelMusicClips[1];
            audioSource.loop = true;
            audioSource.Play();
        }
        if (sceneName == "Level2")
        {
            audioSource.clip = levelMusicClips[2];
            audioSource.loop = true;
            audioSource.Play();
        }
        if (sceneName == "Level3")
        {
            audioSource.clip = levelMusicClips[3];
            audioSource.loop = true;
            audioSource.Play();
        }
        if (sceneName == "Level4")
        {
            audioSource.clip = levelMusicClips[4];
            audioSource.loop = true;
            audioSource.Play();
        }
        if (sceneName == "Level5")
        {
            audioSource.clip = levelMusicClips[5];
            audioSource.loop = true;
            audioSource.Play();
        }
        if (sceneName == "Level6")
        {
            audioSource.clip = levelMusicClips[6];
            audioSource.loop = true;
            audioSource.Play();
        }
        if (sceneName == "Level7")
        {
            audioSource.clip = levelMusicClips[7];
            audioSource.loop = true;
            audioSource.Play();
        }
        
    }
}
