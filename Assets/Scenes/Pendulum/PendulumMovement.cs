using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PendulumMovement : MonoBehaviour
{
    [Header("Swing Settings")]
    public float swingSpeed = 2f;   // Sallanma hızı
    public float rightAngle = 0f;   // Sağ sınır açısı
    public float leftAngle = 180f;  // Sol sınır açısı

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Kod ile kontrol edeceğimiz için Kinematik yapıyoruz
        rb.isKinematic = true;
    }

    void FixedUpdate()
    {
        // Mathf.Sin bize -1 ile 1 arasında zamanla değişen bir dalga verir
        float timeValue = Mathf.Sin(Time.time * swingSpeed);

        // Bu dalgayı 0 ile 1 arasına sıkıştırıp açılarımız arasında yumuşak geçiş yapıyoruz
        float currentAngle = Mathf.Lerp(leftAngle, rightAngle, (timeValue + 1f) / 2f);

        // Objenin fiziksel rotasyonunu güncelliyoruz (duvarın içinden geçmemesi için MoveRotation)
        rb.MoveRotation(currentAngle);
    }
}