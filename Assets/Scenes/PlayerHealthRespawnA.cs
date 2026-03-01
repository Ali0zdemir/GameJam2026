using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerHealthRespawnA : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("UI")]
    [Tooltip("Var olan health slider'ını buraya sürükle.")]
    public Slider healthSlider;

    [Header("Respawn")]
    public Transform respawnPoint;             // Inspector’dan sürükle
    public string respawnPointTag = "Respawn"; // boşsa tag ile bul
    public float respawnDelay = 1.5f;

    [Header("Invincibility")]
    public float invincibilityDuration = 1.0f;

    public event Action OnDied;

    bool isDead;
    bool isInvincible;

    Rigidbody rb;
    PlayerMovementA movement;
    Coroutine invRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<PlayerMovementA>();
    }

    void Start()
    {
        // respawn point otomatik bul
        if (respawnPoint == null && !string.IsNullOrEmpty(respawnPointTag))
        {
            GameObject rp = GameObject.FindGameObjectWithTag(respawnPointTag);
            if (rp) respawnPoint = rp.transform;
        }

        // slider setup
        SetupSlider();

        // oyun başı
        ResetToFullAndRespawn();
    }

    void SetupSlider()
    {
        if (healthSlider == null) return;

        healthSlider.minValue = 0f;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    void UpdateSlider()
    {
        if (healthSlider == null) return;

        // maxHealth inspector’da değişirse slider da uysun
        if (!Mathf.Approximately(healthSlider.maxValue, maxHealth))
            healthSlider.maxValue = maxHealth;

        healthSlider.value = currentHealth;
    }

    public void ResetToFullAndRespawn()
    {
        currentHealth = maxHealth;
        isDead = false;
        isInvincible = false;

        StopAllCoroutines();
        invRoutine = null;

        TeleportToRespawn();
        StartInvincibility();

        UpdateSlider();
    }

    public bool TakeDamage(float amount)
    {
        if (isDead) return false;
        if (isInvincible) return false;
        if (amount <= 0f) return false;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        UpdateSlider();

        if (currentHealth <= 0f)
        {
            Die();
            return true;
        }

        StartInvincibility();
        return true;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        OnDied?.Invoke();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        ResetToFullAndRespawn();
    }

    void TeleportToRespawn()
    {
        if (respawnPoint == null) return;

        Vector3 targetPos = respawnPoint.position;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = targetPos;
        }
        else
        {
            transform.position = targetPos;
        }

        // PlayerMovementA state temizliği
        if (movement != null)
        {
            movement.isGround = true;
            movement.isWalking = false;
        }
    }

    void StartInvincibility()
    {
        if (invRoutine != null) StopCoroutine(invRoutine);
        invRoutine = StartCoroutine(InvincibilityRoutine());
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }
}