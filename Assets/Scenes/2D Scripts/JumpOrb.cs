using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class JumpOrb : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    void Start()
    {
        // Kristalin görselini ve çarpışma alanını bul
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // Bu objenin katı bir duvar değil, içinden geçilebilir bir alan olmasını garantile
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            WalkRun2D_Rigidbody player = collision.GetComponent<WalkRun2D_Rigidbody>();
            if (player != null)
            {
                player.EnterJumpOrb(this); // Oyuncuya "kristalin içindesin" de
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            WalkRun2D_Rigidbody player = collision.GetComponent<WalkRun2D_Rigidbody>();
            if (player != null)
            {
                player.ExitJumpOrb(this); // Oyuncuya "kristalden çıktın" de
            }
        }
    }

    // Oyuncu zıpladığında görseli ve tetikleyiciyi kapatır
    public void DeactivateOrb()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        col.enabled = false;
    }

    // Oyuncu yere değdiğinde görseli ve tetikleyiciyi geri açar
    public void ActivateOrb()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        col.enabled = true;
    }
}