using UnityEngine;

public class SwordPickup : MonoBehaviour
{
    public SwordData swordToGive; // The sword data to assign to the player
    private bool isPlayerInRange = false; // To track if the player is in range of the pickup

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true; // Player is in range of the pickup
            Debug.Log("Player entered pickup range.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false; // Player has left the pickup range
            Debug.Log("Player exited pickup range.");
        }
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F)) // If player presses F while in range
        {
            Debug.Log("F key pressed, attempting to pickup sword...");
            PickupSword();
        }
    }

    private void PickupSword()
    {
        // Find the SwordSwing script on the Sword child object of the Player
        SwordSwing swordSwing = GameObject.FindGameObjectWithTag("Player").transform.Find("Sword").GetComponent<SwordSwing>();
        if (swordSwing != null)
        {
            Debug.Log("SwordSwing found on Sword (child of Player).");

            // Add the sword to the inventory
            swordSwing.AddSwordToInventory(swordToGive); 
            Debug.Log($"Sword {swordToGive.swordName} added to inventory.");

            // Equip the newly added sword
            swordSwing.EquipSword(swordSwing.inventory.Count - 1);
            Debug.Log($"Sword {swordToGive.swordName} equipped.");

            // Destroy the pickup object
            Destroy(gameObject); 
            Debug.Log("Sword pickup destroyed.");
        }
        else
        {
            Debug.LogError("SwordSwing script not found on Sword (child of Player).");
        }
    }
}
