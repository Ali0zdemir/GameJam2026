using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BossAI : MonoBehaviour
{
    [Header("Hedef ve Görsel")]
    public Animator anim;
    public Transform firePoint;

    [Header("Bob sadece görselde olsun (opsiyonel)")]
    public Transform visualRoot; // boşsa anim.transform kullanır

    [Header("Saldırı: Ahtapot (State 1)")]
    public GameObject electricBallPrefab;
    public int tentacleCount = 8;
    public int bulletsPerTentacle = 6;
    public float curveAngle = 10f;
    public float fireDelay = 0.1f;
    public int octopusWaves = 3;
    public float timeBetweenWaves = 1f;

    [Header("Saldırı: Seri Atış (State 2)")]
    public GameObject burstProjectilePrefab;
    public float burstStateDuration = 4f;
    public int shotsPerBurst = 3;
    public float timeBetweenShots = 0.15f;
    public float timeBetweenBursts = 1f;

    [Header("Saldırı: Düşman Çağırma (State 3)")]
    public GameObject[] minionPrefabs;
    public int minionCount = 3;
    public Transform[] spawnPoints;

    [Header("Akıllı Süzülme")]
    public float glideSpeed = 2.5f;
    public float orbitSpeed = 1.5f;
    public float preferredDistance = 5f;
    public float tooCloseDistance = 3f;
    public float tooFarDistance = 9f;
    public float glideSmoothing = 3f;
    public float directionChangeInterval = 2f;

    [Header("Bob (Yukarı Aşağı Süzülme) - sadece görsel")]
    public float bobSpeed = 1.5f;
    public float bobAmount = 0.2f;

    [Header("Mermi Hızları")]
    public float octopusBulletSpeed = 7f;
    public float burstBulletSpeed = 12f;

    [Header("Mermi Y İnişi (SADECE Y)")]
    [Tooltip("Mermiler Y'de aşağı kaç hızla insin?")]
    public float projectileDropSpeed = 6f;

    [Tooltip("Hedef Y = playerY - bu değer. (Daha fazla insin istiyorsan artır)")]
    public float projectileYBelowPlayer = 0.3f;

    [Tooltip("Hedef Y'ye gelince Y'yi kilitlesin mi?")]
    public bool lockProjectileYAtTarget = true;

    [Header("Can ve Temas Hasarı")]
    public float maxHealth = 500f;
    public float contactDamage = 30f;

    float currentHealth;
    bool isDead = false;
    Transform player;
    int lastAttackState = 0;
    int consecutiveAttackCount = 0;

    Vector3 glideVelocity = Vector3.zero;
    float orbitDirection = 1f;
    bool isAttacking = false;

    float bobTimer = 0f;
    Vector3 visualBaseLocalPos;

    Rigidbody rb;
    float fixedY;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    void Awake()
{
    // boss objesi sahnede inactive başlasa bile health hazır olsun
    currentHealth = maxHealth;
}

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Top-down fizik ayarları (duvarlardan geçmesin diye)
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ
                       | RigidbodyConstraints.FreezePositionY;

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        fixedY = transform.position.y;

        GameObject p = GameObject.FindWithTag("Player");
        if (p) player = p.transform;

        currentHealth = maxHealth;
        orbitDirection = Random.value > 0.5f ? 1f : -1f;

        // Bob sadece görselde
        if (visualRoot == null && anim != null) visualRoot = anim.transform;
        if (visualRoot != null) visualBaseLocalPos = visualRoot.localPosition;

        StartCoroutine(AttackCycleRoutine());
        StartCoroutine(OrbitDirectionChanger());
    }

    void Update()
    {
        if (isDead) return;

        // Bob'u fizik objesinde değil, görsel child’da yap
        if (visualRoot != null)
        {
            bobTimer += Time.deltaTime;
            float bobY = Mathf.Sin(bobTimer * bobSpeed) * bobAmount;
            Vector3 lp = visualBaseLocalPos;
            lp.y += bobY;
            visualRoot.localPosition = lp;
        }
    }

    void FixedUpdate()
    {
        if (isDead || player == null) return;

        Vector3 targetVelocity = CalculateGlideVelocity();
        glideVelocity = Vector3.Lerp(glideVelocity, targetVelocity, Time.fixedDeltaTime * glideSmoothing);

        rb.velocity = new Vector3(glideVelocity.x, 0f, glideVelocity.z);

        // Y sabit kalsın
        Vector3 pos = rb.position;
        pos.y = fixedY;
        rb.position = pos;
    }

    Vector3 CalculateGlideVelocity()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0;
        float distance = toPlayer.magnitude;
        if (distance < 0.001f) return Vector3.zero;

        Vector3 dirToPlayer = toPlayer.normalized;
        Vector3 finalVelocity;

        if (distance < tooCloseDistance)
        {
            finalVelocity = -dirToPlayer * glideSpeed * 1.8f;
        }
        else if (distance > tooFarDistance)
        {
            finalVelocity = dirToPlayer * glideSpeed * 1.5f;
        }
        else
        {
            Vector3 orbitDir = Vector3.Cross(Vector3.up, dirToPlayer) * orbitDirection;
            float distanceDiff = distance - preferredDistance;
            Vector3 radialAdjust = dirToPlayer * distanceDiff * 0.5f;
            finalVelocity = (orbitDir * orbitSpeed + radialAdjust).normalized * glideSpeed;
        }

        if (isAttacking)
            finalVelocity *= 0.3f;

        return finalVelocity;
    }

    IEnumerator OrbitDirectionChanger()
    {
        while (!isDead)
        {
            float waitTime = directionChangeInterval + Random.Range(-0.5f, 1f);
            yield return new WaitForSeconds(waitTime);
            orbitDirection *= -1f;
        }
    }

    // ==========================================
    // CAN SİSTEMİ
    // ==========================================
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
        rb.velocity = Vector3.zero;
        Destroy(gameObject, 0.5f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth3D ph = collision.gameObject.GetComponent<PlayerHealth3D>();
            if (ph != null) ph.TakeDamage(contactDamage);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth3D ph = other.GetComponent<PlayerHealth3D>();
            if (ph != null) ph.TakeDamage(contactDamage);
        }
    }

    // ==========================================
    // Projectile'lara SADECE Y inişi uygula (XZ değişmez)
    // ==========================================
    void ApplyDropToPlayerY(GameObject proj)
    {
        if (proj == null) return;

        float targetY = proj.transform.position.y;
        if (player != null)
            targetY = player.position.y - projectileYBelowPlayer;

        var drop = proj.GetComponent<ProjectileDropToY>();
        if (drop == null) drop = proj.AddComponent<ProjectileDropToY>();

        drop.Init(targetY, projectileDropSpeed, lockProjectileYAtTarget);
    }

    // ==========================================
    // SALDIRI DÖNGÜSÜ
    // ==========================================
    IEnumerator AttackCycleRoutine()
    {
        yield return new WaitForSeconds(2f);

        while (!isDead)
        {
            int state;
            do
            {
                state = Random.Range(1, 4); // 1, 2 ve 3
            }
            while (state == lastAttackState && consecutiveAttackCount >= 2);

            if (state == lastAttackState) consecutiveAttackCount++;
            else
            {
                lastAttackState = state;
                consecutiveAttackCount = 1;
            }

            isAttacking = true;

            if (state == 1) yield return StartCoroutine(State1_OctopusAttack());
            else if (state == 2) yield return StartCoroutine(State2_BurstAttack());
            else if (state == 3) yield return StartCoroutine(State3_SpawnEnemies());

            isAttacking = false;
            yield return new WaitForSeconds(1.5f);
        }
    }

    // ==========================================
    // STATE 1 — AHTAPOT
    // ==========================================
    IEnumerator State1_OctopusAttack()
    {
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

    void FireOctopusWave(float angleOffset)
    {
        if (!electricBallPrefab || !firePoint) return;

        float angleStep = 360f / Mathf.Max(1, tentacleCount);

        for (int i = 0; i < tentacleCount; i++)
        {
            float angle = i * angleStep + angleOffset;
            float rad = angle * Mathf.Deg2Rad;

            // XZ yönü: ahtapot pattern (player'a bakma yok)
            Vector3 fireDir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));

            GameObject ball = Instantiate(electricBallPrefab, firePoint.position, Quaternion.identity);

            // SADECE Y indir
            ApplyDropToPlayerY(ball);

            Rigidbody prb = ball.GetComponent<Rigidbody>();
            if (prb == null) prb = ball.GetComponentInChildren<Rigidbody>();
            if (prb)
            {
                prb.useGravity = false;
                prb.velocity = fireDir * octopusBulletSpeed; // XZ hız
            }
        }
    }

    // ==========================================
    // STATE 2 — SERİ ATIŞ
    // ==========================================
    IEnumerator State2_BurstAttack()
    {
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

    void FireAimedProjectile()
    {
        if (!burstProjectilePrefab || !firePoint || player == null) return;

        // Bu state zaten XZ'de player'a nişanlı. (Sadece Y inişi ekliyoruz.)
        Vector3 aimDir = (player.position - firePoint.position);
        aimDir.y = 0f;
        if (aimDir.sqrMagnitude < 0.001f) return;
        aimDir.Normalize();

        GameObject proj = Instantiate(burstProjectilePrefab, firePoint.position, Quaternion.identity);

        // SADECE Y indir (XZ yönü değişmez)
        ApplyDropToPlayerY(proj);

        Rigidbody prb = proj.GetComponent<Rigidbody>();
        if (prb == null) prb = proj.GetComponentInChildren<Rigidbody>();
        if (prb)
        {
            prb.useGravity = false;
            prb.velocity = aimDir * burstBulletSpeed;
        }
    }

    // ==========================================
    // STATE 3 — DÜŞMAN ÇAĞIRMA
    // ==========================================
    IEnumerator State3_SpawnEnemies()
    {
        if (anim != null) anim.SetTrigger("HandUp");
        yield return new WaitForSeconds(1.8f);

        if (minionPrefabs == null || minionPrefabs.Length == 0) yield break;

        for (int i = 0; i < minionCount; i++)
        {
            Vector3 spawnPos = (spawnPoints != null && spawnPoints.Length > 0)
                ? spawnPoints[i % spawnPoints.Length].position
                : transform.position + new Vector3(Random.Range(-4f, 4f), 0f, Random.Range(-4f, 4f));

            spawnPos.y = fixedY;

            int randomIndex = Random.Range(0, minionPrefabs.Length);
            if (minionPrefabs[randomIndex] != null)
                Instantiate(minionPrefabs[randomIndex], spawnPos, Quaternion.identity);

            yield return new WaitForSeconds(0.2f);
        }

        float remaining = 1.8f - (minionCount * 0.2f);
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);
    }
}