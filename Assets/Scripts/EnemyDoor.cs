using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyDoor : MonoBehaviour
{
    [Header("Enemy Tracking")]
    public List<GameObject> enemyList = new List<GameObject>();

    [Header("Door Movement")]
    public float moveAmountY = 3f;
    public float moveSpeed = 5f;

    [Header("Camera Tracking")]
    [SerializeField] private bool trackDoor = true;
    [SerializeField] private bool trackMovingDoor = true;

    private bool isOpen = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Coroutine moveCoroutine;

    private void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + new Vector3(0f, moveAmountY, 0f);
    }

    private void Update()
    {
        if (!isOpen && AllEnemiesDefeated())
        {
            OpenDoor();
        }
    }

    private bool AllEnemiesDefeated()
    {
        // Clean up null references in case enemies were destroyed
        enemyList.RemoveAll(enemy => enemy == null);
        return enemyList.Count == 0;
    }

    private void OpenDoor()
    {
        isOpen = true;

        if (trackDoor && CameraFollow.Instance != null)
        {
            CameraFollow.Instance.FocusTemporarilyOnTarget(transform, 1f, 5f, trackMovingDoor);
        }

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveDoor(openPosition));
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
