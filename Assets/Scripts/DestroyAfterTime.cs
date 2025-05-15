using UnityEngine;
using System.Collections;

public class DestroyAfterTime : MonoBehaviour
{
    public float lifetime = 3f;  // Total time before gib is destroyed
    public float fadeDuration = 1f;  // Duration for the fade effect
    private SpriteRenderer spriteRenderer;
    private float timer = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on " + gameObject.name);
            return;
        }

        // Start the fadeout process after the lifetime has passed
        StartCoroutine(DestroyAfterFade());
    }

    private IEnumerator DestroyAfterFade()
    {
        // Wait for the full lifetime duration before starting the fadeout
        yield return new WaitForSeconds(lifetime);

        // Now start the fadeout process
        float fadeTimer = 0f;
        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;

            yield return null;
        }

        // Once the fade is complete, destroy the object
        Destroy(gameObject);
    }
}
