using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Enemy Prefablar")]
    public GameObject meleePrefab;
    public GameObject armoredPrefab;
    public GameObject flyingPrefab;

    [Header("Spawn Noktaları")]
    public Transform[] spawnPoints;
    public float playerSpawnBlockRadius = 10f;

    [Header("Wave Ayarları")]
    public float timeBetweenGroups = 3f;
    public float timeBetweenWaves = 5f;

    [Header("Kapı")]
    public GameObject door;
    public float doorOpenAngle = 90f;
    public float doorOpenDuration = 1.5f;

    [Header("Kapı Sonrası Trigger")]
    public GameObject nextTrigger;

    Transform player;
    int currentWave = 0;

    List<List<List<(GameObject prefab, int count)>>> allWaves;

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        BuildWaveData();
        StartCoroutine(RunWaves());
    }

    void BuildWaveData()
    {
        allWaves = new List<List<List<(GameObject, int)>>>();

        // WAVE 1
        allWaves.Add(new List<List<(GameObject, int)>>
        {
            new List<(GameObject, int)> { (meleePrefab, 3) },
            new List<(GameObject, int)> { (meleePrefab, 4) },
            new List<(GameObject, int)> { (meleePrefab, 5) },
        });

        // WAVE 2
        allWaves.Add(new List<List<(GameObject, int)>>
        {
            new List<(GameObject, int)> { (meleePrefab, 3), (armoredPrefab, 1) },
            new List<(GameObject, int)> { (meleePrefab, 2), (armoredPrefab, 2) },
            new List<(GameObject, int)> { (meleePrefab, 5), (armoredPrefab, 2) },
        });

        // WAVE 3
        allWaves.Add(new List<List<(GameObject, int)>>
        {
            new List<(GameObject, int)> { (meleePrefab, 5) },
            new List<(GameObject, int)> { (meleePrefab, 3), (armoredPrefab, 2), (flyingPrefab, 2) },
            new List<(GameObject, int)> { (meleePrefab, 2), (armoredPrefab, 3), (flyingPrefab, 3) },
        });

        // WAVE 4
        allWaves.Add(new List<List<(GameObject, int)>>
        {
            new List<(GameObject, int)> { (flyingPrefab, 5) },
            new List<(GameObject, int)> { (armoredPrefab, 5) },
            new List<(GameObject, int)> { (meleePrefab, 3), (armoredPrefab, 3), (flyingPrefab, 3) },
        });
    }

    IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(2f);

        for (int w = 0; w < allWaves.Count; w++)
        {
            currentWave = w + 1;
            Debug.Log($"Wave {currentWave} başladı!");

            var groups = allWaves[w];

            for (int g = 0; g < groups.Count; g++)
            {
                Debug.Log($"Wave {currentWave} - Grup {g + 1} spawn oluyor...");

                List<GameObject> spawnedEnemies = SpawnGroup(groups[g]);

                yield return StartCoroutine(WaitForEnemiesDead(spawnedEnemies));

                Debug.Log($"Grup {g + 1} temizlendi!");

                if (g < groups.Count - 1)
                    yield return new WaitForSeconds(timeBetweenGroups);
            }

            Debug.Log($"Wave {currentWave} tamamlandı!");

            if (w < allWaves.Count - 1)
            {
                Debug.Log($"{timeBetweenWaves} saniye sonra Wave {currentWave + 1} başlıyor...");
                yield return new WaitForSeconds(timeBetweenWaves);
            }
            else
            {
                Debug.Log("Tüm waveler tamamlandı!");
                if (door != null)
                    StartCoroutine(OpenDoor());
            }
        }
    }

    IEnumerator OpenDoor()
    {
        Quaternion startRot = door.transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0f, doorOpenAngle, 0f);
        float elapsed = 0f;

        while (elapsed < doorOpenDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / doorOpenDuration);
            door.transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }

        door.transform.rotation = targetRot;

        if (nextTrigger != null)
            nextTrigger.SetActive(true);
    }

    List<GameObject> SpawnGroup(List<(GameObject prefab, int count)> group)
    {
        List<GameObject> spawned = new List<GameObject>();
        List<Transform> activePoints = GetActiveSpawnPoints();

        if (activePoints.Count == 0)
        {
            Debug.LogWarning("Aktif spawn noktası yok!");
            return spawned;
        }

        foreach (var (prefab, count) in group)
        {
            for (int i = 0; i < count; i++)
            {
                if (prefab == null) continue;

                Transform spawnPoint = activePoints[Random.Range(0, activePoints.Count)];
                GameObject enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
                spawned.Add(enemy);
            }
        }

        return spawned;
    }

    List<Transform> GetActiveSpawnPoints()
    {
        List<Transform> active = new List<Transform>();

        foreach (Transform sp in spawnPoints)
        {
            if (sp == null) continue;

            if (player != null)
            {
                float dist = Vector3.Distance(sp.position, player.position);
                if (dist < playerSpawnBlockRadius) continue;
            }

            active.Add(sp);
        }

        return active;
    }

    IEnumerator WaitForEnemiesDead(List<GameObject> enemies)
    {
        float timeout = 50f;
        float elapsed = 0f;

        bool anyAlive = true;

        while (anyAlive)
        {
            anyAlive = false;
            foreach (GameObject e in enemies)
            {
                if (e != null)
                {
                    anyAlive = true;
                    break;
                }
            }

            elapsed += 0.5f;

            if (elapsed >= timeout)
            {
                foreach (GameObject e in enemies)
                    if (e != null) Destroy(e);
                break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}