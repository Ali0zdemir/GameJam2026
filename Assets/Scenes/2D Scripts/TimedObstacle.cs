using System.Collections;
using UnityEngine;

public class TimedObstacle : MonoBehaviour
{
    [Header("Trap Settings")]
    public float activeDuration = 2f;     // Ateşin ne kadar süre yanacağı (açık kalacağı)
    public float inactiveDuration = 2f;   // Ateşin ne kadar süre sönük kalacağı (kapalı kalacağı)
    public float startDelay = 0f;         // Oyuna başlarken ne kadar bekleyeceği (Dalga efekti için)

    [Header("References")]
    public GameObject trapObject;         // Açılıp kapanacak olan asıl ateş objesi

    void Start()
    {
        // Döngüyü başlat
        StartCoroutine(TrapCycleRoutine());
    }

    private IEnumerator TrapCycleRoutine()
    {
        // Eğer bir başlangıç gecikmesi varsa önce onu bekle
        if (startDelay > 0f)
        {
            if (trapObject) trapObject.SetActive(false);
            yield return new WaitForSeconds(startDelay);
        }

        // Sonsuz döngü (Oyun çalıştığı sürece devam eder)
        while (true)
        {
            // Ateşi YAK (Görünür ve çarpılabilir yap)
            if (trapObject) trapObject.SetActive(true);
            yield return new WaitForSeconds(activeDuration);

            // Ateşi SÖNDÜR (Görünmez ve içinden geçilebilir yap)
            if (trapObject) trapObject.SetActive(false);
            yield return new WaitForSeconds(inactiveDuration);
        }
    }
}