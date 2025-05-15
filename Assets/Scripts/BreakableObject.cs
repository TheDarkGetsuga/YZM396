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
    private AudioSource audioSource;

    // Add Rigidbody2D if this object should be affected by collisions with physics
    private Rigidbody2D rb;

    void Start()
    {
        // Add a Rigidbody2D if it doesn't have one
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // Use bodyType instead of isKinematic
        }

        // Get the AudioSource component if it exists
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Take damage from a collision or other event
    public void TakeDamage(int damage)
    {
        health -= damage;

        // If health drops to 0 or below, break the object
        if (health <= 0)
        {
            BreakObject();
        }
    }

    private void BreakObject()
    {
        // Play a random breaking sound
        if (breakingSoundList != null && breakingSoundList.Length > 0 && audioSource != null)
        {
            AudioClip breakSound = breakingSoundList[Random.Range(0, breakingSoundList.Length)];
            audioSource.PlayOneShot(breakSound);
        }

        // Generate a random number of coins between minCoins and maxCoins
        int randomCoinAmount = Random.Range(minCoins, maxCoins + 1);

        // Spawn coins
        for (int i = 0; i < randomCoinAmount; i++)
        {
            // Adjust the spawn position to ensure coins spawn slightly above the ground
            Vector3 coinSpawnPosition = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z); // Adjust Y-position

            GameObject coin = Instantiate(coinPrefab, coinSpawnPosition, Quaternion.identity);

            // Apply random outward force to each coin
            Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();
            if (coinRb != null)
            {
                // Apply explosion force in a random direction
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                coinRb.AddForce(randomDirection * explosionForce, ForceMode2D.Impulse);
            }
        }

        // Destroy the breakable object
        Destroy(gameObject);
    }

    // Coroutine to enable the coin's collider after a delay
    private IEnumerator EnableCoinColliderAfterDelay(GameObject coin, float delay)
    {
        yield return new WaitForSeconds(delay);
        coin.GetComponent<Collider2D>().enabled = true;
    }

    // Detect if the breakable object is hit by a sword (check for sword tag)
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the object hit is a sword (assuming sword has the tag "Sword")
        if (collision.gameObject.CompareTag("Sword"))
        {
            // Example damage applied from sword (You may modify this logic based on your damage system)
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
