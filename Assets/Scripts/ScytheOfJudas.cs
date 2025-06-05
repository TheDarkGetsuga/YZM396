using UnityEngine;

public class ScytheOfJudas : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float startSpeed = 900f;
    public float normalSpeed = 200f;
    public float speedDecayDuration = 1f;

    [Header("Damage Settings")]
    public int damageAmount = 30;

    private float angle; 
    private float orbitRadius;
    private float rotationSpeed;
    private float decayTimer = 0f;

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
            //Smoothstep tweening
            t = Mathf.SmoothStep(0f, 1f, t);

            rotationSpeed = Mathf.Lerp(startSpeed, normalSpeed, t);
        }
        else
        {
            rotationSpeed = normalSpeed;
        }
        angle += rotationSpeed * Time.deltaTime;
        if (angle >= 360f)
            angle -= 360f;

        UpdatePosition();
    }
    void UpdatePosition()
    {
        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
        if (transform.parent != null)
        {
            transform.position = transform.parent.position + offset;
        }
        else
        {
            transform.position = offset;
        }

        // Note to self, should point outwards from the center of rotation
        float zRotation = angle - 90f;

        // World rotation shouldn't be local
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
