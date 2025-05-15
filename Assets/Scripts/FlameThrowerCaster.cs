using UnityEngine;
using System.Collections;

public class FlamethrowerCaster : MonoBehaviour
{
    public GameObject flamePrefab;
    public Transform spawnTransform; // Updated from static Vector3 to dynamic Transform
    public bool facingRight = true;

    public void BeginCasting(float duration)
    {
        StartCoroutine(Flamethrower(duration));
    }

    IEnumerator Flamethrower(float duration)
    {
        float interval = 0.05f;
        float timer = 0f;

        while (timer < duration)
        {
            SpawnFlame();
            SpawnFlame();
            SpawnFlame();

            timer += interval;
            yield return new WaitForSeconds(interval);
        }

        Destroy(gameObject); // Self-destruct after casting
    }

    void SpawnFlame()
    {
        if (spawnTransform == null || flamePrefab == null) return;

        GameObject flame = Instantiate(flamePrefab, spawnTransform.position, Quaternion.identity);
        Rigidbody2D rb = flame.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 baseDirection = facingRight ? Vector2.right : Vector2.left;
            float angleOffset = Random.Range(-15f, 15f);
            float angleRad = angleOffset * Mathf.Deg2Rad;

            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);

            Vector2 rotated = new Vector2(
                baseDirection.x * cos - baseDirection.y * sin,
                baseDirection.x * sin + baseDirection.y * cos
            );

            rotated.y += Random.Range(0.1f, 0.3f);
            rb.linearVelocity = rotated.normalized * Random.Range(3f, 5f);
        }
    }
}
