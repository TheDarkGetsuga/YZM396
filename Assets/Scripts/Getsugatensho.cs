using UnityEngine;
using System.Collections.Generic;

public class Getsugatensho : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public bool isFacingRight = true;

    [Header("Lifetime")]
    public float maxLifetime = 3f;

    [Header("Effects")]
    public GameObject hitEffectPrefab;
    public List<AudioClip> hitSounds = new List<AudioClip>();
    public AudioClip[] explosionSounds;
    public float screenShakeX = 1.5f;
    public float screenShakeY = 1.5f;
    public float shakeDuration = 0.2f;

    [Header("Damage")]
    public int damage = 15;
    private bool hitstop = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = (isFacingRight ? Vector2.right : Vector2.left) * speed;

        Destroy(gameObject, maxLifetime);
    }

    public void SetDirection(bool facingRight)
    {
        isFacingRight = facingRight;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1 : -1);
        transform.localScale = scale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        bool hit = false;
        hitstop = false;

        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                hitstop = true;
                enemy.TakeDamage(damage, transform.position, false, transform.position, false);
                hit = true;
            }

            BasicEnemy basicEnemy = other.GetComponentInParent<BasicEnemy>();
            if (basicEnemy != null)
            {
                hitstop = true;
                basicEnemy.TakeDamage(damage, transform.position, false, transform.position, false);
                hit = true;
            }
        }
        else
        {
            // This section exists solely for the final boss to function properly
            Enemy enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null && HasTaggedChild(enemy.transform, "Enemy"))
            {
                hitstop = true;
                enemy.TakeDamage(damage, transform.position, false, transform.position, false);
                hit = true;
            }
            BasicEnemy basicEnemy = other.GetComponentInParent<BasicEnemy>();
            if (basicEnemy != null && HasTaggedChild(basicEnemy.transform, "Enemy"))
            {
                hitstop = true;
                basicEnemy.TakeDamage(damage, transform.position, false, transform.position, false);
                hit = true;
            }
        }
        if (hit)
        {
            TriggerEffects();
            Destroy(gameObject);
            return;
        }
        if (!other.isTrigger)
        {
            TriggerEffects();
            Destroy(gameObject);
        }
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
    void TriggerEffects()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        if (hitSounds.Count > 0)
        {
            int index = Random.Range(0, hitSounds.Count);
            PlaySoundAtPosition(hitSounds[index], transform.position, 3f);
            int index2 = Random.Range(0, explosionSounds.Length);
            PlaySoundAtPosition(explosionSounds[index], transform.position, 1f);
        }

        CameraFollow.Instance?.ScreenShake(screenShakeX, screenShakeY, shakeDuration);

        if (hitstop)
        {
            HitstopManager.Instance.TriggerHitstop(0.1f);
            hitstop = false;
        }
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
