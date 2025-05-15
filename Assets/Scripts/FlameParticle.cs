using UnityEngine;

public class FlameParticle : MonoBehaviour
{
    public float lifetime = 1.5f;
    public int damage = 5;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, transform.position, false, transform.position, false);
            }
        }
    }
}
