using UnityEngine;

public class SoulswordProjectile : MonoBehaviour
{
    public float delayBeforeHoming = 0.35f;
    public float speed = 40f;
    public float seekRadius = 12f;
    public float rotationOffset = -45f;
    public GameObject impactEffectPrefab;

    [Header("Audio")]
    public AudioClip spawnSound;
    public AudioClip[] impactSounds;
    public float volume = 1f;

    private Transform target;
    private Vector2 moveDirection;
    private bool isHoming = false;
    private Collider2D projectileCollider;

    void Start()
    {
        PlaySoundAtPosition(spawnSound, transform.position, volume);

        projectileCollider = GetComponent<Collider2D>();
        if (projectileCollider != null)
            projectileCollider.enabled = false; // Disable collisions until ready

        Invoke(nameof(StartHoming), delayBeforeHoming);
    }

    void StartHoming()
    {
        if (projectileCollider != null)
            projectileCollider.enabled = true;

        // Find closest enemy
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, seekRadius);
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    target = hit.transform;
                }
            }
        }

        if (target != null)
        {
            moveDirection = (target.position - transform.position).normalized;
            RotateTowards(moveDirection);
        }
        else
        {
            moveDirection = transform.right.normalized;
        }

        isHoming = true;
    }

    void Update()
    {
        if (isHoming)
        {
            transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ground") && !other.CompareTag("Enemy") && !HasTaggedChild(other.transform, "Enemy"))
            return;

        if (impactEffectPrefab)
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);

        if (impactSounds != null && impactSounds.Length > 0)
        {
            int index = Random.Range(0, impactSounds.Length);
            PlaySoundAtPosition(impactSounds[index], transform.position, volume);
        }

        Enemy enemy = other.GetComponentInParent<Enemy>(); 
        if (enemy != null)
        {
            enemy.TakeDamage(15, transform.position, false, transform.position, false);
        }
        else
        {
            BasicEnemy basicEnemy = other.GetComponentInParent<BasicEnemy>();
            if (basicEnemy != null)
            {
                basicEnemy.TakeDamage(15, transform.position, false, transform.position, false);
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

    private void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += rotationOffset;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;

        GameObject soundObj = new GameObject("TempAudio_" + clip.name);
        soundObj.transform.position = position;

        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 20f;
        audioSource.Play();

        Destroy(soundObj, clip.length);
    }
}
