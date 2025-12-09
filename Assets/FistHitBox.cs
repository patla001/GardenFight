using UnityEngine;

public class FistHitbox : MonoBehaviour
{
    public int damage = 10;
    public string bossTag = "Boss";
    public Collider col;
    private bool hasHit = false;

    void Start()
    {
        col = GetComponent<Collider>();
        col.enabled = false;
    }

    public void EnableHitbox()
    {
        Debug.Log("Fist Collider ENABLED");
        hasHit = false;
        col.enabled = true;
    }

    public void DisableHitbox()
    {
        Debug.Log("Fist Collider DISABLED");
        col.enabled = false;
        hasHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit)
            return;

        Debug.Log("Hit collider: " + other.name);
        Debug.Log("Collider type: " + other.GetType().Name);
        Debug.Log("Bounds size: " + other.bounds.size);


        BossHealth boss = other.GetComponentInParent<BossHealth>();

        if (boss != null)
        {
            Debug.Log("Damage dealt: " + damage);
            boss.TakeDamage(damage);

            hasHit = true;    
        }
    }
}
