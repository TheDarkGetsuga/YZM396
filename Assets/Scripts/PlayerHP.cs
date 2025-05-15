using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;


public class PlayerHP : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 100;
    public int HP;

    [Header("Mana")]
    public int maxMana = 100;
    public int mana;

    [Header("Damage Visuals")]
    [SerializeField] private ParticleSystem damageParticles;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Knockback")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float verticalKnockbackMultiplier = 1.5f;

    [Header("Invincibility Frames")]
    [SerializeField] private float invincibilityDuration = 0.4f;
    private bool isInvincible = false;

    private Vector3 lastCheckpointPosition;
    private ParticleSystem damageparticleinstance;
    private Light2D damageLight;


    void Start()
    {
        HP = maxHP;
        mana = 0;
        lastCheckpointPosition = transform.position; // Default to spawn point
        // Find DamageLight by name in children
        Transform damageLightTransform = transform.Find("DamageLight");
        if (damageLightTransform != null)
        {
            damageLight = damageLightTransform.GetComponent<Light2D>();
            if (damageLight != null)
                damageLight.enabled = false; // Off by default
        }
        else
        {
            Debug.LogWarning("DamageLight child object not found.");
        }

    }

    void Update()
    {
        // Optional: Debug respawn with R key
        if (Input.GetKeyDown(KeyCode.R))
        {
            Die();
        }
    }

    public void TakeDamage(int damage, Vector3 sourcePosition)
    {
        if (isInvincible) return;

        HP -= damage;
        SpawnBloodParticles();
        StartCoroutine(FlashRed());
        StartCoroutine(Invincibility());

        ApplyKnockback(sourcePosition);
        CameraFollow.Instance.ScreenShake(3f, 0.8f, 0.5f);

        if (HP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died. Respawning at checkpoint.");
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        // Optional: add death animation or delay
        yield return new WaitForSeconds(0.5f);

        transform.position = lastCheckpointPosition;
        HP = maxHP;
        mana = 0; // Optional: reset mana or keep

        // Optional: screen flash or respawn effects
    }

    public void SetCheckpoint(Vector3 checkpointPosition)
    {
        lastCheckpointPosition = checkpointPosition;
        Debug.Log("Checkpoint updated to: " + checkpointPosition);
    }

    private void ApplyKnockback(Vector3 sourcePosition)
    {
        if (rb == null) return;

        Vector2 knockbackDirection = (transform.position - sourcePosition).normalized;
        if (knockbackDirection.y < 0.2f)
            knockbackDirection.y = 0.2f;

        knockbackDirection.y = 1.2f;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
    }

    private void SpawnBloodParticles()
    {
        damageparticleinstance = Instantiate(damageParticles, transform.position, Quaternion.identity);
    }

    private IEnumerator FlashRed()
    {
        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRenderer not assigned to PlayerHP.");
            yield break;
        }

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = flashColor;

        if (damageLight != null)
            damageLight.enabled = true;

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.color = originalColor;

        if (damageLight != null)
            damageLight.enabled = false;
    }


    private IEnumerator Invincibility()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    public void RegenerateMana(int amount)
    {
        mana = Mathf.Clamp(mana + amount, 0, maxMana);
    }

    public bool ConsumeMana(int amount)
    {
        if (mana >= amount)
        {
            mana -= amount;
            return true;
        }
        return false;
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 200, 30), "HP: " + HP + "/" + maxHP, style);
        GUI.Label(new Rect(10, 40, 200, 30), "Mana: " + mana + "/" + maxMana, style);
    }
}