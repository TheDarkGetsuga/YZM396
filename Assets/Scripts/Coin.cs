using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;
    public float pickupDelay = 0.2f;
    private bool canBePickedUp = false;

    public float drag = 2f; // Set this to control how much friction the coin has
    public float angularDrag = 5f; // Set this to control how much the coin slows down rotating

    private Rigidbody2D rb;

    private void Start()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();

        // Set drag and angular drag
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;

        // Enable trigger-based pickup after delay
        Invoke(nameof(EnablePickup), pickupDelay);

        // Ignore physical collision with the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider2D[] coinColliders = GetComponents<Collider2D>();
            Collider2D[] playerColliders = player.GetComponents<Collider2D>();

            foreach (var coinCol in coinColliders)
            {
                if (coinCol.isTrigger) continue; // Only ignore physical colliders

                foreach (var playerCol in playerColliders)
                {
                    if (!playerCol.isTrigger)
                    {
                        Physics2D.IgnoreCollision(coinCol, playerCol);
                    }
                }
            }
        }
    }

    private void EnablePickup()
    {
        canBePickedUp = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canBePickedUp) return;

        if (collision.CompareTag("Player"))
        {
            CoinInventory coinInventory = collision.GetComponent<CoinInventory>();
            if (coinInventory != null)
            {
                coinInventory.AddCoins(value);
            }

            Destroy(gameObject);
        }
    }
}
