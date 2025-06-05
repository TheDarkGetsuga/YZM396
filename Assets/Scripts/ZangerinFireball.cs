using UnityEngine;

public class ZangerinFireball : MonoBehaviour
{
    public float delayBeforeLaunch = 1f;
    public float speed = 10f;
    public float seekRadius = 10f;
    public GameObject explosionPrefab;

    [Header("Audio")]
    public AudioClip spawnSound;
    public AudioClip[] explosionSounds;
    public float volume = 1f;

    private Transform target;
    private Vector2 launchDirection;
    private bool hasLaunched = false;

    void Start()
    {
        PlaySoundAtPosition(spawnSound, transform.position, volume);
        Invoke(nameof(FindTargetAndLaunch), delayBeforeLaunch);
    }

    void FindTargetAndLaunch()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, seekRadius);
        float closestDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    target = hit.transform;
                }
            }
        }

        if (target != null)
        {
            launchDirection = (target.position - transform.position).normalized;
        }
        else
        {
            float angle = Random.Range(0f, 360f);
            launchDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
        }

        hasLaunched = true;
    }

    void Update()
    {
        if (hasLaunched)
        {
            transform.position += (Vector3)(launchDirection * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ground") && !other.CompareTag("Enemy") && !HasTaggedChild(other.transform, "Enemy"))
            return;
        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        if (explosionSounds != null && explosionSounds.Length > 0)
        {
            int index = Random.Range(0, explosionSounds.Length);
            PlaySoundAtPosition(explosionSounds[index], transform.position, volume);
        }
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(10, transform.position, false, transform.position, false);
        }
        else
        {
            BasicEnemy basicEnemy = other.GetComponentInParent<BasicEnemy>();
            if (basicEnemy != null)
            {
                basicEnemy.TakeDamage(10, transform.position, false, transform.position, false);
            }
        }

        Destroy(gameObject);
    }
    private bool HasTaggedChild(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
                return true;
        }
        return false;
    }
    void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;

        GameObject soundObj = new GameObject("TempAudio_" + clip.name);
        soundObj.transform.position = position;

        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.spatialBlend = 1f; // Fully 3D
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 20f;
        audioSource.Play();

        Destroy(soundObj, clip.length);
    }
}
