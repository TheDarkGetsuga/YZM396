using System.Collections.Generic;
using UnityEngine;

public class VortexSphere : MonoBehaviour
{
    public float speed = 8f;
    public float vortexRadius = 5f;
    public float pullForce = 200f;
    public float vortexDuration = 3f;
    public float explosionDamage = 20f;
    public GameObject explosionEffectPrefab;

    [Header("Audio")]
    public AudioClip spawnSound;
    public AudioClip explosionSound;
    public float volume = 1f;

    private float vortexTimer;
    private bool isVortexActive = false;
    private Rigidbody2D rb;
    private List<Transform> enemiesInRange = new List<Transform>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        PlaySoundAtPosition(spawnSound, transform.position, volume);
        rb.linearVelocity = transform.right * speed;
        Invoke(nameof(StartVortex), 1f);
    }

    void StartVortex()
    {
        isVortexActive = true;
        vortexTimer = vortexDuration;
        rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        if (!isVortexActive) return;

        vortexTimer -= Time.deltaTime;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, vortexRadius);
        enemiesInRange.Clear();

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy") || HasTaggedChild(hit.transform, "Enemy"))
            {
                enemiesInRange.Add(hit.transform);
            }
        }

        foreach (Transform enemy in enemiesInRange)
        {
            if (enemy.TryGetComponent<Rigidbody2D>(out Rigidbody2D enemyRb))
            {
                Vector2 dir = (transform.position - enemy.position);
                float distance = dir.magnitude;
                if (distance > 0.1f)
                {
                    Vector2 directionToCenter = dir.normalized;
                    float strengthMultiplier = 1f - Mathf.Clamp01(distance / vortexRadius);
                    float finalForce = pullForce * strengthMultiplier;
                    enemyRb.AddForce(directionToCenter * finalForce, ForceMode2D.Force);
                    Debug.DrawLine(enemy.position, transform.position, Color.red);
                }
            }
        }

        if (vortexTimer <= 0)
        {
            Explode();
        }
    }

    void Explode()
    {
        foreach (Transform enemy in enemiesInRange)
        {
            var enemyScript = enemy.GetComponentInParent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(explosionDamage, transform.position, false, transform.position, false);
            }
            else
            {
                var basicEnemyScript = enemy.GetComponentInParent<BasicEnemy>();
                if (basicEnemyScript != null)
                {
                    basicEnemyScript.TakeDamage(explosionDamage, transform.position, false, transform.position, false);
                }
            }

            if (enemy.TryGetComponent<Rigidbody2D>(out Rigidbody2D enemyRb))
            {
                Vector2 pushDir = (enemy.position - transform.position).normalized;
                float explosionPushForce = 15;
                enemyRb.AddForce(pushDir * explosionPushForce, ForceMode2D.Impulse);
            }
        }

        if (explosionEffectPrefab)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        PlaySoundAtPosition(explosionSound, transform.position, volume);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, vortexRadius);
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

    // 🔧 Reusable tag check for children
    private bool HasTaggedChild(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
                return true;
        }
        return false;
    }
}
