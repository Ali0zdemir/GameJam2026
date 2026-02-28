using System.Collections;
using UnityEngine;

public class PlayerHealth3D : MonoBehaviour
{
    [Header("Health")]
    public HealthData healthData;

    [Header("Invincibility")]
    public float invincibilityDuration = 1f;

    bool isInvincible = false;

    public bool TakeDamage(float amount)
    {
        if (healthData == null) return false;
        if (isInvincible) return false;

        healthData.currentHealth -= amount;
        healthData.currentHealth = Mathf.Max(healthData.currentHealth, 0f);

        if (healthData.currentHealth <= 0)
            Debug.Log("3D Player Died!");

        StartCoroutine(InvincibilityRoutine());
        return true;
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }
}