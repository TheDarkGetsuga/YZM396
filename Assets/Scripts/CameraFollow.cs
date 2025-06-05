using UnityEngine;
using System.Collections;
// https://www.youtube.com/watch?v=ZBj3LBA2vUY
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

    private bool isTemporarilyFocusing = false;

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
        if (!isTemporarilyFocusing)
        {
            Vector3 targetPosition = target.position + offset;

            if (Mathf.Abs(targetRb.linearVelocity.y) > movementSpeedThreshold && targetRb.linearVelocity.y < 0)
            {
                targetPosition.y += fallOffsetY;
            }
            if (shakeDuration > 0)
            {
                targetPosition.x += Random.Range(-shakeStrengthX, shakeStrengthX);
                targetPosition.y += Random.Range(-shakeStrengthY, shakeStrengthY);
                shakeDuration -= Time.unscaledDeltaTime;
            }
            else
            {
                shakeStrengthX = 0f;
                shakeStrengthY = 0f;
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }

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

        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, targetZoom, zoomSpeed * Time.unscaledDeltaTime);
    }

    public void FocusTemporarilyOnTarget(Transform targetTransform, float duration = 1f, float moveSpeed = 5f, bool trackDuringWait = false)
    {
        StopAllCoroutines();
        StartCoroutine(MoveToTargetAndBack(targetTransform, duration, moveSpeed, trackDuringWait));
    }


    private IEnumerator MoveToTargetAndBack(Transform targetTransform, float duration, float moveSpeed, bool trackDuringWait = false)
    {
        isTemporarilyFocusing = true;

        Vector3 originalPosition = transform.position;
        Vector3 focusPosition = new Vector3(targetTransform.position.x, targetTransform.position.y, originalPosition.z);

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            transform.position = Vector3.Lerp(originalPosition, focusPosition, elapsed);
            elapsed += Time.unscaledDeltaTime * moveSpeed;
            yield return null;
        }
        transform.position = focusPosition;

        float waitTime = 0f;
        while (waitTime < duration)
        {
            if (trackDuringWait && targetTransform != null)
            {
                Vector3 updatedFocus = new Vector3(targetTransform.position.x, targetTransform.position.y, originalPosition.z);
                transform.position = Vector3.Lerp(transform.position, updatedFocus, Time.unscaledDeltaTime * moveSpeed);
            }

            waitTime += Time.unscaledDeltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < 1f)
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition, elapsed);
            elapsed += Time.unscaledDeltaTime * moveSpeed;
            yield return null;
        }
        transform.position = originalPosition;

        isTemporarilyFocusing = false;
    }


    public void ScreenShake(float xStrength, float yStrength, float duration)
    {
        //bi daha screenshake yazanı siksinler https://github.com/andersonaddo/EZ-Camera-Shake-Unity
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

    public bool IsInBossFightMode()
    {
        return isInBossFight;
    }
}
