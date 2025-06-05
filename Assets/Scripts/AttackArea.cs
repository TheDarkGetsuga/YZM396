using UnityEngine;

public class AttackArea : MonoBehaviour
{
    public SwordSwing swordSwing;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Breakable"))
        {
            BreakableObject breakableObject = collider.GetComponent<BreakableObject>();
            if (breakableObject != null)
            {
                int damage = Mathf.RoundToInt(swordSwing.CurrentSword.Damage);
                breakableObject.TakeDamage(damage);
                Debug.Log($"Damage dealt to breakable object: {damage}");
            }
        }
    }
}
