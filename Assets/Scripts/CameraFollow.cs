using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Vector3 baseOffset = new Vector3(0f, 1f, -10f);
    private Vector3 bossOffset = new Vector3(0f, 4f, -10f); // 1 + 3 = 4 for boss offset
    private Vector3 offset;

    private float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;

    private bool isInCombat = false;
    private bool isInBossFight = false;

    [SerializeField] private float combatZoomMultiplier = 0.8f; // 20% zoom in
    [SerializeField] private float bossZoomMultiplier = 2f;     // 100% zoom out

    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody2D targetRb;
    [SerializeField] private float defaultZoom = 5f;
    [SerializeField] private float fallZoom = 8f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float fallOffsetY = -3f;
    [SerializeField] private float movementSpeedThreshold = 19.5f;

    private float shakeDuration = 0f;
    private float shakeStrengthX = 0f;
    private float shakeStrengthY = 0f;

    public static CameraFollow Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        offset = baseOffset;
    }

    void Update()
    {
        Vector3 targetPosition = target.position + offset;

        if (Mathf.Abs(targetRb.linearVelocity.y) > movementSpeedThreshold)
        {
            if (targetRb.linearVelocity.y < 0)
            {
                targetPosition.y += fallOffsetY;
            }
        }

        // Screen shake logic
        if (shakeDuration > 0)
        {
            targetPosition.x += Random.Range(-shakeStrengthX, shakeStrengthX);
            targetPosition.y += Random.Range(-shakeStrengthY, shakeStrengthY);
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            shakeStrengthX = 0f;
            shakeStrengthY = 0f;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        float targetZoom;

        if (Mathf.Abs(targetRb.linearVelocity.y) > movementSpeedThreshold)
        {
            targetZoom = fallZoom;
        }
        else if (isInBossFight)
        {
            targetZoom = defaultZoom * bossZoomMultiplier;
        }
        else if (isInCombat)
        {
            targetZoom = defaultZoom * combatZoomMultiplier;
        }
        else
        {
            targetZoom = defaultZoom;
        }

        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, targetZoom, zoomSpeed * Time.deltaTime);
    }

    public void ScreenShake(float xStrength, float yStrength, float duration)
    {
        shakeStrengthX = xStrength;
        shakeStrengthY = yStrength;
        shakeDuration = duration;
    }

    public void SetCombatMode(bool active)
    {
        isInCombat = active;
    }

    public void SetBossFightMode(bool active)
    {
        isInBossFight = active;
        offset = active ? bossOffset : baseOffset;
    }
}
