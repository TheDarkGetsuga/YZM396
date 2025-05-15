using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!activated && other.CompareTag("Player"))
        {
            PlayerHP playerHP = other.GetComponent<PlayerHP>();
            if (playerHP != null)
            {
                playerHP.SetCheckpoint(transform.position);
                activated = true;
            }
        }
    }
}
