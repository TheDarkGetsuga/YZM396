using UnityEngine;

public class EnemyDMG : MonoBehaviour
{
    public int damage;
    public PlayerHP playerHP;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get the PlayerHP component from the collided player (not the serialized field)
            PlayerHP hp = collision.gameObject.GetComponent<PlayerHP>();
            if (hp != null)
            {
                hp.TakeDamage(damage, transform.position);
            }
        }
    }
}
