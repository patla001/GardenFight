using UnityEngine;
using System.Collections;
using Mirror;

public class BulletSpray : NetworkBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab; // prefab for the bullets
    public int damage = 10; // bullet damage
    public float bulletSpeed = 10f; // speed of bullets

    [Header("Spray Settings")]
    public float duration = 5f; // total duration of the spray attack
    public float cooldownBetweenShots = 1f; // time between each bullet
    public float spawnRadius = 1.5f; // distance from boss center to spawn bullet

    private bool running = false;

    public bool IsRunning()
    {
        return running;
    }

    public void StartSpray()
    {
        if (!running)
            StartCoroutine(SpraySequence());
    }

    private IEnumerator SpraySequence()
    {
        running = true;
        float endTime = Time.time + duration;

        while (Time.time < endTime)
        {
            SpawnBullet();
            yield return new WaitForSeconds(cooldownBetweenShots);
        }

        running = false;
    }

    private void SpawnBullet()
    {
        if (bulletPrefab == null) return;

        // spawn point randomly around boss
        Vector3 offset = new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            Random.Range(0.5f, 2f),
            Random.Range(-spawnRadius, spawnRadius)
        );

        GameObject bullet = Instantiate(bulletPrefab, transform.position + offset, Quaternion.identity);

        // shoot in random direction
        Vector3 direction = Random.onUnitSphere;
        direction.y = Mathf.Abs(direction.y); // optional upward bias
        bullet.transform.forward = direction;

        NetworkServer.Spawn(bullet);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * bulletSpeed;
        }

        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.damage = damage;
        }
    }
}
