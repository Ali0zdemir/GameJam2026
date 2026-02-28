using System.Collections;
using System.Collections.Generic;
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
    [Tooltip("Yüksek değer = daha hızlı düşer. Önerilen: 2-5")]
    public float fallMultiplier = 3f;
    [Tooltip("Zıplama yukarı çıkışı çarpanı. Önerilen: 1-3")]
    public float lowJumpMultiplier = 2f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.25f;

    [Header("Wall")]
    public string wallTag = "Wall";
    public float wallSlideSpeed = 2f;
    public float wallGrabDuration = 0.2f;
    public Vector2 wallJumpForce = new Vector2(6f, 5f);

    [Header("Air Momentum")]
    public float airControlLockTime = 0.5f;
    public float airAcceleration = 100f;

    [Header("Ground")]
    public string groundTag = "Ground";

    [Header("Health")]
    public HealthData healthData;

    [Header("Invincibility")]
    public float invincibilityDuration = 1.5f;

    [Header("Knockback")]
    public float knockbackDuration = 0.2f;

    // ==========================================
    // SAVAŞ (MELEE) AYARLARI
    // ==========================================
    [Header("Melee Attack")]
    public Transform attackPoint;      // Kılıcın vuracağı merkez nokta
    public float attackRange = 1.2f;   // Kılıcın menzili (yarıçapı)
    public LayerMask enemyLayers;      // Hasar verebileceğimiz katman (Düşmanlar)
    public float attackRate = 2f;      // Saniyede kaç kere vurabilir (2 = yarım saniyede bir)

    private float nextAttackTime = 0f;
    private bool isFacingRight = true; // Karakterin yönünü takip etmek için
    // ==========================================

    Rigidbody2D rb;
    float inputX;
    bool isGrounded;

    // Dash
    bool isDashing;
    float dashTimer;
    float dashCooldownTimer;
    float dashDirection;

    // Invincibility & Knockback
    bool isInvincible;
    bool isKnockedBack;
    float knockbackTimer;

    // Jump Orb
    JumpOrb currentOrb = null;
    List<JumpOrb> usedOrbs = new List<JumpOrb>();

    // Wall
    bool isWalled;
    int wallDirection;
    bool isWallSliding;
    float currentWallGrabTimer;
    float currentAirControlTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f) isKnockedBack = false;
            return;
        }

        if (healthData != null && healthData.currentHealth <= 0) return;

        if (currentAirControlTimer > 0f)
            currentAirControlTimer -= Time.deltaTime;

        inputX = Input.GetAxisRaw("Horizontal");

        // ==========================================
        // YÖN DÖNDÜRME (FLIP) KONTROLÜ
        // ==========================================
        if (inputX > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (inputX < 0 && isFacingRight)
        {
            Flip();
        }

        // ==========================================
        // SALDIRI (ATTACK) KONTROLÜ
        // ==========================================
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0)) // Sol Tık
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }

        // Dash
        if (Input.GetKeyDown(runKey) && !isDashing && dashCooldownTimer <= 0f && inputX != 0f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            dashDirection = inputX;
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        CheckWallSliding();
        HandleJump();

        if (Input.GetKeyDown(KeyCode.K))
            TakeDamage(10f);
    }

    void FixedUpdate()
    {
        if (isKnockedBack) return;
        if (healthData != null && healthData.currentHealth <= 0) return;

        // Dash
        if (isDashing)
        {
            rb.velocity = new Vector2(dashDirection * dashSpeed, 0f);
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f) isDashing = false;
            return;
        }

        // Wall slide hareketi engelle
        if (isWallSliding) return;
        if (currentAirControlTimer > 0f) return;

        // Hareket
        if (isGrounded)
        {
            rb.velocity = new Vector2(inputX * walkSpeed, rb.velocity.y);
        }
        else
        {
            float newX = Mathf.MoveTowards(rb.velocity.x, inputX * walkSpeed, airAcceleration * Time.fixedDeltaTime);
            rb.velocity = new Vector2(newX, rb.velocity.y);
        }

        // Fall multiplier
        if (rb.velocity.y < 0f)
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        else if (rb.velocity.y > 0f && !Input.GetKey(KeyCode.Space))
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
    }

    void CheckWallSliding()
    {
        if (!isGrounded && isWalled && currentAirControlTimer <= 0f)
        {
            if (!isWallSliding)
            {
                isWallSliding = true;
                currentWallGrabTimer = wallGrabDuration;
            }

            if (currentWallGrabTimer > 0f)
            {
                currentWallGrabTimer -= Time.deltaTime;
                rb.velocity = new Vector2(rb.velocity.x, 0f);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
        }
        else
        {
            isWallSliding = false;
        }
    }

    void HandleJump()
    {
        // Wall jump
        if (enableJump && Input.GetKeyDown(KeyCode.Space) && isWalled && !isGrounded)
        {
            isWallSliding = false;
            int jumpDir = -wallDirection;
            rb.velocity = Vector2.zero;
            rb.velocity = new Vector2(jumpDir * wallJumpForce.x, wallJumpForce.y);
            currentAirControlTimer = airControlLockTime;
            return;
        }

        // Normal jump
        if (enableJump && Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            PerformJump();
            return;
        }

        // Orb jump
        if (enableJump && Input.GetKeyDown(KeyCode.Space) && !isGrounded && currentOrb != null)
        {
            PerformJump();
            JumpOrb orbToSave = currentOrb;
            currentOrb = null;
            usedOrbs.Add(orbToSave);
            orbToSave.DeactivateOrb();
        }
    }

    void PerformJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public bool TakeDamage(float amount)
    {
        if (isInvincible) return false;
        if (healthData == null) return false;

        healthData.currentHealth -= amount;
        healthData.currentHealth = Mathf.Max(healthData.currentHealth, 0f);

        if (healthData.currentHealth <= 0)
        {
            PlayerRespawn respawn = GetComponent<PlayerRespawn>();
            if (respawn != null) respawn.TriggerInstantDeath();
            else Debug.Log("Player Died!");
            return true;
        }

        StartCoroutine(InvincibilityRoutine());
        return true;
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }

    public void EnterJumpOrb(JumpOrb orb) { currentOrb = orb; }
    public void ExitJumpOrb(JumpOrb orb) { if (currentOrb == orb) currentOrb = null; }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag(groundTag))
        {
            isGrounded = true;
            if (usedOrbs.Count > 0) ResetOrbState();
        }

        if (col.gameObject.CompareTag(wallTag))
        {
            isWalled = true;
            float normalX = col.GetContact(0).normal.x;
            wallDirection = normalX < -0.5f ? 1 : -1;
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag(groundTag))
        {
            isGrounded = true;
            if (usedOrbs.Count > 0) ResetOrbState();
        }

        if (col.gameObject.CompareTag(wallTag))
        {
            isWalled = true;
            float normalX = col.GetContact(0).normal.x;
            wallDirection = normalX < -0.5f ? 1 : -1;
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag(groundTag))
            isGrounded = false;

        if (col.gameObject.CompareTag(wallTag))
        {
            isWalled = false;
            isWallSliding = false;
        }
    }

    void ResetOrbState()
    {
        foreach (JumpOrb orb in usedOrbs)
            if (orb != null) orb.ActivateOrb();
        usedOrbs.Clear();
    }

    // ==========================================
    // SALDIRI VE DÖNÜŞ METOTLARI
    // ==========================================
    void Attack()
    {
        if (attackPoint == null) return;

        // İleride animasyon ekleneceğinde burayı açacağız:
        // anim.SetTrigger("Attack");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth2D enemyHealth = enemy.GetComponent<EnemyHealth2D>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeHit();
                Debug.Log(enemy.name + " tek yedi ve öldü!");
            }
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}