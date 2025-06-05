using UnityEngine;

public class FrostbreakerSpike : MonoBehaviour
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
    private Transform origin;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (rb == null || col == null)
        {
            Debug.LogError("FrostbreakerSpike: Missing Rigidbody2D or Collider2D.");
            Destroy(gameObject);
            return;
        }

        col.enabled = false;
        origin = transform.parent;

        if (origin != null)
            launchDirection = (transform.position - origin.position).normalized;

        PlaySound(spawnSound);
        Invoke(nameof(Launch), launchDelay);
    }

    void Launch()
    {
        transform.parent = null;
        col.enabled = true;
        rb.linearVelocity = launchDirection * launchSpeed;
        launched = true;
        Destroy(gameObject, maxLifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!launched) return;

        if (other.CompareTag("Enemy") || HasTaggedChild(other.transform, "Enemy"))
        {
            var enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(20, transform.position, false, transform.position, false);

            var basicEnemy = other.GetComponentInParent<BasicEnemy>();
            if (basicEnemy != null)
                basicEnemy.TakeDamage(20, transform.position, false, transform.position, false);

            Explode();
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Environment") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Explode();
        }
    }

    void Explode()
    {
        if (explosionEffectPrefab)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        if (explosionSounds != null && explosionSounds.Length > 0)
        {
            int index = Random.Range(0, explosionSounds.Length);
            PlaySound(explosionSounds[index]);
        }

        Destroy(gameObject);
    }

    bool HasTaggedChild(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag)) return true;
        }
        return false;
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
