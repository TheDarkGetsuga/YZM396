using UnityEngine;

public class AttackArea : MonoBehaviour
{
    public SwordSwing swordSwing; // Reference to the SwordSwing component to get the current sword data
    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Check if the collider is tagged as "Breakable"
        if (collider.CompareTag("Breakable"))
        {
            BreakableObject breakableObject = collider.GetComponent<BreakableObject>();
            if (breakableObject != null)
            {
                // Apply damage from the sword
                int damage = Mathf.RoundToInt(swordSwing.CurrentSword.Damage);
                breakableObject.TakeDamage(damage);
                Debug.Log($"Damage dealt to breakable object: {damage}");

                // Optionally, you can add effects like sparks or sounds here
            }
        }
    }
}
