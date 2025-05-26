using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyDMG : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage;

    [Header("Fireball Burst Settings")]
    public bool useFireballs = false;
    public GameObject fireballPrefab;
    public float fireballCooldownMin = 8f;
    public float fireballCooldownMax = 12f;
    public int fireballsPerBurstMin = 3;
    public int fireballsPerBurstMax = 7;
    public float timeBetweenShots = 0.1f;

    [Header("Ring of Fire Settings")]
    public bool useRingOfFire = false;
    public float ringOfFireCooldownMin = 10f;
    public float ringOfFireCooldownMax = 15f;

    [Header("Spiral Fire Settings")]
    public bool useSpiralFire = false;
    public float spiralFireCooldownMin = 10f;
    public float spiralFireCooldownMax = 15f;
    public float spiralFireStepDelay = 0.05f;

    [Header("Spirit Lance Settings")]
    public bool useSpiritLance = false;
    public GameObject spiritLancePrefab;
    public float spiritLanceCooldownMin = 10f;
    public float spiritLanceCooldownMax = 15f;
    public float spiritLanceOffsetRadius = 2f;
    public float spiritLanceDelayBeforeLaunch = 1.5f;

    [Header("Spiral Spirit Lance Settings")]
    public bool useSpiralSpiritLance = false;
    public float spiralSpiritLanceCooldownMin = 10f;
    public float spiralSpiritLanceCooldownMax = 15f;
    public float spiralSpiritLanceStepDelay = 0.05f;

    [Header("Soulseekers Settings")]
    public bool useSoulseekers = false;
    public GameObject soulseekerPrefab;
    public float soulseekerCooldownMin = 10f;
    public float soulseekerCooldownMax = 15f;
    public float soulseekerStepDelay = 0.05f;
    public float soulseekerOffsetRadius = 2f;

    [Header("Summon Demonica Settings")]
    public bool useSummonDemonica = false;
    public List<GameObject> demonicaPrefabs;
    public int summonCount = 5;
    public float summonCooldownMin = 15f;
    public float summonCooldownMax = 25f;
    public float summonExplosionForce = 8f;

    private MonoBehaviour enemyScript;

    private void Start()
    {
        enemyScript = GetComponent<Enemy>() as MonoBehaviour ??
                      GetComponent<BasicEnemy>() as MonoBehaviour ??
                      GetComponentInParent<Enemy>() as MonoBehaviour ??
                      GetComponentInParent<BasicEnemy>() as MonoBehaviour;

        if (enemyScript == null)
            return;

        if (useFireballs && fireballPrefab != null)
            StartCoroutine(FireballRoutine());

        if (useRingOfFire && fireballPrefab != null)
            StartCoroutine(RingOfFireRoutine());

        if (useSpiralFire && fireballPrefab != null)
            StartCoroutine(SpiralFireRoutine());

        if (useSpiritLance && spiritLancePrefab != null)
            StartCoroutine(SpiritLanceRoutine());

        if (useSpiralSpiritLance && spiritLancePrefab != null)
            StartCoroutine(SpiralSpiritLanceRoutine());

        if (useSoulseekers && soulseekerPrefab != null)
            StartCoroutine(SoulseekerRoutine());

        if (useSummonDemonica && demonicaPrefabs != null && demonicaPrefabs.Count > 0)
            StartCoroutine(SummonDemonicaRoutine());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHP hp = collision.gameObject.GetComponent<PlayerHP>();
            if (hp != null)
            {
                hp.TakeDamage(damage, transform.position);

                if (enemyScript is Enemy enemy)
                {
                    enemy.PlayAttackAnimation();
                }
            }
        }
    }

    private IEnumerator FireballRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(fireballCooldownMin, fireballCooldownMax));
            if (IsAttacking())
            {
                int burstCount = Random.Range(fireballsPerBurstMin, fireballsPerBurstMax + 1);
                for (int i = 0; i < burstCount; i++)
                {
                    SpawnFireballAtPlayer();
                    yield return new WaitForSeconds(timeBetweenShots);
                }
            }
        }
    }

    private IEnumerator RingOfFireRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(ringOfFireCooldownMin, ringOfFireCooldownMax));
            if (IsAttacking())
                SpawnRingOfFire();
        }
    }

    private IEnumerator SpiralFireRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spiralFireCooldownMin, spiralFireCooldownMax));
            if (IsAttacking())
            {
                for (int i = 0; i < 36; i++)
                {
                    float angle = i * 10f;
                    float radians = angle * Mathf.Deg2Rad;
                    Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;

                    GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
                    Rigidbody2D rb = fireball.GetComponent<Rigidbody2D>();
                    if (rb != null)
                        rb.linearVelocity = direction * 10f;

                    yield return new WaitForSeconds(spiralFireStepDelay);
                }
            }
        }
    }

    private IEnumerator SpiritLanceRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spiritLanceCooldownMin, spiritLanceCooldownMax));
            if (IsAttacking())
            {
                List<GameObject> spawnedLances = new List<GameObject>();
                for (int i = 0; i < 36; i++)
                {
                    float angle = i * 10f;
                    float radians = angle * Mathf.Deg2Rad;
                    Vector2 offset = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * spiritLanceOffsetRadius;
                    Vector3 spawnPos = transform.position + (Vector3)offset;
                    Quaternion rotation = Quaternion.Euler(0, 0, angle);

                    GameObject lance = Instantiate(spiritLancePrefab, spawnPos, rotation, transform);
                    spawnedLances.Add(lance);
                    Collider2D collider = lance.GetComponent<Collider2D>();
                    if (collider != null) collider.enabled = false;
                }

                yield return new WaitForSeconds(spiritLanceDelayBeforeLaunch);

                foreach (GameObject lance in spawnedLances)
                {
                    if (lance == null) continue;

                    lance.transform.SetParent(null);

                    Collider2D collider = lance.GetComponent<Collider2D>();
                    if (collider != null) collider.enabled = true;

                    Rigidbody2D rb = lance.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 direction = (lance.transform.position - transform.position).normalized;
                        rb.linearVelocity = direction * 12f;
                    }
                }
            }
        }
    }

    private IEnumerator SpiralSpiritLanceRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spiralSpiritLanceCooldownMin, spiralSpiritLanceCooldownMax));
            if (IsAttacking())
            {
                yield return new WaitForSeconds(spiritLanceDelayBeforeLaunch);
                for (int i = 0; i < 36; i++)
                {
                    float angle = i * 10f;
                    float radians = angle * Mathf.Deg2Rad;
                    Vector2 offset = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * spiritLanceOffsetRadius;
                    Vector3 spawnPos = transform.position + (Vector3)offset;
                    Quaternion rotation = Quaternion.Euler(0, 0, angle);

                    GameObject lance = Instantiate(spiritLancePrefab, spawnPos, rotation, transform);
                    Collider2D collider = lance.GetComponent<Collider2D>();
                    if (collider != null) collider.enabled = false;

                    lance.transform.SetParent(null);

                    if (collider != null) collider.enabled = true;

                    Rigidbody2D rb = lance.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 dir = (lance.transform.position - transform.position).normalized;
                        rb.linearVelocity = dir * 12f;
                    }

                    yield return new WaitForSeconds(spiralSpiritLanceStepDelay);
                }
            }
        }
    }

    private IEnumerator SoulseekerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(soulseekerCooldownMin, soulseekerCooldownMax));
            if (IsAttacking())
            {
                for (int i = 0; i < 18; i++)
                {
                    float angle = 90f - (i * 10f); // From +90 to -90
                    float radians = angle * Mathf.Deg2Rad;
                    Vector2 offset = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * soulseekerOffsetRadius;
                    Vector3 spawnPos = transform.position + (Vector3)offset;
                    Quaternion rotation = Quaternion.Euler(0, 0, angle);

                    Instantiate(soulseekerPrefab, spawnPos, rotation, transform);
                    yield return new WaitForSeconds(soulseekerStepDelay);
                }
            }
        }
    }

    private IEnumerator SummonDemonicaRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(summonCooldownMin, summonCooldownMax));
            if (!IsAttacking()) continue;

            for (int i = 0; i < summonCount; i++)
            {
                GameObject prefab = demonicaPrefabs[Random.Range(0, demonicaPrefabs.Count)];
                Vector3 spawnPos = transform.position + (Vector3)(Random.insideUnitCircle * 0.5f);
                GameObject summoned = Instantiate(prefab, spawnPos, Quaternion.identity);

                Rigidbody2D rb = summoned.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 explosionDir = (rb.position - (Vector2)transform.position).normalized;
                    rb.AddForce(explosionDir * summonExplosionForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    private void SpawnFireballAtPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || fireballPrefab == null)
            return;

        Vector2 direction = (player.transform.position - transform.position).normalized;

        GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = fireball.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * 10f;
        }
    }

    private void SpawnRingOfFire()
    {
        if (fireballPrefab == null)
            return;

        for (int i = 0; i < 36; i++)
        {
            float angle = i * 10f;
            float radians = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;

            GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = fireball.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * 10f;
            }
        }
    }

    private bool IsAttacking()
    {
        if (enemyScript is Enemy enemy)
            return enemy.IsInAttackState();
        if (enemyScript is BasicEnemy basicEnemy)
            return basicEnemy.IsInAttackState();

        return false;
    }
}
