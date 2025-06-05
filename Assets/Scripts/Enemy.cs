using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    public enum EnemyType { Grounded, Flying, Slime, AnimatedGround } //not enough time for flying enemies fuck it we ball
    public EnemyType enemyType = EnemyType.Grounded;                  //also note to self, centralising all enemies into one was not a good idea dont do it again
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
    private string currentAnim = "";
    private bool isPlayingAttackAnimation = false;
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

    [Header("Slime Animation Clips")] //Note to self, never fucking do this again. THE FUCK YOU MEAN I CAN IMPORT ANIMATIONS DIRECTLY
    public Sprite[] slimeIdleFrames;
    public Sprite[] slimeJumpStartupFrames;
    public Sprite[] slimeJumpFallFrames;
    public Sprite[] slimeJumpLandFrames;
    public Sprite[] slimeHurtFrames;
    public Sprite[] slimeDeathFrames;

    [Header("AnimatedGround Animation Clips")]
    public Sprite[] animatedIdleFrames;
    public Sprite[] animatedWalkFrames;
    public Sprite[] animatedHurtFrames;
    public Sprite[] animatedDeathFrames;
    public Sprite[] animatedAttackFrames;

    [Header("General Animation")]
    public float slimeFrameRate = 0.1f;
    public float animatedFrameRate = 0.1f;

    public GameObject bloodPrefab;
    public GameObject bloodExplosionPrefab;
    public GameObject[] gibPrefabs;
    public int gibCount = 4;

    public AudioClip[] damageSoundList1;
    public AudioClip[] damageSoundList2;
    public AudioClip[] damageSoundList3;

    private bool isSlime => enemyType == EnemyType.Slime;
    private bool isAnimated => enemyType == EnemyType.AnimatedGround;
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

        if (isSlime) PlayAnimation(slimeIdleFrames, slimeFrameRate, "Idle");
        else if (isAnimated) PlayAnimation(animatedIdleFrames, animatedFrameRate, "Idle");
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
                EnterAttackState(); //Make sure this doesnt bug out if dupes are called
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

            MoveTowardPlayer(); //Todo: add jump checks for grounded enemies too
        }

        if (isSlime)
        {
            UpdateSlimeJumpAnimations();
            if (slimeJumpTimer > 0f) slimeJumpTimer -= Time.deltaTime;
        }

        else if (isAnimated && !isPlayingAttackAnimation)
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                PlayAnimation(animatedWalkFrames, animatedFrameRate, "Walk");
            }
            else
            {
                PlayAnimation(animatedIdleFrames, animatedFrameRate, "Idle");
            }
        }
    }
    void EnterAttackState()
    {
        if (enemyStyle == EnemyStyle.Boss)
        {   //Boss fight music and camera adjustments 
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "Level4") FindFirstObjectByType<AudioManager>().PlayMusicClip(5);
            else if (sceneName == "Level8") FindFirstObjectByType<AudioManager>().PlayMusicClip(10);
            else if (sceneName == "Level9") FindFirstObjectByType<AudioManager>().PlayMusicClip(12);
            else if (sceneName == "Level13") FindFirstObjectByType<AudioManager>().PlayMusicClip(14);
            CameraFollow.Instance.SetBossFightMode(true);
            if (bossHPBar != null)
            {
                bossHPBar.SetHealth(currentHealth); //this doesnt work
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
    public void PlayAttackAnimation()
    {
        if (isDead || !isAnimated || isPlayingAttackAnimation) return;

        isPlayingAttackAnimation = true;
        PlayAnimationOnce(animatedAttackFrames, animatedFrameRate, "Attack", () =>
        {
            isPlayingAttackAnimation = false;
            // Resume previous animation after attacking
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                PlayAnimation(animatedWalkFrames, animatedFrameRate, "Walk");
            }
            else
            {
                PlayAnimation(animatedIdleFrames, animatedFrameRate, "Idle");
            }
        });
    }
    public bool IsInAttackState() => isInAttackState;

    void MoveTowardPlayer()
    {
        if (enemyType == EnemyType.Flying)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * moveSpeed;
        }
        else if (enemyType == EnemyType.Grounded || enemyType == EnemyType.AnimatedGround)
        {
            Vector2 dir = new Vector2(player.position.x - transform.position.x, 0f).normalized;
            rb.linearVelocity = new Vector2(dir.x * moveSpeed, rb.linearVelocity.y);

            // This is bugged, flipping to the right doesnt work properly
            // gaben pls fix
            if (dir.x > 0.05f)
                transform.localScale = new Vector3(transform.localScale.x * 1, transform.localScale.y, transform.localScale.z);
            else if (dir.x < -0.05f && transform.localScale.x > 0)
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        else if (enemyType == EnemyType.Slime)
        {
            if (rb.linearVelocity.y <= 0.01f && slimeJumpTimer <= 0f)
            {
                Vector2 dir = (player.position - transform.position).normalized;
                dir.y = 1f;
                rb.AddForce(dir * moveSpeed * 2f, ForceMode2D.Impulse);

                PlayAnimation(slimeJumpStartupFrames, slimeFrameRate, "JumpStartup");
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
            PlayAnimation(slimeJumpFallFrames, slimeFrameRate, "JumpFall"); //WHY THE FUCK DOES THIS MAKE SLIMES IMMORTAL AAAAAAAAAAAAAAA
        }

        if (isGrounded && !wasGroundedLastFrame)
        {
            PlayAnimationOnce(slimeJumpLandFrames, slimeFrameRate, "JumpLand", () =>
            {
                PlayAnimation(slimeIdleFrames, slimeFrameRate, "Idle");
            });
        }

        if (isGrounded && wasGroundedLastFrame && currentAnimationCoroutine == null)
        {
            PlayAnimation(slimeIdleFrames, slimeFrameRate, "Idle");
        }

        wasGroundedLastFrame = isGrounded;
    }

    void PlayAnimation(Sprite[] frames, float frameRate, string animName)
    {
        if (isDead && animName != "Death") return;
        if (frames == null || frames.Length == 0 || currentAnim == animName) return;

        if (currentAnimationCoroutine != null) StopCoroutine(currentAnimationCoroutine);
        currentAnimationCoroutine = StartCoroutine(PlayAnimationCoroutine(frames, frameRate));
        currentAnim = animName;
    }
    void PlayAnimationOnce(Sprite[] frames, float frameRate, string animName, System.Action onComplete)
    {
        if (isDead && animName != "Death") return;
        if (frames == null || frames.Length == 0 || currentAnim == animName) return;

        if (currentAnimationCoroutine != null) StopCoroutine(currentAnimationCoroutine);
        currentAnimationCoroutine = StartCoroutine(PlayAnimationOnceCoroutine(frames, frameRate, onComplete));
        currentAnim = animName;
    }

    IEnumerator PlayAnimationCoroutine(Sprite[] frames, float frameRate)
    {
        int index = 0;
        while (true)
        {
            spriteRenderer.sprite = frames[index];
            index = (index + 1) % frames.Length;
            yield return new WaitForSeconds(frameRate);
        }
    }

    IEnumerator PlayAnimationOnceCoroutine(Sprite[] frames, float frameRate, System.Action onComplete)
    {
        for (int i = 0; i < frames.Length; i++)
        {
            spriteRenderer.sprite = frames[i];
            yield return new WaitForSeconds(frameRate);
        }

        currentAnim = "";
        currentAnimationCoroutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator WaitThenPlayFallAnimation() //note to self, dont tie slime enemies to enemy doors they might not die properly
    {
        yield return new WaitForSeconds(0.2f);
        if (!isDead)
        {
            PlayAnimation(slimeJumpFallFrames, slimeFrameRate, "JumpFall");
        }
    }
    public void TakeDamage(float amount, Vector3 hitPoint, bool isWeakPoint, Vector3 source, bool givesMana)
    {
        if (currentHealth <= 0f || isDead) return;

        if (isWeakPoint) amount *= 2f;
        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        Debug.Log($"Enemy took {amount} damage. Current health: {currentHealth}");
        StartCoroutine(FlashRed());

        // Animation handling
        if (isSlime)
        {
            PlayAnimation(slimeHurtFrames, slimeFrameRate, "Hurt");
        }
        else if (isAnimated)
        {
            PlayAnimation(animatedHurtFrames, animatedFrameRate, "Hurt");
        }

        CameraFollow.Instance.ScreenShake(2f, 0.8f, 0.5f);
        SpawnBloodEffect(hitPoint, source);
        ApplyKnockback(source);
        PlayDamageSounds();

        if (givesMana && playerHP != null) playerHP.RegenerateMana(manaAmount); //todo, come up with a better mana system this is too boring
        if (enemyStyle == EnemyStyle.Boss && bossHPBar != null) bossHPBar.SetHealth(currentHealth);
        if (currentHealth <= 0f) Die();
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
    void SpawnBloodEffect(Vector3 hitPoint, Vector3 source)
    {
        if (bloodPrefab) //yeah this doesnt work and i have no idea why
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
        if (redLight != null) redLight.enabled = true; //for slime enemies make sure this red light is the first in the hierarchy order or they flash green instead
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
        if (redLight != null) redLight.enabled = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        if (currentAnimationCoroutine != null) StopCoroutine(currentAnimationCoroutine);
        StopAllCoroutines();
        SpawnGibs();
        if (enemyStyle == EnemyStyle.Boss)
        {
            CameraFollow.Instance.SetBossFightMode(false);
            CameraFollow.Instance.SetCombatMode(false);
            if (bossHPBar != null) bossHPBar.Hide();
        }
        else
        {
            CameraFollow.Instance.SetCombatMode(false);
        }
        if (isSlime)
        {
            //This is cursed
            PlayAnimationOnce(slimeDeathFrames, slimeFrameRate, "Death", () => Destroy(gameObject));
        }
        else if (isAnimated)
        {
            PlayAnimationOnce(animatedDeathFrames, animatedFrameRate, "Death", () => Destroy(gameObject));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void PlayDamageSounds()
    {
        AudioClip clip = null;
        int r = Random.Range(0, 3);
        if (r == 0 && damageSoundList1.Length > 0) clip = damageSoundList1[Random.Range(0, damageSoundList1.Length)];
        if (r == 1 && damageSoundList2.Length > 0) clip = damageSoundList2[Random.Range(0, damageSoundList2.Length)];
        if (r == 2 && damageSoundList3.Length > 0) clip = damageSoundList3[Random.Range(0, damageSoundList3.Length)];
        if (clip) audioSource.PlayOneShot(clip);
    }
}
