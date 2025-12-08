using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    // damage value for this bullet (applied when it hits a player)
    public int damage = 10;

    // called automatically when the bullet collides with another collider
    private void OnTriggerEnter(Collider other)
    {
        // only the server should handle damage and destruction logic
        if (!isServer) return;

        // check if the object we hit has a Player component
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            // if it’s a player, apply damage
            player.TakeDamage(damage, "Magic");

            // then destroy the bullet so it doesn’t keep flying
            NetworkServer.Destroy(gameObject);
        }
        else
        {
            // if it hits anything else (like walls or environment), just destroy it
            NetworkServer.Destroy(gameObject);
        }
    }

    // optional lifetime so bullets don’t persist forever in the scene
    public float lifetime = 5f;

    // called when the bullet is created on the server
    public override void OnStartServer()
    {
        // schedule the bullet to destroy itself after its lifetime expires
        Invoke(nameof(DestroySelf), lifetime);
    }

    // server-only method to remove the bullet
    [Server]
    void DestroySelf()
    {
        // destroy the bullet object across the network
        NetworkServer.Destroy(gameObject);
    }
}
