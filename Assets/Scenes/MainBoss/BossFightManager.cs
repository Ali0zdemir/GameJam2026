using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossFightManager : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth3D playerHealth;

    public GameObject bossObject;
    public GameObject boardObject;

    [Header("Boss UI")]
    [Tooltip("Boss HP UI objesi (Canvas içindeki panel vb). Başta kapalı olacak.")]
    public GameObject bossHpUIRoot;

    [Tooltip("Boss HP Slider referansı.")]
    public Slider bossHpSlider;

    [Header("Wave Prefabs")]
    public GameObject meleePrefab;
    public GameObject shieldPrefab;
    public GameObject flyerPrefab;

    [Header("Wave Counts (Toplam)")]
    public int meleeCount = 6;
    public int shieldCount = 4;
    public int flyerCount = 3;

    [Header("Spawn")]
    public Transform[] spawnPoints;
    public float randomSpawnRadius = 4f;
    public Transform arenaCenter;

    [Header("Wave Batch Spawn")]
    public float batchDelayMin = 4f;
    public float batchDelayMax = 6f;

    [Header("UI (Death)")]
    public GameObject deathUI;
    public float restartDelay = 2f;

    [Header("Boss Appear")]
    public float bossAppearDelay = 0.3f;

    private HashSet<WaveMember> aliveWaveEnemies = new HashSet<WaveMember>();

    private BossAI bossAI; // slider için boss referansı
    private bool bossStarted;

    void Start()
    {
        if (arenaCenter == null) arenaCenter = transform;

        if (deathUI != null) deathUI.SetActive(false);

        // Boss HP UI başta kapalı
        if (bossHpUIRoot != null) bossHpUIRoot.SetActive(false);

        // Boss başta yok
        if (bossObject != null) bossObject.SetActive(false);

        // Tablo başta var
        if (boardObject != null) boardObject.SetActive(true);

        if (playerHealth != null)
        {
            playerHealth.ResetHealthToRestartValue();
            playerHealth.OnDied += HandlePlayerDied;
        }

        StartCoroutine(RunEncounter());
    }

    void Update()
    {
        // Boss başladıysa slider’ı sürekli güncelle
        if (!bossStarted) return;

        // Boss öldüyse (destroy olduysa) UI kapat
        if (bossAI == null)
        {
            if (bossHpUIRoot != null && bossHpUIRoot.activeSelf)
                bossHpUIRoot.SetActive(false);
            return;
        }

        if (bossHpSlider != null)
            bossHpSlider.value = bossAI.CurrentHealth;
    }

    IEnumerator RunEncounter()
    {
        aliveWaveEnemies.Clear();

        // 1) Pre-wave 3 batch halinde spawn
        yield return StartCoroutine(SpawnWaveIn3Batches());

        // 2) Hepsi ölünce boss gelsin
        yield return new WaitUntil(() => aliveWaveEnemies.Count == 0);

        yield return new WaitForSeconds(bossAppearDelay);

        if (boardObject != null) boardObject.SetActive(false);

        if (bossObject != null)
        {
            bossObject.SetActive(true);

            // Boss'un Start/Awake düzenli çalışsın diye 1 frame bekle
            yield return null;

            bossAI = bossObject.GetComponent<BossAI>();
            if (bossAI == null) bossAI = bossObject.GetComponentInChildren<BossAI>();

            SetupBossHpUI();
            bossStarted = true;
        }
    }

    void SetupBossHpUI()
    {
        if (bossHpUIRoot != null) bossHpUIRoot.SetActive(true);

        if (bossHpSlider != null && bossAI != null)
        {
            bossHpSlider.minValue = 0f;
            bossHpSlider.maxValue = bossAI.MaxHealth;
            bossHpSlider.value = bossAI.CurrentHealth;
        }
    }

    IEnumerator SpawnWaveIn3Batches()
    {
        // 6 melee, 4 shield, 3 flyer -> 3 batch
        int[] meleeB  = new int[] { 2, 2, 2 };
        int[] shieldB = new int[] { 2, 1, 1 };
        int[] flyerB  = new int[] { 1, 1, 1 };

        // clamp (inspector değişirse patlamasın)
        meleeB[0] = Mathf.Min(meleeB[0], meleeCount);
        meleeB[1] = Mathf.Min(meleeB[1], Mathf.Max(0, meleeCount - meleeB[0]));
        meleeB[2] = Mathf.Max(0, meleeCount - meleeB[0] - meleeB[1]);

        shieldB[0] = Mathf.Min(shieldB[0], shieldCount);
        shieldB[1] = Mathf.Min(shieldB[1], Mathf.Max(0, shieldCount - shieldB[0]));
        shieldB[2] = Mathf.Max(0, shieldCount - shieldB[0] - shieldB[1]);

        flyerB[0] = Mathf.Min(flyerB[0], flyerCount);
        flyerB[1] = Mathf.Min(flyerB[1], Mathf.Max(0, flyerCount - flyerB[0]));
        flyerB[2] = Mathf.Max(0, flyerCount - flyerB[0] - flyerB[1]);

        for (int batch = 0; batch < 3; batch++)
        {
            SpawnMany(meleePrefab, meleeB[batch]);
            SpawnMany(shieldPrefab, shieldB[batch]);
            SpawnMany(flyerPrefab, flyerB[batch]);

            if (batch < 2)
                yield return new WaitForSeconds(Random.Range(batchDelayMin, batchDelayMax));
        }
    }

    void SpawnMany(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = GetSpawnPos();
            GameObject go = Instantiate(prefab, pos, Quaternion.identity);

            var member = go.GetComponent<WaveMember>();
            if (member == null) member = go.AddComponent<WaveMember>();
            member.Init(this);
        }
    }

    Vector3 GetSpawnPos()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return sp.position;
        }

        Vector2 r = Random.insideUnitCircle * randomSpawnRadius;
        Vector3 basePos = arenaCenter != null ? arenaCenter.position : transform.position;
        return new Vector3(basePos.x + r.x, basePos.y, basePos.z + r.y);
    }

    public void RegisterWaveEnemy(WaveMember m)
    {
        if (m == null) return;
        aliveWaveEnemies.Add(m);
    }

    public void UnregisterWaveEnemy(WaveMember m)
    {
        if (m == null) return;
        aliveWaveEnemies.Remove(m);
    }

    void HandlePlayerDied()
    {
        StartCoroutine(RestartRoutine());
    }

    IEnumerator RestartRoutine()
    {
        if (deathUI != null) deathUI.SetActive(true);

        yield return new WaitForSeconds(restartDelay);

        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= HandlePlayerDied;
    }
}