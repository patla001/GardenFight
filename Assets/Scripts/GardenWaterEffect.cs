using UnityEngine;

/// <summary>
/// Simple script to animate water in the garden arena.
/// Attach this to water plane objects to create gentle wave motion.
/// </summary>
public class GardenWaterEffect : MonoBehaviour
{
    [Header("Water Animation Settings")]
    [Tooltip("Speed of the water wave animation")]
    public float waveSpeed = 0.5f;
    
    [Tooltip("Height of the waves")]
    public float waveHeight = 0.05f;
    
    [Tooltip("Enable ripple effect")]
    public bool enableRipples = true;
    
    [Header("Material Animation")]
    [Tooltip("Speed of texture scrolling")]
    public float textureScrollSpeed = 0.1f;
    
    private Vector3 startPosition;
    private Material waterMaterial;
    private float timeOffset;
    
    void Start()
    {
        startPosition = transform.position;
        
        // Get the material if renderer exists
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            waterMaterial = renderer.material;
        }
        
        // Random offset so multiple water objects don't sync
        timeOffset = Random.Range(0f, 100f);
    }
    
    void Update()
    {
        // Keep water at fixed position (no vertical pulsing)
        transform.position = startPosition;
        
        // Scroll texture to create ripple effect
        if (waterMaterial != null && textureScrollSpeed > 0)
        {
            Vector2 offset = waterMaterial.mainTextureOffset;
            
            // Create flowing ripple effect with different speeds on X and Y
            offset.x += textureScrollSpeed * Time.deltaTime * 0.7f;
            offset.y += textureScrollSpeed * Time.deltaTime * 0.3f;
            
            // Add subtle wave pattern
            if (enableRipples)
            {
                float ripple = Mathf.Sin((Time.time + timeOffset) * waveSpeed * 2f) * 0.01f;
                offset.x += ripple;
                offset.y += ripple * 0.5f;
            }
            
            waterMaterial.mainTextureOffset = offset;
        }
    }
    
    void OnDestroy()
    {
        // Clean up material instance
        if (waterMaterial != null)
        {
            Destroy(waterMaterial);
        }
    }
}

