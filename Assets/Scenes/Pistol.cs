using UnityEngine;

public class Pistol : MonoBehaviour
{
    [Header("Ateş")]
    public Transform firePoint;          // Namlu ucu (child obje)
    public GameObject bulletPrefab;      // Mermi prefab
    public float fireForce = 30f;
    public float fireRate = 0.5f;        // Kaç saniyede bir ateş edebilir

    [Header("Recoil")]
    public float recoilAmount = 0.05f;
    public float recoilSpeed = 5f;
    public float returnSpeed = 3f;

    [Header("Ses (opsiyonel)")]
    //public AudioClip fireSound;

    AudioSource audioSource;
    float nextFireTime;
    Vector3 originalPosition;
    bool isRecoiling;

    void Start()
    {
        originalPosition = transform.localPosition;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }

        // Recoil animasyonu
        HandleRecoil();
    }

    void Shoot()
    {
        if (!bulletPrefab || !firePoint) return;

        // Mermi oluştur
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Kuvvet ver
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb) rb.AddForce(firePoint.forward * fireForce, ForceMode.Impulse);

        // Ses
        //if (fireSound && audioSource) audioSource.PlayOneShot(fireSound);

        // Recoil başlat
        isRecoiling = true;
    }

    void HandleRecoil()
    {
        if (isRecoiling)
        {
            Vector3 recoilTarget = originalPosition + new Vector3(0f, 0f, -recoilAmount);
            transform.localPosition = Vector3.Lerp(transform.localPosition, recoilTarget, recoilSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.localPosition, recoilTarget) < 0.001f)
                isRecoiling = false;
        }
        else
        {
            // Orijinal pozisyona dön
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, returnSpeed * Time.deltaTime);
        }
    }
}