using System.Collections;
using UnityEngine;

public class EnemyAI_Melee : MonoBehaviour
{
    [Header("Hareket")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 10f;

    [Header("Saldırı")]
    public float damage = 10f;
    public float attackWaitTime = 0.2f;
    public float attackCooldown = 0.3f;

    [Header("Zıplama Saldırısı")]
    public float jumpForce = 8f;
    public float jumpDamage = 10f;
    public float jumpAnimDuration = 0.6f; // Zıplama animasyon süresi

    [Header("Charge (Titreme)")]
    public float chargeDuration = 0.8f;   // Titreme süresi
    public float chargeShakeAmount = 0.05f; // Titreme şiddeti

    [Header("Saldırı Menzili")]
    public BoxCollider attackCollider;

    [Header("Animasyon")]
    public Animator animator;
    public string walkAnimName = "MeleeWalk";
    public string attackAnimName = "MeleeAttack";

    Transform player;
    Rigidbody rb;
    SpriteRenderer spriteRenderer;
    PlayerHealth3D playerHealth;

    bool playerInRange = false;
    bool isAttacking = false;
    bool attackQueued = false;
    bool isGrounded = false;
    bool isJumping = false;

    Vector3 spriteOriginalPos;
    EnemyAudioController enemyAudio;

    void Start()
    {
        enemyAudio = GetComponent<EnemyAudioController>();
        GameObject p = GameObject.FindWithTag("Player");
        if (p)
        {
            player = p.transform;
            playerHealth = p.GetComponent<PlayerHealth3D>();
        }

        rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer) spriteOriginalPos = spriteRenderer.transform.localPosition;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        RotateTowardsPlayer();
        UpdateFlip();

        if (isAttacking) return;

        if (playerInRange && !attackQueued)
        {
            attackQueued = true;
            StartCoroutine(AttackRoutine());
            return;
        }

        if (!playerInRange)
        {
            MoveTowardsPlayer();
            PlayAnim(walkAnimName);
        }
        else if (!isAttacking)
        {
            StopAnim();
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position);
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    void UpdateFlip()
    {
        if (spriteRenderer == null || player == null) return;
        float xDiff = player.position.x - transform.position.x;
        if (Mathf.Abs(xDiff) > 0.1f)
            spriteRenderer.flipX = xDiff < 0f;
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (rb)
            rb.velocity = new Vector3(direction.x * moveSpeed, rb.velocity.y, direction.z * moveSpeed);
        else
            transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void PlayAnim(string animName)
    {
        if (animator)
        {
            animator.speed = 1f;
            animator.Play(animName);
        }
    }

    void StopAnim()
    {
        if (animator) animator.speed = 0f;
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (rb) rb.velocity = Vector3.zero;
        yield return new WaitForSeconds(attackWaitTime);

        // Charge: titreme başlat
        yield return StartCoroutine(ChargeRoutine());

        // Player'ın o anki pozisyonunu al (kaçtığı yön)
        Vector3 jumpTarget = player.position;
        Vector3 jumpDir = (jumpTarget - transform.position).normalized;
        jumpDir.y = 0f;

        // Zıplama animasyonu
        PlayAnim(attackAnimName);

        if (rb)
        {
            rb.velocity = Vector3.zero;
            isGrounded = false;
            isJumping = true;
            rb.AddForce(new Vector3(jumpDir.x * jumpForce * 2.5f, jumpForce, jumpDir.z * jumpForce * 1.5f), ForceMode.Impulse);
            enemyAudio?.PlayJump();
        }

        // Zıplama animasyon süresi kadar bekle
        yield return new WaitForSeconds(jumpAnimDuration);

        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        float waitTimer = 0f;
        while (!isGrounded && waitTimer < 2f)
        {
            waitTimer += Time.deltaTime;
            yield return null;
        }

        isJumping = false;
        if (rb) rb.velocity = Vector3.zero;

        PlayAnim(walkAnimName);
        StopAnim();

        yield return new WaitForSeconds(0.3f);

        if (playerInRange && playerHealth != null)
            playerHealth.TakeDamage(damage);

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        attackQueued = false;

        animator.speed = 1f;
        animator.Play(walkAnimName, 0, 0f);
    }

    IEnumerator ChargeRoutine()
    {
        float elapsed = 0f;

        enemyAudio?.PlayCharge();

        while (elapsed < chargeDuration)
        {
            elapsed += Time.deltaTime;

            // Sprite titreme
            if (spriteRenderer != null)
            {
                float shakeX = Random.Range(-chargeShakeAmount, chargeShakeAmount);
                float shakeY = Random.Range(-chargeShakeAmount, chargeShakeAmount);
                spriteRenderer.transform.localPosition = spriteOriginalPos + new Vector3(shakeX, shakeY, 0f);
            }

            yield return null;
        }

        // Titreme bitti, sprite sıfırla
        if (spriteRenderer != null)
            spriteRenderer.transform.localPosition = spriteOriginalPos;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            {
                isGrounded = true;
                enemyAudio?.PlayLand(); // ← BURAYA EKLE
            }

        if (col.gameObject.CompareTag("Player") && isJumping)
        {
            PlayerHealth3D ph = col.gameObject.GetComponent<PlayerHealth3D>();
            if (ph) ph.TakeDamage(jumpDamage);
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    public void OnAttackRangeEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    public void OnAttackRangeExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}