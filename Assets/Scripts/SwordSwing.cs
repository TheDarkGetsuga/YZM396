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
    public Transform spawnPoint;
    private bool isFacingRight = true;
    private bool isCastingMagic = false;

    public AudioClip[] swingSoundList1;
    public AudioClip[] swingSoundList2;
    public AudioClip[] swingSoundList3;
    public AudioClip[] swingSoundList4;

    public GameObject attackArea;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        weaponHitbox = transform.Find("Sword").GetComponent<WeaponHitbox>();

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

        // Play all 4 swing sound lists (only if not magic)
        StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
        StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
        StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
        StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
    }

    void ResetCombo() => comboStep = 0;

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

    void TriggerMagicAttack()
    {
        if (isCastingMagic) return;

        Debug.Log($"Triggering magic attack with {currentSword.swordName}.");

        if (currentSword.swordName == "Spear of Longinus")
        {   
            StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
            StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
            animator.SetTrigger("SpearOfLonginusAttack");
        }
        else if (currentSword.swordName == "The Trailblaze")
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

    // Animation Event Methods (optional, unused now)
    public void PlaySwingSoundList1() => StartCoroutine(PlayRandomFromList(swingSoundList1, 0.5f));
    public void PlaySwingSoundList2() => StartCoroutine(PlayRandomFromList(swingSoundList2, 0.5f));
    public void PlaySwingSoundList3() => StartCoroutine(PlayRandomFromList(swingSoundList3, 0.5f));
    public void PlaySwingSoundList4() => StartCoroutine(PlayRandomFromList(swingSoundList4, 0.5f));
}
