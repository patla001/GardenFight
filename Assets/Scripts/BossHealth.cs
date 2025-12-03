using UnityEngine;
using UnityEngine.UI;


//Almost everything here is pretty straight forward
public class BossHealth : MonoBehaviour
{


    public int maxHealth = 100;
    public int currentHealth;

    public Slider healthSlider; 

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;

        //
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log("Boss Should Be Dead Now");
   
    }
}
