using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    [Header("References")]
    public RectTransform fillBar;
    public TMP_Text nameText;

    [Header("Custom Size")]
    public float customWidth = 800f;
    public float customHeight = 20f;

    private float maxHealth;

    // This isnt working and i have no idea why sky pls help aaaaaaaaaaaaa
    void Start()
    {
        ResizeBar(customWidth, customHeight);
        gameObject.SetActive(false);
    }
    public void Init(float maxHealth) //prolly here
    {
        this.maxHealth = maxHealth;
        SetHealth(maxHealth);
        gameObject.SetActive(true);
    }
    public void SetHealth(float currentHealth)
    {
        float normalized = Mathf.Clamp01(currentHealth / maxHealth);
        float newWidth = customWidth * normalized;
        fillBar.sizeDelta = new Vector2(newWidth, customHeight);
        float offsetX = (customWidth - newWidth) / 2f;
        fillBar.anchoredPosition = new Vector2(offsetX, 0f);
    }
    public void SetName(string bossName)
    {
        if (nameText != null)
            nameText.text = bossName;
    }
    public void ResizeBar(float width, float height)
    {
        customWidth = width;
        customHeight = height;
        RectTransform background = GetComponent<RectTransform>();
        background.sizeDelta = new Vector2(width, height);
        if (fillBar != null)
        {
            fillBar.sizeDelta = new Vector2(width, height);
            fillBar.anchoredPosition = Vector2.zero;
        }
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
