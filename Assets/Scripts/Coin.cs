using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour
{
    public int value = 1;
    public float pickupDelay = 0.2f;
    private bool canBePickedUp = false;

    public float drag = 2f;
    public float angularDrag = 5f;

    private Rigidbody2D rb;
    private static GameObject player;
    private static CoinInventory coinInventory;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;

        Invoke(nameof(EnablePickup), pickupDelay);

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                coinInventory = player.GetComponent<CoinInventory>();
            }
        }

        if (player != null)
        {
            Collider2D[] coinColliders = GetComponents<Collider2D>();
            Collider2D[] playerColliders = player.GetComponentsInChildren<Collider2D>();

            foreach (var coinCol in coinColliders)
            {
                if (coinCol.isTrigger) continue;

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

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!canBePickedUp) return;

        if (collision.transform.root.CompareTag("Player"))
        {
            TryCollect();
        }
    }

    private void TryCollect() //this lags too much
    {
        if (coinInventory != null)
        {
            coinInventory.AddCoins(value);
        }
        GetComponent<Collider2D>().enabled = false;
        if (rb != null) rb.simulated = false;
        StartCoroutine(DestroyDelayed()); //todo: use pooling
    }

    private IEnumerator DestroyDelayed()
    {
        yield return null; // 1 frame delay here
        Destroy(gameObject);
    }
}
