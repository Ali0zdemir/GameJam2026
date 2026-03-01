using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        // 1 numaralý sahneyi yükler
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Debug.Log("Oyundan çýkýldý!");

        // Bu komut gerçek oyunu (çýktý alýndýðýnda) kapatýr:
        Application.Quit();

        // Bu komut ise SADECE Unity Editörü içindeyken Play modunu durdurur:
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}