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

    [Header("GUI Font")]
    public Font customFont;
    public Color hpColor = Color.white;
    public Color manaColor = Color.cyan;
    public Color goldColor = Color.yellow;
    public Color deathsColor = Color.red;
    public Color playtimeColor = Color.green;
    public Color swordNameColor = Color.magenta;

    [Header("SwordSwing Reference")]
    [Tooltip("Assign the GameObject that has the SwordSwing component attached.")]
    [SerializeField] private SwordSwing swordSwing;

    private void Awake()
    {
        // Removed GetComponent to allow manual assignment via inspector
        if (swordSwing == null)
        {
            Debug.LogWarning("SwordSwing reference not assigned in PlayerHP inspector.");
        }
    }

    void Start()
    {
        HP = maxHP;
        mana = 0;
        lastCheckpointPosition = transform.position; // Default to spawn point
        Transform damageLightTransform = transform.Find("DamageLight");
        if (damageLightTransform != null)
        {
            damageLight = damageLightTransform.GetComponent<Light2D>();
            if (damageLight != null)
                damageLight.enabled = false;
        }
        else
        {
            Debug.LogWarning("DamageLight child object not found.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Die();
        }
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;
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
        if (SaveManager.Instance != null && SaveManager.Instance.currentSave != null)
        {
            SaveManager.Instance.currentSave.playerDeaths += 1; //log death
            SaveManager.Instance.SaveGame();
        }
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(0.5f);

        transform.position = lastCheckpointPosition;
        HP = maxHP;
        mana = 0;
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

    private void OnGUI() //this was supposed to be temporary but we ball
    {
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        GUIStyle baseStyle = new GUIStyle();
        baseStyle.fontSize = 32;
        if (customFont != null) baseStyle.font = customFont;

        GUIStyle smallStyle = new GUIStyle();
        smallStyle.fontSize = 24;
        if (customFont != null) smallStyle.font = customFont;
        smallStyle.normal.textColor = playtimeColor;

        string hpText = $"HP: {HP}/{maxHP}";
        string manaText = $"MANA: {mana}";

        GUIStyle hpStyle = new GUIStyle(baseStyle);
        hpStyle.normal.textColor = hpColor;
        float hpWidth = hpStyle.CalcSize(new GUIContent(hpText)).x;
        GUI.Label(new Rect(10, screenHeight - 40, hpWidth, 40), hpText, hpStyle);

        GUIStyle manaStyle = new GUIStyle(baseStyle);
        manaStyle.normal.textColor = manaColor;
        GUI.Label(new Rect(10 + hpWidth + 20, screenHeight - 40, 200, 40), manaText, manaStyle);

        if (SaveManager.Instance != null && SaveManager.Instance.currentSave != null)
        {
            var save = SaveManager.Instance.currentSave;

            string goldText = $"Gold: {save.gold}";
            string deathsText = $"Deaths: {save.playerDeaths}";

            GUIStyle goldStyle = new GUIStyle(baseStyle);
            goldStyle.normal.textColor = goldColor;
            float goldWidth = goldStyle.CalcSize(new GUIContent(goldText)).x;

            GUIStyle deathsStyle = new GUIStyle(baseStyle);
            deathsStyle.normal.textColor = deathsColor;
            float deathsWidth = deathsStyle.CalcSize(new GUIContent(deathsText)).x;

            float totalWidth = goldWidth + 20 + deathsWidth;
            float startX = screenWidth - totalWidth - 10;

            GUI.Label(new Rect(startX, screenHeight - 40, goldWidth, 40), goldText, goldStyle);
            GUI.Label(new Rect(startX + goldWidth + 20, screenHeight - 40, deathsWidth, 40), deathsText, deathsStyle);

            float liveTotalSeconds = TimeTracker.Instance?.GetLiveTotalPlayTime() ?? 0f;
            int totalSeconds = Mathf.FloorToInt(liveTotalSeconds);
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;
            string formattedTime = $"{hours:00}:{minutes:00}:{seconds:00}";

            GUI.Label(new Rect(10, 10, 300, 30), "Play Time: " + formattedTime, smallStyle);
        }

        if (swordSwing != null)
        {
            if (swordSwing.CurrentSword != null)
            {
                string swordKey = swordSwing.CurrentSword.swordName;

                string displayName;

                // Map internal names to display names
                if (swordKey == "AdventurersBlade")
                    displayName = "Adventurer's Blade";
                else if (swordKey == "Kingslayer")
                    displayName = "Kingslayer";
                else if (swordKey == "Soulbinder")
                    displayName = "Soulbinder";
                else if (swordKey == "Shadowrend")
                    displayName = "Shadowrend";
                else if (swordKey == "SpearofLonginus")
                    displayName = "Spear of Longinus";
                else if (swordKey == "TheTrailblaze")
                    displayName = "The Trailblazer";
                else if (swordKey == "TheWarpath")
                    displayName = "Path of Caim";
                else if (swordKey == "Shingetsu")
                    displayName = "Shingetsu";
                else if (swordKey == "WanderersRepose")
                    displayName = "Wanderer's Repose";
                else if (swordKey == "Dauntless")
                    displayName = "Dauntless";
                else if (swordKey == "Frostbreaker")
                    displayName = "Frostbreaker";
                else if (swordKey == "TheIntoner")
                    displayName = "The Intoner's Curse";
                else if (swordKey == "TertiusDecimus")
                    displayName = "Tertius Decimus";
                else
                    displayName = swordKey;

                string swordText = $"{displayName}";

                GUIStyle swordStyle = new GUIStyle(baseStyle);
                swordStyle.normal.textColor = swordNameColor;
                float swordWidth = swordStyle.CalcSize(new GUIContent(swordText)).x;

                GUI.Label(new Rect(screenWidth - swordWidth - 10, 10, swordWidth, 40), swordText, swordStyle);
            }
            else
            {
                Debug.Log("CurrentSword is null.");
            }
        }
        else
        {
            Debug.Log("swordSwing is null.");
        }
    }
}
