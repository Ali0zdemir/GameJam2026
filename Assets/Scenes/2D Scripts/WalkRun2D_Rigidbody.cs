using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WalkRun2D_Rigidbody : MonoBehaviour
{
    [Header("Move")]
    public float walkSpeed = 6f;
    public KeyCode runKey = KeyCode.LeftShift;

    [Header("Jump")]
    public bool enableJump = true;
    public float jumpForce = 12f;
    [Tooltip("Yüksek değer = daha hızlı düşer (daha az asılı kalır). Önerilen: 2-5")]
    public float fallMultiplier = 3f;
    [Tooltip("Zıplama yukarı çıkışı çarpanı. Düşük = kısa zıplama. Önerilen: 1-3")]
    public float lowJumpMultiplier = 2f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    [Tooltip("Dash süresi (saniye)")]
    public float dashDuration = 0.15f;
    [Tooltip("Dash cooldown (saniye)")]
    public float dashCooldown = 0.25f;

    [Header("Ground")]
    public string groundTag = "Ground";

    [Header("Health")]
    public HealthData healthData;

    [Header("Invincibility")]
    public float invincibilityDuration = 1.5f;

    [Header("Knockback")]
    public float knockbackDuration = 0.2f;

    Rigidbody2D rb;
    float inputX;
    bool isGrounded;

    bool isDashing;
    float dashTimer;
    float dashCooldownTimer;
    float dashDirection;

    bool isInvincible;
    bool isKnockedBack;
    float knockbackTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Knockback süresi sayacı
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
                isKnockedBack = false;
            return; // Knockback sırasında hareket etme
        }

        if (healthData != null && healthData.currentHealth <= 0) return;

        inputX = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(runKey) && !isDashing && dashCooldownTimer <= 0f && inputX != 0f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            dashDirection = inputX;
        }

        if (enableJump && Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        // TEST: K tuşu ile 10 hasar al
        if (Input.GetKeyDown(KeyCode.K))
            TakeDamage(10f);
    }

    void FixedUpdate()
    {
        if (isKnockedBack) return;
        if (healthData != null && healthData.currentHealth <= 0) return;

        if (isDashing)
        {
            rb.velocity = new Vector2(dashDirection * dashSpeed, 0f);
            dashTimer -= Time.fixedDeltaTime;

            if (dashTimer <= 0f)
                isDashing = false;

            return;
        }

        rb.velocity = new Vector2(inputX * walkSpeed, rb.velocity.y);

        if (rb.velocity.y < 0f)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0f && !Input.GetKey(KeyCode.Space))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    // Hasar al (dokunulmazlık destekli)
    public bool TakeDamage(float amount)
    {
        if (isInvincible) return false;
        if (healthData == null) return false;

        healthData.currentHealth -= amount;
        healthData.currentHealth = Mathf.Max(healthData.currentHealth, 0f);

        if (healthData.currentHealth <= 0)
            Debug.Log("Player Died!");

        StartCoroutine(InvincibilityRoutine());
        return true;
    }

    // Knockback uygula
    public void ApplyKnockback(Vector2 direction, float force)
    {
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag(groundTag))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag(groundTag))
            isGrounded = false;
    }
}