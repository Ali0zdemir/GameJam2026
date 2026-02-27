using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Mermi")]
    public float lifetime = 3f;
    public float damage = 25f;
    public GameObject hitEffect;

    [Header("Layer")]
    public string playerTag = "Player";

    void Start()
    {
        Destroy(gameObject, lifetime);

        GameObject player = GameObject.FindWithTag(playerTag);
        if (player)
        {
            Collider playerCollider = player.GetComponent<Collider>();
            Collider bulletCollider = GetComponent<Collider>();
            if (playerCollider && bulletCollider)
                Physics.IgnoreCollision(bulletCollider, playerCollider);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag(playerTag)) return;

        if (col.gameObject.CompareTag("Enemy"))
        {
            EnemyHealth enemy = col.gameObject.GetComponent<EnemyHealth>();
            if (enemy) enemy.TakeDamage(damage);
        }

        // Enemy olsun olmasın HER ÇARPMADA yok ol
        if (hitEffect)
            Instantiate(hitEffect, transform.position, Quaternion.LookRotation(col.contacts[0].normal));

        Destroy(gameObject);
    }
}