using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] levelMusicClips;
    private AudioSource audioSource;

    private static AudioManager instance;
    private int currentlyPlayingIndex = -1;

    private bool isMuted = false;
    private float savedVolume = 1f;

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
            return;
        }

        audioSource = GetComponent<AudioSource>();
        savedVolume = audioSource.volume;
    }

    void Start()
    {
        PlayMusicForCurrentScene();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMute();
        }
    }

    void OnLevelWasLoaded(int level)
    {
        currentlyPlayingIndex = -1;
        PlayMusicForCurrentScene();
    }

    private void PlayMusicForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        //Note to self, find a better fucking way to do this
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
        float startVolume = isMuted ? savedVolume : audioSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float targetVolume = Mathf.Lerp(startVolume, 0f, t / duration);
            audioSource.volume = isMuted ? 0f : targetVolume;
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = isMuted ? 0f : savedVolume;
    }

    public void PlayMusicClip(int index)
    {
        if (index == currentlyPlayingIndex)
            return;

        if (index >= 0 && index < levelMusicClips.Length)
        {
            audioSource.clip = levelMusicClips[index];
            audioSource.loop = true;
            audioSource.Play();
            currentlyPlayingIndex = index;
            audioSource.volume = isMuted ? 0f : savedVolume;
        }
        else
        {
            Debug.LogWarning("Music clip index out of range: " + index);
        }
    }

    private void ToggleMute()
    {
        isMuted = !isMuted;
        audioSource.volume = isMuted ? 0f : savedVolume;
    }
}
