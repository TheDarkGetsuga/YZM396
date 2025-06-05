using UnityEngine;

public class SwordPickup : MonoBehaviour
{
    public SwordData swordToGive;
    private bool isPlayerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player entered pickup range.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("Player exited pickup range.");
        }
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F)) // Note to self: Use Unity Input System for console support next time
        {
            Debug.Log("F key pressed, attempting to pickup sword...");
            PickupSword();
        }
    }

    private void PickupSword()
    {
        SwordSwing swordSwing = GameObject.FindGameObjectWithTag("Player").transform.Find("Sword").GetComponent<SwordSwing>();
        if (swordSwing != null)
        {
            swordSwing.AddSwordToInventory(swordToGive);

            // Only add to save if not already there
            if (!SaveManager.Instance.currentSave.obtainedSwordNames.Contains(swordToGive.swordName))
            {
                SaveManager.Instance.currentSave.obtainedSwordNames.Add(swordToGive.swordName);
                SaveManager.Instance.SaveGame();
            }

            swordSwing.EquipSword(swordSwing.inventory.Count - 1);
            Destroy(gameObject);
        }
    }
}
