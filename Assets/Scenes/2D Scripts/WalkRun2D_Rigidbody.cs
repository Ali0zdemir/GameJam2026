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

    [Header("Coyote Time & Jump Buffer")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

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
    public float knockbackDuration = 0.5f;

    [Header("Melee Attack")]
    public Transform attackPoint;
    public float attackRange = 1.2f;
    public LayerMask enemyLayers;
    public float attackRate = 2f;

    [Header("Animation")]
    public Animator anim;
    public float animBlendSpeed = 10f;

    private float nextAttackTime = 0f;
    private bool isFacingRight = true;

    Rigidbody2D rb;
    float inputX;
    bool isGrounded;

    float coyoteTimer;
    float jumpBufferTimer;

    bool isDashing;
    float dashTimer;
    float dashCooldownTimer;
    float dashDirection;

    bool isInvincible;
    bool isKnockedBack;
    float knockbackTimer;

    JumpOrb currentOrb = null;
    List<JumpOrb> usedOrbs = new List<JumpOrb>();

    bool isWalled;
    int wallDirection;
    bool isWallSliding;
    float currentWallGrabTimer;
    float currentAirControlTimer;

    float smoothSpeed;
    float smoothVelocityY;

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

        if (!isWallSliding)
        {
            if (inputX > 0 && !isFacingRight) Flip();
            else if (inputX < 0 && isFacingRight) Flip();
        }

        // SALDIRI KONTROLÜ (BEKLEME SÜRESİ KORUMASI)
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Attack(); // Süre dolduysa vur ve animasyonu oynat
                nextAttackTime = Time.time + 1f / attackRate; // Yeniden vurabilmek için süreyi kilitle
            }
        }

        if (Input.GetKeyDown(runKey) && !isDashing && dashCooldownTimer <= 0f && inputX != 0f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
            dashDirection = inputX;
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        CheckWallSliding();
        HandleJump();
        UpdateAnimations();

        if (Input.GetKeyDown(KeyCode.K))
            TakeDamage(10f);
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        smoothSpeed = Mathf.Lerp(smoothSpeed, Mathf.Abs(inputX), animBlendSpeed * Time.deltaTime);

        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("speed", smoothSpeed);

        bool wallSlideAnim = isWallSliding && !isGrounded;
        anim.SetBool("isWallSliding", wallSlideAnim);
    }

    void FixedUpdate()
    {
        if (isKnockedBack) return;
        if (healthData != null && healthData.currentHealth <= 0) return;

        if (isDashing)
        {
            rb.velocity = new Vector2(dashDirection * dashSpeed, 0f);
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f) isDashing = false;
            return;
        }

        if (isWallSliding) return;
        if (currentAirControlTimer > 0f) return;

        if (isGrounded)
            rb.velocity = new Vector2(inputX * walkSpeed, rb.velocity.y);
        else
        {
            float newX = Mathf.MoveTowards(rb.velocity.x, inputX * walkSpeed, airAcceleration * Time.fixedDeltaTime);
            rb.velocity = new Vector2(newX, rb.velocity.y);
        }
    }

    void CheckWallSliding()
    {
        if (!isGrounded && isWalled && currentAirControlTimer <= 0f)
        {
            if (!isWallSliding)
            {
                isWallSliding = true;
                currentWallGrabTimer = wallGrabDuration;

                if (wallDirection == 1 && !isFacingRight) Flip();
                else if (wallDirection == -1 && isFacingRight) Flip();
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
            if (isWallSliding)
            {
                isWallSliding = false;

                if (inputX > 0 && !isFacingRight) Flip();
                else if (inputX < 0 && isFacingRight) Flip();
            }
        }
    }

    void HandleJump()
    {
        if (enableJump && jumpBufferTimer > 0f && isWalled && !isGrounded)
        {
            isWallSliding = false;
            int jumpDir = -wallDirection;
            rb.velocity = Vector2.zero;
            rb.velocity = new Vector2(jumpDir * wallJumpForce.x, wallJumpForce.y);
            currentAirControlTimer = airControlLockTime;
            jumpBufferTimer = 0f;

            if (jumpDir > 0 && !isFacingRight) Flip();
            else if (jumpDir < 0 && isFacingRight) Flip();
            return;
        }

        if (enableJump && jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            PerformJump();
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            return;
        }

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
        if (col.gameObject.CompareTag(groundTag)) { isGrounded = true; if (usedOrbs.Count > 0) ResetOrbState(); }
        if (col.gameObject.CompareTag(wallTag))
        {
            isWalled = true;
            float normalX = col.GetContact(0).normal.x;
            wallDirection = normalX < -0.5f ? 1 : -1;
        }

        if (col.gameObject.CompareTag("Transition"))
        {
            if (ScreenFader.Instance != null)
                ScreenFader.Instance.FadeOutIn(0.5f, 1.5f);
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag(groundTag)) { isGrounded = true; if (usedOrbs.Count > 0) ResetOrbState(); }
        if (col.gameObject.CompareTag(wallTag))
        {
            isWalled = true;
            float normalX = col.GetContact(0).normal.x;
            wallDirection = normalX < -0.5f ? 1 : -1;
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag(groundTag)) isGrounded = false;
        if (col.gameObject.CompareTag(wallTag)) { isWalled = false; isWallSliding = false; }
    }

    void ResetOrbState()
    {
        foreach (JumpOrb orb in usedOrbs) if (orb != null) orb.ActivateOrb();
        usedOrbs.Clear();
    }

    void Attack()
    {
        if (attackPoint == null) return;

        // ANİMASYONU TETİKLE
        if (anim != null)
            anim.SetTrigger("Attack");

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth2D enemyHealth = enemy.GetComponent<EnemyHealth2D>();
            if (enemyHealth != null) enemyHealth.TakeHit();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}