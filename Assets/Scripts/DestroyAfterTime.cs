using UnityEngine;
using System.Collections;
//gibbieeee
public class DestroyAfterTime : MonoBehaviour
{
    public float lifetime = 3f;
    public float fadeDuration = 1f;
    private SpriteRenderer spriteRenderer;
    private float timer = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(DestroyAfterFade());
    }
    private IEnumerator DestroyAfterFade()
    {
        yield return new WaitForSeconds(lifetime);
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
        Destroy(gameObject);
    }
}
