using UnityEngine;

/// <summary>
/// Creates a subtle shimmer effect on water by animating material properties.
/// This works with solid color materials (no texture needed).
/// </summary>
public class GardenWaterShimmer : MonoBehaviour
{
    [Header("Shimmer Settings")]
    [Tooltip("Speed of the shimmer animation")]
    [Range(0.1f, 2f)]
    public float shimmerSpeed = 0.5f;
    
    [Tooltip("Intensity of shimmer effect")]
    [Range(0f, 0.5f)]
    public float shimmerIntensity = 0.2f;
    
    private Material waterMaterial;
    private Color baseColor;
    private float baseMetallic;
    private float baseSmoothness;
    private float timeOffset;
    
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            waterMaterial = renderer.material;
            
            // Store base values
            baseColor = waterMaterial.color;
            
            // Set initial water properties for nice look
            waterMaterial.SetFloat("_Metallic", 0.3f);
            waterMaterial.SetFloat("_Glossiness", 0.9f);
            
            baseMetallic = 0.3f;
            baseSmoothness = 0.9f;
        }
        else
        {
            Debug.LogWarning("GardenWaterShimmer: No Renderer found!");
            enabled = false;
        }
        
        timeOffset = Random.Range(0f, 100f);
    }
    
    void Update()
    {
        if (waterMaterial == null) return;
        
        // Animate color brightness slightly
        float brightness = Mathf.Sin((Time.time + timeOffset) * shimmerSpeed) * shimmerIntensity;
        Color shimmerColor = baseColor + new Color(brightness, brightness, brightness * 1.5f, 0);
        waterMaterial.color = shimmerColor;
        
        // Animate smoothness (reflectivity)
        float smoothnessVariation = Mathf.Sin((Time.time + timeOffset) * shimmerSpeed * 1.3f) * (shimmerIntensity * 0.5f);
        waterMaterial.SetFloat("_Glossiness", baseSmoothness + smoothnessVariation);
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

