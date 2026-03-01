using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth3D : MonoBehaviour
{
    [Header("Health")]
    public HealthData healthData;
    public float restartHealth = 100f;

    [Header("Restart Pozisyonu")]
    public Transform restartPoint; // Buraya spawn noktasını sürükle

    [Header("Invincibility")]
    public float invincibilityDuration = 1f;

    public event Action OnDied;

    bool isInvincible = false;
    bool isDead = false;

    void Awake()
    {
        ResetHealthToRestartValue();
    }

    public void ResetHealthToRestartValue()
    {
        if (healthData == null) return;

        healthData.currentHealth = restartHealth;
        isInvincible = false;
        isDead = false;
        StopAllCoroutines();

        // Restart noktası varsa oraya ışınla
        if (restartPoint != null)
            transform.position = restartPoint.position;
    }

    public bool TakeDamage(float amount)
    {
        if (healthData == null) return false;
        if (isDead) return false;
        if (isInvincible) return false;

        healthData.currentHealth -= amount;
        healthData.currentHealth = Mathf.Max(healthData.currentHealth, 0f);

        if (healthData.currentHealth <= 0f)
        {
            isDead = true;
            OnDied?.Invoke();
            return true;
        }

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