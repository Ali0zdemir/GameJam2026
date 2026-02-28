using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Sağlık")]
    public float maxHealth = 100f;
    float currentHealth;

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
        Destroy(gameObject, 0.2f);
    }
}