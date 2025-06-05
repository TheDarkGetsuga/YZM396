using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwordSwing : MonoBehaviour
{
    public List<SwordData> inventory = new List<SwordData>();
    public int currentSwordIndex = 0;

    private SwordData currentSword;
    public SwordData CurrentSword => currentSword;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Animator animator;

    private int comboStep = 0;
    private int bufferedInputs = 0;
    private float comboResetTimer = 0f;
    private bool animationPlaying = false;
    private WeaponHitbox weaponHitbox;
    public PlayerHP playerHP;
    public GameObject flameParticlePrefab;
    public GameObject getsugatenshoPrefab;
    public GameObject zangerinFireballPrefab;
    public GameObject shadowcloakPrefab;
    public GameObject soulbinderPrefab;
    public GameObject soulswordProjectilePrefab;
    public GameObject vortexPrefab;
    public GameObject titanfallCleavePrefab;
    public Transform spawnPoint;
    private bool isFacingRight = true;
    private bool isCastingMagic = false;

    public AudioClip[] swingSoundList1;
    public AudioClip[] swingSoundList2;
    public AudioClip[] swingSoundList3;
    public AudioClip[] swingSoundList4;

    public GameObject attackArea;
    [SerializeField] private float getsugaOffsetX = 1f;
    [SerializeField] private float getsugaOffsetY = 0f;
    [SerializeField] private GameObject frostbreakerSpikePrefab;    
    [SerializeField] private float frostbreakerRadius = 2f;
    public GameObject intonerFireballPrefab;
    public GameObject scytheOfJudasPrefab;
    private Transform playerTransform;
    private bool scythesActive = false;
    private List<SpriteRenderer> allRenderers = new List<SpriteRenderer>();
    public PlayerMovement playerMovement;
    void Start()
    {
        playerTransform = transform.root;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        weaponHitbox = transform.Find("Sword").GetComponent<WeaponHitbox>();
        allRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>());

        if (inventory.Count > 0)
        {
            EquipSword(0);
        }

        playerHP = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHP>();

        if (attackArea != null)
        {
            attackArea.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) CycleSword(-1);
        else if (Input.GetKeyDown(KeyCode.E)) CycleSword(1);

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isFacingRight = (mouseWorldPos.x > transform.position.x);

        if (Input.GetMouseButtonDown(0) && !isCastingMagic)
        {
            bufferedInputs++;
            comboResetTimer = 0f;
            TryPlaySwing();
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryUseMagic();
        }
        else
        {
            comboResetTimer += Time.deltaTime;
            if (comboResetTimer > 1f && !animationPlaying && comboStep > 0)
            {
                ResetCombo();
            }
        }

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (animationPlaying && state.normalizedTime >= 1f && !state.IsTag("Swinging"))
        {
            animationPlaying = false;
            if (comboStep >= currentSword.comboLength && bufferedInputs > 0)
            {
                ResetCombo();
            }
            TryPlaySwing();
        }
    }

    void TryPlaySwing()
    {
        if (animationPlaying || bufferedInputs <= 0 || isCastingMagic) return;

        if (comboStep >= currentSword.comboLength)
        {
            ResetCombo();
        }

        comboStep++;
        bufferedInputs--;

        animator.SetInteger("ComboStep", comboStep);
        animator.SetTrigger($"Play{currentSword.animationPrefix}");

        animationPlaying = true;
        comboResetTimer = 0f;

        if (attackArea != null)
        {
            attackArea.SetActive(true);
        }
        StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
        StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
        StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
        StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
    }
    private void SetPlayerTransparency(float alpha)
    {
        foreach (var renderer in allRenderers)
        {
            if (renderer != null)
            {
                Color c = renderer.color;
                c.a = alpha;
                renderer.color = c;
            }
        }
    }
    void ResetCombo() => comboStep = 0;
    public void LoadSwordsFromSave()
    {
        inventory.Clear();

        foreach (string savedSwordName in SaveManager.Instance.currentSave.obtainedSwordNames)
        {
            SwordData sword = Resources.Load<SwordData>("Swords/" + savedSwordName);
            if (sword != null)
            {
                inventory.Add(sword);
            }
            else
            {
                Debug.LogWarning($"SwordData not found in Resources/Swords/ for name: {savedSwordName}");
            }
        }

        if (inventory.Count > 0)
        {
            EquipSword(0);
        }
    }
    public void EquipSword(int index)
    {
        if (index >= 0 && index < inventory.Count)
        {
            currentSwordIndex = index;
            currentSword = inventory[index];
            spriteRenderer.sprite = currentSword.swordSprite;

            if (weaponHitbox != null)
            {
                weaponHitbox.SetSwordData(currentSword);
            }

            Debug.Log($"Equipped {currentSword.swordName} (Level {currentSword.level}) | Damage: {currentSword.Damage}, Speed: {currentSword.SwingSpeed}");
        }
    }

    private void CycleSword(int direction)
    {
        if (inventory.Count == 0) return;

        currentSwordIndex += direction;
        if (currentSwordIndex < 0) currentSwordIndex = inventory.Count - 1;
        else if (currentSwordIndex >= inventory.Count) currentSwordIndex = 0;

        EquipSword(currentSwordIndex);
    }

    public void AddSwordToInventory(SwordData sword)
    {
        if (!inventory.Contains(sword))
        {
            inventory.Add(sword);
            Debug.Log($"Added {sword.swordName} to inventory.");
        }
    }

    void TryUseMagic()
    {
        if (comboStep >= currentSword.comboLength)
        {
            TriggerMagicAttack();
        }
        else if (playerHP.ConsumeMana(10))
        {
            TriggerMagicAttack();
        }
    }

    void TriggerMagicAttack() //Cavemen if else chain for magic attacks, use switch statements on later refactoring
    {
        if (isCastingMagic) return;

        Debug.Log($"Triggering magic attack with {currentSword.swordName}.");

        if (currentSword.swordName == "SpearofLonginus")
        {
            StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
            animator.SetTrigger("SpearOfLonginusAttack");
        }
        else if (currentSword.swordName == "AdventurersBlade")
        {
            StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
            animator.SetTrigger("AdventurersBladeAttack");
        }
        else if (currentSword.swordName == "Kingslayer")
        {
            StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
            animator.SetTrigger("Zangerin");
            StartCoroutine(FinishCastingAfter(2f));
        }
        else if (currentSword.swordName == "Shadowrend")
        {
            StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
            animator.SetTrigger("Shadowcast");

            GameObject cloak = Instantiate(shadowcloakPrefab, transform.root.position, Quaternion.identity, transform.root);
            cloak.transform.localPosition = Vector3.zero;

            StartCoroutine(FinishCastingAfter(2f));
        }
        else if (currentSword.swordName == "Soulbinder")
        {
            StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
            animator.SetTrigger("Shadowcast");
            StartCoroutine(SoulbinderEffect());
        }
        else if (currentSword.swordName == "TheWarpath")
        {
            StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
            animator.SetTrigger("WarpathSwordDance");
            StartCoroutine(FinishCastingAfter(2f));
        }
        else if (currentSword.swordName == "Frostbreaker")
        {
            StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
            SpawnFrostbreakerCircle();
            animator.SetTrigger("Shadowcast");
            StartCoroutine(FinishCastingAfter(2f));
        }
        else if (currentSword.swordName == "TheIntoner")
        {
            StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
            animator.SetTrigger("IntonerCast");
            StartCoroutine(FinishCastingAfter(5f));
        }
        else if (currentSword.swordName == "WanderersRepose")
            {
                StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
                StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
                StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
                StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
                animator.SetTrigger("Shadowcast");
                Vector2 spawnPosition = transform.position + transform.right * 2f;
                Instantiate(vortexPrefab, spawnPosition, Quaternion.identity);
                StartCoroutine(FinishCastingAfter(2f));
            }
            else if (currentSword.swordName == "Dauntless")
            {
                StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
                StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
                StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
                StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
                animator.SetTrigger("TitanfallCleave");
                StartCoroutine(FinishCastingAfter(2f));
            }
            else if (currentSword.swordName == "TertiusDecimus")
            {
                if (scythesActive) 
                {
                    // Already active, ignore further triggers
                    return;
                }
                scythesActive = true;
                StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
                StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
                StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
                StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
                animator.SetTrigger("Shadowcast");
                StartCoroutine(SpawnAndRotateScythes());
                StartCoroutine(FinishCastingAfter(2f));
            }
            else if (currentSword.swordName == "TheTrailblaze")
        {
            isCastingMagic = true;

            GameObject casterGO = new GameObject("TrailblazeCaster");
            FlamethrowerCaster caster = casterGO.AddComponent<FlamethrowerCaster>();
            caster.flamePrefab = flameParticlePrefab;
            caster.spawnTransform = spawnPoint;
            caster.facingRight = isFacingRight;

            caster.BeginCasting(2f);
            animator.SetTrigger("TrailblazeMagic");

            StartCoroutine(FinishCastingAfter(2f));
        }
        else if (currentSword.swordName == "Shingetsu")
        {
            isCastingMagic = true;
            animator.SetTrigger("Getsugatensho");

            StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));

            StartCoroutine(SpawnGetsugaAfterDelay(0.3f)); // Wait 0.2s before firing
            StartCoroutine(FinishCastingAfter(1.5f));
        }

        else
        {
            Debug.LogWarning($"No magic attack defined for {currentSword.swordName}.");
        }
    }
    public void TriggerTitanfallCleave()
    {
        if (titanfallCleavePrefab == null)
        {
            Debug.LogWarning("Titanfall Cleave prefab not assigned!");
            return;
        }

        Transform player = transform.root;
        GameObject cleave = Instantiate(titanfallCleavePrefab, player.position, Quaternion.identity, player);
        Vector3 scale = cleave.transform.localScale;
        if (player.localScale.x < 0)
            scale.x *= -1;
        cleave.transform.localScale = scale;
        cleave.transform.localPosition += new Vector3(1f * Mathf.Sign(player.localScale.x), -0.5f, 0f);
    }
    private IEnumerator SpawnAndRotateScythes()
    {
        if (playerTransform == null || scytheOfJudasPrefab == null)
        {
            Debug.LogError("PlayerTransform or scytheOfJudasPrefab is not assigned!");
            yield break;
        }

        int scytheCount = 4;
        float radius = 2f;
        float duration = 15f;

        GameObject[] scythes = new GameObject[scytheCount];

        for (int i = 0; i < scytheCount; i++)
        {
            GameObject scythe = Instantiate(scytheOfJudasPrefab, playerTransform.position, Quaternion.identity);
            scythe.transform.parent = playerTransform;

            // Pass initial angle and radius to the scythe script
            ScytheOfJudas scytheScript = scythe.GetComponent<ScytheOfJudas>();
            if (scytheScript != null)
            {
                float initialAngle = i * (360f / scytheCount);
                scytheScript.SetInitialOrbit(initialAngle, radius);
            }

            scythes[i] = scythe;
        }
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < scytheCount; i++)
        {
            if (scythes[i] != null)
                Destroy(scythes[i]);
        }
        scythesActive = false;
    }

    public void SpawnIntonerFireballsFromTip()
    {
        if (intonerFireballPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("Intoner fireball prefab or spawn point not assigned.");
            return;
        }

        StartCoroutine(SpawnFireballsRoutine());
    }

    private IEnumerator SpawnFireballsRoutine()
    {
        float duration = 3f;
        float fireballsPerSecond = 10f;
        int totalFireballs = Mathf.RoundToInt(duration * fireballsPerSecond);
        float interval = 1f / fireballsPerSecond;

        for (int i = 0; i < totalFireballs; i++)
        {
            GameObject fireball = Instantiate(
                intonerFireballPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            fireball.transform.localScale = spawnPoint.lossyScale;

            yield return new WaitForSeconds(interval);
        }
    }
    void SpawnFrostbreakerCircle()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is null. Make sure this object is under the Player in the hierarchy.");
            return;
        }

        int spikeCount = 8;
        float angleStep = 360f / spikeCount;

        for (int i = 0; i < spikeCount; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * frostbreakerRadius;
            Vector3 spawnPos = playerTransform.position + (Vector3)offset;

            GameObject spike = Instantiate(frostbreakerSpikePrefab, spawnPos, Quaternion.identity);
            spike.transform.parent = playerTransform;
        }
    }
    public void SpawnSoulswordFromTip()
    {
        if (spawnPoint == null || soulswordProjectilePrefab == null)
        {
            Debug.LogWarning("Sword Tip or Soulsword projectile prefab not assigned.");
            return;
        }
        GameObject projectile = Instantiate(
            soulswordProjectilePrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        projectile.transform.localScale = spawnPoint.lossyScale;

        Transform projectileTip = projectile.transform.Find("SwordTip");
        if (projectileTip != null)
        {
            projectileTip.position = spawnPoint.position;
            projectileTip.rotation = spawnPoint.rotation;
            projectileTip.localScale = spawnPoint.lossyScale;
        }
    }
    public void SpawnZangerinFireballFromEvent()
    {
        if (currentSword == null || currentSword.swordName != "Kingslayer") return;
        Vector3 spawnPos = spawnPoint.position;
        GameObject fireball = Instantiate(zangerinFireballPrefab, spawnPos, Quaternion.identity);
    }
    IEnumerator SoulbinderEffect()
    {
        // DO NOT set isCastingMagic to true here, so attacking is still allowed

        // Transparency is bugged
        playerMovement.Speed = 12f;
        Color originalColor = spriteRenderer.color;
        Color transparentColor = originalColor;
        transparentColor.a = 0.35f;
        SetPlayerTransparency(0.35f);
        spriteRenderer.color = transparentColor;

        // Note to self: Invincibility doesn't work consistently if cast too rapidly, find a fix
        playerHP.SetInvincible(true);
        GameObject soulbinderEffect = Instantiate(soulbinderPrefab, transform.root.position, Quaternion.identity, transform.root);
        soulbinderEffect.transform.localPosition = Vector3.zero;

        yield return new WaitForSeconds(10f);
        spriteRenderer.color = originalColor;
        playerHP.SetInvincible(false);
        SetPlayerTransparency(1f);
        animator.SetTrigger("ReturnToIdle");
        playerMovement.Speed = 8f;
        Destroy(soulbinderEffect);
    }

    private IEnumerator SpawnGetsugaAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        float directionMultiplier = isFacingRight ? 1f : -1f;
        Vector3 offset = new Vector3(getsugaOffsetX * directionMultiplier, getsugaOffsetY, 0f);
        Vector3 spawnPos = spawnPoint.position + offset;

        GameObject projectile = Instantiate(getsugatenshoPrefab, spawnPos, Quaternion.identity);
        Getsugatensho getsuga = projectile.GetComponent<Getsugatensho>();
        getsuga.SetDirection(isFacingRight);
    }
    IEnumerator FinishCastingAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        isCastingMagic = false;
        animator.SetTrigger("ReturnToIdle");
    }

    IEnumerator PlayRandomFromList(AudioClip[] list, float volume = 1f)
    {
        if (list != null && list.Length > 0)
        {
            AudioClip clip = list[Random.Range(0, list.Length)];
            yield return StartCoroutine(PlayClipLoud(clip, transform.position, volume));
        }
    }

    IEnumerator PlayClipLoud(AudioClip clip, Vector3 position, float volume = 2f)
    {
        if (clip == null) yield break;

        GameObject tempGO = new GameObject("TempAudioLoud");
        tempGO.transform.position = position;

        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.volume = Mathf.Clamp(volume, 0f, 10f);
        aSource.spatialBlend = 1f;
        aSource.minDistance = 1f;
        aSource.maxDistance = 20f;
        aSource.rolloffMode = AudioRolloffMode.Linear;

        aSource.Play();
        Destroy(tempGO, clip.length);

        yield return null;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Breakable"))
        {
            BreakableObject breakable = collision.gameObject.GetComponent<BreakableObject>();
            if (breakable != null)
            {
                int damage = Mathf.RoundToInt(currentSword.Damage);
                breakable.TakeDamage(damage);
                Debug.Log($"Hit breakable object with {damage} damage.");
            }
        }
    }

    public void ActivateAttackArea()
    {
        if (attackArea != null)
        {
            attackArea.SetActive(true);
        }
    }

    public void DeactivateAttackArea()
    {
        if (attackArea != null)
        {
            attackArea.SetActive(false);
        }
    }
    //Unused animation event methods
    public void PlaySwingSoundList1() => StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
    public void PlaySwingSoundList2() => StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
    public void PlaySwingSoundList3() => StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
    public void PlaySwingSoundList4() => StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
}
