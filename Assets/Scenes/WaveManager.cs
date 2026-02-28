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
    public float playerSpawnBlockRadius = 10f; // Player'a bu kadar yakın spawnlar pasif

    [Header("Wave Ayarları")]
    public float timeBetweenGroups = 3f; // Grup içi bekleme
    public float timeBetweenWaves = 5f;  // Wave'ler arası bekleme

    Transform player;
    int currentWave = 0;
    bool waveRunning = false;

    // Wave yapısı: her wave = grup listesi, her grup = enemy listesi
    // Format: WaveData[wave][group][enemy] = (prefab, count)
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
            new List<(GameObject, int)> { (meleePrefab, 3) },           // Grup 1: 3 melee
            new List<(GameObject, int)> { (meleePrefab, 4) },           // Grup 2: 4 melee
            new List<(GameObject, int)> { (meleePrefab, 5) },           // Grup 3: 5 melee
        });

        // WAVE 2
        allWaves.Add(new List<List<(GameObject, int)>>
        {
            new List<(GameObject, int)> { (meleePrefab, 3), (armoredPrefab, 1) },           // Grup 1
            new List<(GameObject, int)> { (meleePrefab, 2), (armoredPrefab, 2) },           // Grup 2
            new List<(GameObject, int)> { (meleePrefab, 5), (armoredPrefab, 2) },           // Grup 3
        });

        // WAVE 3
        allWaves.Add(new List<List<(GameObject, int)>>
        {
            new List<(GameObject, int)> { (meleePrefab, 5) },                                           // Grup 1
            new List<(GameObject, int)> { (meleePrefab, 3), (armoredPrefab, 2), (flyingPrefab, 2) },   // Grup 2
            new List<(GameObject, int)> { (meleePrefab, 2), (armoredPrefab, 3), (flyingPrefab, 3) },   // Grup 3
        });

        // WAVE 4
        allWaves.Add(new List<List<(GameObject, int)>>
        {
            new List<(GameObject, int)> { (flyingPrefab, 5) },                                          // Grup 1
            new List<(GameObject, int)> { (armoredPrefab, 5) },                                         // Grup 2
            new List<(GameObject, int)> { (meleePrefab, 3), (armoredPrefab, 3), (flyingPrefab, 3) },   // Grup 3
        });
    }

    IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(2f); // Başlangıç gecikmesi

        for (int w = 0; w < allWaves.Count; w++)
        {
            currentWave = w + 1;
            Debug.Log($"Wave {currentWave} başladı!");

            var groups = allWaves[w];

            for (int g = 0; g < groups.Count; g++)
            {
                Debug.Log($"Wave {currentWave} - Grup {g + 1} spawn oluyor...");

                List<GameObject> spawnedEnemies = SpawnGroup(groups[g]);

                // Tüm enemyler ölene kadar bekle
                yield return StartCoroutine(WaitForEnemiesDead(spawnedEnemies));

                Debug.Log($"Grup {g + 1} temizlendi!");

                // Son grup değilse gruplar arası bekle
                if (g < groups.Count - 1)
                    yield return new WaitForSeconds(timeBetweenGroups);
            }

            Debug.Log($"Wave {currentWave} tamamlandı!");

            // Son wave değilse waveler arası bekle
            if (w < allWaves.Count - 1)
            {
                Debug.Log($"{timeBetweenWaves} saniye sonra Wave {currentWave + 1} başlıyor...");
                yield return new WaitForSeconds(timeBetweenWaves);
            }
            else
            {
                Debug.Log("Tüm waveler tamamlandı!");
            }
        }
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

            // Player'a çok yakınsa pasif say
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
        // Null olmayan (ölmemiş) enemy kalmayana kadar bekle
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
            yield return new WaitForSeconds(0.5f);
        }
    }
}