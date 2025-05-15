using UnityEngine;
using TMPro;

public class TextHandler : MonoBehaviour
{
    public GameObject tutorialTextPrefab; // Prefab with Canvas and Text
    public string message = "Press E to interact"; // The message text to display
    public Vector3 offset = new Vector3(15.2f, 0.5f, 0); // Desired offset above the trigger

    private GameObject spawnedText;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && spawnedText == null)
        {
            // Calculate spawn position with offset
            Vector3 spawnPosition = transform.position + offset;
            Debug.Log($"Spawning text at position: {spawnPosition}");

            // Instantiate the tutorial text prefab at the correct position
            spawnedText = Instantiate(tutorialTextPrefab, spawnPosition, Quaternion.identity);

            // Set the message for the text
            TextMeshProUGUI textComponent = spawnedText.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = message;
            }

            // Set the spawned canvas to world space explicitly if not already set
            Canvas canvas = spawnedText.GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;  // Ensure it’s in world space
                RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
                if (canvasRectTransform != null)
                {
                    // Set position directly
                    canvasRectTransform.position = spawnPosition;

                    // Ensure size settings
                    canvasRectTransform.sizeDelta = new Vector2(6f, 1f);  // Set size of canvas (width, height)

                    // Debug log the canvas position
                    Debug.Log($"Canvas position set to: {canvasRectTransform.position}");
                }
            }

            // Ensure the text RectTransform is centered inside the canvas
            RectTransform textRectTransform = textComponent.GetComponent<RectTransform>();
            if (textRectTransform != null)
            {
                // Reset position within the canvas (centered)
                textRectTransform.anchoredPosition = Vector2.zero;

                // Ensure text RectTransform size
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
