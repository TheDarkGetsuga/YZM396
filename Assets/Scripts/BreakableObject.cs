using UnityEngine;
using System.Collections;

public class BreakableObject : MonoBehaviour
{
    public int health = 10; // Health of the breakable object
    public int minCoins = 3; // Minimum number of coins to drop
    public int maxCoins = 10; // Maximum number of coins to drop
    public GameObject coinPrefab; // Coin prefab to spawn when broken
    public float explosionForce = 5f; // Force to apply for the coins to explode outwards

    public AudioClip[] breakingSoundList; // List of breaking sounds
    public AudioClip[] damageSoundList; // List of damage sounds
    private AudioSource audioSource;

    private Rigidbody2D rb;

    void Start()
    {
        // Add Rigidbody2D if not present
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Optional AudioSource in case you want to play preview/test sounds
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (damageSoundList != null && damageSoundList.Length > 0)
        {
            AudioClip damageSound = damageSoundList[Random.Range(0, damageSoundList.Length)];
            PlaySound2D(damageSound, transform.position);
        }
        if (health <= 0)
        {
            BreakObject();
        }
    }

    private void BreakObject()
    {
        // Play breaking sound via a temporary GameObject
        if (breakingSoundList != null && breakingSoundList.Length > 0)
        {
            AudioClip breakSound = breakingSoundList[Random.Range(0, breakingSoundList.Length)];
            PlaySound2D(breakSound, transform.position);
        }

        // Spawn coins
        int randomCoinAmount = Random.Range(minCoins, maxCoins + 1);
        for (int i = 0; i < randomCoinAmount; i++)
        {
            Vector3 coinSpawnPosition = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            GameObject coin = Instantiate(coinPrefab, coinSpawnPosition, Quaternion.identity);

            Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();
            if (coinRb != null)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                coinRb.AddForce(randomDirection * explosionForce, ForceMode2D.Impulse);
            }
        }

        Destroy(gameObject);
    }

    private void PlaySound2D(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;

        GameObject soundObject = new GameObject("BreakableObject_Sound");
        soundObject.transform.position = position;

        AudioSource source = soundObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 1f;
        source.spatialBlend = 0f; // 2D sound
        source.Play();

        Destroy(soundObject, clip.length);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Sword"))
        {
            SwordSwing swordSwing = collision.gameObject.GetComponentInParent<SwordSwing>();
            if (swordSwing != null)
            {
                int swordDamage = Mathf.RoundToInt(swordSwing.CurrentSword.Damage);
                TakeDamage(swordDamage);
                Debug.Log($"Breakable object hit by sword, dealing {swordDamage} damage.");
            }
        }
    }
}
