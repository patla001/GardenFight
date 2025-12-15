using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles boss health, damage, and death
/// </summary>
public class BossHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100;
    public float currentHealth;
    public Slider healthSlider;
    
    [Header("Death Settings")]
    public string deathAnimationTrigger = "Die";
    public float deathDelay = 3f; // Time before hiding the boss
    
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return; // Don't take damage if already dead
        
        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        Debug.Log($"Boss took {dmg} damage! Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return; // Prevent multiple calls
        isDead = true;
        
        Debug.Log("Boss Dragon Defeated!");
        
        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger(deathAnimationTrigger);
        }
        
        // Stop the boss AI
        var bossAI = GetComponent<BossAI>();
        if (bossAI != null)
        {
            bossAI.enabled = false;
        }
        
        // Stop any attack scripts
        var bulletHell = GetComponent<BulletHell>();
        if (bulletHell != null) bulletHell.enabled = false;
        
        var bulletSpray = GetComponent<BulletSpray>();
        if (bulletSpray != null) bulletSpray.enabled = false;
        
        var laserAttack = GetComponent<LaserAttack>();
        if (laserAttack != null) laserAttack.enabled = false;
        
        // Trigger player win (this saves the win count)
        TriggerPlayerWin();
        
        // Show victory UI
        ShowVictoryMessage();
        
        // Optionally hide/destroy boss after delay
        Invoke("HideBoss", deathDelay);
    }
    
    void TriggerPlayerWin()
    {
        // Find the local player and trigger their win
        Player player = FindFirstObjectByType<Player>();
        if (player != null && player.isLocalPlayer)
        {
            player.Win();
            Debug.Log("Player win triggered - stats saved!");
        }
        else
        {
            // If player not found directly, try finding by name
            GameObject playerObj = GameObject.Find("local player");
            if (playerObj != null)
            {
                Player p = playerObj.GetComponent<Player>();
                if (p != null)
                {
                    p.Win();
                    Debug.Log("Player win triggered via local player object!");
                }
            }
        }
        
        // Also save PlayerPrefs immediately
        PlayerPrefs.Save();
    }
    
    void ShowVictoryMessage()
    {
        // Try to find existing victory text or create one
        GameObject victoryObj = GameObject.Find("VictoryText");
        if (victoryObj == null)
        {
            // Create a canvas for victory message
            GameObject canvasObj = new GameObject("VictoryCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200; // On top of everything
            canvasObj.AddComponent<CanvasScaler>();
            
            // Create victory text
            victoryObj = new GameObject("VictoryText");
            victoryObj.transform.SetParent(canvasObj.transform, false);
            
            RectTransform rect = victoryObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(800, 200);
            
            TextMeshProUGUI text = victoryObj.AddComponent<TextMeshProUGUI>();
            text.text = "VICTORY!\nBoss Defeated!";
            text.fontSize = 72;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.yellow;
            
            // Add outline for visibility
            text.outlineWidth = 0.2f;
            text.outlineColor = Color.black;
        }
        
        Debug.Log("Victory! Boss has been defeated!");
    }
    
    void HideBoss()
    {
        // Disable or hide the boss
        gameObject.SetActive(false);
        Debug.Log("Boss hidden after death");
    }
    
    // Public method to check if boss is dead
    public bool IsDead()
    {
        return isDead;
    }
    
    // Reset the boss for a new fight
    public void ResetBoss()
    {
        isDead = false;
        currentHealth = maxHealth;
        
        if (healthSlider != null)
            healthSlider.value = maxHealth;
        
        // Re-enable the boss
        gameObject.SetActive(true);
        
        // Re-enable AI and attacks
        var bossAI = GetComponent<BossAI>();
        if (bossAI != null) bossAI.enabled = true;
        
        var bulletHell = GetComponent<BulletHell>();
        if (bulletHell != null) bulletHell.enabled = true;
        
        var bulletSpray = GetComponent<BulletSpray>();
        if (bulletSpray != null) bulletSpray.enabled = true;
        
        var laserAttack = GetComponent<LaserAttack>();
        if (laserAttack != null) laserAttack.enabled = true;
        
        // Reset animator
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
        
        Debug.Log("Boss has been reset for new fight!");
    }
}
