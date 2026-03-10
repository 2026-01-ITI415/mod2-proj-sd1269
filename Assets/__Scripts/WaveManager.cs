using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class WaveData
    {
        public string waveName = "Wave";
        public GameObject[] enemyPrefabs;
        public int enemyCount = 5;
        public float spawnRate = 1f;
    }

    [Header("Wave Setup")]
    public List<WaveData> waves = new List<WaveData>();
    public float timeBetweenWaves = 3f;
    public bool autoStart = true;

    [Header("UI")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI messageText;
    public float messageDuration = 2f;

    private int currentWaveIndex = -1;
    private int aliveEnemies = 0;
    private bool isSpawningWave = false;

    void Start()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }

        if (autoStart)
        {
            StartNextWave();
        }
    }

    public void StartNextWave()
    {
        currentWaveIndex++;

        if (currentWaveIndex >= waves.Count)
        {
            AllWavesComplete();
            return;
        }

        StartCoroutine(RunWave(waves[currentWaveIndex]));
    }

    private IEnumerator RunWave(WaveData wave)
    {
        isSpawningWave = true;

        if (waveText != null)
        {
            waveText.text = "Wave " + (currentWaveIndex + 1);
        }

        yield return StartCoroutine(ShowMessage("Wave " + (currentWaveIndex + 1), messageDuration));

        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemyForWave(wave);

            float delay = 1f;
            if (wave.spawnRate > 0f)
            {
                delay = 1f / wave.spawnRate;
            }

            yield return new WaitForSeconds(delay);
        }

        isSpawningWave = false;

        while (aliveEnemies > 0)
        {
            yield return null;
        }

        yield return StartCoroutine(ShowMessage("Wave Cleared!", messageDuration));
        yield return new WaitForSeconds(timeBetweenWaves);

        StartNextWave();
    }

    private void SpawnEnemyForWave(WaveData wave)
    {
        if (wave.enemyPrefabs == null || wave.enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("WaveManager: No enemy prefabs assigned for this wave.");
            return;
        }

        Main main = Main.Get();
        if (main == null)
        {
            Debug.LogWarning("WaveManager: Main singleton not found.");
            return;
        }

        GameObject prefab = wave.enemyPrefabs[Random.Range(0, wave.enemyPrefabs.Length)];
        GameObject spawnedEnemy = main.SpawnEnemyFromWave(prefab);

        if (spawnedEnemy != null)
        {
            aliveEnemies++;

            EnemyWaveTracker tracker = spawnedEnemy.GetComponent<EnemyWaveTracker>();
            if (tracker == null)
            {
                tracker = spawnedEnemy.AddComponent<EnemyWaveTracker>();
            }

            tracker.SetWaveManager(this);
        }
    }

    public void NotifyEnemyDestroyed()
    {
        aliveEnemies--;

        if (aliveEnemies < 0)
        {
            aliveEnemies = 0;
        }
    }

    private IEnumerator ShowMessage(string msg, float duration)
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = msg;
            yield return new WaitForSeconds(duration);
            messageText.gameObject.SetActive(false);
        }
        else
        {
            yield return null;
        }
    }

    private void AllWavesComplete()
    {
        Debug.Log("WaveManager: All waves complete!");

        if (waveText != null)
        {
            waveText.text = "All Waves Complete!";
        }

        StartCoroutine(ShowMessage("Victory!", messageDuration));
    }

    public int GetAliveEnemyCount()
    {
        return aliveEnemies;
    }

    public int GetCurrentWaveNumber()
    {
        return currentWaveIndex + 1;
    }

    public bool IsSpawningWave()
    {
        return isSpawningWave;
    }
}