using UnityEngine;

public class FlameParticle : MonoBehaviour
{
    public float lifetime = 1.5f;
    public int damage = 5;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other) //In hindsight maybe i shouldnt have two seperate fucking enemy scripts but it works
    {   
        if (other.CompareTag("Enemy"))
        {
            // Try Enemy type first
            Enemy enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                Debug.Log("Sending damage to Enemy: " + enemy.name);
                enemy.TakeDamage(damage, transform.position, false, transform.position, false);
                return;
            }

            // Try BasicEnemy type if Enemy wasn't found
            BasicEnemy basicEnemy = other.GetComponentInParent<BasicEnemy>();
            if (basicEnemy != null)
            {
                Debug.Log("Sending damage to BasicEnemy: " + basicEnemy.name);
                basicEnemy.TakeDamage(damage, transform.position, false, transform.position, false);
                return;
            }
        }
    }
}
