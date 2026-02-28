using UnityEngine;

public class BulletTrail : MonoBehaviour
{
    [Header("Trail")]
    public Sprite trailSprite;           // Bırakılacak 2D sprite
    public float spawnInterval = 0.05f; // Ne sıklıkla sprite bırakılsın
    public float trailLifetime = 0.3f;  // Sprite ne kadar süre kalsın
    public float trailSize = 0.3f;      // Sprite boyutu
    public Color trailColor = Color.white;

    float timer;
    Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnTrailSprite();
        }
    }

    void SpawnTrailSprite()
    {
        if (trailSprite == null) return;

        // Boş obje oluştur
        GameObject trailObj = new GameObject("TrailSprite");
        trailObj.transform.position = transform.position;

        // Kameraya bak (billboard)
        if (mainCam)
            trailObj.transform.rotation = Quaternion.Euler(0f, mainCam.transform.eulerAngles.y, 0f);

        // SpriteRenderer ekle
        SpriteRenderer sr = trailObj.AddComponent<SpriteRenderer>();
        sr.sprite = trailSprite;
        sr.color = trailColor;
        trailObj.transform.localScale = Vector3.one * trailSize;

        // Sorting layer ayarla (enemy sprite'ların üstünde görünsün)
        sr.sortingOrder = 10;

        // Belirli süre sonra yok et
        Destroy(trailObj, trailLifetime);

        // Fade out efekti
        StartCoroutine(FadeOut(sr, trailLifetime));
    }

    System.Collections.IEnumerator FadeOut(SpriteRenderer sr, float duration)
    {
        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < duration)
        {
            if (sr == null) yield break;
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
    }
}