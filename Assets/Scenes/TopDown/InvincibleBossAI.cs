using System.Collections;
using UnityEngine;

/// <summary>
/// Yenilmez Boss AI - Oyuncu bu bossa hasar veremez.
/// 3 saldırı:
///   State 1: Ahtapot (Elektrik topları her yöne)
///   State 2: Seri Atış (Oyuncuya nişanlı)
///   State 3: Çapraz Atışlı Atılma (Dash)
/// </summary>
public class InvincibleBossAI : MonoBehaviour
{
    // ==========================================
    // GÖRSEL ve ANİMASYON
    // ==========================================
    [Header("Görsel / Animasyon")]
    public Animator anim;
    public Transform firePoint;

    // ==========================================
    // STATE 1: AHTAPOT SALDIRISI
    // ==========================================
    [Header("State 1 - Ahtapot")]
    public GameObject electricBallPrefab;
    public int tentacleCount = 8;
    public int bulletsPerTentacle = 6;
    public float curveAngle = 10f;
    public float fireDelay = 0.1f;
    public int octopusWaves = 3;
    public float timeBetweenWaves = 1f;

    // ==========================================
    // STATE 2: SERİ ATIŞ
    // ==========================================
    [Header("State 2 - Seri Atış")]
    public GameObject burstProjectilePrefab;
    public float burstStateDuration = 4f;
    public int shotsPerBurst = 3;
    public float timeBetweenShots = 0.15f;
    public float timeBetweenBursts = 1f;

    // ==========================================
    // STATE 3: DASH SALDIRISI
    // ==========================================
    [Header("State 3 - Dash")]
    public float chargeTime = 1f;
    public float dashSpeed = 35f;
    public float dashOvershoot = 7f;
    public int shotsDuringDash = 4;
    public float dashDiagonalAngle = 45f;

    // ==========================================
    // HAREKET
    // ==========================================
    [Header("Hareket")]
    public float moveSpeed = 8f;
    public float moveDistance = 6f;
    public float moveCooldown = 5f;

    // ==========================================
    // TEMAS HASARI
    // ==========================================
    [Header("Temas Hasarı")]
    public float contactDamage = 30f;

    // ==========================================
    // YENİLMEZLİK
    // ==========================================
    [Header("Yenilmezlik")]
    [Tooltip("True iken bossa hiçbir hasar uygulanmaz.")]
    public bool isInvincible = true;
    public GameObject invincibilityEffect;

    // ==========================================
    // ÖZEL DEĞİŞKENLER
    // ==========================================
    private Transform player;
    private bool isMoving = false;
    private float nextMoveTime = 0f;

    private int lastAttackState = 0;
    private int consecutiveAttackCount = 0;

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p) player = p.transform;

        nextMoveTime = Time.time + moveCooldown;

        if (invincibilityEffect != null)
            invincibilityEffect.SetActive(isInvincible);

        StartCoroutine(AttackCycleRoutine());
    }

    void Update()
    {
        if (player == null) return;

        if (Time.time >= nextMoveTime && !isMoving)
            StartCoroutine(GlideTowardsPlayer());
    }

    // ==========================================
    // YENİLMEZLİK: Hasar alma fonksiyonu
    // ==========================================
    public void TakeDamage(float amount)
    {
        if (isInvincible)
        {
            Debug.Log("[InvincibleBoss] Hasar engellendi! Boss yenilmez.");
            return;
        }

        // İleride yenilmezlik kalkarsa burada can düşürme vs. eklenir.
    }

    // ==========================================
    // TEMAS HASARI
    // ==========================================
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
    // SALDIRI DÖNGÜSÜ
    // ==========================================
    IEnumerator AttackCycleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);

            int state;

            // Anti-spam: aynı saldırı 2 kereden fazla üst üste gelmesin
            do
            {
                state = Random.Range(1, 4); // 1, 2 veya 3
            }
            while (state == lastAttackState && consecutiveAttackCount >= 2);

            if (state == lastAttackState) consecutiveAttackCount++;
            else
            {
                lastAttackState = state;
                consecutiveAttackCount = 1;
            }

            if (state == 1) yield return StartCoroutine(State1_OctopusAttack());
            else if (state == 2) yield return StartCoroutine(State2_BurstAttack());
            else if (state == 3) yield return StartCoroutine(State3_DashAttack());
        }
    }

    // ==========================================
    // STATE 1: AHTAPOT SALDIRISI
    // ==========================================
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

    void FireOctopusWave(float angleOffset)
    {
        if (!electricBallPrefab || !firePoint) return;

        float angleStep = 360f / Mathf.Max(1, tentacleCount);

        for (int i = 0; i < tentacleCount; i++)
        {
            float angle = i * angleStep + angleOffset;

            Vector3 fireDir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            fireDir.y = 0f;         // TOP-DOWN düzeltme
            fireDir.Normalize();

            GameObject ball = Instantiate(electricBallPrefab, firePoint.position, Quaternion.LookRotation(fireDir));

            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("electricBallPrefab üzerinde Rigidbody (3D) yok! Rigidbody2D değil, Rigidbody olmalı.");
                continue;
            }

            rb.useGravity = false;  // Top-down
            rb.velocity = fireDir * 15f;
        }
    }

    // ==========================================
    // STATE 2: SERİ ATIŞ
    // ==========================================
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

    void FireAimedProjectile()
    {
        if (!burstProjectilePrefab || !firePoint || player == null) return;

        Vector3 aimDir = player.position - firePoint.position;
        aimDir.y = 0f;          // TOP-DOWN düzeltme
        if (aimDir.sqrMagnitude < 0.001f) return;
        aimDir.Normalize();

        // Debug için: yön doğru mu?
        Debug.DrawRay(firePoint.position, aimDir * 5f, Color.red, 0.5f);

        GameObject proj = Instantiate(burstProjectilePrefab, firePoint.position, Quaternion.LookRotation(aimDir));

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("burstProjectilePrefab üzerinde Rigidbody (3D) yok! Rigidbody2D değil, Rigidbody olmalı.");
            return;
        }

        rb.useGravity = false;
        rb.velocity = aimDir * 25f;
    }

    // ==========================================
    // STATE 3: DASH SALDIRISI
    // ==========================================
    IEnumerator State3_DashAttack()
    {
        if (anim != null) anim.SetTrigger("PreDash");
        yield return new WaitForSeconds(chargeTime);

        if (player == null) yield break;

        Vector3 startPos = transform.position;
        Vector3 playerPos = player.position;
        playerPos.y = startPos.y;

        Vector3 dashDir = (playerPos - startPos);
        dashDir.y = 0f;
        if (dashDir.sqrMagnitude < 0.001f) yield break;
        dashDir.Normalize();

        Vector3 targetPos = playerPos + (dashDir * dashOvershoot);

        float dashDistance = Vector3.Distance(startPos, targetPos);
        if (dashDistance < 2f) yield break;

        float t = 0f;
        int shotsFired = 0;
        isMoving = true;

        while (t < 1f)
        {
            t += (Time.deltaTime * dashSpeed) / dashDistance;
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            float progressThreshold = (shotsFired + 1f) / (shotsDuringDash + 1f);
            if (t >= progressThreshold && shotsFired < shotsDuringDash)
            {
                FireDashProjectiles(dashDir);
                shotsFired++;
            }

            yield return null;
        }

        isMoving = false;
    }

    void FireDashProjectiles(Vector3 dashDirection)
    {
        if (!burstProjectilePrefab || !firePoint) return;

        Vector3 backwardDir = -dashDirection;
        backwardDir.y = 0f;
        backwardDir.Normalize();

        // Sağ çapraz
        Vector3 rightBackDir = Quaternion.Euler(0f, dashDiagonalAngle, 0f) * backwardDir;
        rightBackDir.y = 0f;
        rightBackDir.Normalize();

        GameObject rightProj = Instantiate(burstProjectilePrefab, firePoint.position, Quaternion.LookRotation(rightBackDir));
        Rigidbody rightRb = rightProj.GetComponent<Rigidbody>();
        if (rightRb == null)
        {
            Debug.LogError("burstProjectilePrefab üzerinde Rigidbody (3D) yok!");
        }
        else
        {
            rightRb.useGravity = false;
            rightRb.velocity = rightBackDir * 20f;
        }

        // Sol çapraz
        Vector3 leftBackDir = Quaternion.Euler(0f, -dashDiagonalAngle, 0f) * backwardDir;
        leftBackDir.y = 0f;
        leftBackDir.Normalize();

        GameObject leftProj = Instantiate(burstProjectilePrefab, firePoint.position, Quaternion.LookRotation(leftBackDir));
        Rigidbody leftRb = leftProj.GetComponent<Rigidbody>();
        if (leftRb == null)
        {
            Debug.LogError("burstProjectilePrefab üzerinde Rigidbody (3D) yok!");
        }
        else
        {
            leftRb.useGravity = false;
            leftRb.velocity = leftBackDir * 20f;
        }
    }

    // ==========================================
    // HAREKET (Oyuncuya doğru kayma)
    // ==========================================
    IEnumerator GlideTowardsPlayer()
    {
        if (player == null) yield break;

        isMoving = true;
        if (anim != null) anim.SetTrigger("Move");

        Vector3 startPos = transform.position;

        Vector3 dirToPlayer = (player.position - transform.position);
        dirToPlayer.y = 0f;
        if (dirToPlayer.sqrMagnitude < 0.001f)
        {
            isMoving = false;
            yield break;
        }
        dirToPlayer.Normalize();

        Vector3 targetPos = startPos + dirToPlayer * moveDistance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed / Mathf.Max(0.001f, moveDistance);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        nextMoveTime = Time.time + moveCooldown;
        isMoving = false;
    }
}