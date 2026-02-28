using System.Collections;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public float respawnDelay = 1f;

    [Tooltip("Karakter gaza değdikten kaç saniye sonra donsun?")]
    public float freezeDelay = 0.15f;

    private Rigidbody2D rb;
    private WalkRun2D_Rigidbody controller;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<WalkRun2D_Rigidbody>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void TriggerInstantDeath()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        // 1. Kontrolü hemen kes
        if (controller != null) controller.enabled = false;

        // 2. Biraz batması için bekle
        yield return new WaitForSeconds(freezeDelay);

        // 3. Havada dondur
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // 4. Görseli kapat
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        // 5. Respawn noktasına ışınla
        if (respawnPoint != null)
            transform.position = respawnPoint.position;

        // 6. Canı full yap (WalkRun2D_Rigidbody üzerinden)
        if (controller != null && controller.healthData != null)
            controller.healthData.currentHealth = controller.healthData.maxHealth;

        // 7. Her şeyi eski haline getir
        if (spriteRenderer != null) spriteRenderer.enabled = true;

        if (rb != null) rb.isKinematic = false;

        if (controller != null) controller.enabled = true;
    }
}