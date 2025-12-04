using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    public int damage = 10;
    public string bossTag = "Boss";

    public Collider col;

    // prevents multiple damage hits every swing
    private bool hasHit = false;

    void Start()
    {
        col = GetComponent<Collider>();
        col.enabled = false;
    }

    public void EnableHitbox()
    {
        Debug.Log("Sword Collider ENABLED");

        // reset so this swing can deal damage once
        hasHit = false;

        col.enabled = true;
    }

    public void DisableHitbox()
    {
        Debug.Log("Sword Collider DISABLED");
        col.enabled = false;
        hasHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // if weve already hit during this swing then ignore all further triggers
        if (hasHit)
            return;

        Debug.Log("Hit collider: " + other.name);

        BossHealth boss = other.GetComponentInParent<BossHealth>();

        if (boss != null)
        {
            Debug.Log("Damage dealt: " + damage);

            boss.TakeDamage(damage);

            // lock the hit for this attack window
            hasHit = true;
        }
    }
}
