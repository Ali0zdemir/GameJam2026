using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    public float damage = 10f;
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            WalkRun2D_Rigidbody playerHealth = col.GetComponent<WalkRun2D_Rigidbody>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        // DÜZELTME BURADA: Sadece "Wall" (Duvar) tagine çarpýnca yok ol, Ground'u umursama!
        else if (col.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}