using UnityEngine;
using System.Collections;

public class LaserAttack : MonoBehaviour
{
    [Header("Laser Settings")]
    public GameObject laserPrefab;       // prefab used to represent the laser visually
    public float chargeTime = 1.5f;      // time spent charging before the laser reaches full size
    public float laserLength = 15f;      // maximum length of the laser beam
    public float laserFullWidth = 1f;    // maximum width of the laser beam
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
    // includes charging, damaging, and cleanup phases
    private IEnumerator LaserCoroutine(Vector3 startPosition, Vector3 targetPosition)
    {
        // safety check: if no prefab is assigned, exit early
        if (laserPrefab == null) yield break;

        // create the laser object at the start position
        GameObject laser = Instantiate(laserPrefab, startPosition, Quaternion.identity);

        // orient the laser to face the target position (beam points forward)
        laser.transform.LookAt(targetPosition);

        // initialize laser as very thin (charging starts small)
        laser.transform.localScale = new Vector3(0.1f, laserHeight, 0.1f);

        // add collider if not already present (used for detecting player contact)
        BoxCollider col = laser.GetComponent<BoxCollider>();
        if (col == null) col = laser.AddComponent<BoxCollider>();
        col.isTrigger = true; // set collider to trigger mode so it doesn’t block movement

        // attach damage handler with cooldown logic
        LaserDamageHandler handler = laser.AddComponent<LaserDamageHandler>();
        handler.damage = damage;               // pass damage value to handler
        handler.damageInterval = damageInterval; // pass damage interval to handler

        float timer = 0f;

        // charging phase: gradually increase width and length over chargeTime
        while (timer < chargeTime)
        {
            timer += Time.deltaTime;
            float t = timer / chargeTime; // normalized progress (0 → 1)

            // smoothly interpolate scale from thin to full size
            float width = Mathf.Lerp(0.1f, laserFullWidth, t);
            float length = Mathf.Lerp(0.1f, laserLength, t);

            laser.transform.localScale = new Vector3(width, laserHeight, length);

            // update collider size to match beam’s current scale
            col.size = new Vector3(width, laserHeight, length);
            col.center = new Vector3(0, 0, length * 0.5f); // shift collider forward

            yield return null; // wait until next frame
        }

        // damage phase: laser is fully charged and active
        // keep the laser visible for a short time after firing
        yield return new WaitForSeconds(laserVisibleTime);

        // cleanup: destroy the laser object to free memory
        Destroy(laser);
    }
}

// helper component that applies damage when the player touches the laser
// uses a cooldown so damage is applied once per interval instead of every frame
public class LaserDamageHandler : MonoBehaviour
{
    public int damage = 30;              // damage dealt per tick
    public string damageType = "Laser";  // type of damage (for player logic)
    public float damageInterval = 1f;    // seconds between damage ticks

    private float lastDamageTime = -Mathf.Infinity; // tracks last time damage was applied

    // called every frame while another collider stays inside this trigger
    private void OnTriggerStay(Collider other)
    {
        // check if the object inside the trigger is the player
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            // only apply damage if enough time has passed since the last tick
            if (Time.time - lastDamageTime >= damageInterval)
            {
                player.TakeDamage(damage, damageType); // apply damage to player
                Debug.Log("Player damaged by laser");  // log for debugging
                lastDamageTime = Time.time;            // update last damage time
            }
        }
    }
}
