using UnityEngine;
using Mirror;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SyncVar]
    public float currentHealth = 100f;

    public float maxHealth = 100f;

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
    }

    // This is the function the BossAI.cs script calls to initiate damage.
    // It must be public to be called from another script.
    public void TakeDamage(float damageAmount)
    {
        // Damage logic MUST happen on the Server in a networked game.
        // If this player is the server/host, we can call the command immediately.
        if (isServer)
        {
            CmdApplyDamage(damageAmount);
        }
        else if (isClient)
        {
            // If we are a client, we must send a Command to the server to process the damage.
            CmdApplyDamage(damageAmount);
        }
    }

    // [Command] tells Mirror to run this function on the server.
    // Only the local player can send commands for their own player object, 
    // but the BossAI is a server-controlled object, so it will call this from the server.
    [Command(requiresAuthority = false)] // requiresAuthority=false allows the BossAI (which doesn't have authority) to call this.
    void CmdApplyDamage(float damageAmount)
    {
        // Only run the actual health deduction on the server
        currentHealth -= damageAmount;

        // Check for death (Server handles the death logic)
        if (currentHealth <= 0)
        {
            currentHealth = 0; // Prevent negative health
            // Implement death/respawn logic here, e.g.:
            // RpcDie(); 
        }
    }

    // Note: Typically would add a function here to update the UI on clients 
    // using the [ClientRpc] attribute or by watching the SyncVar change.

    // Start and Update are left empty as you had them, but can be used for UI updates:
    void Start()
    {
        // No logic needed here for network sync vars
    }

    void Update()
    {
        // Example: Update health bar UI based on currentHealth here
    }
}