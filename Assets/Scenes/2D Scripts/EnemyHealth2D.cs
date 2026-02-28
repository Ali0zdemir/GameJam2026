using System.Collections;
using UnityEngine;

public class EnemyHealth2D : MonoBehaviour
{
    [Header("Ölüm Ayarları")]
    public Animator anim;
    [Tooltip("Ölüm animasyonu bitene kadar geçecek süre (Saniye)")]
    public float deathDelay = 0.5f;

    private bool isDead = false;

    // Player scripti burayı çağıracak
    public void TakeHit()
    {
        // Eğer zaten öldüyse, aynı anda iki kere kılıç değerse diye kodu durdur
        if (isDead) return;

        isDead = true;

        // 1. Öldüğü an fiziksel kutusunu kapat ki karakterin içinden geçebilsin
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 2. Öldüğünde yere düşmesin veya hareket etmesin diye fiziğini dondur
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        // 3. Ölüm animasyonunu tetikle (İleride eklediğinde çalışacak)
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // 4. Silinme sayacını başlat
        StartCoroutine(DestroyRoutine());
    }

    IEnumerator DestroyRoutine()
    {
        // Animasyonun oynanması için belirlediğin süre kadar bekle
        yield return new WaitForSeconds(deathDelay);

        // Obje sahneden tamamen silinsin
        Destroy(gameObject);
    }
}