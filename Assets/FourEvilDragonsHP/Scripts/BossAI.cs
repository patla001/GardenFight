using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class BossAI : NetworkBehaviour
{
    [Header("References")]
    public Transform player; // reference to the player object we’re targeting
    public BulletHell bulletHell; // script that controls the bullet ring attack
    public BulletSpray bulletSpray; // script that controls the bullet spray attack
    public LaserAttack laserAttack; // script that controls the laser attack

    [Header("AI Settings")]
    public float detectionRadius = 20f; // how far away the boss can detect the player
    public float attackRadius = 3f; // how close the player must be before the boss attacks
    public float roamRadius = 5f; // radius around the spawn point for idle roaming
    public float roamDelay = 4f; // delay before the boss decides to roam again

    [Header("Attack Settings")]
    public float meleeAttackRange = 1.5f; // distance required for melee attack
    public float meleeDamage = 10f; // damage dealt by melee attack
    public float attackCooldown = 5f; // minimum time between attacks
    private float lastAttackTime = -Mathf.Infinity; // keeps track of last attack time

    private bool useRingNext = true; // flag to alternate between ring and spray attacks
    private bool useLaserNext = false; // flag to determine when laser should be used

    [Header("Laser Cooldown")]
    public float laserCooldown = 30f; // cooldown time for laser attack
    private float lastLaserTime = -Mathf.Infinity; // keeps track of last laser usage

    private NavMeshAgent agent; // unity’s navmesh agent for pathfinding
    private Animator animator; // animator for triggering animations
    private Vector3 startPosition; // boss’s initial spawn position
    private float roamTimer; // timer to control roaming behavior

    // simple finite state machine for boss behavior
    private enum BossState { Idle, Roam, Chase, Attack }
    private BossState state = BossState.Idle;

    public override void OnStartServer()
    {
        base.OnStartServer();
        agent = GetComponent<NavMeshAgent>(); // grab navmesh agent
        animator = GetComponent<Animator>(); // grab animator

        // make sure attack scripts are assigned, fallback to local components
        if (bulletHell == null)
            bulletHell = GetComponent<BulletHell>();
        if (bulletSpray == null)
            bulletSpray = GetComponent<BulletSpray>();
        if (laserAttack == null)
            laserAttack = GetComponent<LaserAttack>();

        startPosition = transform.position; // record spawn position for roaming
        roamTimer = roamDelay; // initialize roam timer
    }

    void Update()
    {
        if (!isServer) return; // only run this logic on the server

        // if player reference is missing, try to find one
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return; // no player found, skip update
        }

        float distance = Vector3.Distance(transform.position, player.position); // measure distance to player

        // run behavior based on current state
        switch (state)
        {
            case BossState.Idle: HandleIdle(distance); break;
            case BossState.Roam: HandleRoam(distance); break;
            case BossState.Chase: HandleChase(distance); break;
            case BossState.Attack: HandleAttack(distance); break;
        }
    }

    private void HandleIdle(float distance)
    {
        // if player is close enough, switch to chase
        if (distance <= detectionRadius)
        {
            state = BossState.Chase;
            return;
        }

        // otherwise, count down until roaming
        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0)
        {
            state = BossState.Roam;
            roamTimer = roamDelay; // reset timer
        }
    }

    private void HandleRoam(float distance)
    {
        // if player enters detection radius, switch to chase
        if (distance <= detectionRadius)
        {
            state = BossState.Chase;
            return;
        }

        // pick a random position near spawn to wander to
        if (!agent.hasPath)
        {
            Vector3 newPos = startPosition + Random.insideUnitSphere * roamRadius;
            agent.SetDestination(new Vector3(newPos.x, transform.position.y, newPos.z));
        }

        // once destination is reached, go back to idle
        if (agent.remainingDistance <= 1f)
            state = BossState.Idle;
    }

    private void HandleChase(float distance)
    {
        // if player runs too far away, stop chasing
        if (distance > detectionRadius * 1.5f)
        {
            state = BossState.Idle;
            return;
        }

        // otherwise, keep moving toward player
        agent.SetDestination(player.position);

        // if close enough, switch to attack
        if (distance <= attackRadius)
            state = BossState.Attack;
    }

    private void HandleAttack(float distance)
    {
        // stop moving and face the player
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        // check if attack cooldown has expired
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            // melee attack if very close
            if (distance <= meleeAttackRange)
            {
                TriggerMeleeAttack();
            }
            else
            {
                // laser attack if flagged and off cooldown
                if (useLaserNext && laserAttack != null && Time.time - lastLaserTime >= laserCooldown)
                {
                    laserAttack.FireLaser(transform.position, player.position);
                    useLaserNext = false; // reset flag
                    lastLaserTime = Time.time; // record laser usage
                    lastAttackTime = Time.time; // record attack usage
                }
                // bullet ring attack if flagged and not already running
                else if (useRingNext && bulletHell != null && !bulletHell.IsRunning())
                {
                    bulletHell.StartRingSequence();
                    useRingNext = false; // switch to spray next
                    useLaserNext = true; // prep laser for next cycle
                    lastAttackTime = Time.time;
                }
                // bullet spray attack if ring isn’t next
                else if (!useRingNext && bulletSpray != null && !bulletSpray.IsRunning())
                {
                    bulletSpray.StartSpray();
                    useRingNext = true; // switch back to ring next
                    lastAttackTime = Time.time;
                }
            }
        }

        // if player moves out of attack range, chase again
        if (distance > attackRadius + 2f)
            state = BossState.Chase;
    }

    private void TriggerMeleeAttack()
    {
        // trigger melee animation
        if (animator != null)
            animator.SetTrigger("MeleeAttack");
        lastAttackTime = Time.time; // record attack usage
    }

    public void InflictMeleeDamage()
    {
        // check for player within melee range
        Collider[] hits = Physics.OverlapSphere(transform.position, meleeAttackRange, LayerMask.GetMask("Player"));
        foreach (Collider hit in hits)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(meleeDamage); // apply damage
                return; // stop after hitting one player
            }
        }
    }
}
