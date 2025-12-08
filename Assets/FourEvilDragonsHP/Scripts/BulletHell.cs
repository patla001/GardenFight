using UnityEngine;
using System.Collections;

public class BulletHell : MonoBehaviour
{
    [Header("Ring Radii (base values)")]
    public float innerRadius = 3f; // base radius for the innermost ring
    public float middleRadius = 6f; // base radius for the middle ring
    public float outerRadius = 9f; // base radius for the outermost ring

    [Header("Runtime Scaling")]
    [Range(0.1f, 2f)]
    public float radiusScale = 1f;      // global scale factor applied to all radii and physics checks
    [Range(0.1f, 2f)]
    public float visualScale = 1f;      // independent scale factor for visuals (lets you tune appearance separately)

    [Header("Visual Height")]
    [Range(0.001f, 1f)]
    public float ringHeight = 0.05f;    // thickness of the ring visual in the Y axis (keeps it flat/thin)

    [Header("Damage Settings")]
    public int damage = 20; // amount of damage dealt to the player when hit
    public string damageType = "Magic"; // type of damage (could be used for resistances, etc.)
    public LayerMask playerLayer; // layer mask used to detect players inside the rings
    public float delayBetweenWaves = 1.5f; // time delay between each wave of rings
    public float ringWidth = 1f; // thickness of the damaging ring area

    [Header("Visuals (optional)")]
    public GameObject ringPrefab; // prefab used to show ring visuals in the scene
    public float ringVisualDuration = 1.4f; // how long the ring visuals remain before being destroyed

    private bool running = false; // flag to track if the sequence is currently active

    // check if the sequence is running
    public bool IsRunning()
    {
        return running;
    }

    // entry point to start the ring sequence
    public void StartRingSequence()
    {
        if (!running) // only start if not already active
            StartCoroutine(RingSequence());
    }

    // coroutine that runs the full ring attack sequence
    private IEnumerator RingSequence()
    {
        running = true; // mark as active

        // optional visuals: create ring prefabs if assigned
        GameObject outerVis = null, midVis = null, innerVis = null;
        if (ringPrefab != null)
        {
            outerVis = InstantiateRing(outerRadius);
            midVis = InstantiateRing(middleRadius);
            innerVis = InstantiateRing(innerRadius);
        }

        // wave 1: outer ring is safe, inner/middle deal damage
        DoWave(safeRing: 3);
        UpdateRingVisuals(ref outerVis, ref midVis, ref innerVis, safeRing: 3);
        yield return new WaitForSeconds(delayBetweenWaves);

        // wave 2: middle ring is safe
        DoWave(safeRing: 2);
        UpdateRingVisuals(ref outerVis, ref midVis, ref innerVis, safeRing: 2);
        yield return new WaitForSeconds(delayBetweenWaves);

        // wave 3: inner ring is safe
        DoWave(safeRing: 1);
        UpdateRingVisuals(ref outerVis, ref midVis, ref innerVis, safeRing: 1);
        yield return new WaitForSeconds(delayBetweenWaves);

        // clean up visuals after sequence ends
        if (outerVis != null) Destroy(outerVis, ringVisualDuration);
        if (midVis != null) Destroy(midVis, ringVisualDuration);
        if (innerVis != null) Destroy(innerVis, ringVisualDuration);

        running = false; // mark as finished
    }

    // executes a single wave, damaging all rings except the designated safe one
    private void DoWave(int safeRing)
    {
        if (safeRing != 1) DamageRing(innerRadius);   // damage inner ring if not safe
        if (safeRing != 2) DamageRing(middleRadius);  // damage middle ring if not safe
        if (safeRing != 3) DamageRing(outerRadius);   // damage outer ring if not safe

        Debug.Log("BulletHell: wave fired, safe ring " + safeRing);
    }

    // applies damage to players standing inside a specific ring
    private void DamageRing(float radius)
    {
        // scale radius and width based on global scale factor
        float scaledRadius = radius * radiusScale;
        float scaledWidth = ringWidth * radiusScale;

        // find all colliders within the ring’s area
        Collider[] hits = Physics.OverlapSphere(transform.position, scaledRadius + scaledWidth * 0.5f, playerLayer);

        foreach (var hit in hits)
        {
            // measure distance from center to player
            float dist = Vector3.Distance(transform.position, hit.transform.position);

            // check if player is inside the ring’s band (between inner and outer edges)
            if (dist >= scaledRadius - scaledWidth * 0.5f && dist <= scaledRadius + scaledWidth * 0.5f)
            {
                // apply damage if player component is found
                Player player = hit.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage(damage, damageType);
                }
            }
        }

        // draw debug ring in editor for visualization
        DebugDrawRing(scaledRadius, Color.red, 1f);
    }

    #region Visual Helpers
    // creates a visual ring prefab at the given radius
    private GameObject InstantiateRing(float radius)
    {
        if (ringPrefab == null) return null;

        float scaledRadius = radius * radiusScale;
        GameObject g = Instantiate(ringPrefab, transform.position, Quaternion.identity, transform);

        // calculate diameter and apply visual scaling
        float diameter = scaledRadius * 2f;

        // set local scale: diameter in X/Z, thin height in Y
        g.transform.localScale = new Vector3(diameter * visualScale, ringHeight, diameter * visualScale);

        return g;
    }

    // updates ring colors to show which ring is safe (green) and which are unsafe (red)
    private void UpdateRingVisuals(ref GameObject outer, ref GameObject mid, ref GameObject inner, int safeRing)
    {
        SetVisualColor(outer, (safeRing == 3) ? Color.green : Color.red);
        SetVisualColor(mid, (safeRing == 2) ? Color.green : Color.red);
        SetVisualColor(inner, (safeRing == 1) ? Color.green : Color.red);
    }

    // helper to set the color of a ring prefab
    private void SetVisualColor(GameObject g, Color color)
    {
        if (g == null) return;
        var rend = g.GetComponent<Renderer>();
        if (rend != null && rend.material != null)
            rend.material.color = color;
    }
    #endregion

    #region Debug
    // draws a debug circle in the editor to visualize ring positions
    private void DebugDrawRing(float radius, Color color, float duration)
    {
        int segments = 60; // number of line segments to approximate the circle
        float step = 360f / segments; // angle step per segment

        for (int i = 0; i < segments; i++)
        {
            float a1 = Mathf.Deg2Rad * (i * step);
            float a2 = Mathf.Deg2Rad * ((i + 1) * step);

            // calculate two points on the circle
            Vector3 p1 = transform.position + new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1)) * radius;
            Vector3 p2 = transform.position + new Vector3(Mathf.Cos(a2), 0, Mathf.Sin(a2)) * radius;

            // draw a line between them
            Debug.DrawLine(p1, p2, color, duration);
        }
    }
    #endregion
}
