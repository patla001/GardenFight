using UnityEngine;
using UnityEngine.AI;
using Mirror;
using System.Collections; // ✅ Needed for IEnumerator-based melee timing

public class BossAI : NetworkBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 7f;
    public float runDistance = 10f; // distance where boss starts running

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

    private bool hasTakenOff = false;
    private bool bulletHellScreamPlayed = false;


    [Header("Bullet Hell Audio")]
    public AudioSource audioSource;
    public AudioClip bulletHellScreamClip;

    [Header("Landing Audio")]
    public AudioClip landingClip;



    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

    }

    [ClientRpc]
    private void RpcPlayBulletHellScream()
    {
        Debug.Log("CLIENT: RpcPlayBulletHellScream received");

        if (audioSource == null)
        {
            Debug.LogError("CLIENT: AudioSource is NULL");
            return;
        }

        if (bulletHellScreamClip == null)
        {
            Debug.LogError("CLIENT: Scream clip is NULL");
            return;
        }

        audioSource.PlayOneShot(bulletHellScreamClip);
    }
    public void RpcPlayLandingSound()
    {
        if (audioSource == null || landingClip == null) return;
        audioSource.PlayOneShot(landingClip);
    }




    private BossState lastState;
    private bool isAttacking = false;
    private enum BossAttackType
    {
        None,
        Melee,
        Ring,
        Spray,
        Laser
    }

    private BossAttackType currentAttack = BossAttackType.None;




    public override void OnStartServer()
    {
        base.OnStartServer();

        

        // auto-assign required components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = walkSpeed;
        // auto-assign attack scripts if not set in Inspector
        if (bulletHell == null) bulletHell = GetComponent<BulletHell>();
        if (bulletSpray == null) bulletSpray = GetComponent<BulletSpray>();
        if (laserAttack == null) laserAttack = GetComponent<LaserAttack>();

        // store starting position for roaming logic
        startPosition = transform.position;
        roamTimer = roamDelay;

        lastState = state;
        OnStateChanged(state);

    }

    void Update()
    {
        if (!isServer) return; // only server controls AI

        if (currentAttack == BossAttackType.Ring
            || currentAttack == BossAttackType.Melee
            || currentAttack == BossAttackType.Laser)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
        }



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

        if (state != lastState)
        {
            OnStateChanged(state);
            lastState = state;
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
            agent.speed = walkSpeed;
            state = BossState.Idle;
            return;
        }

        // make boss run if far
        if (distance > runDistance)
        {
            agent.speed = runSpeed;
            PlayRunAnimation();
        }
        else
        {
            agent.speed = walkSpeed;
            PlayWalkAnimation();
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
        //agent.SetDestination(transform.position);

        // face the player for accuracy
        transform.LookAt(player);

        if (currentAttack == BossAttackType.Ring || currentAttack == BossAttackType.Melee)
            return;

        // If laser (or spray) is currently active, don't start a new attack
        if (currentAttack == BossAttackType.Laser || currentAttack == BossAttackType.Spray)
            return;


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
                if (useLaserNext && laserAttack != null && Time.time - lastLaserTime >= laserCooldown
                    && currentAttack == BossAttackType.None)
                {

                    currentAttack = BossAttackType.Laser; 

                    EnsureTakeOff();
                    SetLaserActive(true);

                    laserAttack.Initialize(this);
                    laserAttack.FireLaser(transform.position, player.position);

                    useLaserNext = false;
                    lastLaserTime = Time.time;
                    lastAttackTime = Time.time;


                }
                // ring attack if available
                else if (useRingNext && bulletHell != null && !bulletHell.IsRunning())
                {

                    agent.isStopped = true;
                    agent.SetDestination(player.position);

                    if (!bulletHellScreamPlayed)
                    {
                        bulletHellScreamPlayed = true;
                        RpcPlayBulletHellScream();
                    }



                    PlayBulletHellAnimation();
                    bulletHell.Initialize(this);

                    bulletHell.StartRingSequence();
                    currentAttack = BossAttackType.Ring;
                    useRingNext = false;
                    useLaserNext = true;
                    lastAttackTime = Time.time;
                }
                // spray attack as fallback
                else if (!useRingNext && bulletSpray != null && !bulletSpray.IsRunning())
                {
                    currentAttack = BossAttackType.Spray; 

                    EnsureTakeOff();
                    bulletSpray.Initialize(this);
                    bulletSpray.StartSpray();

                    useRingNext = true;
                    lastAttackTime = Time.time;
                }
            }
        }

        // if player backs away, resume chasing
        if (distance > attackRadius + 2f)
            state = BossState.Chase;

        if (!isAttacking && distance > meleeAttackRange)
        {
            state = BossState.Chase;
        }
    }

    private void TriggerMeleeAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        currentAttack = BossAttackType.Melee;
        agent.isStopped = true;

        PlayBasicAttackAnimation();


        // wait for animation to reach hit frame
        StartCoroutine(MeleeDamageWindow());

        lastAttackTime = Time.time;
    }

    private IEnumerator MeleeDamageWindow()
    {
        // small delay to sync with animation impact window
        yield return new WaitForSeconds(0.4f);
        ApplyMeleeDamage();

        EndAttack();

    }

    private void ApplyMeleeDamage()
    {
        Debug.Log("BossAI: ApplyMeleeDamage called!");
        
        // detect all objects within melee range
        Collider[] hits = Physics.OverlapSphere(transform.position, meleeAttackRange);
        
        Debug.Log($"BossAI: Found {hits.Length} colliders in melee range");

        foreach (Collider hit in hits)
        {
            Debug.Log($"BossAI: Checking collider: {hit.name}, Tag: {hit.tag}");
            
            // only damage the player
            if (hit.CompareTag("Player"))
            {
                Player player = hit.GetComponentInParent<Player>();
                if (player != null)
                {
                    Debug.Log($"BossAI: Dealing {meleeDamage} damage to player!");
                    player.TakeDamage((int)meleeDamage, "Fist");
                }
                else
                {
                    Debug.LogWarning("BossAI: Found Player tag but no Player component");
                }
                return; // stop after first valid hit
            }
        }
        
        Debug.Log("BossAI: No player found in melee range");
    }

    private void OnDrawGizmosSelected()
    {
        // visualize melee range in editor for debugging
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
    }

    private void OnStateChanged(BossState newState)
    {
        switch (newState)
        {
            case BossState.Idle:
                PlayIdleAnimation(); 
                break;

            case BossState.Roam:
                PlayWalkAnimation();
                break;

            case BossState.Chase:
                PlayWalkAnimation();
                break;
            case BossState.Attack:
                
                break;
        }
    }

    public void SetBulletSprayFinished(bool value)
    {
        if (animator != null)
            animator.SetBool("BulletSprayFinished", value);
    }

    public void SetLaserActive(bool value)
    {
        if (animator == null) return;

        animator.SetBool("LaserActive", value);

        if (value)
        {
            // flying / laser should not blend with locomotion
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsIdle", false);
        }
    }


    public bool IsCurrentAttackLaser()
    {
        return currentAttack == BossAttackType.Laser;
    }


    public void EndAttack()
    {
        currentAttack = BossAttackType.None;
        isAttacking = false;

        hasTakenOff = false;

        agent.isStopped = false;
        agent.speed = walkSpeed;     // reset to base boss speed
        agent.ResetPath();

        ResetMovementAnimations();   
        PlayWalkAnimation();
        bulletHellScreamPlayed = false;


        state = BossState.Chase;
    }

    private void ResetMovementAnimations()
    {
        if (animator == null) return;

        animator.SetBool("IsIdle", false);
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsRunning", false);
    }

    private void EnsureTakeOff()
    {
        if (hasTakenOff) return;

        animator.SetTrigger("TakeOff");
        hasTakenOff = true;
    }

    private void PlayIdleAnimation()
    {
        if (animator == null) return;
        ResetMovementAnimations();
        animator.SetBool("IsIdle", true);
    }

    private void PlayWalkAnimation()
    {
        if (animator == null) return;
        ResetMovementAnimations();
        animator.SetBool("IsWalking", true);
    }

    private void PlayRunAnimation()
    {
        if (animator == null) return;
        ResetMovementAnimations();
        animator.SetBool("IsRunning", true);
    }

    private void PlayBasicAttackAnimation()
    {
        if (animator == null) return;
        ResetMovementAnimations();
        animator.SetTrigger("BasicAttack");
    }

    private void PlayBulletHellAnimation()
    {
        if (animator == null) return;
        ResetMovementAnimations();
        animator.SetTrigger("Scream");
        animator.SetTrigger("BulletHell");
    }

}
