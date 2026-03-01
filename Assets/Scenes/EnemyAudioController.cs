using System.Collections;
using UnityEngine;

public class EnemyAudioController : MonoBehaviour
{
    [Header("Ses Kaynağı")]
    public AudioSource audioSource;

    [Header("Melee Enemy Sesleri")]
    public AudioClip chargeSound;
    public AudioClip jumpSound;
    public AudioClip landSound;

    [Header("Hit Sesleri (rastgele çalar)")]
    public AudioClip[] hitSounds;

    [Header("Kalkan Sesleri")]
    public AudioClip[] shieldHitSounds;
    public AudioClip shieldCloseSound;
    public AudioClip shieldOpenSound;

    [Header("Ateş Sesi")]
    public AudioClip[] shootSounds;

    [Header("Uçan Enemy Sesi")]
    public AudioClip flyingLoopSound; // Sürekli çalacak tek ses (kanat veya pervane)

    [Header("Ses Seviyeleri")]
    [Range(0f, 1f)] public float chargeVolume = 1f;
    [Range(0f, 1f)] public float jumpVolume = 1f;
    [Range(0f, 1f)] public float landVolume = 1f;
    [Range(0f, 1f)] public float hitVolume = 1f;
    [Range(0f, 1f)] public float shieldHitVolume = 1f;
    [Range(0f, 1f)] public float shieldCloseVolume = 1f;
    [Range(0f, 1f)] public float shieldOpenVolume = 1f;
    [Range(0f, 1f)] public float shootVolume = 1f;
    [Range(0f, 1f)] public float flyingLoopVolume = 0.4f;

    // Uçan ses için ayrı AudioSource (diğer sesleri kesmemek için)
    AudioSource flyingAudioSource;

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    public void PlayCharge()
    {
        if (chargeSound != null)
            audioSource.PlayOneShot(chargeSound, chargeVolume);
    }

    public void PlayJump()
    {
        if (jumpSound != null)
            audioSource.PlayOneShot(jumpSound, jumpVolume);
    }

    public void PlayLand()
    {
        if (landSound != null)
            audioSource.PlayOneShot(landSound, landVolume);
    }

    public void PlayHit()
    {
        if (hitSounds == null || hitSounds.Length == 0) return;
        AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
        if (clip != null)
            audioSource.PlayOneShot(clip, hitVolume);
    }

    public void PlayShieldHit()
    {
        if (shieldHitSounds == null || shieldHitSounds.Length == 0) return;
        AudioClip clip = shieldHitSounds[Random.Range(0, shieldHitSounds.Length)];
        if (clip != null)
            audioSource.PlayOneShot(clip, shieldHitVolume);
    }

    public void PlayShieldOpen()
    {
        if (shieldOpenSound != null)
            audioSource.PlayOneShot(shieldOpenSound, shieldOpenVolume);
    }

    public void PlayShieldClose()
    {
        if (shieldCloseSound != null)
        {
            audioSource.Stop();
            audioSource.clip = shieldCloseSound;
            audioSource.volume = shieldCloseVolume;
            audioSource.time = 0f;
            audioSource.Play();
            StopCoroutine("StopShieldSound");
            StartCoroutine("StopShieldSound");
        }
    }

    IEnumerator StopShieldSound()
    {
        yield return new WaitForSeconds(0.4f);
        audioSource.Stop();
    }

    public void PlayShoot()
    {
        if (shootSounds == null || shootSounds.Length == 0) return;
        AudioClip clip = shootSounds[Random.Range(0, shootSounds.Length)];
        if (clip != null)
            audioSource.PlayOneShot(clip, shootVolume);
    }

    // Uçan enemy spawn olunca çağır, ölünce StopFlyingLoop çağır
    public void StartFlyingLoop()
    {
        if (flyingLoopSound == null) return;

        // Ayrı bir AudioSource oluştur ki diğer sesleri kesmesin
        flyingAudioSource = gameObject.AddComponent<AudioSource>();
        flyingAudioSource.clip = flyingLoopSound;
        flyingAudioSource.loop = true;
        flyingAudioSource.volume = flyingLoopVolume;
        flyingAudioSource.spatialBlend = 1f;
        flyingAudioSource.playOnAwake = false;
        flyingAudioSource.Play();
    }

    public void StopFlyingLoop()
    {
        if (flyingAudioSource != null)
            flyingAudioSource.Stop();
    }
}