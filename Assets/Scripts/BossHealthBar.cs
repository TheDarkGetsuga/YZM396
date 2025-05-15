using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    [Header("References")]
    public RectTransform fillBar;           // The red fill bar (should be inside the background)
    public TMP_Text nameText;               // Text displaying the boss name

    [Header("Custom Size")]
    public float customWidth = 800f;        // Total width of the health bar
    public float customHeight = 20f;        // Height of the health bar

    private float maxHealth;

    void Start()
    {
        ResizeBar(customWidth, customHeight);
        gameObject.SetActive(false); // Hide by default
    }

    /// <summary>
    /// Initializes the health bar with max health and name.
    /// </summary>
    public void Init(float maxHealth)
    {
        this.maxHealth = maxHealth;
        SetHealth(maxHealth);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Updates the health bar to match current health.
    /// </summary>
    public void SetHealth(float currentHealth)
    {
        float normalized = Mathf.Clamp01(currentHealth / maxHealth);
        float newWidth = customWidth * normalized;

        // Resize the fill bar
        fillBar.sizeDelta = new Vector2(newWidth, customHeight);

        // Center it so it shrinks inward from both ends
        float offsetX = (customWidth - newWidth) / 2f;
        fillBar.anchoredPosition = new Vector2(offsetX, 0f);
    }

    /// <summary>
    /// Sets the boss name text.
    /// </summary>
    public void SetName(string bossName)
    {
        if (nameText != null)
            nameText.text = bossName;
    }

    /// <summary>
    /// Resize the bar dimensions.
    /// </summary>
    public void ResizeBar(float width, float height)
    {
        customWidth = width;
        customHeight = height;

        // Resize background
        RectTransform background = GetComponent<RectTransform>();
        background.sizeDelta = new Vector2(width, height);

        // Resize fill bar to match
        if (fillBar != null)
        {
            fillBar.sizeDelta = new Vector2(width, height);
            fillBar.anchoredPosition = Vector2.zero;
        }
    }

    /// <summary>
    /// Hide the health bar.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
