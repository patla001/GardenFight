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
            Debug.LogError("PlayerHealthSlider not found!");
        if (myRatio == null)
            Debug.LogError("PlayerRatio text not found!");
    }

    private void SetHealthBar(int amount)
    {
        if (myHealthSlider != null)
            myHealthSlider.value = (float)amount / maxHealth;
    }

    public void TakeDamage(int damage, string type)
    {
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
        }
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
        RefreshRatio();
        animator.SetBool("Dead", true);
        manager.reMatchButton.gameObject.SetActive(true);
        manager.disableControl = true;
        audioManager.PlaySFX("Defeat");
        print("I am dead :(");
    }

    public void Win()
    {
        isWinner = true;
        PlayerPrefs.SetInt("Win Count", PlayerPrefs.GetInt("Win Count") + 1);
        RefreshRatio();
        animator.SetBool("Win", true);
        audioManager.PlaySFX("Victory");
        manager.disableControl = true;
        print("I win :D");
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
