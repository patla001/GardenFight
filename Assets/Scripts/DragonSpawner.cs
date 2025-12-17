using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Spawns multiple dragons flying in the background automatically
/// Add this to an empty GameObject in your scene
/// </summary>
public class DragonSpawner : MonoBehaviour
{
    [Header("Dragon Prefabs")]
    [Tooltip("Drag your dragon prefabs here (e.g., Red, Blue, Green, Grey)")]
    public GameObject[] dragonPrefabs;
    
    [Header("Spawn Settings")]
    [Tooltip("Number of dragons to spawn")]
    public int numberOfDragons = 4;
    
    [Tooltip("Center point for dragon flight paths")]
    public Vector3 arenaCenter = Vector3.zero;
    
    [Header("Flight Range Settings")]
    [Tooltip("Minimum flight radius")]
    public float minRadius = 10f;
    
    [Tooltip("Maximum flight radius")]
    public float maxRadius = 18f;
    
    [Tooltip("Minimum flight height")]
    public float minHeight = 5f;
    
    [Tooltip("Maximum flight height")]
    public float maxHeight = 10f;
    
    [Tooltip("Minimum flight speed")]
    public float minSpeed = 2f;
    
    [Tooltip("Maximum flight speed")]
    public float maxSpeed = 5f;
    
    [Header("Dragon Scale")]
    [Tooltip("Minimum dragon scale")]
    public float minScale = 0.8f;
    
    [Tooltip("Maximum dragon scale")]
    public float maxScale = 1.5f;
    
    private void Start()
    {
        SpawnDragons();
    }
    
    void SpawnDragons()
    {
        if (dragonPrefabs == null || dragonPrefabs.Length == 0)
        {
            Debug.LogError("DragonSpawner: No dragon prefabs assigned! Please drag dragon prefabs into the array.");
            return;
        }
        
        // Array of flight patterns to choose from
        BackgroundDragonFlight.FlightPattern[] patterns = new BackgroundDragonFlight.FlightPattern[]
        {
            BackgroundDragonFlight.FlightPattern.Circle,
            BackgroundDragonFlight.FlightPattern.Figure8,
            BackgroundDragonFlight.FlightPattern.BackAndForth,
            BackgroundDragonFlight.FlightPattern.Circle // More circles since they look good
        };
        
        for (int i = 0; i < numberOfDragons; i++)
        {
            // Pick a random dragon prefab
            GameObject prefab = dragonPrefabs[Random.Range(0, dragonPrefabs.Length)];
            
            // Random flight settings
            float radius = Random.Range(minRadius, maxRadius);
            float height = Random.Range(minHeight, maxHeight);
            float speed = Random.Range(minSpeed, maxSpeed);
            float scale = Random.Range(minScale, maxScale);
            
            // Calculate starting position on the flight path (not at center!)
            float startAngle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 startPosition = new Vector3(
                arenaCenter.x + Mathf.Cos(startAngle) * radius,
                height,
                arenaCenter.z + Mathf.Sin(startAngle) * radius
            );
            
            // Create the dragon directly on its flight path (not at arena center)
            GameObject dragon = Instantiate(prefab, startPosition, Quaternion.identity, transform);
            dragon.name = $"FlyingDragon_{i + 1}_{prefab.name}";
            
            // Scale the dragon
            dragon.transform.localScale = Vector3.one * scale;
            
            // Add and configure the flight script
            BackgroundDragonFlight flight = dragon.GetComponent<BackgroundDragonFlight>();
            if (flight == null)
            {
                flight = dragon.AddComponent<BackgroundDragonFlight>();
            }
            
            // Mark as spawned by spawner to prevent auto-initialization in Start()
            flight.spawnedBySpawner = true;
            
            // Configure flight settings BEFORE initializing
            flight.flightPattern = patterns[i % patterns.Length];
            flight.radius = radius;
            flight.flightHeight = height;
            flight.speed = speed;
            flight.flyAnimationName = "Fly Forward";
            flight.forceFlightAnimation = true;  // Always fly, never land/attack/rest
            flight.smoothRotation = true;
            flight.rotationSpeed = 2f;
            flight.addBobbing = true;
            flight.bobbingAmount = 0.5f;
            flight.bobbingSpeed = 1f;
            
            // Initialize the flight path with arena center and the pre-calculated starting angle
            flight.Initialize(arenaCenter, startAngle);
            
            // Disable any AI, combat, or other scripts that might interfere with flying
            DisableNonFlightComponents(dragon);
            
            Debug.Log($"Spawned {dragon.name} - Pattern: {flight.flightPattern}, Radius: {radius:F1}, Height: {height:F1}, Speed: {speed:F1}");
        }
        
        Debug.Log($"DragonSpawner: Spawned {numberOfDragons} dragons!");
    }
    
    /// <summary>
    /// Disables AI, combat, physics, and other components that would interfere with background flying
    /// </summary>
    void DisableNonFlightComponents(GameObject dragon)
    {
        // Disable any AI scripts
        var bossAI = dragon.GetComponent<BossAI>();
        if (bossAI != null) bossAI.enabled = false;
        
        // Disable health (background dragons shouldn't be damageable)
        var bossHealth = dragon.GetComponent<BossHealth>();
        if (bossHealth != null) bossHealth.enabled = false;
        
        // Disable NavMeshAgent (we control movement via BackgroundDragonFlight)
        var navAgent = dragon.GetComponent<NavMeshAgent>();
        if (navAgent != null) navAgent.enabled = false;
        
        // Disable any Rigidbody physics
        var rb = dragon.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        // Disable colliders so they don't interfere with gameplay
        var colliders = dragon.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        
        // Disable any attack scripts that might exist
        var bulletHell = dragon.GetComponent<BulletHell>();
        if (bulletHell != null) bulletHell.enabled = false;
        
        var bulletSpray = dragon.GetComponent<BulletSpray>();
        if (bulletSpray != null) bulletSpray.enabled = false;
        
        var laserAttack = dragon.GetComponent<LaserAttack>();
        if (laserAttack != null) laserAttack.enabled = false;
        
        // Reset animator to clean state and force fly animation
        var animator = dragon.GetComponent<Animator>();
        if (animator != null)
        {
            // Reset all triggers that might cause other animations
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("MeleeAttack");
            animator.ResetTrigger("Die");
            animator.ResetTrigger("GetHit");
            animator.ResetTrigger("Land");
            animator.ResetTrigger("TakeOff");
            
            // Set any common bool parameters to false
            animator.SetBool("IsGrounded", false);
            animator.SetBool("IsAttacking", false);
            animator.SetBool("IsDead", false);
            
            // Force the fly animation immediately
            animator.Play("Fly Forward", 0, 0f);
        }
    }
    
    // Visualize the flight area in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        
        // Draw min radius circle
        DrawCircle(arenaCenter + Vector3.up * minHeight, minRadius);
        
        // Draw max radius circle
        Gizmos.color = Color.red;
        DrawCircle(arenaCenter + Vector3.up * maxHeight, maxRadius);
        
        // Draw center point
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(arenaCenter, 1f);
    }
    
    void DrawCircle(Vector3 center, float radius)
    {
        for (int i = 0; i < 360; i += 10)
        {
            float rad = i * Mathf.Deg2Rad;
            float nextRad = (i + 10) * Mathf.Deg2Rad;
            
            Vector3 start = center + new Vector3(Mathf.Cos(rad) * radius, 0, Mathf.Sin(rad) * radius);
            Vector3 end = center + new Vector3(Mathf.Cos(nextRad) * radius, 0, Mathf.Sin(nextRad) * radius);
            
            Gizmos.DrawLine(start, end);
        }
    }
}
