using UnityEngine;

public class Soulseeker : MonoBehaviour
{
    [Header("Homing Settings")]
    public float delayBeforeHoming = 1f;
    public float outwardDuration = 0.4f;
    public float outwardSpeed = 3f;
    public float homingSpeed = 6f;
    public float seekRadius = 15f;
    public float rotationOffset = -45f;

    [Header("Effects")]
    public GameObject impactEffectPrefab;

    [Header("Audio")]
    public AudioClip spawnSound;
    public AudioClip[] impactSounds;
    public float volume = 1f;

    private Transform player;
    private Vector2 moveDirection;
    private bool isHoming = false;
    private Collider2D projectileCollider;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();
        if (projectileCollider != null)
            projectileCollider.enabled = false;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Start()
    {
        PlaySoundAtPosition(spawnSound, transform.position, volume);
        moveDirection = (transform.position - transform.parent.position).normalized;
        transform.SetParent(null);
        StartCoroutine(BehaviorRoutine());
    }

    private System.Collections.IEnumerator BehaviorRoutine()
    {
        float elapsed = 0f;

        while (elapsed < outwardDuration)
        {
            rb.linearVelocity = moveDirection * outwardSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(delayBeforeHoming);

        if (projectileCollider != null)
            projectileCollider.enabled = true;

        StartHoming();
    }

    private void StartHoming()
    {
        isHoming = true;

        if (player != null && Vector2.Distance(transform.position, player.position) <= seekRadius)
        {
            moveDirection = (player.position - transform.position).normalized;
            RotateTowards(moveDirection);
        }
        else
        {
            moveDirection = transform.right.normalized;
        }
    }

    private void Update()
    {
        if (isHoming)
        {
            transform.position += (Vector3)(moveDirection * homingSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (impactEffectPrefab)
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);

        if (impactSounds != null && impactSounds.Length > 0)
        {
            int index = Random.Range(0, impactSounds.Length);
            PlaySoundAtPosition(impactSounds[index], transform.position, volume);
        }
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerHP hp = other.gameObject.GetComponent<PlayerHP>();
            if (hp != null)
            {
                hp.TakeDamage(5, transform.position);
        Destroy(gameObject);   
            }
        }
    }
    private void RotateTowards(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += rotationOffset;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;

        GameObject soundObj = new GameObject("TempAudio_" + clip.name);
        soundObj.transform.position = position;

        AudioSource audioSource = soundObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 20f;
        audioSource.Play();

        Destroy(soundObj, clip.length);
    }
}
