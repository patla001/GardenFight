using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaserAttack : MonoBehaviour
{
    [Header("Laser Settings")]
    public GameObject laserPrefab;       // prefab used to represent the laser visually (should contain mesh only)
    public float chargeTime = 1.5f;      // time spent charging before the laser reaches full size
    public float laserLength = 15f;      // maximum length of the laser beam (Z axis of visual)
    public float laserFullWidth = 1f;    // maximum width of the laser beam (X axis of visual)
    public float laserHeight = 0.1f;     // thickness of the laser visual in the Y axis
    public int damage = 30;              // damage dealt to the player when hit
    public float damageInterval = 1f;    // seconds between damage ticks while touching the laser
    public float laserVisibleTime = 0.5f;// how long the laser remains visible after firing

    // public method to trigger the laser attack
    // takes a start position (where the laser originates) and a target position (where it points)
    public void FireLaser(Vector3 startPosition, Vector3 targetPosition)
    {
        StartCoroutine(LaserCoroutine(startPosition, targetPosition));
    }

    // coroutine that handles the full lifecycle of the laser attack
    private IEnumerator LaserCoroutine(Vector3 startPosition, Vector3 targetPosition)
    {
        if (laserPrefab == null) yield break;

        // create a non-scaled parent which will hold the collider and damage handler.
        GameObject root = new GameObject("LaserRoot");
        root.transform.position = startPosition;
        root.transform.rotation = Quaternion.LookRotation((targetPosition - startPosition).normalized, Vector3.up);

        // add collider on the root (so it's not affected by visual scaling).
        BoxCollider col = root.AddComponent<BoxCollider>();
        col.isTrigger = true;

        // damage handler on the root (handles OnTriggerEnter/Exit and ticking).
        LaserDamageHandler handler = root.AddComponent<LaserDamageHandler>();
        handler.damage = damage;
        handler.damageInterval = damageInterval;
        handler.damageType = "Laser";

        // instantiate the visual prefab as a child so it can be scaled for the charging animation.
        GameObject visual = Instantiate(laserPrefab, root.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;

        // ensure root has scale 1 so collider stays predictable.
        root.transform.localScale = Vector3.one;
        visual.transform.localScale = new Vector3(0.1f, laserHeight, 0.1f);

        float timer = 0f;

        // charging phase: gradually increase width and length over chargeTime
        while (timer < chargeTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / chargeTime);

            float width = Mathf.Lerp(0.1f, laserFullWidth, t);
            float length = Mathf.Lerp(0.1f, laserLength, t);

            // Scale only the visual child
            visual.transform.localScale = new Vector3(width, laserHeight, length);

            // Update collider (on the root) — because root scale is (1,1,1) this is straightforward.
            col.size = new Vector3(width, laserHeight, length);
            // Move the collider forward so it covers the beam area: center.z = length/2
            col.center = new Vector3(0f, 0f, length * 0.5f);

            yield return null;
        }

        // damage phase: keep laser visible for a short time after firing
        yield return new WaitForSeconds(laserVisibleTime);

        Destroy(root); // destroys child visual as well
    }
}


// helper component that applies damage when the player touches the laser
// uses OnTriggerEnter/Exit and per-player coroutines to avoid lingering damage
public class LaserDamageHandler : MonoBehaviour
{
    public int damage = 30;              // damage dealt per tick
    public string damageType = "Laser";  // type of damage (for player logic)
    public float damageInterval = 1f;    // seconds between damage ticks

    // track active damage coroutines per player so we can stop them on exit
    private Dictionary<int, Coroutine> activeDamageRoutines = new Dictionary<int, Coroutine>();

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            int id = player.GetInstanceID();
            // avoid starting multiple coroutines for the same player
            if (!activeDamageRoutines.ContainsKey(id))
            {
                Coroutine c = StartCoroutine(ApplyDamageOverTime(player));
                activeDamageRoutines[id] = c;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            int id = player.GetInstanceID();
            if (activeDamageRoutines.TryGetValue(id, out Coroutine c))
            {
                StopCoroutine(c);
                activeDamageRoutines.Remove(id);
            }
        }
    }

    // coroutine that applies damage repeatedly while the player remains inside
    private IEnumerator ApplyDamageOverTime(Player player)
    {
        // immediate damage on entry (optional). If you don't want immediate tick, wait first.
        player.TakeDamage(damage, damageType);

        // then wait and apply repeated ticks
        while (true)
        {
            yield return new WaitForSeconds(damageInterval);
            if (player == null) yield break; // safety
            player.TakeDamage(damage, damageType);
        }
    }

    private void OnDisable()
    {
        // cleanup any remaining coroutines if object is destroyed early
        foreach (var kv in activeDamageRoutines)
            if (kv.Value != null) StopCoroutine(kv.Value);
        activeDamageRoutines.Clear();
    }
}
