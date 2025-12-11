using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    [Header("Bullet Settings")]
    public int damage = 10;      // how much damage this bullet deals on hit
    public float lifetime = 5f;  // how long the bullet exists before auto-despawning

    private void Start()
    {
        // automatically destroy the bullet after its lifetime expires
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // only the server should handle damage + destruction logic
        if (!isServer) return;

        // check if the bullet hit the player
        if (collision.collider.CompareTag("Player"))
        {
            // try to get the Player component from the hit object
            Player player = collision.collider.GetComponent<Player>();
            if (player != null)
            {
                // apply damage using the player's damage handler
                player.TakeDamage(damage, "Magic"); // attack type can be changed as needed
            }

            // destroy the bullet across the network so all clients see it disappear
            NetworkServer.Destroy(gameObject);
        }
    }
}
