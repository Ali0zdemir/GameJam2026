using UnityEngine;

public class SimpleNPC : MonoBehaviour
{
    [Header("Ekranda Çýkacak Yazý Paneli")]
    public GameObject dialoguePanel;

    void Start()
    {
        // Oyun baþladýðýnda panelin kapalý (görünmez) olduðundan emin olalým
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    // Oyuncu NPC'nin görünmez çemberine (Trigger) girdiðinde çalýþýr
    void OnTriggerEnter2D(Collider2D col)
    {
        // Giren obje "Player" etiketine sahipse
        if (col.CompareTag("Player"))
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true); // Paneli görünür yap!
            }
        }
    }

    // Oyuncu NPC'nin çemberinden (Trigger) çýkýp uzaklaþtýðýnda çalýþýr
    void OnTriggerExit2D(Collider2D col)
    {
        // Çýkan obje "Player" etiketine sahipse
        if (col.CompareTag("Player"))
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false); // Paneli tekrar gizle!
            }
        }
    }
}