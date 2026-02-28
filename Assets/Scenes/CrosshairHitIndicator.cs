using System.Collections;
using UnityEngine;

public class CrosshairHitIndicator : MonoBehaviour
{
    public static CrosshairHitIndicator Instance;

    [Header("Hit Indicator")]
    public GameObject hitIndicator; // Crosshair'daki gizli obje
    public float showDuration = 0.1f;

    void Awake()
    {
        Instance = this;
        if (hitIndicator) hitIndicator.SetActive(false);
    }

    public void ShowHit()
    {
        StopAllCoroutines();
        StartCoroutine(HitRoutine());
    }

    IEnumerator HitRoutine()
    {
        if (hitIndicator) hitIndicator.SetActive(true);
        yield return new WaitForSeconds(showDuration);
        if (hitIndicator) hitIndicator.SetActive(false);
    }
}