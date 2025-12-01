using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class BossAI : NetworkBehaviour
{
    [Header("References")]
    public Transform player;
    public BulletHell bulletHell;

    [Header("AI Settings")]
    public float detectionRadius = 20f;
    public float attackRadius = 3f;
    public float roamRadius = 5f;
    public float roamDelay = 4f;

    [Header("Bullet Hell Settings")]
    public float ringCooldown = 30f; // seconds between full ring sequences
    private float lastRingTime = -Mathf.Infinity;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private float roamTimer;

    private enum BossState { Idle, Roam, Chase, Attack }
    private BossState state = BossState.Idle;

    #region Unity Methods
    public override void OnStartServer()
    {
        base.OnStartServer();

        agent = GetComponent<NavMeshAgent>();
        if (bulletHell == null)
            bulletHell = GetComponent<BulletHell>();

        startPosition = transform.position;
        roamTimer = roamDelay;
    }

    void Update()
    {
        if (!isServer) return; // Only the server controls AI

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
        agent.SetDestination(transform.position); // stop moving
        transform.LookAt(player);

        if (bulletHell != null && !bulletHell.IsRunning())
        {
            // Only fire full sequence if cooldown has passed
            if (Time.time - lastRingTime >= ringCooldown)
            {
                bulletHell.StartRingSequence();
                lastRingTime = Time.time;
            }
        }

        // Return to chasing if player backs away
        if (distance > attackRadius + 2f)
            state = BossState.Chase;
    }
    #endregion
}
