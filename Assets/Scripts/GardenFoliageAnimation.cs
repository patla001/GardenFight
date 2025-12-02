using UnityEngine;

/// <summary>
/// Animates foliage (bushes, trees, flowers) with gentle swaying motion.
/// Creates a more alive and dynamic garden environment.
/// </summary>
public class GardenFoliageAnimation : MonoBehaviour
{
    [Header("Sway Settings")]
    [Tooltip("Speed of the sway animation")]
    public float swaySpeed = 1.0f;
    
    [Tooltip("Maximum angle of sway in degrees")]
    public float swayAmount = 5.0f;
    
    [Tooltip("Apply sway on X axis (side to side)")]
    public bool swayX = true;
    
    [Tooltip("Apply sway on Z axis (front to back)")]
    public bool swayZ = true;
    
    [Header("Wind Effect")]
    [Tooltip("Random wind gusts")]
    public bool enableWindGusts = true;
    
    [Tooltip("Time between wind gusts")]
    public float gustInterval = 5.0f;
    
    private Vector3 originalRotation;
    private float timeOffset;
    private float nextGustTime;
    private float gustStrength;
    
    void Start()
    {
        originalRotation = transform.localEulerAngles;
        
        // Random offset so plants don't all sway in sync
        timeOffset = Random.Range(0f, 100f);
        
        // Initialize gust timing
        nextGustTime = Time.time + Random.Range(gustInterval * 0.5f, gustInterval * 1.5f);
        gustStrength = 0f;
    }
    
    void Update()
    {
        // Calculate base sway
        float xRotation = originalRotation.x;
        float zRotation = originalRotation.z;
        
        if (swayX)
        {
            xRotation += Mathf.Sin((Time.time + timeOffset) * swaySpeed) * swayAmount;
        }
        
        if (swayZ)
        {
            zRotation += Mathf.Cos((Time.time + timeOffset) * swaySpeed * 0.8f) * swayAmount;
        }
        
        // Add wind gust effect
        if (enableWindGusts)
        {
            if (Time.time >= nextGustTime)
            {
                gustStrength = Random.Range(1.5f, 3.0f);
                nextGustTime = Time.time + Random.Range(gustInterval * 0.5f, gustInterval * 1.5f);
            }
            
            // Decay gust strength
            gustStrength = Mathf.Lerp(gustStrength, 0f, Time.deltaTime * 2f);
            
            xRotation += Mathf.Sin(Time.time * swaySpeed * 2f) * swayAmount * gustStrength;
            zRotation += Mathf.Cos(Time.time * swaySpeed * 2f) * swayAmount * gustStrength;
        }
        
        // Apply rotation
        transform.localEulerAngles = new Vector3(xRotation, originalRotation.y, zRotation);
    }
    
    // Visualize sway range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = transform.position;
        Gizmos.DrawWireSphere(center, 0.2f);
        
        // Show sway direction
        if (swayX)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(center, center + transform.right * swayAmount * 0.1f);
            Gizmos.DrawLine(center, center - transform.right * swayAmount * 0.1f);
        }
        
        if (swayZ)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(center, center + transform.forward * swayAmount * 0.1f);
            Gizmos.DrawLine(center, center - transform.forward * swayAmount * 0.1f);
        }
    }
}

