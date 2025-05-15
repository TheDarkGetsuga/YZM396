using UnityEngine;
using System.Collections;

public class SpikeColumn : MonoBehaviour
{
    public float moveAmountY = 3f;
    public float moveSpeed = 5f;
    public float activeDuration = 2f; // Time the spike stays up or down

    private Vector3 downPosition;
    private Vector3 upPosition;
    private bool isUp = false;
    private Coroutine moveCoroutine;

    private void Start()
    {
        downPosition = transform.position;
        upPosition = downPosition + new Vector3(0f, moveAmountY, 0f);
        StartCoroutine(SpikeRoutine());
    }

    private IEnumerator SpikeRoutine()
    {
        while (true)
        {
            // Toggle position
            isUp = !isUp;

            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);

            moveCoroutine = StartCoroutine(MoveSpike(isUp ? upPosition : downPosition));

            yield return new WaitForSeconds(activeDuration);
        }
    }

    private IEnumerator MoveSpike(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
    }
}
