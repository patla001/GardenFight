using UnityEngine;
using System.Collections;

public class LaserAttack : MonoBehaviour
{
    [Header("Laser Settings")]
    public GameObject laserPrefab; // prefab used to represent the laser visually
    public float chargeTime = 1.5f; // time spent charging before the laser reaches full size
    public float laserLength = 15f; // maximum length of the laser beam
    public float laserFullWidth = 1f; // maximum width of the laser beam
    public float laserHeight = 0.1f;  // thickness of the laser visual in the Y axis
    public int damage = 30; // damage dealt to the player when hit
    public LayerMask playerLayer; // layer mask used to detect players
    public float laserVisibleTime = 0.5f; // how long the laser remains visible after firing

    // public method to trigger the laser attack
    // takes a start position (where the laser originates) and a target position (where it points)
    public void FireLaser(Vector3 startPosition, Vector3 targetPosition)
    {
        StartCoroutine(LaserCoroutine(startPosition, targetPosition));
    }

    // coroutine that handles the full lifecycle of the laser attack
    private IEnumerator LaserCoroutine(Vector3 startPosition, Vector3 targetPosition)
    {
        // safety check: if no prefab is assigned, exit
        if (laserPrefab == null) yield break;

        // create the laser object at the start position
        GameObject laser = Instantiate(laserPrefab, startPosition, Quaternion.identity);

        // orient the laser to face the target position
        laser.transform.LookAt(targetPosition);

        // initialize laser as very thin (charging starts small)
        laser.transform.localScale = new Vector3(0.1f, laserHeight, 0.1f);

        float timer = 0f;

        // charging phase: gradually increase width and length over chargeTime
        while (timer < chargeTime)
        {
            timer += Time.deltaTime;
            float t = timer / chargeTime; // normalized progress (0 → 1)

            // smoothly interpolate scale from thin to full size
            laser.transform.localScale = new Vector3(
                Mathf.Lerp(0.1f, laserFullWidth, t), // width grows
                laserHeight,                        // height stays constant
                Mathf.Lerp(0.1f, laserLength, t)    // length grows
            );

            yield return null; // wait until next frame
        }

        // damage phase: cast a ray along the laser’s forward direction
        RaycastHit[] hits = Physics.RaycastAll(startPosition, laser.transform.forward, laserLength, playerLayer);
        foreach (var hit in hits)
        {
            // check if the hit object has a Player component
            Player player = hit.collider.GetComponent<Player>();
            if (player != null)
            {
                // apply damage to the player
                player.TakeDamage(damage, "Laser");
            }
        }

        // keep the laser visible for a short time after firing
        yield return new WaitForSeconds(laserVisibleTime);

        // destroy the laser object to clean up
        Destroy(laser);
    }
}
