using System.Collections.Generic;
using UnityEngine;

public class Shadowcloak : MonoBehaviour
{
    public float damage = 0.5f;
    public float duration = 15f;
    public float damageRate = 10f; // times per second

    private float damageInterval;
    private float damageTimer = 0f;

    private HashSet<Collider2D> enemiesInRange = new HashSet<Collider2D>();
    private CircleCollider2D triggerCollider;

    void Start()
    {
        triggerCollider = GetComponent<CircleCollider2D>();
        if (triggerCollider == null)
        {
            Debug.LogError("Shadowcloak: Missing CircleCollider2D!");
        }

        damageInterval = 1f / damageRate;
        Destroy(gameObject, duration); // Destroy after duration
    }

    void Update()
    {
        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            ApplyDamage();
            damageTimer = 0f;
        }
    }

    void ApplyDamage()
    {
        foreach (var enemyCollider in enemiesInRange)
        {
            if (enemyCollider == null) continue;

            Enemy enemy = enemyCollider.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, transform.position, false, transform.position, false);
                continue;
            }

            BasicEnemy basicEnemy = enemyCollider.GetComponentInParent<BasicEnemy>();
            if (basicEnemy != null)
            {
                basicEnemy.TakeDamage(damage, transform.position, false, transform.position, false);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || HasTaggedChild(other.transform, "Enemy"))
        {
            enemiesInRange.Add(other);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || HasTaggedChild(other.transform, "Enemy"))
        {
            enemiesInRange.Remove(other);
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
}
