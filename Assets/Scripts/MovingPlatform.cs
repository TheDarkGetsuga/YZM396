using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour
{
    public float moveAmountX = 0f;
    public float moveAmountY = 3f;
    public float moveSpeed = 5f;
    public float activeDuration = 2f; // Time the platform stays at each position

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isAtTarget = false;
    private Coroutine moveCoroutine;

    private void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + new Vector3(moveAmountX, moveAmountY, 0f);
        StartCoroutine(MoveLoop());
    }

    private IEnumerator MoveLoop()
    {
        while (true)
        {
            isAtTarget = !isAtTarget;

            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);

            moveCoroutine = StartCoroutine(MovePlatform(isAtTarget ? targetPosition : startPosition));

            yield return new WaitForSeconds(activeDuration);
        }
    }

    private IEnumerator MovePlatform(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
    }
}
