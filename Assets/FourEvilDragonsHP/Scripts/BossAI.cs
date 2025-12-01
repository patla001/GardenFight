using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class BossAI : NetworkBehaviour
{
    [Header("References")]
    public Transform player; // reference to player transform
    public BulletHell bulletHell; // reference to bullet hell script

    [Header("AI Settings")]
    public float detectionRadius = 20f; // how far boss can detect player
    public float attackRadius = 3f; // how close boss needs to be to attack
    public float roamRadius = 5f; // how far boss can roam from start
    public float roamDelay = 4f; // delay before roaming again

    [Header("Bullet Hell Settings")]
    public float ringCooldown = 30f; // cooldown between bullet ring attacks
    private float lastRingTime = -Mathf.Infinity; // track last time ring was fired

    private NavMeshAgent agent; // navmesh agent for movement
    private Vector3 startPosition; // starting position of boss
    private float roamTimer; // timer for roaming

    private enum BossState { Idle, Roam, Chase, Attack } // states boss can be in
    private BossState state = BossState.Idle; // default state is idle

    #region Unity Methods
    public override void OnStartServer()
    {
        base.OnStartServer();

        agent = GetComponent<NavMeshAgent>(); // get navmesh agent
        if (bulletHell == null)
            bulletHell = GetComponent<BulletHell>(); // get bullet hell script if not assigned

        startPosition = transform.position; // save starting position
        roamTimer = roamDelay; // set initial roam timer
    }

    void Update()
    {
        if (!isServer) return; // only server runs ai logic

        if (player == null) // if player not assigned try to find one
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
            else
                return; // no player found stop here
        }

        float distance = Vector3.Distance(transform.position, player.position); // distance to player

        // handle state logic
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
        if (distance <= detectionRadius) // if player close enough start chasing
        {
            state = BossState.Chase;
            return;
        }

        roamTimer -= Time.deltaTime; // count down roam timer
        if (roamTimer <= 0) // when timer runs out start roaming
        {
            state = BossState.Roam;
            roamTimer = roamDelay;
        }
    }

    private void HandleRoam(float distance)
    {
        if (distance <= detectionRadius) // if player enters detection range chase
        {
            state = BossState.Chase;
            return;
        }

        if (!agent.hasPath) // if not already moving pick random roam position
        {
            Vector3 newPos = startPosition + Random.insideUnitSphere * roamRadius;
            agent.SetDestination(new Vector3(newPos.x, transform.position.y, newPos.z));
        }

        if (agent.remainingDistance <= 1f) // once reached roam spot go idle
            state = BossState.Idle;
    }

    private void HandleChase(float distance)
    {
        if (distance > detectionRadius * 1.5f) // if player too far stop chasing
        {
            state = BossState.Idle;
            return;
        }

        agent.SetDestination(player.position); // move toward player

        if (distance <= attackRadius) // if close enough switch to attack
            state = BossState.Attack;
    }

    private void HandleAttack(float distance)
    {
        agent.SetDestination(transform.position); // stop moving
        transform.LookAt(player); // face player

        if (bulletHell != null && !bulletHell.IsRunning()) // if bullet hell not running
        {
            if (Time.time - lastRingTime >= ringCooldown) // check cooldown
            {
                bulletHell.StartRingSequence(); // fire bullet ring
                lastRingTime = Time.time; // update last ring time
            }
        }

        if (distance > attackRadius + 2f) // if player backs away chase again
            state = BossState.Chase;
    }
    #endregion
}
