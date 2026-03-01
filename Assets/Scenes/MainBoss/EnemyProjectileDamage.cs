using UnityEngine;

public class EnemyProjectileDamage : MonoBehaviour
{
    public float damage = 10f;
    public string playerTag = "Player";
    public bool destroyOnHit = true;

    private bool hit;

    void OnCollisionEnter(Collision col)
    {
        TryHit(col.collider);
    }

    void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    void TryHit(Collider col)
    {
        if (hit) return;
        if (!col.CompareTag(playerTag)) return;

        hit = true;

        var ph = col.GetComponentInParent<PlayerHealth3D>();
        if (ph != null) ph.TakeDamage(damage);

        if (destroyOnHit) Destroy(gameObject);
    }
}