using UnityEngine;

public class FistHitbox : MonoBehaviour
{
    public int damage = 10;
    public string bossTag = "Boss";

    public Collider col;

    void Start()
    {
        col = GetComponent<Collider>();
        col.enabled = false;
    }

    //So the hitbox only activates when the animation is active
    public void EnableHitbox() => col.enabled = true;
    public void DisableHitbox() => col.enabled = false;


    private void OnTriggerEnter(Collider other)
    {
        BossHealth boss = other.GetComponent<BossHealth>();

        if (boss != null)
        {
            boss.TakeDamage(damage);
        }
    }
}
