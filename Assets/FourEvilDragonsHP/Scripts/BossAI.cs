using UnityEngine;
using UnityEngine.AI;
using Mirror;
using System.Collections; // ✅ Needed for IEnumerator-based melee timing

public class BossAI : NetworkBehaviour
{
    [Header("References")]
    public Transform player;          // reference to the player the boss will target
    public BulletHell bulletHell;     // ring-style bullet attack
    public BulletSpray bulletSpray;   // random spray bullet attack
    public LaserAttack laserAttack;   // long-range laser attack

    [Header("AI Settings")]
    public float detectionRadius = 20f; // distance at which boss notices the player
    public float attackRadius = 3f;     // distance at which boss switches to attack mode
    public float roamRadius = 5f;       // how far boss wanders from its start point
    public float roamDelay = 4f;        // delay between roam movements

    [Header("Attack Settings")]
    public float meleeAttackRange = 1.5f; // distance required to land melee hits
    public float meleeDamage = 10f;       // damage dealt by melee attack
    public float attackCooldown = 5f;     // delay between any two attacks
    private float lastAttackTime = -Mathf.Infinity; // tracks last time boss attacked

    private bool useRingNext = true;   // toggles between ring and spray attacks
    private bool useLaserNext = false; // determines when laser should be used

    [Header("Laser Cooldown")]
    public float laserCooldown = 30f;     // long cooldown for laser attack
    private float lastLaserTime = -Mathf.Infinity; // tracks last laser usage

    private NavMeshAgent agent;        // handles movement and pathfinding
    private Animator animator;         // controls boss animations
    private Vector3 startPosition;     // original spawn point for roaming
    private float roamTimer;           // countdown timer for roaming behavior

    // simple state machine for boss behavior
    private enum BossState { Idle, Roam, Chase, Attack }
    private BossState state = BossState.Idle;

    public override void OnStartServer()
    {
        base.OnStartServer();

        // auto-assign required components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // auto-assign attack scripts if not set in Inspector
        if (bulletHell == null) bulletHell = GetComponent<BulletHell>();
        if (bulletSpray == null) bulletSpray = GetComponent<BulletSpray>();
        if (laserAttack == null) laserAttack = GetComponent<LaserAttack>();

        // store starting position for roaming logic
        startPosition = transform.position;
        roamTimer = roamDelay;
    }

    void Update()
    {
        if (!isServer) return; // only server controls AI

        // find player if not already assigned
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return; // no player found, skip logic
        }

        // measure distance to player for decision-making
        float distance = Vector3.Distance(transform.position, player.position);

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
        // switch to chase if player enters detection range
        if (distance <= detectionRadius)
        {
            state = BossState.Chase;
            return;
        }

        // count down until next roam movement
        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0)
        {
            state = BossState.Roam;
            roamTimer = roamDelay;
        }
    }

    private void HandleRoam(float distance)
    {
        // stop roaming and chase player if detected
        if (distance <= detectionRadius)
        {
            state = BossState.Chase;
            return;
        }

        // choose a random point near the start position
        if (!agent.hasPath)
        {
            Vector3 newPos = startPosition + Random.insideUnitSphere * roamRadius;
            agent.SetDestination(new Vector3(newPos.x, transform.position.y, newPos.z));
        }

        // return to idle once destination is reached
        if (agent.remainingDistance <= 1f)
            state = BossState.Idle;
    }

    private void HandleChase(float distance)
    {
        // if player gets too far, return to idle
        if (distance > detectionRadius * 1.5f)
        {
            state = BossState.Idle;
            return;
        }

        // move toward the player
        agent.SetDestination(player.position);

        // switch to attack mode when close enough
        if (distance <= attackRadius)
            state = BossState.Attack;
    }

    private void HandleAttack(float distance)
    {
        // stop moving while attacking
        agent.SetDestination(transform.position);

        // face the player for accuracy
        transform.LookAt(player);

        // check if boss is allowed to attack again
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            // melee attack if close enough
            if (distance <= meleeAttackRange)
            {
                TriggerMeleeAttack();
            }
            else
            {
                // laser attack if it's next in rotation and off cooldown
                if (useLaserNext && laserAttack != null && Time.time - lastLaserTime >= laserCooldown)
                {
                    laserAttack.FireLaser(transform.position, player.position);
                    useLaserNext = false;
                    lastLaserTime = Time.time;
                    lastAttackTime = Time.time;
                }
                // ring attack if available
                else if (useRingNext && bulletHell != null && !bulletHell.IsRunning())
                {
                    bulletHell.StartRingSequence();
                    useRingNext = false;
                    useLaserNext = true;
                    lastAttackTime = Time.time;
                }
                // spray attack as fallback
                else if (!useRingNext && bulletSpray != null && !bulletSpray.IsRunning())
                {
                    bulletSpray.StartSpray();
                    useRingNext = true;
                    lastAttackTime = Time.time;
                }
            }
        }

        // if player backs away, resume chasing
        if (distance > attackRadius + 2f)
            state = BossState.Chase;
    }

    private void TriggerMeleeAttack()
    {
        // trigger melee animation if available
        if (animator != null)
            animator.SetTrigger("MeleeAttack");

        // wait for animation to reach hit frame
        StartCoroutine(MeleeDamageWindow());

        lastAttackTime = Time.time;
    }

    private IEnumerator MeleeDamageWindow()
    {
        // small delay to sync with animation impact moment
        yield return new WaitForSeconds(0.4f);
        ApplyMeleeDamage();
    }

    private void ApplyMeleeDamage()
    {
        // detect all objects within melee range
        Collider[] hits = Physics.OverlapSphere(transform.position, meleeAttackRange);

        foreach (Collider hit in hits)
        {
            // only damage the player
            if (hit.CompareTag("Player"))
            {
                Player player = hit.GetComponentInParent<Player>();
                if (player != null)
                {
                    player.TakeDamage((int)meleeDamage, "Fist");
                }
                return; // stop after first valid hit
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // visualize melee range in editor for debugging
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
    }
}
