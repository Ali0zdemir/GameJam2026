using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Menü Arayüzü")]
    public GameObject pauseMenuUI;

    // ÇOK ÖNEMLÝ: Static yaptýk ki diðer tüm kodlar "Oyun durdu mu?" diye buraya sorabilsin.
    public static bool isPaused = false;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false; // Sesleri normale döndür
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        AudioListener.pause = true; // OYUNDAKÝ TÜM SESLERÝ DONDUR

        // Fareyi menüde kullanabilmek için serbest býrak ve görünür yap
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        AudioListener.pause = false; // SESLERÝ GERÝ AÇ

        // Fareyi tekrar oyunun ortasýna kilitle ve gizle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        AudioListener.pause = false;
        SceneManager.LoadScene(0);
    }
}