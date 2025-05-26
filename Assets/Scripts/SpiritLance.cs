using UnityEngine;

public class SpiritLance : MonoBehaviour
{
    [Header("Launch Settings")]
    public float launchDelay = 1.5f;
    public float launchSpeed = 12f;
    public float maxLifetime = 5f;

    [Header("Effects")]
    public AudioClip spawnSound;
    public AudioClip[] explosionSounds;
    public GameObject explosionEffectPrefab;
    public float soundVolume = 1f;

    private Vector2 launchDirection;
    private bool launched = false;
    private Rigidbody2D rb;
    private Collider2D col;
    private Transform parentBeforeLaunch;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (rb == null || col == null)
        {
            Debug.LogError("SpiritLance: Missing Rigidbody2D or Collider2D.");
            Destroy(gameObject);
            return;
        }

        // Disable collision initially
        col.enabled = false;

        // Save parent in case we want to detach later
        parentBeforeLaunch = transform.parent;

        // Determine outward direction (away from center)
        Vector2 offsetFromCenter = (transform.position - parentBeforeLaunch.position).normalized;
        launchDirection = offsetFromCenter;

        // Play spawn sound
        PlaySound(spawnSound);

        // Start delayed launch
        Invoke(nameof(Launch), launchDelay);
    }

    void Launch()
    {
        // Detach from parent
        transform.parent = null;

        // Enable collision
        col.enabled = true;

        // Apply velocity
        rb.linearVelocity = launchDirection * launchSpeed;

        launched = true;

        // Auto-destroy if no impact after maxLifetime
        Destroy(gameObject, maxLifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!launched) return;

        if (other.CompareTag("Player"))
        {
            PlayerHP hp = other.GetComponent<PlayerHP>();
            if (hp != null)
            {
                hp.TakeDamage(1, transform.position); // Change damage as needed
            }
        
            Explode();
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Environment") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Explode();
        }
    }

    void Explode()
    {
        // Explosion visual
        if (explosionEffectPrefab)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        // Play random explosion sound
        if (explosionSounds != null && explosionSounds.Length > 0)
        {
            int index = Random.Range(0, explosionSounds.Length);
            PlaySound(explosionSounds[index]);
        }

        Destroy(gameObject);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        GameObject soundObj = new GameObject("TempAudio_" + clip.name);
        soundObj.transform.position = transform.position;

        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = soundVolume;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 20f;
        audioSource.Play();

        Destroy(soundObj, clip.length);
    }
}
