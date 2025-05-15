using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    public Lever[] requiredLevers;
    public float moveAmountY = 3f;
    public float moveSpeed = 5f; // How fast the door moves

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;
    private Coroutine moveCoroutine;

    private void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + new Vector3(0f, moveAmountY, 0f);
    }

    public void CheckLevers()
    {
        foreach (Lever lever in requiredLevers)
        {
            if (!lever.isActivated)
            {
                CloseDoor();
                return;
            }
        }

        OpenDoor();
    }

    private void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveDoor(openPosition));
    }

    private void CloseDoor()
    {
        if (!isOpen) return;
        isOpen = false;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveDoor(closedPosition));
    }

    private IEnumerator MoveDoor(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
    }
}
