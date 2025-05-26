using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal; // Needed for Light2D

public class FireballShooterManager : MonoBehaviour
{
    public static FireballShooterManager Instance;

    [Header("Fireball Timing")]
    public float cooldownMin = 4f;
    public float cooldownMax = 6f;

    [Header("Warning Lights")]
    public GameObject warningLights; // Parent object with Light2D children

    [Tooltip("Maximum intensity of the pulsing lights")]
    public float pulseMaxIntensity = 1.2f;

    [Tooltip("Pulsing frequency (cycles per second)")]
    public float pulseFrequency = 4f;

    [HideInInspector] public bool shouldShoot = false;

    private Light2D[] lightComponents;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (warningLights != null)
        {
            lightComponents = warningLights.GetComponentsInChildren<Light2D>(includeInactive: true);
            foreach (var light in lightComponents)
            {
                light.intensity = 0f;
                light.enabled = true;
            }
        }

        StartCoroutine(SynchronizedFireballTimer());
    }

    private IEnumerator SynchronizedFireballTimer()
    {
        while (true)
        {
            yield return new WaitUntil(() => CameraFollow.Instance.IsInBossFightMode());

            while (CameraFollow.Instance.IsInBossFightMode())
            {
                float waitTime = Random.Range(cooldownMin, cooldownMax);
                yield return new WaitForSeconds(waitTime - 1f); // 1 second before firing

                // Smooth pulse for 1 second
                if (lightComponents != null)
                    yield return StartCoroutine(PulseLights(1f));

                // Fire
                shouldShoot = true;
                yield return null;
                shouldShoot = false;

                // Turn off lights
                if (lightComponents != null)
                {
                    foreach (var light in lightComponents)
                    {
                        light.intensity = 0f;
                    }
                }
            }

            yield return null;
        }
    }

    private IEnumerator PulseLights(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            float pulse = Mathf.Sin(timer * Mathf.PI * 2 * pulseFrequency) * 0.5f + 0.5f;
            float intensity = pulse * pulseMaxIntensity;

            foreach (var light in lightComponents)
            {
                light.intensity = intensity;
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
