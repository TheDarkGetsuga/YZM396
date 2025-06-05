using UnityEngine;

public class TitanfallCleave : MonoBehaviour
{
    public float damage = 30f;
    public float lifetime = 0.3f;
    public float pushForce = 10f;

    void Start()
    {
        // Detach from parent to freeze position
        transform.parent = null;
        DoCleaveDamage();
        Destroy(gameObject, lifetime);
    }

    void DoCleaveDamage()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null)
        {
            Debug.LogWarning("TitanfallCleave: No BoxCollider2D found.");
            return;
        }

        Vector2 center = (Vector2)transform.position + box.offset;
        Vector2 size = box.size;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy") || HasTaggedChild(hit.transform, "Enemy"))
            {
                Enemy enemy = hit.GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, transform.position, false, transform.position, false);
                }
                else
                {
                    BasicEnemy basicEnemy = hit.GetComponentInParent<BasicEnemy>();
                    if (basicEnemy != null)
                    {
                        basicEnemy.TakeDamage(damage, transform.position, false, transform.position, false);
                    }
                }
                Rigidbody2D enemyRb = hit.GetComponentInParent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 direction = (enemyRb.transform.position - transform.position).normalized;
                    enemyRb.AddForce(direction * pushForce, ForceMode2D.Impulse);
                }
            }
        }
    }
    private bool HasTaggedChild(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
                return true;
        }
        return false;
    }
    void OnDrawGizmosSelected()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) return;

        Gizmos.color = Color.red;
        Vector2 center = (Vector2)transform.position + box.offset;
        Gizmos.DrawWireCube(center, box.size);
    }
}
