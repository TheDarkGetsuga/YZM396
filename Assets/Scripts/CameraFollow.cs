using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Vector3 offset = new Vector3(0f, 1f, -10f);
    private float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;

    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody2D targetRb;
    [SerializeField] private float defaultZoom = 5f;
    [SerializeField] private float fallZoom = 8f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float fallOffsetY = -3f;
    [SerializeField] private float movementSpeedThreshold = 19.5f;

    private Vector3 originalPosition;
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

        // Başka scriptlerde çağırmak için "CameraFollow.Instance.ScreenShake(X, Y, Duration);" kullan
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
        float targetZoom = (Mathf.Abs(targetRb.linearVelocity.y) > movementSpeedThreshold) ? fallZoom : defaultZoom;
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, targetZoom, zoomSpeed * Time.deltaTime);
    }
    public void ScreenShake(float xStrength, float yStrength, float duration)
    {
        shakeStrengthX = xStrength;
        shakeStrengthY = yStrength;
        shakeDuration = duration;
    }
}
