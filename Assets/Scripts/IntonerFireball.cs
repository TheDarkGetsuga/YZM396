using UnityEngine;

public class IntonerFireball : MonoBehaviour
{
    public float speed = 12f;
    public GameObject explosionPrefab;

    [Header("Audio")]
    public AudioClip spawnSound;
    public AudioClip[] explosionSounds;
    public float volume = 1f;

    private Vector2 direction;
    private bool launched = false;

    void Start()
    {
        PlaySoundAtPosition(spawnSound, transform.position, volume);
        FindTargetAndLaunch();
    }

    void FindTargetAndLaunch()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = enemy.transform;
            }
        }

        if (closest != null)
        {
            direction = (closest.position - transform.position).normalized;
        }
        else
        {
            float angle = Random.Range(0f, 360f);
            direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
        }

        launched = true;
    }

    void Update()
    {
        if (launched)
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy") && !HasTaggedChild(other.transform, "Enemy"))
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
            enemy.TakeDamage(30, transform.position, false, transform.position, false);
        else
        {
            BasicEnemy basicEnemy = other.GetComponentInParent<BasicEnemy>();
            if (basicEnemy != null)
                basicEnemy.TakeDamage(30, transform.position, false, transform.position, false);
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
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 20f;
        audioSource.Play();

        Destroy(soundObj, clip.length);
    }
}
