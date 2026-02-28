using System.Collections;
using UnityEngine;

public class Pistol : MonoBehaviour
{
    [Header("Ateş")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public float fireForce = 30f;
    public float fireRate = 0.12f;

    [Header("Seri Atış")]
    public int burstCount = 3;
    public float burstInterval = 0.08f;
    public float burstCooldown = 0.5f;

    [Header("Ses")]
    public float volume = 1f;

    [Header("Muzzle Flash")]
    public GameObject muzzleFlash;
    public float muzzleFlashDuration = 0.05f;

    [Header("Pozisyon Recoil")]
    public float recoilAmount = 0.05f;
    public float recoilSpeed = 8f;
    public float returnSpeed = 4f;

    [Header("Rotasyon Recoil")]
    public float recoilRotationAmount = 5f;
    public float recoilRotationSpeed = 10f;
    public float returnRotationSpeed = 5f;

    [Header("Ekran Sarsıntısı")]
    public float shakeAmount = 0.05f;
    public float shakeDuration = 0.1f;

    AudioSource audioSource;
    float nextFireTime;
    bool isBursting;

    Vector3 originalPosition;
    Quaternion originalRotation;
    bool isRecoiling;
    bool isRotationRecoiling;

    Camera mainCam;
    Vector3 camOriginalPos;
    float shakeTimer;
    bool isShaking;

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
            audioSource.playOnAwake = false;

        mainCam = Camera.main;
        if (mainCam) camOriginalPos = mainCam.transform.localPosition;

        if (muzzleFlash != null) muzzleFlash.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime && !isBursting)
        {
            StartCoroutine(BurstFire());
            nextFireTime = Time.time + burstCooldown;
        }

        HandleRecoil();
        HandleScreenShake();
    }

    IEnumerator BurstFire()
    {
        isBursting = true;

        for (int i = 0; i < burstCount; i++)
        {
            Shoot();
            yield return new WaitForSeconds(burstInterval);
        }

        isBursting = false;
    }

    void Shoot()
    {
        if (!bulletPrefab || !firePoint) return;

        if (audioSource != null && audioSource.clip != null)
            audioSource.PlayOneShot(audioSource.clip, volume);

        if (muzzleFlash != null)
        {
            muzzleFlash.SetActive(true);
            CancelInvoke(nameof(HideMuzzleFlash));
            Invoke(nameof(HideMuzzleFlash), muzzleFlashDuration);
        }

        Camera cam = Camera.main;
        Vector3 targetPoint;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(100f);

        Vector3 aimDir = (targetPoint - firePoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(aimDir));

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb) rb.AddForce(aimDir * fireForce, ForceMode.Impulse);

        isRecoiling = true;
        isRotationRecoiling = true;
        StartScreenShake();
    }

    void HideMuzzleFlash()
    {
        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);
    }

    void HandleRecoil()
    {
        if (isRecoiling)
        {
            Vector3 recoilTarget = originalPosition + new Vector3(recoilAmount * 0.5f, -recoilAmount * 0.3f, -recoilAmount);
            transform.localPosition = Vector3.Lerp(transform.localPosition, recoilTarget, recoilSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.localPosition, recoilTarget) < 0.001f)
                isRecoiling = false;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, returnSpeed * Time.deltaTime);
        }

        if (isRotationRecoiling)
        {
            Quaternion recoilTarget = originalRotation * Quaternion.Euler(-recoilRotationAmount, recoilRotationAmount * 0.3f, recoilRotationAmount * 0.2f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, recoilTarget, recoilRotationSpeed * Time.deltaTime);
            if (Quaternion.Angle(transform.localRotation, recoilTarget) < 0.1f)
                isRotationRecoiling = false;
        }
        else
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRotation, returnRotationSpeed * Time.deltaTime);
        }
    }

    void StartScreenShake()
    {
        shakeTimer = shakeDuration;
        isShaking = true;
    }

    void HandleScreenShake()
    {
        if (!mainCam || !isShaking) return;

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float x = Random.Range(-shakeAmount, shakeAmount);
            float y = Random.Range(-shakeAmount, shakeAmount);
            mainCam.transform.localPosition = camOriginalPos + new Vector3(x, y, 0f);
        }
        else
        {
            isShaking = false;
            mainCam.transform.localPosition = Vector3.Lerp(
                mainCam.transform.localPosition,
                camOriginalPos,
                10f * Time.deltaTime
            );
        }
    }
}