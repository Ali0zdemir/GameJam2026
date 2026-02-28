using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    public Transform[] waypoints; // Platformun gideceği noktalar
    public float speed = 3f;      // Hareket hızı

    private int currentWaypointIndex = 0;

    void Update()
    {
        // Eğer hedef nokta atanmamışsa hata vermesin diye kontrol ediyoruz
        if (waypoints.Length == 0) return;

        // Platformu sıradaki hedefe doğru hareket ettir
        transform.position = Vector2.MoveTowards(transform.position, waypoints[currentWaypointIndex].position, speed * Time.deltaTime);

        // Hedefe yeterince yaklaştık mı?
        if (Vector2.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.1f)
        {
            // Yaklaştıysak bir sonraki hedefe geç
            currentWaypointIndex++;

            // Eğer son hedefe ulaştıysak, başa dön (Sürekli git-gel yapması için)
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Oyuncu platforma değdiğinde, onu platformun alt objesi (Child) yap
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Oyuncu platformdan ayrıldığında bağları kopar
        if (collision.gameObject.CompareTag("Player"))
        {
            // HATAYI ÇÖZEN KISIM: Sadece platform aktif durumdaysa bağları kopar
            if (gameObject.activeInHierarchy)
            {
                collision.transform.SetParent(null);
            }
        }
    }
}