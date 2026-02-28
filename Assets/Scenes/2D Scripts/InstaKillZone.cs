using UnityEngine;

public class InstaKillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerRespawn playerRespawn = collision.GetComponent<PlayerRespawn>();
            WalkRun2D_Rigidbody player = collision.GetComponent<WalkRun2D_Rigidbody>();

            // Canı sıfırla
            if (player != null && player.healthData != null)
                player.healthData.currentHealth = 0;

            // Anında ölümü tetikle
            if (playerRespawn != null)
                playerRespawn.TriggerInstantDeath();
        }
    }
}