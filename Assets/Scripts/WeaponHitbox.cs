using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    private float baseDamage = 10f;
    private SwordData swordData;
    private bool canHit = false;

    public void SetSwordData(SwordData data)
    {
        swordData = data;
        baseDamage = swordData.Damage;
    }

    public void EnableHitbox()
    {
        canHit = true;
    }

    public void DisableHitbox()
    {
        canHit = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canHit) return;

        Vector3 hitPoint = collision.ClosestPoint(transform.position);
        Vector3 source = transform.root.position; // Assuming root is the player
        bool isWeakPoint = collision.gameObject.name == "WeakPoint";

        // Try Enemy first
        Enemy enemy = collision.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(baseDamage, hitPoint, isWeakPoint, source, true);
            return;
        }

        // Then try BasicEnemy
        BasicEnemy basicEnemy = collision.GetComponentInParent<BasicEnemy>();
        if (basicEnemy != null)
        {
            basicEnemy.TakeDamage(baseDamage, hitPoint, isWeakPoint, source, true);
        }
    }
}
