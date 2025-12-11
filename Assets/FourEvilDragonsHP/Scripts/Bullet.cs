using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    public int damage = 10;
    public float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer) return;

        if (collision.collider.CompareTag("Player"))
        {
            Player player = collision.collider.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage, "Magic"); // or "Fist", "Sword", etc.
            }

            NetworkServer.Destroy(gameObject);
        }
    }
}
