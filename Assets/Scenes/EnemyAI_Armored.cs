using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI_Armored : MonoBehaviour
{
    [Header("Zamanlamalar")]
    public float spawnWaitTime = 2f;
    public float attackPrepareTime = 0.5f;
    public float attackCooldown = 2f;

    [Header("Menzil")]
    public float detectionRange = 15f;
    public float stopDistance = 8f;

    [Header("Kalkan Menzili")]
    public float shieldDistance = 4f;

    [Header("Saldırı")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;

    [Header("Animasyon")]
    public Animator anim;

    [Header("Kalkan Sprite")]
    public SpriteRenderer spriteRenderer;
    public Sprite normalSprite;
    public Sprite shieldedSprite;

    [Header("Rotasyon")]
    public float rotationSpeed = 10f;

    [HideInInspector]
    public bool isInvulnerable = false;

    private Transform player;
    private Rigidbody rb;

    private bool isSpawning = true;
    private bool isAttacking = false;
    private bool isShielded = false;

    private Coroutine attackCoroutine;
    private Coroutine shieldAnimCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.freezeRotation = true;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;

        if (anim != null) anim.enabled = false;

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(spawnWaitTime);
        isSpawning = false;
    }

    void Update()
    {
        if (isSpawning || player == null) return;

        // Sürekli player'a bak
        RotateTowardsPlayer();
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position);
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

   void SetShieldState(bool state)
{
    if (isShielded == state) return;

    isShielded = state;

    if (state)
    {
        isInvulnerable = true;

        if (isAttacking)
        {
            isAttacking = false;
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        }

        if (anim != null)
        {
            anim.enabled = true;
            anim.Play("ClosedArmoredEnemy", 0, 0f);
            if (shieldAnimCoroutine != null) StopCoroutine(shieldAnimCoroutine);
            shieldAnimCoroutine = StartCoroutine(WaitForShieldAnim());
        }
        else if (spriteRenderer != null && shieldedSprite != null)
        {
            spriteRenderer.sprite = shieldedSprite;
        }
    }
    else
    {
        // Açılma animasyonu sırasında hala hasar almayacak
        // isInvulnerable animasyon bitince false olacak
        if (shieldAnimCoroutine != null) StopCoroutine(shieldAnimCoroutine);

        if (anim != null)
        {
            anim.enabled = true;
            anim.Play("OpenArmoredEnemy", 0, 0f);
            shieldAnimCoroutine = StartCoroutine(WaitForOpenAnim());
        }
        else
        {
            isInvulnerable = false;
            if (spriteRenderer != null && normalSprite != null)
                spriteRenderer.sprite = normalSprite;
        }
    }
}

IEnumerator WaitForShieldAnim()
{
    yield return null;
    yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
    if (anim != null) anim.enabled = false;
    if (spriteRenderer != null && shieldedSprite != null)
        spriteRenderer.sprite = shieldedSprite;
}

IEnumerator WaitForOpenAnim()
{
    yield return null;
    yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

    // Animasyon bitti, şimdi hasar alabilir
    isInvulnerable = false;
    if (anim != null) anim.enabled = false;
    if (spriteRenderer != null && normalSprite != null)
        spriteRenderer.sprite = normalSprite;
}

 void FixedUpdate()
{
    if (isSpawning || player == null) return;

    rb.velocity = new Vector3(0, rb.velocity.y, 0);

    Vector2 enemyPos2D = new Vector2(transform.position.x, transform.position.z);
    Vector2 playerPos2D = new Vector2(player.position.x, player.position.z);
    float flatDistance = Vector2.Distance(enemyPos2D, playerPos2D);

    if (flatDistance <= shieldDistance)
    {
        SetShieldState(true);
        return;
    }
    else
    {
        SetShieldState(false);
    }

    // Sadece isShielded kontrolü, isInvulnerable kontrolü yok
    if (flatDistance <= detectionRange && !isShielded && !isAttacking)
    {
        attackCoroutine = StartCoroutine(AttackRoutine());
    }
}

IEnumerator AttackRoutine()
{
    isAttacking = true;

    yield return new WaitForSeconds(attackPrepareTime);

    // Sadece isShielded kontrolü
    if (!isShielded && bulletPrefab && firePoint && player)
    {
        Vector3 aimDir = (player.position - firePoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(aimDir));

        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb)
            bulletRb.AddForce(aimDir * bulletForce, ForceMode.Impulse);
    }

    yield return new WaitForSeconds(attackCooldown);

    isAttacking = false;
}
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        DrawWireCircle(transform.position, detectionRange);

        Gizmos.color = Color.red;
        DrawWireCircle(transform.position, stopDistance);

        Gizmos.color = Color.blue;
        DrawWireCircle(transform.position, shieldDistance);
    }

    void DrawWireCircle(Vector3 center, float radius, int segments = 32)
    {
        Vector3 previousPoint = center + new Vector3(radius, 0, 0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * 2 * Mathf.PI / segments;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }
}