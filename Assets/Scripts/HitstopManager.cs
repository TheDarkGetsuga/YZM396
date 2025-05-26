using UnityEngine;
using System.Collections;

public class HitstopManager : MonoBehaviour
{
    public static HitstopManager Instance;

    private bool isHitstopActive = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void TriggerHitstop(float duration)
    {
        if (!isHitstopActive)
            StartCoroutine(DoHitstop(duration));
    }

    private IEnumerator DoHitstop(float duration)
    {
        //isHitstopActive = true;
        //Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        //Time.timeScale = 1f;
        //isHitstopActive = false;
    }
}
