using System.Collections;
using UnityEngine;

public class EnemyAI_Melee : MonoBehaviour
{
    [Header("Hareket")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 10f;

    [Header("Saldırı")]
    public float damage = 10f;
    public float attackWaitTime = 0.5f;
    public float attackCooldown = 1f;

    [Header("Zıplama Saldırısı")]
    public float jumpForce = 8f;
    public float attackAnimDuration = 0.6f;

    [Header("Saldırı Menzili")]
    public BoxCollider attackCollider;

    [Header("Animasyon")]
    public Animator animator;
    public string walkAnimName = "MeleeWalk";
    public string attackAnimName = "MeleeAttack";

    Transform player;
    Rigidbody rb;
    SpriteRenderer spriteRenderer;

    bool playerInRange = false;
    bool isAttacking = false;
    bool attackQueued = false;
    bool isGrounded = false;

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p) player = p.transform;

        rb = GetComponent<Rigidbody>();
        if (rb)
        {
            // Y eksenini serbest bırak, sadece X ve Z kilitle
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        // Her zaman player'a bak (Y ekseni)
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

    void ResumeAnim()
    {
        if (animator) animator.speed = 1f;
    }

    IEnumerator AttackRoutine()
{
    isAttacking = true;

    if (rb) rb.velocity = Vector3.zero;
    yield return new WaitForSeconds(attackWaitTime);

    PlayAnim(attackAnimName);

    Vector3 jumpDir = (player.position - transform.position).normalized;
    jumpDir.y = 0f;

    if (rb)
    {
        rb.velocity = Vector3.zero;
        isGrounded = false;
        rb.AddForce(new Vector3(jumpDir.x * jumpForce, jumpForce, jumpDir.z * jumpForce), ForceMode.Impulse);
    }

    yield return new WaitForFixedUpdate();
    yield return new WaitForFixedUpdate();

    float waitTimer = 0f;
    while (!isGrounded && waitTimer < 2f)
    {
        waitTimer += Time.deltaTime;
        yield return null;
    }

    if (rb) rb.velocity = Vector3.zero;

    // Animasyonu dondurma, direkt walk'a geç
    PlayAnim(walkAnimName);
    StopAnim(); // Walk'ı dondur (yerinde beklerken)

    yield return new WaitForSeconds(1f);

    if (playerInRange)
    {
        WalkRun2D_Rigidbody playerScript = player.GetComponent<WalkRun2D_Rigidbody>();
        if (playerScript) playerScript.TakeDamage(damage);
    }

    yield return new WaitForSeconds(attackCooldown);

    isAttacking = false;
    attackQueued = false;

    // Animasyonu temiz şekilde walk'tan başlat
    animator.speed = 1f;
    animator.Play(walkAnimName, 0, 0f);
}

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
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