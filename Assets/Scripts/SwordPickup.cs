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

    private void PickupSword() //this is on SwordPickup.cs which gives the swords to the player in-game
    {
        SwordSwing swordSwing = GameObject.FindGameObjectWithTag("Player").transform.Find("Sword").GetComponent<SwordSwing>();
        if (swordSwing != null)
        {
            swordSwing.AddSwordToInventory(swordToGive);

            // Only add to save if not already there
            if (!SaveManager.Instance.currentSave.obtainedSwordNames.Contains(swordToGive.swordName))
            {
                SaveManager.Instance.currentSave.obtainedSwordNames.Add(swordToGive.swordName);
                SaveManager.Instance.SaveGame(); // ✅ Correct way to call SaveGame
            }

            swordSwing.EquipSword(swordSwing.inventory.Count - 1);
            Destroy(gameObject);
        }
    }
}
