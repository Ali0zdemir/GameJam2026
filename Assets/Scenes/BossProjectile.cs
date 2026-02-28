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
            // Senin PlayerHealth3D'ni kullan
            PlayerHealth3D ph = other.GetComponent<PlayerHealth3D>();
            if (ph) ph.TakeDamage(damage);
        }

        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}