using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    public int damage = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return; // only server handles damage

        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamage(damage, "Magic"); // server-authoritative
            NetworkServer.Destroy(gameObject);
        }
        else
        {
            // optionally destroy bullets hitting walls
            NetworkServer.Destroy(gameObject);
        }
    }

    // Optional lifetime for bullets
    public float lifetime = 5f;
    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), lifetime);
    }

    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
