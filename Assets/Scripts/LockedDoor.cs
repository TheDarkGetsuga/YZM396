using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    public string requiredKeyName;
    public float moveAmountY = 3f;
    public float moveSpeed = 5f;
    public PlayerKeyInventory keyInventory; // ← Assign this in the Inspector

    private bool isOpen = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Coroutine moveCoroutine;

    private bool playerInRange = false;

    private void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + new Vector3(0f, moveAmountY, 0f);
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            TryOpen();
        }
    }

    private void TryOpen()
    {
        if (isOpen) return;

        if (keyInventory != null && keyInventory.HasKey(requiredKeyName))
        {
            Debug.Log("Key found! Opening door...");
            OpenDoor();
            keyInventory.RemoveKey(requiredKeyName);
        }
        else
        {
            Debug.Log("Missing key: " + requiredKeyName);
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveDoor(openPosition));
    }

    private System.Collections.IEnumerator MoveDoor(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
