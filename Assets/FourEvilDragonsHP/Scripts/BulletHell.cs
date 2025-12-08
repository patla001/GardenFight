using UnityEngine;
using System.Collections;

public class BulletHell : MonoBehaviour
{
    [Header("Ring Radii (base values)")]
    public float innerRadius = 3f; // base radius for inner ring
    public float middleRadius = 6f; // base radius for middle ring
    public float outerRadius = 9f; // base radius for outer ring

    [Header("Runtime Scaling")]
    [Range(0.1f, 2f)]
    public float radiusScale = 1f;      // global scale for radii/physics
    [Range(0.1f, 2f)]
    public float visualScale = 1f;      // independent scale for the ring visual prefab
    [Header("Visual Height")]
    [Range(0.001f, 1f)]
    public float ringHeight = 0.05f;    // Y scale for the visual (makes the ring thin)

    [Header("Damage Settings")]
    public int damage = 20; // amount of damage dealt to player
    public string damageType = "Magic";
    public LayerMask playerLayer; // layer mask to detect player
    public float delayBetweenWaves = 1.5f; // delay between waves
    public float ringWidth = 1f; // thickness of unsafe ring (base)

    [Header("Visuals (optional)")]
    public GameObject ringPrefab; // prefab for ring visuals
    public float ringVisualDuration = 1.4f; // how long visuals last

    private bool running = false; // track if sequence is running

    public bool IsRunning()
    {
        return running; // return current running state
    }

    public void StartRingSequence()
    {
        if (!running) // only start if not already running
            StartCoroutine(RingSequence());
    }

    private IEnumerator RingSequence()
    {
        running = true; // mark as running

        // optional visuals
        GameObject outerVis = null, midVis = null, innerVis = null;
        if (ringPrefab != null)
        {
            outerVis = InstantiateRing(outerRadius);
            midVis = InstantiateRing(middleRadius);
            innerVis = InstantiateRing(innerRadius);
        }

        // wave 1 outer safe
        DoWave(safeRing: 3);
        UpdateRingVisuals(ref outerVis, ref midVis, ref innerVis, safeRing: 3);
        yield return new WaitForSeconds(delayBetweenWaves);

        // wave 2 middle safe
        DoWave(safeRing: 2);
        UpdateRingVisuals(ref outerVis, ref midVis, ref innerVis, safeRing: 2);
        yield return new WaitForSeconds(delayBetweenWaves);

        // wave 3 inner safe
        DoWave(safeRing: 1);
        UpdateRingVisuals(ref outerVis, ref midVis, ref innerVis, safeRing: 1);
        yield return new WaitForSeconds(delayBetweenWaves);

        // destroy visuals after sequence
        if (outerVis != null) Destroy(outerVis, ringVisualDuration);
        if (midVis != null) Destroy(midVis, ringVisualDuration);
        if (innerVis != null) Destroy(innerVis, ringVisualDuration);

        running = false; // mark as finished
    }

    private void DoWave(int safeRing)
    {
        if (safeRing != 1) DamageRing(innerRadius); // damage inner if not safe
        if (safeRing != 2) DamageRing(middleRadius); // damage middle if not safe
        if (safeRing != 3) DamageRing(outerRadius); // damage outer if not safe

        Debug.Log("BulletHell: wave fired safe ring " + safeRing);
    }

    private void DamageRing(float radius)
    {
        // apply global scale
        float scaledRadius = radius * radiusScale;
        float scaledWidth = ringWidth * radiusScale;

        Collider[] hits = Physics.OverlapSphere(transform.position, scaledRadius + scaledWidth * 0.5f, playerLayer);

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist >= scaledRadius - scaledWidth * 0.5f && dist <= scaledRadius + scaledWidth * 0.5f)
            {
                // call player damage
                Player player = hit.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage(damage, damageType);
                }
            }
        }

        DebugDrawRing(scaledRadius, Color.red, 1f); // draw debug ring
    }

    #region Visual Helpers
    private GameObject InstantiateRing(float radius)
    {
        if (ringPrefab == null) return null;

        float scaledRadius = radius * radiusScale;
        GameObject g = Instantiate(ringPrefab, transform.position, Quaternion.identity, transform);

        // diameter * visualScale — use visualScale so visuals can be tuned independently
        float diameter = scaledRadius * 2f;

        // apply the inspector-controlled ringHeight for a thin visual ring
        g.transform.localScale = new Vector3(diameter * visualScale, ringHeight, diameter * visualScale);

        return g;
    }

    private void UpdateRingVisuals(ref GameObject outer, ref GameObject mid, ref GameObject inner, int safeRing)
    {
        SetVisualColor(outer, (safeRing == 3) ? Color.green : Color.red);
        SetVisualColor(mid, (safeRing == 2) ? Color.green : Color.red);
        SetVisualColor(inner, (safeRing == 1) ? Color.green : Color.red);
    }

    private void SetVisualColor(GameObject g, Color color)
    {
        if (g == null) return;
        var rend = g.GetComponent<Renderer>();
        if (rend != null && rend.material != null)
            rend.material.color = color;
    }
    #endregion

    #region Debug
    private void DebugDrawRing(float radius, Color color, float duration)
    {
        int segments = 60; // number of segments for circle
        float step = 360f / segments; // angle step

        for (int i = 0; i < segments; i++)
        {
            float a1 = Mathf.Deg2Rad * (i * step);
            float a2 = Mathf.Deg2Rad * ((i + 1) * step);

            Vector3 p1 = transform.position + new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1)) * radius;
            Vector3 p2 = transform.position + new Vector3(Mathf.Cos(a2), 0, Mathf.Sin(a2)) * radius;

            Debug.DrawLine(p1, p2, color, duration); // draw line between points
        }
    }
    #endregion
}
