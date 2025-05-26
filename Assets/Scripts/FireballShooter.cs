using UnityEngine;

public class FireballShooter : MonoBehaviour
{
    public GameObject fireballPrefab;
    public float fireballSpeed = 10f;

    private void Update()
    {
        if (!CameraFollow.Instance.IsInBossFightMode())
            return;

        if (FireballShooterManager.Instance != null && FireballShooterManager.Instance.shouldShoot)
        {
            ShootUpward();
        }
    }

    private void ShootUpward()
    {
        if (fireballPrefab == null)
            return;

        GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = fireball.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.up * fireballSpeed;
        }
    }
}
