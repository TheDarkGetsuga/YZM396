using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] levelMusicClips; // Array of music clips for each scene
    private AudioSource audioSource;

    private static AudioManager instance;
    private int currentlyPlayingIndex = -1; // Tracks which music clip is playing

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        PlayMusicForCurrentScene();
    }

    void OnLevelWasLoaded(int level)
    {
        currentlyPlayingIndex = -1; // ✅ Reset when loading a new scene
        PlayMusicForCurrentScene();
    }

    private void PlayMusicForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Menu") PlayMusicClip(15);
        else if (sceneName == "Level1") PlayMusicClip(1);
        else if (sceneName == "Level2") PlayMusicClip(2);
        else if (sceneName == "Level3") PlayMusicClip(3);
        else if (sceneName == "Level4") PlayMusicClip(4);
        else if (sceneName == "Level5") PlayMusicClip(6);
        else if (sceneName == "Level6") PlayMusicClip(7);
        else if (sceneName == "Level7") PlayMusicClip(8);
        else if (sceneName == "Level8") PlayMusicClip(9);
        else if (sceneName == "Level9") PlayMusicClip(11);
        else if (sceneName == "Level10") PlayMusicClip(11);
        else if (sceneName == "Level11") PlayMusicClip(11);
        else if (sceneName == "Level12") PlayMusicClip(11);
        else if (sceneName == "Level13") PlayMusicClip(13);
        else if (sceneName == "Credits") PlayMusicClip(0);
    }
    public void FadeOutMusic(float duration)
    {
    StartCoroutine(FadeOutCoroutine(duration));
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = audioSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = startVolume; // Reset volume for next scene
    }
    public void PlayMusicClip(int index)
    {
        // Ignore if the requested clip is already playing
        if (index == currentlyPlayingIndex)
            return;

        if (index >= 0 && index < levelMusicClips.Length)
        {
            audioSource.clip = levelMusicClips[index];
            audioSource.loop = true;
            audioSource.Play();
            currentlyPlayingIndex = index;
        }
        else
        {
            Debug.LogWarning("Music clip index out of range: " + index);
        }
    }
}
