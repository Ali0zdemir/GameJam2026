using UnityEngine;

public class ObstacleDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damageAmount = 20f;
    public float knockbackForce = 10f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision);
    }

    void TryDamagePlayer(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        WalkRun2D_Rigidbody player = collision.gameObject.GetComponent<WalkRun2D_Rigidbody>();
        if (player == null) return;

        bool tookDamage = player.TakeDamage(damageAmount);

        if (tookDamage)
        {
            Vector2 knockbackDirection = (Vector2)collision.transform.position - (Vector2)transform.position;
            knockbackDirection.y = 1f;
            player.ApplyKnockback(knockbackDirection.normalized, knockbackForce);
        }
    }
}