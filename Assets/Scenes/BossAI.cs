using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Hedef ve Görsel")]
    public Animator anim;
    public Transform firePoint;

    [Header("Saldırı: Ahtapot (State 1)")]
    public GameObject electricBallPrefab;
    public int tentacleCount = 8;
    public int bulletsPerTentacle = 6;
    public float curveAngle = 10f;
    public float fireDelay = 0.1f;
    public int octopusWaves = 3;
    public float timeBetweenWaves = 1.5f;

    [Header("Saldırı: Seri Atış (State 2)")]
    public GameObject burstProjectilePrefab;
    public float burstStateDuration = 4f;
    public int shotsPerBurst = 3;
    public float timeBetweenShots = 0.2f;
    public float timeBetweenBursts = 1f;

    [Header("Saldırı: Düşman Çağırma (State 3)")]
    public GameObject[] minionPrefabs;
    public int minionCount = 3;
    public Transform[] spawnPoints;

    [Header("Hareket Ayarları")]
    public float moveSpeed = 5f;
    public float moveDistance = 3f;
    public float moveCooldown = 6f;

    [Header("Can")]
    public float maxHealth = 500f;
    float currentHealth;
    bool isDead = false;

    Transform player;
    bool isMoving = false;
    float nextMoveTime = 0f;
    Vector3 targetMovePosition;

    void Start()
    {
        // Player'ı otomatik bul
        GameObject p = GameObject.FindWithTag("Player");
        if (p) player = p.transform;

        currentHealth = maxHealth;
        nextMoveTime = Time.time + moveCooldown;
        StartCoroutine(AttackCycleRoutine());
    }

    void Update()
    {
        if (isDead || player == null) return;

        if (Time.time >= nextMoveTime && !isMoving)
            StartCoroutine(GlideTowardsPlayer());
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();
        Destroy(gameObject, 0.5f);
    }

    IEnumerator AttackCycleRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(2f);

            int state = Random.Range(1, 4);

            if (state == 1) yield return StartCoroutine(State1_OctopusAttack());
            else if (state == 2) yield return StartCoroutine(State2_BurstAttack());
            else if (state == 3) yield return StartCoroutine(State3_SpawnEnemies());
        }
    }

    IEnumerator State1_OctopusAttack()
    {
        if (anim != null) anim.SetTrigger("PreSpiral");
        yield return new WaitForSeconds(1f);

        for (int wave = 0; wave < octopusWaves; wave++)
        {
            float currentOffset = wave * 15f;

            for (int i = 0; i < bulletsPerTentacle; i++)
            {
                FireOctopusWave(currentOffset);
                currentOffset += curveAngle;
                yield return new WaitForSeconds(fireDelay);
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    IEnumerator State2_BurstAttack()
    {
        if (anim != null) anim.SetTrigger("PreBurst");
        yield return new WaitForSeconds(1f);

        float timer = 0f;
        while (timer < burstStateDuration)
        {
            for (int i = 0; i < shotsPerBurst; i++)
            {
                FireAimedProjectile();
                yield return new WaitForSeconds(timeBetweenShots);
            }

            yield return new WaitForSeconds(timeBetweenBursts);
            timer += (shotsPerBurst * timeBetweenShots) + timeBetweenBursts;
        }
    }

    IEnumerator State3_SpawnEnemies()
    {
        if (anim != null) anim.SetTrigger("PreSpawn");
        yield return new WaitForSeconds(1f);

        if (minionPrefabs == null || minionPrefabs.Length == 0) yield break;

        for (int i = 0; i < minionCount; i++)
        {
            Vector3 spawnPos = (spawnPoints != null && spawnPoints.Length > 0)
                ? spawnPoints[i % spawnPoints.Length].position
                : transform.position + Random.insideUnitSphere * 3f;

            spawnPos.y = transform.position.y;

            int randomIndex = Random.Range(0, minionPrefabs.Length);
            if (minionPrefabs[randomIndex] != null)
                Instantiate(minionPrefabs[randomIndex], spawnPos, Quaternion.identity);
        }
    }

    void FireOctopusWave(float angleOffset)
    {
        if (!electricBallPrefab || !firePoint) return;

        float angleStep = 360f / tentacleCount;
        for (int i = 0; i < tentacleCount; i++)
        {
            float angle = i * angleStep + angleOffset;
            Vector3 fireDir = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            GameObject ball = Instantiate(electricBallPrefab, firePoint.position, Quaternion.LookRotation(fireDir));
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb) rb.velocity = fireDir * 15f;
        }
    }

    void FireAimedProjectile()
    {
        if (!burstProjectilePrefab || !firePoint || player == null) return;

        Vector3 aimDir = (player.position - firePoint.position).normalized;
        GameObject proj = Instantiate(burstProjectilePrefab, firePoint.position, Quaternion.LookRotation(aimDir));
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb) rb.velocity = aimDir * 25f;
    }

    IEnumerator GlideTowardsPlayer()
    {
        if (player == null) yield break;

        isMoving = true;
        if (anim != null) anim.SetTrigger("Move");

        Vector3 startPos = transform.position;
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        dirToPlayer.y = 0;
        targetMovePosition = startPos + dirToPlayer * moveDistance;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, targetMovePosition, t);
            yield return null;
        }

        nextMoveTime = Time.time + moveCooldown;
        isMoving = false;
    }
}