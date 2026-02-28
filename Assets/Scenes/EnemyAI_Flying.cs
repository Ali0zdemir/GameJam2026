using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI_Flying : MonoBehaviour
{
    [Header("Zamanlamalar")]
    public float spawnWaitTime = 2f;
    public float attackPrepareTime = 1.5f;
    public float attackCooldown = 3f;

    [Header("Hareket ve Menzil")]
    public float moveSpeed = 4f;
    public float roamRadius = 30f;
    public float detectionRange = 25f;
    public float stopDistance = 15f;
    public float repositionDistance = 8f;  // YENİ: Ateş ettikten sonra sağa/sola kaçma mesafesi (Orta mesafe)

    [Header("Yükseklik ve Sallanma")]
    public float minGroundHeight = 5f;
    public LayerMask groundLayer;
    public float bobSpeed = 2.5f;
    public float bobAmount = 0.5f;

    [Header("Saldırı (Mermi)")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;

    private Transform player;
    private Rigidbody rb;
    private Vector3 startPos;

    private Vector3 currentTargetPos;
    private bool isSpawning = true;
    private bool isAttacking = false;
    private bool isRepositioning = false;
    private float repositionTimer = 0f;
    private float bobTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = true;

        startPos = transform.position;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;

        StartCoroutine(SpawnRoutine());
        PickNewRoamTarget();
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(spawnWaitTime);
        isSpawning = false;
    }

    void FixedUpdate()
    {
        if (isSpawning || player == null) return;

        Vector2 enemyPos2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 playerPos2D = new Vector2(player.position.x, player.position.z);
        float flatDistance = Vector2.Distance(enemyPos2D, playerPos2D);

        // Kiting (Geri Kaçma) İçgüdüsü
        if (flatDistance < stopDistance - 1f)
        {
            isRepositioning = false;

            Vector3 fleeDir = transform.position - player.position;
            fleeDir.y = 0f;

            if (fleeDir.magnitude < 0.1f)
            {
                fleeDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            }

            Vector3 fleeTarget = transform.position + fleeDir.normalized * 5f;
            MoveTowards(fleeTarget);
            return;
        }

        // Ateş ederken dur
        if (isAttacking)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 10f);
            return;
        }

        // Taktiksel Sağa/Sola Kaçış (Repositioning)
        if (isRepositioning)
        {
            MoveTowards(currentTargetPos);
            repositionTimer += Time.fixedDeltaTime;

            Vector2 target2D = new Vector2(currentTargetPos.x, currentTargetPos.z);
            if (Vector2.Distance(enemyPos2D, target2D) < 1.0f || repositionTimer >= attackCooldown)
            {
                isRepositioning = false;
                repositionTimer = 0f;
            }
            return;
        }

        // Normal Davranış
        if (flatDistance <= detectionRange)
        {
            if (flatDistance > stopDistance)
            {
                MoveTowards(player.position);
            }
            else
            {
                StartCoroutine(AttackRoutine());
            }
        }
        else
        {
            Roam();
        }
    }

    void Roam()
    {
        Vector2 enemyPos2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 target2D = new Vector2(currentTargetPos.x, currentTargetPos.z);

        if (Vector2.Distance(enemyPos2D, target2D) < 2f)
        {
            PickNewRoamTarget();
        }
        MoveTowards(currentTargetPos);
    }

    void PickNewRoamTarget()
    {
        Vector2 randomDir = Random.insideUnitCircle * roamRadius;
        currentTargetPos = startPos + new Vector3(randomDir.x, 0f, randomDir.y);
    }

    // =========================================================================
    // YENİ: Sağa veya Sola Kaçma Mantığı (Strafing)
    // =========================================================================
    void PickRepositionTarget()
    {
        // 1. Oyuncuya olan yönümüzü bul
        Vector3 dirToPlayer = (player.position - transform.position);
        dirToPlayer.y = 0f;
        dirToPlayer.Normalize();

        // 2. Sağ yönü bul (Çapraz çarpım - Cross Product ile 90 derece dik açı buluyoruz)
        Vector3 rightDir = Vector3.Cross(Vector3.up, dirToPlayer).normalized;

        // 3. Rastgele sağa (1) veya sola (-1) gitmeye karar ver
        float randomDirection = Random.value > 0.5f ? 1f : -1f;
        Vector3 strafeDirection = rightDir * randomDirection;

        // 4. Bulunduğumuz noktadan belirlenen "repositionDistance" (Orta Mesafe) kadar sağa/sola hedef koy
        currentTargetPos = transform.position + (strafeDirection * repositionDistance);
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position);
        direction.y = 0f;
        Vector3 moveVelocity = direction.normalized * moveSpeed;

        float targetY = transform.position.y;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 50f, groundLayer))
        {
            targetY = hit.point.y + minGroundHeight;
        }

        bobTimer += Time.fixedDeltaTime * bobSpeed;
        targetY += Mathf.Sin(bobTimer) * bobAmount;

        float yVelocity = (targetY - transform.position.y) * 5f;
        rb.velocity = new Vector3(moveVelocity.x, yVelocity, moveVelocity.z);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        yield return new WaitForSeconds(attackPrepareTime);

        if (bulletPrefab && firePoint && player)
        {
            Vector3 aimDir = (player.position - firePoint.position).normalized;
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(aimDir));

            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb)
            {
                bulletRb.AddForce(aimDir * bulletForce, ForceMode.Impulse);
            }
        }

        yield return new WaitForSeconds(0.5f);

        PickRepositionTarget();
        isAttacking = false;
        isRepositioning = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 drawingStartPos = Application.isPlaying ? startPos : transform.position;
        DrawWireCircle(drawingStartPos, roamRadius);

        Gizmos.color = Color.yellow;
        DrawWireCircle(transform.position, detectionRange);

        Gizmos.color = Color.red;
        DrawWireCircle(transform.position, stopDistance);
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