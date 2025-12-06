using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class BossAI : NetworkBehaviour
{
    [Header("References")]
    public Transform player;
    public BulletHell bulletHell;
    public BulletSpray bulletSpray;

    [Header("AI Settings")]
    public float detectionRadius = 20f;
    public float attackRadius = 3f;
    public float roamRadius = 5f;
    public float roamDelay = 4f;

    [Header("Attack Settings")]
    public float meleeAttackRange = 1.5f; // Very close range for a physical hit
    public float meleeDamage = 10f;       // Damage for the melee hit
    public float attackCooldown = 5f;     // cooldown between alternating attacks
    private float lastAttackTime = -Mathf.Infinity;
    private bool useRingNext = true;       // alternate attacks

    private NavMeshAgent agent;
    private Animator animator;             // Reference to the Animator component
    private Vector3 startPosition;
    private float roamTimer;

    private enum BossState { Idle, Roam, Chase, Attack }
    private BossState state = BossState.Idle;

    #region Unity Methods
    public override void OnStartServer()
    {
        base.OnStartServer();

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (bulletHell == null)
            bulletHell = GetComponent<BulletHell>();

        if (bulletSpray == null)
            bulletSpray = GetComponent<BulletSpray>();

        startPosition = transform.position;
        roamTimer = roamDelay;
    }

    void Update()
    {
        if (!isServer) return;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
            else
                return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case BossState.Idle: HandleIdle(distance); break;
            case BossState.Roam: HandleRoam(distance); break;
            case BossState.Chase: HandleChase(distance); break;
            case BossState.Attack: HandleAttack(distance); break;
        }
    }
    #endregion

    #region AI States
    private void HandleIdle(float distance)
    {
        if (distance <= detectionRadius)
        {
            state = BossState.Chase;
            return;
        }

        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0)
        {
            state = BossState.Roam;
            roamTimer = roamDelay;
        }
    }

    private void HandleRoam(float distance)
    {
        if (distance <= detectionRadius)
        {
            state = BossState.Chase;
            return;
        }

        if (!agent.hasPath)
        {
            Vector3 newPos = startPosition + Random.insideUnitSphere * roamRadius;
            agent.SetDestination(new Vector3(newPos.x, transform.position.y, newPos.z));
        }

        if (agent.remainingDistance <= 1f)
            state = BossState.Idle;
    }

    private void HandleChase(float distance)
    {
        if (distance > detectionRadius * 1.5f)
        {
            state = BossState.Idle;
            return;
        }

        agent.SetDestination(player.position);

        if (distance <= attackRadius)
            state = BossState.Attack;
    }

    private void HandleAttack(float distance)
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            // 1. Priority Attack: Melee (if player is very close)
            if (distance <= meleeAttackRange)
            {
                TriggerMeleeAttack();
            }
            // 2. Ranged Attacks (if player is still in the general attack range)
            else
            {
                if (useRingNext && bulletHell != null && !bulletHell.IsRunning())
                {
                    bulletHell.StartRingSequence();
                    useRingNext = false;
                    lastAttackTime = Time.time;
                }
                else if (!useRingNext && bulletSpray != null && !bulletSpray.IsRunning())
                {
                    bulletSpray.StartSpray();
                    useRingNext = true;
                    lastAttackTime = Time.time;
                }
            }
        }

        if (distance > attackRadius + 2f)
            state = BossState.Chase;
    }

    private void TriggerMeleeAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("MeleeAttack");
        }
        lastAttackTime = Time.time;
    }

    // This function is called by an Animation Event during the attack clip!
    public void InflictMeleeDamage()
    {
        // Check for the player in a small sphere around the boss
        Collider[] hits = Physics.OverlapSphere(transform.position, meleeAttackRange, LayerMask.GetMask("Player"));

        foreach (Collider hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(meleeDamage);
                return;
            }
        }
    }
    #endregion
}