using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Geçilecek sahnenin adı (Build Settings'e ekli olmalı)")]
    public string targetSceneName = "3DScene";

    [Header("Sadece belirli tag'e sahip obje geçiş yapsın")]
    public string playerTag = "Player";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }

    // 3D sahneden 2D'ye geçiş için aynı script ama OnTriggerEnter kullan
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}