using UnityEngine;

public class Lever : MonoBehaviour
{
    public bool isActivated = false;
    public Door[] connectedDoors;

    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            ToggleLever();
        }
    }

    private void ToggleLever()
    {
        isActivated = !isActivated;

        // Flip the lever's visual (scaleX: 1 = off, -1 = on)
        Vector3 newScale = transform.localScale;
        newScale.x = isActivated ? -1 : 1;
        transform.localScale = newScale;

        // Notify all connected doors to check lever states
        foreach (Door door in connectedDoors)
        {
            door.CheckLevers();
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
