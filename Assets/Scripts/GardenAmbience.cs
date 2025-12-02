using UnityEngine;

/// <summary>
/// Manages garden ambient effects like birds, butterflies, and particle systems.
/// Attach to an empty GameObject in the garden scene.
/// </summary>
public class GardenAmbience : MonoBehaviour
{
    [Header("Particle Effects")]
    [Tooltip("Prefab for butterfly particles (assign particle system prefab)")]
    public GameObject butterflyPrefab;
    
    [Tooltip("Number of butterflies to spawn")]
    public int butterflyCount = 5;
    
    [Tooltip("Prefab for firefly particles (for night mode)")]
    public GameObject fireflyPrefab;
    
    [Tooltip("Enable fireflies at night")]
    public bool enableFireflies = false;
    
    [Header("Arena Bounds")]
    [Tooltip("Center of the garden arena")]
    public Vector3 arenaCenter = Vector3.zero;
    
    [Tooltip("Size of the garden arena")]
    public Vector3 arenaSize = new Vector3(10f, 5f, 10f);
    
    [Header("Audio")]
    [Tooltip("Ambient bird chirping audio clip")]
    public AudioClip birdChirpingClip;
    
    [Tooltip("Water fountain/stream audio clip")]
    public AudioClip waterSoundClip;
    
    [Tooltip("Wind rustling audio clip")]
    public AudioClip windRustlingClip;
    
    [Tooltip("Volume for ambient sounds")]
    [Range(0f, 1f)]
    public float ambientVolume = 0.3f;
    
    private GameObject[] butterflies;
    private AudioSource audioSource;
    
    void Start()
    {
        SetupAudio();
        SpawnButterflies();
        
        if (enableFireflies)
        {
            SpawnFireflies();
        }
    }
    
    void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = ambientVolume;
        audioSource.spatialBlend = 0f; // 2D sound for ambient
        
        // Play bird chirping as main ambient
        if (birdChirpingClip != null)
        {
            audioSource.clip = birdChirpingClip;
            audioSource.Play();
        }
        
        // Add water sound source if clip exists
        if (waterSoundClip != null)
        {
            GameObject waterSource = new GameObject("WaterAudioSource");
            waterSource.transform.parent = transform;
            AudioSource waterAudio = waterSource.AddComponent<AudioSource>();
            waterAudio.clip = waterSoundClip;
            waterAudio.loop = true;
            waterAudio.volume = ambientVolume * 0.5f;
            waterAudio.spatialBlend = 0.5f; // Semi-spatial
            waterAudio.Play();
        }
        
        // Add wind sound source if clip exists
        if (windRustlingClip != null)
        {
            GameObject windSource = new GameObject("WindAudioSource");
            windSource.transform.parent = transform;
            AudioSource windAudio = windSource.AddComponent<AudioSource>();
            windAudio.clip = windRustlingClip;
            windAudio.loop = true;
            windAudio.volume = ambientVolume * 0.4f;
            windAudio.spatialBlend = 0f;
            windAudio.Play();
        }
    }
    
    void SpawnButterflies()
    {
        if (butterflyPrefab == null || butterflyCount <= 0)
            return;
        
        butterflies = new GameObject[butterflyCount];
        
        for (int i = 0; i < butterflyCount; i++)
        {
            Vector3 randomPos = GetRandomPositionInArena();
            randomPos.y = Random.Range(1f, 3f); // Butterflies fly at varying heights
            
            butterflies[i] = Instantiate(butterflyPrefab, randomPos, Quaternion.identity);
            butterflies[i].transform.parent = transform;
            butterflies[i].name = $"Butterfly_{i + 1}";
            
            // Add simple float animation
            ButterflyMovement movement = butterflies[i].AddComponent<ButterflyMovement>();
            movement.arenaCenter = arenaCenter;
            movement.arenaSize = arenaSize;
        }
    }
    
    void SpawnFireflies()
    {
        if (fireflyPrefab == null)
            return;
        
        // Spawn fireflies around the perimeter
        int fireflyCount = 10;
        for (int i = 0; i < fireflyCount; i++)
        {
            Vector3 perimeterPos = GetPerimeterPosition(i, fireflyCount);
            perimeterPos.y = Random.Range(0.5f, 2f);
            
            GameObject firefly = Instantiate(fireflyPrefab, perimeterPos, Quaternion.identity);
            firefly.transform.parent = transform;
            firefly.name = $"Firefly_{i + 1}";
        }
    }
    
    Vector3 GetRandomPositionInArena()
    {
        return arenaCenter + new Vector3(
            Random.Range(-arenaSize.x / 2, arenaSize.x / 2),
            Random.Range(0, arenaSize.y),
            Random.Range(-arenaSize.z / 2, arenaSize.z / 2)
        );
    }
    
    Vector3 GetPerimeterPosition(int index, int total)
    {
        float angle = (index / (float)total) * Mathf.PI * 2f;
        float radius = Mathf.Max(arenaSize.x, arenaSize.z) / 2f;
        
        return arenaCenter + new Vector3(
            Mathf.Cos(angle) * radius,
            0,
            Mathf.Sin(angle) * radius
        );
    }
    
    // Draw arena bounds in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(arenaCenter, arenaSize);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(arenaCenter, 0.5f);
    }
}

/// <summary>
/// Simple butterfly movement behavior
/// </summary>
public class ButterflyMovement : MonoBehaviour
{
    public Vector3 arenaCenter;
    public Vector3 arenaSize;
    
    private Vector3 targetPosition;
    private float moveSpeed;
    private float changeTargetTime;
    
    void Start()
    {
        moveSpeed = Random.Range(1f, 3f);
        ChooseNewTarget();
    }
    
    void Update()
    {
        // Move towards target
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        // Look at target
        if (targetPosition != transform.position)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(targetPosition - transform.position),
                Time.deltaTime * 2f
            );
        }
        
        // Choose new target periodically or when reached
        if (Time.time >= changeTargetTime || Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            ChooseNewTarget();
        }
    }
    
    void ChooseNewTarget()
    {
        targetPosition = arenaCenter + new Vector3(
            Random.Range(-arenaSize.x / 2, arenaSize.x / 2),
            Random.Range(1f, 3f),
            Random.Range(-arenaSize.z / 2, arenaSize.z / 2)
        );
        
        changeTargetTime = Time.time + Random.Range(3f, 8f);
    }
}

