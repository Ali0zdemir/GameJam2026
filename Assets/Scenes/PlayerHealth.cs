using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Data Reference")]
    public HealthData healthData;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 1.5f; // Hasar aldıktan sonraki dokunulmazlık süresi

    private bool isInvincible;

    void Update()
    {
        // TEST İÇİN: Klavyeden K tuşuna basıldığında 10 hasar al
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10f);
        }
    }

    public bool TakeDamage(float damageAmount)
    {
        // Eğer karakter şu an ölümsüzlük süresindeyse hasar alma işlemini iptal et
        if (isInvincible) return false;

        // Hasarı uygula
        healthData.currentHealth -= damageAmount;

        // Canın 0'ın altına düşmesini engelle
        if (healthData.currentHealth <= 0)
        {
            healthData.currentHealth = 0;
            Debug.Log("Player Died!");
        }

        // Hasar aldıktan sonra dokunulmazlık sayacını başlat
        StartCoroutine(InvincibilityRoutine());

        return true; // Hasar başarıyla alındı
    }

    private IEnumerator InvincibilityRoutine()
    {
        // 1. Ölümsüzlüğü aç
        isInvincible = true;

        // 2. Belirlenen süre kadar (örneğin 1.5 saniye) hiçbir şey yapmadan bekle
        yield return new WaitForSeconds(invincibilityDuration);

        // 3. Süre dolunca ölümsüzlüğü kapat, artık tekrar hasar alabilir
        isInvincible = false;
    }
}