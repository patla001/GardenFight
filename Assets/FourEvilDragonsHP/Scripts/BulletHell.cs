using UnityEngine;
using System.Collections;

public class BulletHell : MonoBehaviour
{
    [Header("Ring Settings")]
    public float innerRadius = 3f;   // distance from center to inner ring
    public float middleRadius = 6f;  // distance from center to middle ring
    public float outerRadius = 9f;   // distance from center to outer ring

    [Header("Damage Ranges (per ring)")]
    public float innerWidth = 1.5f;  // thickness of the inner ring’s damage band
    public float middleWidth = 1.5f; // thickness of the middle ring’s damage band
    public float outerWidth = 1.5f;  // thickness of the outer ring’s damage band

    public float radiusScale = 1f;   // multiplier to scale all radii (useful for tuning)

    [Header("Damage")]
    public int damage = 10;          // damage dealt per tick when player is hit
    public string damageType = "Magic"; // type of damage (for player logic)

    [Header("Sequence Settings")]
    public float delayBetweenWaves = 2f;   // time between each wave of rings
    public float ringActiveDuration = 1.5f; // how long each ring remains active

    [Header("Visuals (Optional)")]
    public GameObject ringPrefab;    // prefab used to represent rings visually
    public float ringHeight = 0.05f; // thickness of the ring visual in Y axis
    public float visualScale = 1f;   // multiplier to scale ring visuals

    [Header("Player Reference")]
    public Player localPlayer;       // reference to the player object

    private bool running = false;    // tracks whether the sequence is currently running

    private void Update()
    {
        // auto-assign player reference if not set
        if (localPlayer == null)
        {
            localPlayer = FindObjectOfType<Player>();
        }
    }

    // public method to start the ring attack sequence
    public void StartRingSequence()
    {
        if (!running)
            StartCoroutine(RingSequence());
    }

    // helper method to check if the sequence is active
    public bool IsRunning()
    {
        return running;
    }

    // coroutine that handles the full sequence of ring waves
    private IEnumerator RingSequence()
    {
        running = true;

        // declare visuals once (will be reused each wave)
        GameObject innerVis = null;
        GameObject midVis = null;
        GameObject outerVis = null;

        // Wave 1: outer ring is safe, inner + middle deal damage
        innerVis = SpawnRing(innerRadius);
        midVis = SpawnRing(middleRadius);
        StartCoroutine(DamageRingOverTime(innerRadius, innerWidth, ringActiveDuration));
        StartCoroutine(DamageRingOverTime(middleRadius, middleWidth, ringActiveDuration));
        Destroy(innerVis, ringActiveDuration);
        Destroy(midVis, ringActiveDuration);
        yield return new WaitForSeconds(delayBetweenWaves);

        // Wave 2: middle ring is safe, inner + outer deal damage
        innerVis = SpawnRing(innerRadius);
        outerVis = SpawnRing(outerRadius);
        StartCoroutine(DamageRingOverTime(innerRadius, innerWidth, ringActiveDuration));
        StartCoroutine(DamageRingOverTime(outerRadius, outerWidth, ringActiveDuration));
        Destroy(innerVis, ringActiveDuration);
        Destroy(outerVis, ringActiveDuration);
        yield return new WaitForSeconds(delayBetweenWaves);

        // Wave 3: inner ring is safe, middle + outer deal damage
        midVis = SpawnRing(middleRadius);
        outerVis = SpawnRing(outerRadius);
        StartCoroutine(DamageRingOverTime(middleRadius, middleWidth, ringActiveDuration));
        StartCoroutine(DamageRingOverTime(outerRadius, outerWidth, ringActiveDuration));
        Destroy(midVis, ringActiveDuration);
        Destroy(outerVis, ringActiveDuration);
        yield return new WaitForSeconds(delayBetweenWaves);

        running = false; // sequence finished
    }

    // coroutine that applies damage to the player if they are inside a ring’s band
    private IEnumerator DamageRingOverTime(float radius, float width, float duration)
    {
        float scaledRadius = radius * radiusScale; // apply scaling factor
        float halfWidth = width * 0.5f;            // half width used for band calculation
        float timer = 0f;

        float damageCooldown = 1f;                 // seconds between damage ticks
        float lastDamageTime = -Mathf.Infinity;    // tracks last time damage was applied

        while (timer < duration)
        {
            timer += Time.deltaTime;

            if (localPlayer != null)
            {
                float dist = Vector3.Distance(transform.position, localPlayer.transform.position);

                // Only damage if player is within the thin band around the ring radius
                if (Mathf.Abs(dist - scaledRadius) <= halfWidth)
                {
                    // apply damage only if cooldown has passed
                    if (Time.time - lastDamageTime >= damageCooldown)
                    {
                        localPlayer.TakeDamage(damage, damageType);
                        Debug.Log($"Local player hit by ring at radius {radius}");
                        lastDamageTime = Time.time;
                    }
                }
            }

            yield return null; // wait until next frame
        }
    }

    #region Visual Helpers
    // spawns a ring prefab at the given radius and scales it to match
    private GameObject SpawnRing(float radius)
    {
        if (ringPrefab == null) return null;
        GameObject ring = Instantiate(ringPrefab, transform.position, Quaternion.identity, transform);
        float diameter = radius * 2f; // scale prefab so its edge matches the radius
        ring.transform.localScale = new Vector3(diameter * visualScale, ringHeight, diameter * visualScale);
        return ring;
    }
    #endregion
}
