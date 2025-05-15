using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    public string keyName; // Example: "BronzeKey", "RedKey"
    public PlayerKeyInventory keyInventory;  // Reference to PlayerKeyInventory component

    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (keyInventory != null)
            {
                keyInventory.AddKey(keyName);
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("PlayerKeyInventory reference is not set!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
