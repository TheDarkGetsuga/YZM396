using UnityEngine;

public class Lever : MonoBehaviour
{
    public bool isActivated = false;
    public Door[] connectedDoors;
    [SerializeField] private bool trackDoors = true;
    [SerializeField] private bool trackMovingDoor = true;
    [Header("Audio")]
    public AudioClip leverSound;
    public float soundVolume = 1f;
    private AudioSource audioSource;

    private bool playerInRange = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            ToggleLever();
        }
    }

    private void ToggleLever()
    {
        isActivated = !isActivated;

        //scale should be 1 by default 
        Vector3 newScale = transform.localScale;
        newScale.x = isActivated ? -1 : 1;
        transform.localScale = newScale;

        if (leverSound != null)
        {
            audioSource.volume = soundVolume;
            audioSource.clip = leverSound;
            audioSource.Play();
        }

        foreach (Door door in connectedDoors)
        {
            door.CheckLevers();

            // Smooth camera pan
            if (CameraFollow.Instance != null && trackDoors)
            {
                CameraFollow.Instance.FocusTemporarilyOnTarget(door.transform, 1f, 5f, trackMovingDoor);
            }
        }
        
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
