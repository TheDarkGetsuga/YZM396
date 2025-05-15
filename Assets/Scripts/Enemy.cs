using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class Enemy : MonoBehaviour
{
    public enum EnemyType { Grounded, Flying, Slime }
    public EnemyType enemyType = EnemyType.Grounded;
    public enum EnemyStyle { Normal, Boss }
    public EnemyStyle enemyStyle = EnemyStyle.Normal;
    public string bossName = "The Toxic Champion Armisael";
    public float maxHealth = 100f;
    public float knockbackForce = 5f;
    public float detectionRadius = 5f;
    public float moveSpeed = 2f;
    public float attackCheckDelay = 10f;
    public float slimeJumpCooldown = 1.5f;

    private float slimeJumpTimer = 0f;
    private float currentHealth;
    private float attackStateTimer = 0f;
    private bool isInAttackState = false;
    private bool wasGroundedLastFrame = true;
    private string currentSlimeAnim = "";

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float knockbackTimer = 0f;
    private float knockbackDuration = 0.2f;
    private Coroutine currentAnimationCoroutine;

    private Transform player;
    public PlayerHP playerHP;
    public bool givesMana = true;
    public int manaAmount = 1;
    private Light2D redLight;
    private AudioSource audioSource;
    public BossHealthBar bossHPBar;

    [Header("Slime Animation Clips")]
    public Sprite[] slimeIdleFrames;
    public Sprite[] slimeJumpStartupFrames;
    public Sprite[] slimeJumpFallFrames;
    public Sprite[] slimeJumpLandFrames;
    public Sprite[] slimeHurtFrames;
    public Sprite[] slimeDeathFrames;
    public float slimeFrameRate = 0.1f;

    public GameObject bloodPrefab;
    public GameObject bloodExplosionPrefab;
    public GameObject[] gibPrefabs;
    public int gibCount = 4;

    public AudioClip[] damageSoundList1;
    public AudioClip[] damageSoundList2;
    public AudioClip[] damageSoundList3;

    private bool isSlime => enemyType == EnemyType.Slime;
    private bool isDead = false;

    void Start()
    {
        bossHPBar.Hide();
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        audioSource = GetComponent<AudioSource>();
        redLight = GetComponentInChildren<Light2D>();
        if (redLight != null) redLight.enabled = false;

        if (isSlime) PlaySlimeAnimation(slimeIdleFrames, "Idle");
    }

    void Update()
    {
        if (player == null || isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;
            return;
        }

        if (!isInAttackState)
        {
            if (distanceToPlayer <= detectionRadius)
            {
                EnterAttackState();
            }
        }
        else
        {
            if (distanceToPlayer > detectionRadius)
            {
                attackStateTimer += Time.deltaTime;
                if (attackStateTimer >= attackCheckDelay)
                {
                    isInAttackState = false;
                    attackStateTimer = 0f;
                }
            }
            else
            {
                attackStateTimer = 0f;
            }

            MoveTowardPlayer();
        }

        if (isSlime)
        {
            UpdateSlimeJumpAnimations();
            if (slimeJumpTimer > 0f) slimeJumpTimer -= Time.deltaTime;
        }
    }

    void EnterAttackState()
    {
        if (enemyStyle == EnemyStyle.Boss)
        {
            CameraFollow.Instance.SetBossFightMode(true);
            if (bossHPBar != null)
            {
                bossHPBar.SetHealth(currentHealth);
            }
        }
        else
        {
            CameraFollow.Instance.SetCombatMode(true);
        }

        isInAttackState = true;
        attackStateTimer = 0f;
        Debug.Log("Enemy has detected the player and entered attack state.");
    }

    void MoveTowardPlayer()
    {
        if (enemyType == EnemyType.Flying)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;  // Changed to rb.velocity
        }
        else if (enemyType == EnemyType.Grounded)
        {
            Vector2 dir = new Vector2(player.position.x - transform.position.x, 0f).normalized;
            rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);  // Changed to rb.velocity
        }
        else if (enemyType == EnemyType.Slime)
        {
            if (rb.linearVelocity.y <= 0.01f && slimeJumpTimer <= 0f)
            {
                Vector2 dir = (player.position - transform.position).normalized;
                dir.y = 1f;
                rb.AddForce(dir * moveSpeed * 2f, ForceMode2D.Impulse);

                PlaySlimeAnimation(slimeJumpStartupFrames, "JumpStartup");
                StartCoroutine(WaitThenPlayFallAnimation());
                slimeJumpTimer = slimeJumpCooldown;
            }
        }
    }

    void UpdateSlimeJumpAnimations()
    {
        bool isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.01f;

        if (!isGrounded && rb.linearVelocity.y < -0.1f)
        {
            PlaySlimeAnimation(slimeJumpFallFrames, "JumpFall");
        }

        if (isGrounded && !wasGroundedLastFrame)
        {
            PlaySlimeAnimationOnce(slimeJumpLandFrames, "JumpLand", () =>
            {
                PlaySlimeAnimation(slimeIdleFrames, "Idle");
            });
        }

        if (isGrounded && wasGroundedLastFrame && currentAnimationCoroutine == null)
        {
            PlaySlimeAnimation(slimeIdleFrames, "Idle");
        }

        wasGroundedLastFrame = isGrounded;
    }

    void PlaySlimeAnimation(Sprite[] frames, string animName)
    {
        if (!isSlime || frames == null || frames.Length == 0 || currentSlimeAnim == animName) return;

        if (currentAnimationCoroutine != null) StopCoroutine(currentAnimationCoroutine);
        currentAnimationCoroutine = StartCoroutine(PlayAnimationCoroutine(frames));
        currentSlimeAnim = animName;
    }

    void PlaySlimeAnimationOnce(Sprite[] frames, string animName, System.Action onComplete)
    {
        if (!isSlime || frames == null || frames.Length == 0 || currentSlimeAnim == animName) return;

        if (currentAnimationCoroutine != null) StopCoroutine(currentAnimationCoroutine);
        currentAnimationCoroutine = StartCoroutine(PlayAnimationOnceCoroutine(frames, onComplete));
        currentSlimeAnim = animName;
    }

    IEnumerator PlayAnimationCoroutine(Sprite[] frames)
    {
        int index = 0;
        while (true)
        {
            spriteRenderer.sprite = frames[index];
            index = (index + 1) % frames.Length;
            yield return new WaitForSeconds(slimeFrameRate);
        }
    }

    IEnumerator PlayAnimationOnceCoroutine(Sprite[] frames, System.Action onComplete)
    {
        for (int i = 0; i < frames.Length; i++)
        {
            spriteRenderer.sprite = frames[i];
            yield return new WaitForSeconds(slimeFrameRate);
        }

        currentSlimeAnim = "";
        currentAnimationCoroutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator WaitThenPlayFallAnimation()
    {
        yield return new WaitForSeconds(0.2f);
        PlaySlimeAnimation(slimeJumpFallFrames, "JumpFall");
    }

    public void TakeDamage(float amount, Vector3 hitPoint, bool isWeakPoint, Vector3 source, bool givesMana)
    {
        if (currentHealth <= 0f || isDead) return;

        if (isWeakPoint) amount *= 2f;
        currentHealth = Mathf.Max(currentHealth - amount, 0f);

        StartCoroutine(FlashRed());
        if (isSlime) PlaySlimeAnimation(slimeHurtFrames, "Hurt");

        CameraFollow.Instance.ScreenShake(2f, 0.8f, 0.5f);
        SpawnBloodEffect(hitPoint, source);
        ApplyKnockback(source);
        PlayDamageSounds();

        if (givesMana && playerHP != null)
        {
            playerHP.RegenerateMana(manaAmount);
        }
        if (enemyStyle == EnemyStyle.Boss && bossHPBar != null)
        {
            bossHPBar.SetHealth(currentHealth);
        }
        if (currentHealth <= 0f) Die();
    }

    void SpawnBloodEffect(Vector3 hitPoint, Vector3 source)
    {
        if (bloodPrefab)
        {
            GameObject blood = Instantiate(bloodPrefab, hitPoint, Quaternion.identity, transform);
            var sr = blood.GetComponent<SpriteRenderer>();
            if (sr != null) sr.flipX = source.x > transform.position.x;
        }

        if (bloodExplosionPrefab)
        {
            Instantiate(bloodExplosionPrefab, hitPoint, Quaternion.identity, transform);
        }
    }

    void ApplyKnockback(Vector3 source)
    {
        if (rb)
        {
            Vector2 knockDir = (transform.position - source).normalized;
            knockDir.y += 0.3f;
            knockDir = knockDir.normalized;

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
            knockbackTimer = knockbackDuration;
        }
    }

    IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        if (redLight != null) redLight.enabled = true;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
        if (redLight != null) redLight.enabled = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log(isSlime ? "Slime enemy died" : "Enemy died");
        if (enemyStyle == EnemyStyle.Boss)
        {
            bossHPBar.Hide();
            CameraFollow.Instance.SetBossFightMode(false);
        }
        else
        {
            CameraFollow.Instance.SetCombatMode(false);
        }

        SpawnGibs();

        if (isSlime)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
            GetComponent<Collider2D>().enabled = false;

            PlaySlimeAnimationOnce(slimeDeathFrames, "Death", () =>
            {
                Destroy(gameObject);
            });
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SpawnGibs()
    {
        CameraFollow.Instance.ScreenShake(3f, 1.8f, 0.5f);
        for (int i = 0; i < gibCount; i++)
        {
            GameObject gibPrefab = gibPrefabs[Random.Range(0, gibPrefabs.Length)];
            Vector3 offset = Random.insideUnitCircle.normalized * 0.5f;
            GameObject gib = Instantiate(gibPrefab, transform.position + offset, Quaternion.identity);
            var gibRb = gib.GetComponent<Rigidbody2D>();
            if (gibRb != null)
            {
                gibRb.AddForce(offset * Random.Range(2f, 5f), ForceMode2D.Impulse);
            }
        }
    }

    void PlayDamageSounds()
    {
        if (damageSoundList1.Length > 0)
            audioSource.PlayOneShot(damageSoundList1[Random.Range(0, damageSoundList1.Length)]);

        if (damageSoundList2.Length > 0)
            audioSource.PlayOneShot(damageSoundList2[Random.Range(0, damageSoundList2.Length)]);

        if (damageSoundList3.Length > 0)
            audioSource.PlayOneShot(damageSoundList3[Random.Range(0, damageSoundList3.Length)]);
    }
}
