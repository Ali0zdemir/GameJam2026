using UnityEngine;

/// <summary>
/// Projectile'ın XZ hızını bozmaz.
/// Y ekseninde targetY seviyesine çok yavaş & smooth iner ve orada Y'yi kilitler.
/// Gravity kullanmadan çalışır.
/// </summary>
public class ProjectileDropToY : MonoBehaviour
{
    [Tooltip("İnilecek hedef Y seviyesi.")]
    public float targetY;

    [Tooltip("Maksimum iniş hızı (m/sn). Çok yavaş istiyorsan 0.2 - 1 arası kullan.")]
    public float maxDropSpeed = 0.6f;

    [Tooltip("Yaklaştıkça yavaşlama miktarı. Daha büyük = daha smooth/yavaş yaklaşım.")]
    public float smoothRange = 1.5f;

    [Tooltip("TargetY'ye gelince Y'yi kilitlesin mi?")]
    public bool lockAtTarget = true;

    [Tooltip("Hedefe bu kadar yaklaşınca tamam say (metre).")]
    public float snapEpsilon = 0.01f;

    private Rigidbody rb;
    private bool initialized;

    public void Init(float targetY, float maxDropSpeed, bool lockAtTarget = true)
    {
        this.targetY = targetY;
        this.maxDropSpeed = Mathf.Abs(maxDropSpeed);
        this.lockAtTarget = lockAtTarget;
        initialized = true;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = GetComponentInChildren<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!initialized || rb == null) return;

        float y = rb.position.y;

        // Zaten hedefin altındaysa
        if (y <= targetY + snapEpsilon)
        {
            if (lockAtTarget)
            {
                Vector3 p = rb.position;
                p.y = targetY;
                rb.MovePosition(p);

                Vector3 v = rb.velocity;
                v.y = 0f;
                rb.velocity = v;

                enabled = false;
            }
            return;
        }

        // Mesafe: hedefe ne kadar var?
        float dist = y - targetY;

        // Ease-out: uzaksa max speed, yaklaştıkça daha da yavaşlasın
        // t: 0 (hedefe çok yakın) -> 1 (smoothRange ve üstü uzak)
        float t = Mathf.Clamp01(dist / Mathf.Max(0.0001f, smoothRange));

        // Yaklaştıkça hız küçülsün (çok smooth): t^2
        float currentSpeed = Mathf.Lerp(0.02f, maxDropSpeed, t * t);

        // Bu frame'de ne kadar ineceğiz?
        float step = currentSpeed * Time.fixedDeltaTime;

        // Yeni Y (sadece aşağı)
        float newY = Mathf.MoveTowards(y, targetY, step);

        // XZ'yi koru
        Vector3 newPos = rb.position;
        newPos.y = newY;

        // Fizikle uyumlu taşı
        rb.MovePosition(newPos);

        // Y hızını da sıfıra yakın tut (XZ hızını bozmadan)
        Vector3 vel = rb.velocity;
        vel.y = 0f;
        rb.velocity = vel;
    }
}