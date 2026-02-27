using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Can")]
    public float maxHealth = 100f;
    float currentHealth;

    public float destroyDelay = 0.2f;      // Ölünce kaç saniye sonra yok olsun

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        Destroy(gameObject, destroyDelay);
    }
}