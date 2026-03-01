using UnityEngine;

public class KillZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Enemy'leri öldür
        var health = other.GetComponentInParent<EnemyHealth>();
        if (health != null)
        {
            health.TakeDamage(99999f);
            return;
        }

        // EnemyHealth yoksa direkt yok et
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
            Destroy(other.gameObject);
    }
}