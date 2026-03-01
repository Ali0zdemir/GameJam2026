using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 8f;
    public float damage = 10f;
    public float knockbackForce = 12f;
    public float lifetime = 5f;

    Transform target;

    void Start()
    {
        Destroy(gameObject, lifetime);
        GameObject player = GameObject.FindWithTag("Player");
        if (player) target = player.transform;
    }

    void Update()
{
    transform.position += transform.forward * speed * Time.deltaTime;
}

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        WalkRun2D_Rigidbody player = col.GetComponent<WalkRun2D_Rigidbody>();
        if (player != null)
        {
            Vector2 knockDir = (col.transform.position - transform.position).normalized;
            player.TakeDamage(damage);
            player.ApplyKnockback(knockDir, knockbackForce);
        }

        Destroy(gameObject);
    }
}