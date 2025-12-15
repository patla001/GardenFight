using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Mirror;
using TMPro;

public class Player : NetworkBehaviour
{
    private AudioManager audioManager;
    [SerializeField]
    private Shake camShake;
    public Manager manager;
    private PlayerMovement playerMovement;
    public Collider shieldCollider;
    public int maxHealth = 100;
    private int currentHealth;
    private Vector3 initialPos;
    private Vector3 initialRot;
    [SerializeField]
    private MeshRenderer shield;

    private Animator animator;
    private Animator shootAnimator;
    private NetworkAnimator networkAnimator;

    public Transform hitHolder, blockHolder;
    public GameObject fistHitParticle, swordHitParticle;
    public float shakeDelay = 0.1f;

    [SerializeField]
    private Transform fireballPos;
    [SerializeField]
    private GameObject fireBall;
    [SerializeField]
    private int shootForce = 5;
    [SerializeField]
    public float shootingDelay = 0.1f;

    [SyncVar]
    public bool isDead, isWinner;
    [SyncVar]
    public string ratio;

    private Slider myHealthSlider;
    private TextMeshProUGUI myRatio;
    private int winCount, loseCount;
    private float fireBallShakePosMag, fireBallShakeRotMag;

    private void Start()
    {
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
        audioManager = GetComponent<AudioManager>();
        manager = FindFirstObjectByType<Manager>();
        playerMovement = GetComponent<PlayerMovement>();
        shootAnimator = manager.shootAnimator;

        if (fireBall != null)
        {
            fireBallShakePosMag = fireBall.GetComponent<FireBall>().explosionShakeMag;
            fireBallShakeRotMag = fireBall.GetComponent<FireBall>().explosionRotMag;
        }

        initialPos = transform.position;
        initialRot = transform.eulerAngles;

        SetupHealthBar();
        ResetAll();
    }

    private void SetupHealthBar()
    {
        if (isLocalPlayer)
        {
            myHealthSlider = GameObject.Find("PlayerHealthSlider")?.GetComponent<Slider>();
            myRatio = GameObject.Find("PlayerRatio")?.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            myHealthSlider = GameObject.Find("OponentHealthSlider")?.GetComponent<Slider>();
            myRatio = GameObject.Find("OponentRatio")?.GetComponent<TextMeshProUGUI>();
        }

        if (myHealthSlider == null)
            Debug.LogWarning("PlayerHealthSlider not found - health bar won't display");
        if (myRatio == null)
            Debug.LogWarning("PlayerRatio text not found - ratio won't display");
    }

    private RectTransform healthFillRect;
    private float healthBarWidth = 342f; // barWidth (350) - padding (4*2=8) = 342
    
    private void SetHealthBar(int amount)
    {
        // Try to find slider again if not found yet
        if (myHealthSlider == null)
        {
            myHealthSlider = GameObject.Find("PlayerHealthSlider")?.GetComponent<Slider>();
            if (myHealthSlider != null)
                Debug.Log("Player: Found PlayerHealthSlider!");
        }
        
        // Try to find fill rect if not found yet (unique name: PlayerHealthFill)
        if (healthFillRect == null)
        {
            GameObject fillObj = GameObject.Find("PlayerHealthFill");
            if (fillObj != null)
            {
                healthFillRect = fillObj.GetComponent<RectTransform>();
                if (healthFillRect != null)
                    Debug.Log("Player: Found PlayerHealthFill rect!");
            }
        }
        
        float healthPercent = (float)amount / maxHealth;
        
        if (myHealthSlider != null)
        {
            myHealthSlider.value = healthPercent;
        }
        
        // Directly update the fill bar visual
        if (healthFillRect != null)
        {
            float fillWidth = healthPercent * healthBarWidth;
            healthFillRect.sizeDelta = new Vector2(fillWidth, healthFillRect.sizeDelta.y);
            Debug.Log($"Player: Health bar visual updated to {amount}/{maxHealth} ({healthPercent * 100:F0}%)");
        }
        else
        {
            Debug.Log($"Player: Health value updated to {amount}/{maxHealth} = {healthPercent}");
        }
    }

    public void TakeDamage(int damage, string type)
    {
        Debug.Log($"Player.TakeDamage called! Damage: {damage}, Type: {type}, CurrentHealth: {currentHealth}");
        
        if (isDead) return;

        if (type == "Magic")
            camShake.ShakeCam(fireBallShakePosMag, fireBallShakeRotMag);
        else
            camShake.ShakeCam(shakeDelay);

        if (type == "Sword")
        {
            audioManager.PlaySFX("Sword Hit");
            GameObject particle = Instantiate(swordHitParticle, hitHolder);
            Destroy(particle, 1f);
        }
        else if (type == "Fist")
        {
            audioManager.PlaySFX("Fist Hit");
            GameObject particle = Instantiate(fistHitParticle, hitHolder);
            Destroy(particle, 1f);
        }

        audioManager.PlaySFX("Pain");

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        SetHealthBar(currentHealth);

        if (isLocalPlayer)
        {
            if (currentHealth == 0) Die();
            animator.SetInteger("HitNO", Random.Range(1, 4));
            animator.SetTrigger("GotHit");
        }

        if (currentHealth == 0 && !isLocalPlayer)
        {
            manager.localPlayer.GetComponent<Player>().Win();
        }
    }

    public void BlockAttack(string type)
    {
        animator.SetTrigger("Blocked");
    }

    public void ResetAll()
    {
        manager.disableControl = false;
        isDead = false;
        isWinner = false;
        Attack.isWinner = false;
        animator.SetBool("Dead", false);
        animator.SetBool("Win", false);
        animator.SetBool("Armed", false);

        currentHealth = maxHealth;
        SetHealthBar(maxHealth);

        transform.position = initialPos;
        transform.eulerAngles = initialRot;

        shield.enabled = true;
        GetComponent<PlayerMovement>().isGrounded = true;

        if (isLocalPlayer)
        {
            shootAnimator.Play("Shoot Recovering");
            RefreshRatio();
            
            // Reset the boss for a new fight
            ResetBossFight();
            
            // Clean up UI messages
            CleanupGameMessages();
        }
    }
    
    void ResetBossFight()
    {
        // Find and reset the boss
        BossHealth bossHealth = FindFirstObjectByType<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.ResetBoss();
        }
        
        Debug.Log("Boss fight reset!");
    }
    
    void CleanupGameMessages()
    {
        // Remove Game Over canvas
        GameObject gameOverCanvas = GameObject.Find("GameOverCanvas");
        if (gameOverCanvas != null)
        {
            Destroy(gameOverCanvas);
        }
        
        // Remove Victory canvas
        GameObject victoryCanvas = GameObject.Find("VictoryCanvas");
        if (victoryCanvas != null)
        {
            Destroy(victoryCanvas);
        }
        
        Debug.Log("Game messages cleaned up");
    }

    public void RefreshRatio()
    {
        winCount = PlayerPrefs.GetInt("Win Count");
        loseCount = PlayerPrefs.GetInt("Lose Count");

        if (winCount == 0 && loseCount == 0)
            ratio = "First Match!";
        else if (loseCount == 0)
            ratio = "Undefeated!";
        else
            ratio = ((float)winCount / loseCount).ToString("F2");

        GetComponent<ActionControl>().SetOwnRatio(netId.ToString(), ratio);

        if (myRatio != null)
            myRatio.SetText(ratio);
    }

    public void SetMyRatio(string newRatio)
    {
        SetupHealthBar();
        ratio = newRatio;
        if (myRatio != null)
            myRatio.SetText(ratio);
    }

    public void Die()
    {
        isDead = true;
        PlayerPrefs.SetInt("Lose Count", PlayerPrefs.GetInt("Lose Count") + 1);
        PlayerPrefs.Save(); // Save immediately
        RefreshRatio();
        animator.SetBool("Dead", true);
        manager.reMatchButton.gameObject.SetActive(true);
        manager.disableControl = true;
        audioManager.PlaySFX("Defeat");
        print("I am dead :(");
        
        // Stop the boss from attacking
        StopBossAttacks();
        
        // Show Game Over message
        ShowGameOverMessage();
    }
    
    void StopBossAttacks()
    {
        // Find and stop the boss AI
        BossAI bossAI = FindFirstObjectByType<BossAI>();
        if (bossAI != null)
        {
            bossAI.enabled = false;
            Debug.Log("Boss AI stopped - player died");
        }
        
        // Stop boss attack scripts
        BulletHell bulletHell = FindFirstObjectByType<BulletHell>();
        if (bulletHell != null) bulletHell.enabled = false;
        
        BulletSpray bulletSpray = FindFirstObjectByType<BulletSpray>();
        if (bulletSpray != null) bulletSpray.enabled = false;
        
        LaserAttack laserAttack = FindFirstObjectByType<LaserAttack>();
        if (laserAttack != null) laserAttack.enabled = false;
    }
    
    void ShowGameOverMessage()
    {
        // Check if message already exists
        GameObject gameOverObj = GameObject.Find("GameOverText");
        if (gameOverObj != null) return;
        
        // Create a canvas for game over message
        GameObject canvasObj = new GameObject("GameOverCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // Lower than buttons so it doesn't block them
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        
        // Create game over text - positioned between rematch button and cancel button
        gameOverObj = new GameObject("GameOverText");
        gameOverObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform rect = gameOverObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.4f); // Middle-lower area
        rect.anchorMax = new Vector2(0.5f, 0.4f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(800, 100);
        
        TMPro.TextMeshProUGUI text = gameOverObj.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = "GAME OVER";
        text.fontSize = 64;
        text.fontStyle = TMPro.FontStyles.Bold;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.color = Color.red;
        text.outlineWidth = 0.2f;
        text.outlineColor = Color.black;
        
        Debug.Log("Game Over message displayed");
    }

    public void Win()
    {
        if (isWinner) return; // Prevent multiple wins
        
        isWinner = true;
        PlayerPrefs.SetInt("Win Count", PlayerPrefs.GetInt("Win Count") + 1);
        PlayerPrefs.Save(); // Save immediately
        RefreshRatio();
        animator.SetBool("Win", true);
        audioManager.PlaySFX("Victory");
        manager.disableControl = true;
        // Don't show rematch button on victory - player has won!
        print("I win :D - Win count saved!");
    }

    private void OnDisable()
    {
        if (myHealthSlider != null)
            myHealthSlider.value = 0f;
    }

    public void DisableShield()
    {
        shieldCollider.enabled = false;
    }

    public void EnableShield()
    {
        shieldCollider.enabled = true;
    }

    public void HideShield()
    {
        shield.enabled = false;
    }

    public void ShootFireball()
    {
        Invoke("Shooting", shootingDelay);
    }

    public void Shooting()
    {
        GameObject tempFireBall = Instantiate(fireBall, fireballPos);
        tempFireBall.GetComponent<Rigidbody>().AddForce(fireballPos.forward * shootForce, ForceMode.Impulse);
        audioManager.PlaySFX("Magic Shoot");
    }
}
