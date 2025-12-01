using UnityEngine;
using System.Collections;

public class BulletHell : MonoBehaviour
{
    [Header("Ring Radii")]
    public float innerRadius = 3f;
    public float middleRadius = 6f;
    public float outerRadius = 9f;

    [Header("Damage Settings")]
    public int damage = 20;                     // matches Player.TakeDamage(int damage, string type)
    public string damageType = "Magic";         // can be "Magic", "Sword", or "Fist"
    public LayerMask playerLayer;               // set this to the Player layer
    public float delayBetweenWaves = 1.5f;

    [Header("Visuals (optional)")]
    public GameObject ringPrefab;
    public float ringVisualDuration = 1.4f;

    private bool running = false;

    public bool IsRunning()
    {
        return running;
    }

    public void StartRingSequence()
    {
        if (!running)
            StartCoroutine(RingSequence());
    }

    private IEnumerator RingSequence()
    {
        running = true;

        // Optional visuals
        GameObject outerVis = null, midVis = null, innerVis = null;
        if (ringPrefab != null)
        {
            outerVis = InstantiateRing(outerRadius);
            midVis = InstantiateRing(middleRadius);
            innerVis = InstantiateRing(innerRadius);
        }

        // WAVE 1 → Outer safe
        DoWave(safeRing: 3);
        UpdateRingVisuals(ref outerVis, ref midVis, ref innerVis, safeRing: 3);
        yield return new WaitForSeconds(delayBetweenWaves);

        // WAVE 2 → Middle safe
        DoWave(safeRing: 2);
        UpdateRingVisuals(ref outerVis, ref midVis, ref innerVis, safeRing: 2);
        yield return new WaitForSeconds(delayBetweenWaves);

        // WAVE 3 → Inner safe
        DoWave(safeRing: 1);
        UpdateRingVisuals(ref outerVis, ref midVis, ref innerVis, safeRing: 1);
        yield return new WaitForSeconds(delayBetweenWaves);

        // Destroy visuals
        if (outerVis != null) Destroy(outerVis, ringVisualDuration);
        if (midVis != null) Destroy(midVis, ringVisualDuration);
        if (innerVis != null) Destroy(innerVis, ringVisualDuration);

        running = false;
    }

    private void DoWave(int safeRing)
    {
        if (safeRing != 1) DamageRing(innerRadius);
        if (safeRing != 2) DamageRing(middleRadius);
        if (safeRing != 3) DamageRing(outerRadius);

        Debug.Log("BulletHell: Wave fired. Safe ring: " + safeRing);
    }

    private void DamageRing(float radius)
    {
        float ringWidth = 1f; // thickness of unsafe ring

        Collider[] hits = Physics.OverlapSphere(transform.position, radius + ringWidth * 0.5f, playerLayer);

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist >= radius - ringWidth * 0.5f && dist <= radius + ringWidth * 0.5f)
            {
                // call your Player.TakeDamage
                Player player = hit.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage(damage, damageType);
                }
            }
        }

        DebugDrawRing(radius, Color.red, 1f);
    }

    #region Visual Helpers
    private GameObject InstantiateRing(float radius)
    {
        if (ringPrefab == null) return null;
        GameObject g = Instantiate(ringPrefab, transform.position, Quaternion.identity, transform);
        float diameter = radius * 2f;
        g.transform.localScale = new Vector3(diameter, 1f, diameter);
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
        int segments = 60;
        float step = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float a1 = Mathf.Deg2Rad * (i * step);
            float a2 = Mathf.Deg2Rad * ((i + 1) * step);

            Vector3 p1 = transform.position + new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1)) * radius;
            Vector3 p2 = transform.position + new Vector3(Mathf.Cos(a2), 0, Mathf.Sin(a2)) * radius;

            Debug.DrawLine(p1, p2, color, duration);
        }
    }
    #endregion
}
