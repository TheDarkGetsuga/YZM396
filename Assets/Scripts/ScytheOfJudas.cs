using UnityEngine;

public class ScytheOfJudas : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float startSpeed = 900f;    // Initial rotation speed (deg/sec)
    public float normalSpeed = 200f;   // Final rotation speed (deg/sec)
    public float speedDecayDuration = 1f; // Duration in seconds to slow down

    [Header("Damage Settings")]
    public int damageAmount = 30;

    private float angle;              // Current orbit angle in degrees
    private float orbitRadius;        // Distance from player (local)
    private float rotationSpeed;      // Current rotation speed
    private float decayTimer = 0f;    // Timer to track decay progress

    // Called by SwordSwing.cs to set starting position on the orbit
    public void SetInitialOrbit(float initialAngle, float radius)
    {
        angle = initialAngle;
        orbitRadius = radius;
        decayTimer = 0f;
        rotationSpeed = startSpeed;

        UpdatePosition();
    }

    void Update()
    {
        // Handle speed decay (smooth curve)
        if (decayTimer < speedDecayDuration)
        {
            decayTimer += Time.deltaTime;
            float t = Mathf.Clamp01(decayTimer / speedDecayDuration);

            // Use a smooth step for steep but smooth decay
            t = Mathf.SmoothStep(0f, 1f, t);

            rotationSpeed = Mathf.Lerp(startSpeed, normalSpeed, t);
        }
        else
        {
            rotationSpeed = normalSpeed;
        }

        // Increase angle by current rotation speed
        angle += rotationSpeed * Time.deltaTime;
        if (angle >= 360f)
            angle -= 360f;

        UpdatePosition();
    }
    void UpdatePosition()
    {
        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;

        // Set world position relative to player position + offset
        if (transform.parent != null)
        {
            transform.position = transform.parent.position + offset;
        }
        else
        {
            transform.position = offset;
        }

        // Calculate rotation so scythe tip points outward (away from player)
        float zRotation = angle - 90f;

        // Apply world rotation (not local)
        transform.rotation = Quaternion.Euler(0f, 0f, zRotation);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy") && !HasTaggedChild(other.transform, "Enemy"))
            return;

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damageAmount, transform.position, false, transform.position, false);
            return;
        }

        BasicEnemy basicEnemy = other.GetComponentInParent<BasicEnemy>();
        if (basicEnemy != null)
        {
            basicEnemy.TakeDamage(damageAmount, transform.position, false, transform.position, false);
        }
    }

    bool HasTaggedChild(Transform obj, string tag)
    {
        foreach (Transform child in obj)
        {
            if (child.CompareTag(tag))
                return true;

            if (HasTaggedChild(child, tag))
                return true;
        }
        return false;
    }
}
