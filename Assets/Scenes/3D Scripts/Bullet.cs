using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    [Header("Mermi Ayarları")]
    public float lifetime = 3f;
    public float damage = 25f;
    public GameObject hitEffect; 
    public GameObject bloodPrefab;

    [Header("Kan Efekti Ayarları")]
    public int bloodCount = 5;
    public float bloodInterval = 0.05f;
    public float spreadAmount = 0.2f;

    [Header("Mesafe & Etiket")]
    public float maxDistance = 50f;
    public string playerTag = "Player";

    private Vector3 startPosition;
    private bool hasHit = false;

    void Start()
    {
        startPosition = transform.position;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
            Destroy(gameObject);
    }

    void OnCollisionEnter(Collision col)
    {
        if (hasHit) return;
        if (col.gameObject.CompareTag(playerTag) || col.gameObject.CompareTag("Bullet")) return;

        hasHit = true;

        if (col.gameObject.CompareTag("Enemy") || col.gameObject.CompareTag("Boss"))
        {
            HandleDamage(col.gameObject);
            
            // Kan efektini başlatmak için merminin "konumunu" ve "çarptığı objeyi" kullanıyoruz
            // Mermi yok olacağı için Coroutine'i sahnede yaşayan bir objede (Düşmanda) başlatıyoruz
            if (bloodPrefab != null)
            {
                // Düşmanın üzerinde bir script aracılığıyla veya boş bir obje yaratarak efekti başlat
                SpawnBloodIndependent(col);
            }
        }
        else
        {
            if (hitEffect)
                Instantiate(hitEffect, col.contacts[0].point, Quaternion.LookRotation(col.contacts[0].normal));
        }

        // MERMİYİ ANINDA YOK ET
        Destroy(gameObject);
    }

    // Bu fonksiyon mermiden bağımsız objeler oluşturur
    void SpawnBloodIndependent(Collision col)
    {
        ContactPoint contact = col.contacts[0];
        
        // Boş bir obje oluşturup kan efektini mermiden bağımsız olarak ona devrediyoruz
        GameObject bloodManager = new GameObject("BloodEffect_Temp");
        bloodManager.transform.position = contact.point;
        
        // Bu geçici objeye bir script ekleyip efektleri o objenin yönetmesini sağlıyoruz
        BloodEffectTask task = bloodManager.AddComponent<BloodEffectTask>();
        task.StartEffect(bloodPrefab, col, bloodCount, bloodInterval, spreadAmount);
    }

    void HandleDamage(GameObject obj)
    {
        var enemy = obj.GetComponentInParent<EnemyHealth>();
        if (enemy != null) enemy.TakeDamage(damage);

        var boss = obj.GetComponentInParent<BossAI>();
        if (boss != null) boss.TakeDamage(damage);

        if (CrosshairHitIndicator.Instance != null)
            CrosshairHitIndicator.Instance.ShowHit();
    }
}

// BU AYRI BİR CLASS / DOSYA OLABİLİR VEYA AYNI DOSYANIN ALTINA EKLEYEBİLİRSİN
public class BloodEffectTask : MonoBehaviour
{
    public void StartEffect(GameObject prefab, Collision col, int count, float interval, float spread)
    {
        StartCoroutine(Process(prefab, col, count, interval, spread));
    }

    IEnumerator Process(GameObject prefab, Collision col, int count, float interval, float spread)
    {
        ContactPoint contact = col.contacts[0];
        Transform enemyTransform = col.transform;

        for (int i = 0; i < count; i++)
        {
            if (enemyTransform == null) break;

            Vector3 randomOffset = Random.insideUnitSphere * spread;
            GameObject blood = Instantiate(prefab, contact.point + randomOffset, Quaternion.LookRotation(contact.normal));
            
            blood.transform.SetParent(enemyTransform);
            blood.transform.Rotate(Vector3.forward, Random.Range(0, 360));
            
            // Kan sprite'ında collider varsa kapat
            Collider c = blood.GetComponent<Collider>();
            if (c) c.enabled = false;

            Destroy(blood, 1.5f);
            yield return new WaitForSeconds(interval);
        }
        
        // İşlem bitince bu geçici yöneticiyi de yok et
        Destroy(gameObject);
    }
}