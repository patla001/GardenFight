using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Creates and manages the Player Health UI
/// Attach this to any GameObject - it will create its own Canvas!
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI Settings")]
    public Color healthBarColor = new Color(0f, 1f, 0f); // Bright Green
    public Color healthBarBackgroundColor = new Color(0.5f, 0f, 0f); // Dark Red
    public Color borderColor = Color.white;
    
    [Header("Position (Bottom-Left)")]
    public float marginLeft = 20f;
    public float marginBottom = 100f; // Raised above keyboard controls
    public float barWidth = 350f;
    public float barHeight = 40f;
    
    // References
    private Slider playerHealthSlider;
    private Image fillImage;
    private RectTransform fillRect;
    private float lastHealthValue = 1f;
    
    void Awake()
    {
        CreateHealthBarUI();
    }
    
    void Update()
    {
        // Update visual directly using stored references from CreateHealthBarUI
        if (playerHealthSlider != null && fillRect != null)
        {
            float currentValue = playerHealthSlider.value;
            if (Mathf.Abs(currentValue - lastHealthValue) > 0.001f)
            {
                // Calculate fill width: barWidth=350, padding=4 on each side, so max fill = 342
                float maxFillWidth = 342f;
                float fillWidth = currentValue * maxFillWidth;
                fillRect.sizeDelta = new Vector2(fillWidth, fillRect.sizeDelta.y);
                lastHealthValue = currentValue;
                Debug.Log($"PlayerHealthUI: Visual updated to {currentValue * 100:F0}%");
            }
        }
    }
    
    void CreateHealthBarUI()
    {
        // Create a dedicated Canvas for the health bar
        GameObject canvasObj = new GameObject("PlayerHealthCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create container (this is the "PlayerHealthSlider" that Player.cs finds)
        GameObject container = new GameObject("PlayerHealthSlider");
        container.transform.SetParent(canvasObj.transform, false);
        
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0); // Bottom-left
        containerRect.anchorMax = new Vector2(0, 0);
        containerRect.pivot = new Vector2(0, 0);
        // Force consistent sizing (ignore serialized values)
        float width = 350f;
        float height = 40f;
        float padding = 4f;
        
        containerRect.anchoredPosition = new Vector2(20f, 250f);
        containerRect.sizeDelta = new Vector2(width, height);
        
        // Add Slider component (for value storage)
        playerHealthSlider = container.AddComponent<Slider>();
        playerHealthSlider.minValue = 0;
        playerHealthSlider.maxValue = 1;
        playerHealthSlider.value = 1;
        playerHealthSlider.interactable = false;
        playerHealthSlider.transition = Selectable.Transition.None;
        
        // Create Border
        GameObject border = new GameObject("Border");
        border.transform.SetParent(container.transform, false);
        RectTransform borderRect = border.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;
        Image borderImage = border.AddComponent<Image>();
        borderImage.color = borderColor;
        
        // Create Background (shows when health is lost - dark red)
        GameObject background = new GameObject("Background");
        background.transform.SetParent(container.transform, false);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = new Vector2(padding, padding);
        bgRect.offsetMax = new Vector2(-padding, -padding);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = healthBarBackgroundColor;
        
        // Create Fill (green bar that shrinks) - unique name for easy finding
        GameObject fill = new GameObject("PlayerHealthFill");
        fill.transform.SetParent(container.transform, false);
        fillRect = fill.AddComponent<RectTransform>();
        // Use fixed size, anchored to bottom-left of container
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(0, 0);
        fillRect.pivot = new Vector2(0, 0);
        fillRect.anchoredPosition = new Vector2(padding, padding);
        float fillMaxWidth = width - (padding * 2);
        float fillHeight = height - (padding * 2);
        fillRect.sizeDelta = new Vector2(fillMaxWidth, fillHeight);
        
        // Store the max width for updates
        barWidth = width;
        barHeight = height;
        
        fillImage = fill.AddComponent<Image>();
        fillImage.color = healthBarColor;
        
        // Create Label
        GameObject label = new GameObject("HealthLabel");
        label.transform.SetParent(container.transform, false);
        RectTransform labelRect = label.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.pivot = new Vector2(0.5f, 0);
        labelRect.anchoredPosition = new Vector2(0, 5);
        labelRect.sizeDelta = new Vector2(0, 25);
        
        TextMeshProUGUI labelText = label.AddComponent<TextMeshProUGUI>();
        labelText.text = "PLAYER HEALTH";
        labelText.fontSize = 18;
        labelText.fontStyle = FontStyles.Bold;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = Color.white;
        
        Debug.Log("PlayerHealthUI: Created health bar in bottom-left corner!");
    }
    
    void OnDestroy()
    {
        // Clean up the canvas we created
        GameObject canvas = GameObject.Find("PlayerHealthCanvas");
        if (canvas != null)
        {
            Destroy(canvas);
        }
    }
}
