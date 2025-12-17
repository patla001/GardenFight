using UnityEngine;

/// <summary>
/// Makes a dragon fly in a circular or path pattern in the background
/// Add this to a dragon prefab to create ambient background animations
/// </summary>
public class BackgroundDragonFlight : MonoBehaviour
{
    [Header("Flight Path Settings")]
    [Tooltip("Type of flight pattern")]
    public FlightPattern flightPattern = FlightPattern.Circle;
    
    [Tooltip("Radius of circular flight path")]
    public float radius = 12f;
    
    [Tooltip("Height at which the dragon flies")]
    public float flightHeight = 6f;
    
    [Tooltip("Speed of the dragon flight")]
    public float speed = 3f;
    
    [Header("Waypoint Path (for Waypoint pattern)")]
    [Tooltip("Set waypoints for custom path. Leave empty for circle pattern")]
    public Transform[] waypoints;
    
    [Header("Animation Settings")]
    [Tooltip("Name of the flying animation state")]
    public string flyAnimationName = "Fly Forward";
    
    [Tooltip("Force flying animation only (prevents landing, attacking, etc.)")]
    public bool forceFlightAnimation = true;
    
    [Tooltip("Smoothly rotate dragon to face direction of movement")]
    public bool smoothRotation = true;
    
    [Tooltip("Rotation speed when turning")]
    public float rotationSpeed = 2f;
    
    [Header("Optional Effects")]
    [Tooltip("Add some bobbing motion for realism")]
    public bool addBobbing = true;
    
    [Tooltip("Bobbing amplitude")]
    public float bobbingAmount = 1f;
    
    [Tooltip("Bobbing speed")]
    public float bobbingSpeed = 1f;
    
    private Animator animator;
    private float angle = 0f;
    private int currentWaypoint = 0;
    private Vector3 centerPoint;
    private float bobbingTimer = 0f;
    private bool isInitialized = false;
    
    /// <summary>
    /// Set this to true before Start() runs to prevent auto-initialization
    /// Used by DragonSpawner to control initialization timing
    /// </summary>
    [HideInInspector]
    public bool spawnedBySpawner = false;
    
    public enum FlightPattern
    {
        Circle,
        Figure8,
        Waypoints,
        BackAndForth
    }
    
    void Start()
    {
        // Skip auto-initialization if spawned by DragonSpawner (it will initialize us)
        if (!isInitialized && !spawnedBySpawner)
        {
            Initialize(transform.position);
        }
    }
    
    /// <summary>
    /// Initialize the dragon flight with a specific center point and starting angle
    /// Call this after setting flight parameters if spawning dynamically
    /// </summary>
    public void Initialize(Vector3 center, float startingAngle = -1f)
    {
        animator = GetComponent<Animator>();
        
        // Store the center for circular paths
        centerPoint = center;
        
        // Set initial angle (use provided angle or randomize)
        if (startingAngle >= 0f)
        {
            angle = startingAngle;
        }
        else if (!isInitialized)
        {
            angle = Random.Range(0f, Mathf.PI * 2f);
        }
        
        // Move dragon to correct position on flight path
        UpdatePosition();
        
        // Start the flying animation
        if (animator != null && !string.IsNullOrEmpty(flyAnimationName))
        {
            animator.Play(flyAnimationName);
        }
        
        isInitialized = true;
        
        Debug.Log($"{gameObject.name} initialized at center {center}, radius {radius}, height {flightHeight}");
    }
    
    void UpdatePosition()
    {
        float x = centerPoint.x + Mathf.Cos(angle) * radius;
        float z = centerPoint.z + Mathf.Sin(angle) * radius;
        float y = flightHeight + (addBobbing ? Mathf.Sin(bobbingTimer) * bobbingAmount : 0);
        
        transform.position = new Vector3(x, y, z);
        
        // Face direction of movement
        Vector3 direction = new Vector3(-Mathf.Sin(angle), 0, Mathf.Cos(angle));
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    void Update()
    {
        // Force flying animation to prevent landing, attacking, resting, etc.
        if (forceFlightAnimation && animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName(flyAnimationName))
            {
                // Reset any triggers that might cause unwanted animations
                animator.ResetTrigger("Attack");
                animator.ResetTrigger("MeleeAttack");
                animator.ResetTrigger("Die");
                animator.ResetTrigger("GetHit");
                animator.ResetTrigger("Land");
                animator.ResetTrigger("TakeOff");
                animator.ResetTrigger("Scream");
                
                // Force back to flying
                animator.Play(flyAnimationName, 0, 0f);
            }
        }
        
        switch (flightPattern)
        {
            case FlightPattern.Circle:
                FlyInCircle();
                break;
            case FlightPattern.Figure8:
                FlyInFigure8();
                break;
            case FlightPattern.Waypoints:
                FlyAlongWaypoints();
                break;
            case FlightPattern.BackAndForth:
                FlyBackAndForth();
                break;
        }
        
        // Add bobbing motion
        if (addBobbing)
        {
            bobbingTimer += Time.deltaTime * bobbingSpeed;
        }
    }
    
    void FlyInCircle()
    {
        angle += speed * Time.deltaTime / radius;
        
        float x = centerPoint.x + Mathf.Cos(angle) * radius;
        float z = centerPoint.z + Mathf.Sin(angle) * radius;
        float y = flightHeight + (addBobbing ? Mathf.Sin(bobbingTimer) * bobbingAmount : 0);
        
        Vector3 newPosition = new Vector3(x, y, z);
        transform.position = newPosition;
        
        // Rotate to face direction of movement
        if (smoothRotation)
        {
            Vector3 direction = new Vector3(-Mathf.Sin(angle), 0, Mathf.Cos(angle));
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    void FlyInFigure8()
    {
        angle += speed * Time.deltaTime / radius;
        
        float x = centerPoint.x + Mathf.Sin(angle) * radius;
        float z = centerPoint.z + Mathf.Sin(angle * 2) * radius * 0.5f;
        float y = flightHeight + (addBobbing ? Mathf.Sin(bobbingTimer) * bobbingAmount : 0);
        
        Vector3 newPosition = new Vector3(x, y, z);
        Vector3 direction = (newPosition - transform.position).normalized;
        transform.position = newPosition;
        
        // Rotate to face direction of movement
        if (smoothRotation && direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    void FlyAlongWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints set! Defaulting to circle pattern.");
            FlyInCircle();
            return;
        }
        
        Transform targetWaypoint = waypoints[currentWaypoint];
        Vector3 targetPosition = new Vector3(
            targetWaypoint.position.x,
            flightHeight + (addBobbing ? Mathf.Sin(bobbingTimer) * bobbingAmount : 0),
            targetWaypoint.position.z
        );
        
        // Move towards waypoint
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        
        // Rotate to face direction
        if (smoothRotation && direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Check if reached waypoint
        if (Vector3.Distance(transform.position, targetPosition) < 1f)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
        }
    }
    
    void FlyBackAndForth()
    {
        angle += speed * Time.deltaTime / radius;
        
        float x = centerPoint.x + Mathf.Sin(angle) * radius;
        float z = centerPoint.z;
        float y = flightHeight + (addBobbing ? Mathf.Sin(bobbingTimer) * bobbingAmount : 0);
        
        Vector3 newPosition = new Vector3(x, y, z);
        Vector3 direction = (newPosition - transform.position).normalized;
        transform.position = newPosition;
        
        // Rotate to face direction of movement
        if (smoothRotation && direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    // Helper method to visualize the flight path in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        
        if (flightPattern == FlightPattern.Circle || flightPattern == FlightPattern.BackAndForth)
        {
            Vector3 center = Application.isPlaying ? centerPoint : transform.position;
            
            // Draw circle path
            for (int i = 0; i < 360; i += 10)
            {
                float rad = i * Mathf.Deg2Rad;
                float nextRad = (i + 10) * Mathf.Deg2Rad;
                
                Vector3 start = center + new Vector3(Mathf.Cos(rad) * radius, flightHeight, Mathf.Sin(rad) * radius);
                Vector3 end = center + new Vector3(Mathf.Cos(nextRad) * radius, flightHeight, Mathf.Sin(nextRad) * radius);
                
                Gizmos.DrawLine(start, end);
            }
        }
        else if (flightPattern == FlightPattern.Waypoints && waypoints != null && waypoints.Length > 0)
        {
            // Draw waypoint path
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawSphere(waypoints[i].position, 1f);
                    
                    if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                    }
                    else if (i == waypoints.Length - 1 && waypoints[0] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                    }
                }
            }
        }
    }
}

