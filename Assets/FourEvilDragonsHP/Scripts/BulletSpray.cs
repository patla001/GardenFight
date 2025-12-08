using UnityEngine;
using System.Collections;
using Mirror;

public class BulletSpray : NetworkBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab; // prefab used to create bullets
    public int damage = 10; // damage each bullet will deal
    public float bulletSpeed = 10f; // speed at which bullets travel

    [Header("Spray Settings")]
    public float duration = 5f; // total time the spray attack lasts
    public float cooldownBetweenShots = 1f; // delay between consecutive bullet spawns
    public float spawnRadius = 1.5f; // distance from boss center where bullets are spawned

    private bool running = false; // flag to track if spray sequence is active

    // check if spray sequence is currently running
    public bool IsRunning()
    {
        return running;
    }

    // entry point to start the spray attack
    public void StartSpray()
    {
        if (!running) // only start if not already active
            StartCoroutine(SpraySequence());
    }

    // coroutine that controls the spray attack over time
    private IEnumerator SpraySequence()
    {
        running = true; // mark as active
        float endTime = Time.time + duration; // calculate when to stop

        // keep spawning bullets until duration expires
        while (Time.time < endTime)
        {
            SpawnBullet(); // create and launch a bullet
            yield return new WaitForSeconds(cooldownBetweenShots); // wait before next shot
        }

        running = false; // mark as finished
    }

    // handles spawning and launching a single bullet
    private void SpawnBullet()
    {
        if (bulletPrefab == null) return; // safety check

        // choose a random spawn offset around the boss
        Vector3 offset = new Vector3(
            Random.Range(-spawnRadius, spawnRadius), // random X offset
            Random.Range(0.5f, 2f),                 // random Y offset (slightly above ground)
            Random.Range(-spawnRadius, spawnRadius) // random Z offset
        );

        // instantiate bullet at boss position + offset
        GameObject bullet = Instantiate(bulletPrefab, transform.position + offset, Quaternion.identity);

        // assign a random direction for the bullet to travel
        Vector3 direction = Random.onUnitSphere; // random unit vector
        direction.y = Mathf.Abs(direction.y);    // bias upward so bullets don’t shoot downward
        bullet.transform.forward = direction;    // orient bullet toward chosen direction

        // spawn bullet across the network so all clients see it
        NetworkServer.Spawn(bullet);

        // apply velocity to bullet’s rigidbody if present
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * bulletSpeed;
        }

        // assign damage value to bullet script if present
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = damage;
        }
    }
}
