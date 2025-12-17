using UnityEngine;

/// <summary>
/// Spawns multiple flying dragons in the background with varied flight patterns
/// Add this to an empty GameObject in your scene
/// </summary>
public class DragonSwarmSpawner : MonoBehaviour
{
    [Header("Dragon Prefab")]
    [Tooltip("Drag your dragon prefab here (e.g., Red.prefab from DragonSoulEater)")]
    public GameObject dragonPrefab;
    
    [Header("Spawn Settings")]
    [Tooltip("How many dragons to spawn")]
    [Range(1, 10)]
    public int numberOfDragons = 3;
    
    [Tooltip("Center point for dragon flight paths")]
    public Vector3 flightCenter = Vector3.zero;
    
    [Header("Flight Variation")]
    [Tooltip("Minimum flight radius")]
    public float minRadius = 10f;
    
    [Tooltip("Maximum flight radius")]
    public float maxRadius = 20f;
    
    [Tooltip("Minimum flight height")]
    public float minHeight = 5f;
    
    [Tooltip("Maximum flight height")]
    public float maxHeight = 12f;
    
    [Tooltip("Minimum flight speed")]
    public float minSpeed = 2f;
    
    [Tooltip("Maximum flight speed")]
    public float maxSpeed = 5f;
    
    [Header("Dragon Scale")]
    [Tooltip("Minimum dragon scale")]
    public float minScale = 0.8f;
    
    [Tooltip("Maximum dragon scale")]
    public float maxScale = 1.5f;
    
    [Header("Animation")]
    [Tooltip("Flying animation name")]
    public string flyAnimationName = "Fly Forward";
    
    void Start()
    {
        SpawnDragons();
    }
    
    void SpawnDragons()
    {
        if (dragonPrefab == null)
        {
            Debug.LogError("DragonSwarmSpawner: No dragon prefab assigned!");
            return;
        }
        
        // Array of flight patterns to vary
        BackgroundDragonFlight.FlightPattern[] patterns = {
            BackgroundDragonFlight.FlightPattern.Circle,
            BackgroundDragonFlight.FlightPattern.Figure8,
            BackgroundDragonFlight.FlightPattern.BackAndForth
        };
        
        for (int i = 0; i < numberOfDragons; i++)
        {
            // Random position offset for starting point
            float angle = (360f / numberOfDragons) * i; // Evenly space starting angles
            float radius = Random.Range(minRadius, maxRadius);
            float height = Random.Range(minHeight, maxHeight);
            
            Vector3 spawnPosition = flightCenter + new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                height,
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius
            );
            
            // Spawn the dragon
            GameObject dragon = Instantiate(dragonPrefab, spawnPosition, Quaternion.identity, transform);
            dragon.name = $"FlyingDragon_{i + 1}";
            
            // Random scale
            float scale = Random.Range(minScale, maxScale);
            dragon.transform.localScale = Vector3.one * scale;
            
            // Add or get the flight script
            BackgroundDragonFlight flight = dragon.GetComponent<BackgroundDragonFlight>();
            if (flight == null)
            {
                flight = dragon.AddComponent<BackgroundDragonFlight>();
            }
            
            // Configure with random variation
            flight.flightPattern = patterns[i % patterns.Length];
            flight.radius = radius;
            flight.flightHeight = height;
            flight.speed = Random.Range(minSpeed, maxSpeed);
            flight.flyAnimationName = flyAnimationName;
            flight.smoothRotation = true;
            flight.rotationSpeed = Random.Range(1.5f, 3f);
            flight.addBobbing = true;
            flight.bobbingAmount = Random.Range(0.5f, 1.5f);
            flight.bobbingSpeed = Random.Range(0.8f, 1.2f);
        }
        
        Debug.Log($"DragonSwarmSpawner: Spawned {numberOfDragons} dragons!");
    }
    
    // Visualize spawn area in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        
        // Draw min radius circle
        DrawCircle(flightCenter, minRadius, minHeight);
        
        // Draw max radius circle
        Gizmos.color = Color.cyan;
        DrawCircle(flightCenter, maxRadius, maxHeight);
        
        // Draw center point
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(flightCenter, 1f);
    }
    
    void DrawCircle(Vector3 center, float radius, float height)
    {
        int segments = 36;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * 360f * Mathf.Deg2Rad;
            float angle2 = ((i + 1) / (float)segments) * 360f * Mathf.Deg2Rad;
            
            Vector3 p1 = center + new Vector3(Mathf.Cos(angle1) * radius, height, Mathf.Sin(angle1) * radius);
            Vector3 p2 = center + new Vector3(Mathf.Cos(angle2) * radius, height, Mathf.Sin(angle2) * radius);
            
            Gizmos.DrawLine(p1, p2);
        }
    }
}
