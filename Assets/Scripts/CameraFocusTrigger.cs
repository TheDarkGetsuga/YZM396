using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraFocusTrigger : MonoBehaviour
{
    [Header("Platform to Focus On")]
    public Transform platformToFocus;
    public float focusDuration = 1f;
    public float cameraSpeed = 5f;
    public bool trackWhileMoving = true;

    [Header("Cooldown Settings")]
    public float cooldownAfterExit = 2f;

    private bool hasTriggered = false;
    private bool isCoolingDown = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && !isCoolingDown && other.CompareTag("Player") && CameraFollow.Instance != null)
        {
            hasTriggered = true;
            CameraFollow.Instance.FocusTemporarilyOnTarget(platformToFocus, focusDuration, cameraSpeed, trackWhileMoving); // This causes bugs with the player interactions for some magical reason
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = false;
            StartCoroutine(Cooldown());
        }
    }

    private System.Collections.IEnumerator Cooldown()
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(cooldownAfterExit);
        isCoolingDown = false;
    }
}
