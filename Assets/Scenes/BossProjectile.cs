using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float damage = 20f;
    public GameObject hitEffect;
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Projectile")) return;

        if (other.CompareTag("Player"))
        {
            // BURASI DEĞİŞTİ: Artık senin PlayerHealth3D scriptini arıyor!
            PlayerHealth3D ph = other.GetComponent<PlayerHealth3D>();

            if (ph != null)
            {
                ph.TakeDamage(damage);
                Debug.Log("Oyuncuya vuruldu! Hasar: " + damage);
            }
        }

        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}