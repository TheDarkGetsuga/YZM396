using UnityEngine;
using TMPro;

public class TextHandler : MonoBehaviour
{
    public GameObject tutorialTextPrefab;
    public string message = "Press E to interact";
    public Vector3 offset = new Vector3(15.2f, 0.5f, 0);

    private GameObject spawnedText;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && spawnedText == null)
        {
            Vector3 spawnPosition = transform.position + offset;
            Debug.Log($"Spawning text at position: {spawnPosition}");
            spawnedText = Instantiate(tutorialTextPrefab, spawnPosition, Quaternion.identity);
            TextMeshProUGUI textComponent = spawnedText.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = message;
            }
            // Set the spawned canvas to world space explicitly if not already set otherwise it might not display correctly
            Canvas canvas = spawnedText.GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;  // Ensure it’s in world space
                RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
                if (canvasRectTransform != null)
                {
                    canvasRectTransform.position = spawnPosition;
                    canvasRectTransform.sizeDelta = new Vector2(6f, 1f); //W,H
                    Debug.Log($"Canvas position set to: {canvasRectTransform.position}");
                }
            }
            RectTransform textRectTransform = textComponent.GetComponent<RectTransform>();
            if (textRectTransform != null)
            {
                textRectTransform.anchoredPosition = Vector2.zero;
                textRectTransform.sizeDelta = new Vector2(6f, 1f);  // Set size of text box (width, height)
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && spawnedText != null)
        {
            Destroy(spawnedText);
        }
    }
}
