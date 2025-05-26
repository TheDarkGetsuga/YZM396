using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Damage & Lifetime")]
    public float lifetime = 5f;
    public int damage = 1;

    [Header("Audio")]
    public AudioClip spawnSound;
    public AudioClip[] explosionSounds;

    [Header("Effects")]
    public GameObject explosionEffect;

    private int frameCount = 0;
    private int framesToIgnoreGround = 120;
    private int groundLayer;

    private void Start()
    {
        // Set ground layer index
        groundLayer = LayerMask.NameToLayer("Ground");

        // Play spawn sound via temporary 2D audio object
        if (spawnSound)
        {
            Play2DSound(spawnSound);
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Count frames since spawn
        frameCount++;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHP hp = other.GetComponent<PlayerHP>();
            if (hp != null)
            {
                hp.TakeDamage(damage, transform.position);
            }
            Explode();
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Environment"))
        {
            Explode();
        }
        else if (frameCount > framesToIgnoreGround && other.gameObject.layer == groundLayer)
        {
            Explode();
        }
    }
    private void Explode()
    {
        // Spawn explosion visual effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Play random explosion sound via temporary 2D object
        if (explosionSounds.Length > 0)
        {
            AudioClip randomClip = explosionSounds[Random.Range(0, explosionSounds.Length)];
            Play2DSound(randomClip);
        }

        Destroy(gameObject);
    }

    private void Play2DSound(AudioClip clip)
    {
        GameObject soundObj = new GameObject("Temp2DSound");
        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = 0.5f;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.Play();
        Destroy(soundObj, clip.length + 0.1f);
    }
}
