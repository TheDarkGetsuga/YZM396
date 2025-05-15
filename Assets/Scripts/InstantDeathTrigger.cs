using UnityEngine;

public class InstantDeathTrigger : MonoBehaviour
{
    public int deathDamage = 500;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHP playerHP = other.GetComponent<PlayerHP>();
            if (playerHP == null)
            {
                // Try to find on children (like BreakableObjectCheck)
                playerHP = other.GetComponentInParent<PlayerHP>();
            }

            if (playerHP != null)
            {
                playerHP.TakeDamage(deathDamage, transform.position);
                Debug.Log("Player took instant death damage.");
            }
        }
    }
}
