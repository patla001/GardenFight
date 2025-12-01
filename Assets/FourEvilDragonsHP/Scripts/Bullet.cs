using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    public int damage = 10; // how much damage this bullet deals

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return; // only server applies damage

        Player player = other.GetComponent<Player>(); // check if hit object is player
        if (player != null)
        {
            player.TakeDamage(damage, "Magic"); // apply damage to player
            NetworkServer.Destroy(gameObject); // destroy bullet after hit
        }
        else
        {
            // destroy bullet if it hits something else like walls
            NetworkServer.Destroy(gameObject);
        }
    }

    // optional lifetime so bullets dont stay forever
    public float lifetime = 5f;
    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), lifetime); // schedule destroy after lifetime
    }

    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject); // remove bullet from server
    }
}
