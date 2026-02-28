using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Mermi")]
    public float lifetime = 3f;
    public float damage = 25f;
    public GameObject hitEffect;

    [Header("Mesafe")]
    public float maxDistance = 50f;

    [Header("Layer")]
    public string playerTag = "Player";

    Vector3 startPosition;
    bool hasHit = false; // Çarpma kilidi

    void Start()
    {
        Destroy(gameObject, lifetime);
        startPosition = transform.position;

        GameObject player = GameObject.FindWithTag(playerTag);
        if (player)
        {
            Collider playerCollider = player.GetComponent<Collider>();
            Collider bulletCollider = GetComponent<Collider>();
            if (playerCollider && bulletCollider)
                Physics.IgnoreCollision(bulletCollider, playerCollider);
        }
    }

    void Update()
    {
        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
            Destroy(gameObject);
    }

    void OnCollisionEnter(Collision col)
{
    if (hasHit) return;
    if (col.gameObject.CompareTag(playerTag)) return;

    if (col.gameObject.CompareTag("Enemy"))
    {
        // Armored kontrolü
        EnemyAI_Armored armored = col.gameObject.GetComponentInParent<EnemyAI_Armored>();
        if (armored != null && armored.isInvulnerable)
        {
            hasHit = true;
            Destroy(gameObject);
            return;
        }

        // Normal enemy
        EnemyHealth enemy = col.gameObject.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            hasHit = true;
            enemy.TakeDamage(damage);
        }
    }

    // Boss'a çarptıysa
    if (col.gameObject.CompareTag("Boss"))
    {
        BossAI boss = col.gameObject.GetComponentInParent<BossAI>();
        if (boss != null)
        {
            hasHit = true;
            boss.TakeDamage(damage);
        }
    }

    if (hitEffect)
        Instantiate(hitEffect, transform.position, Quaternion.LookRotation(col.contacts[0].normal));

    hasHit = true;
    Destroy(gameObject);
}
}