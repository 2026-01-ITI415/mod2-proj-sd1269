using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(BoundsCheck))]
public class Main : MonoBehaviour
{
    static private Main S;
    static private Dictionary<eWeaponType, WeaponDefinition> WEAP_DICT;

    [Header("Inscribed")]
    public bool spawnEnemies = true;
    public GameObject[] prefabEnemies;
    public float enemySpawnPerSecond = 0.5f;
    public float enemyInsetDefault = 1.5f;
    public float gameRestartDelay = 2.0f;
    public GameObject prefabPowerUp;
    public WeaponDefinition[] weaponDefinitions;
    public eWeaponType[] powerUpFrequency = new eWeaponType[] {
        eWeaponType.blaster, eWeaponType.blaster,
        eWeaponType.spread,
        eWeaponType.shield,
        eWeaponType.phaser,
        eWeaponType.missile,
        eWeaponType.laser
    };
    [Header("Score")]
    public int score = 0;
    public Text scoreText;

    private BoundsCheck bndCheck;

    void Awake()
    {
        S = this;
        bndCheck = GetComponent<BoundsCheck>();

        WEAP_DICT = new Dictionary<eWeaponType, WeaponDefinition>();
        foreach (WeaponDefinition def in weaponDefinitions)
        {
            WEAP_DICT[def.type] = def;
        }
        ResetScore();
    }

    public GameObject SpawnEnemyFromWave(GameObject enemyPrefab)
    {
        if (!spawnEnemies || enemyPrefab == null)
        {
            return null;
        }

        GameObject go = Instantiate(enemyPrefab);

        float enemyInset = enemyInsetDefault;
        BoundsCheck enemyBndCheck = go.GetComponent<BoundsCheck>();
        if (enemyBndCheck != null)
        {
            enemyInset = Mathf.Abs(enemyBndCheck.radius);
        }

        Vector3 pos = Vector3.zero;
        float xMin = -bndCheck.camWidth + enemyInset;
        float xMax = bndCheck.camWidth - enemyInset;
        pos.x = Random.Range(xMin, xMax);
        pos.y = bndCheck.camHeight + enemyInset;
        go.transform.position = pos;

        return go;
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    public GameObject SpawnRandomEnemy()
    {
        if (prefabEnemies == null || prefabEnemies.Length == 0)
        {
            Debug.LogWarning("Main: No enemy prefabs assigned.");
            return null;
        }

        int ndx = Random.Range(0, prefabEnemies.Length);
        return SpawnEnemyFromWave(prefabEnemies[ndx]);
    }

    void DelayedRestart()
    {
        Invoke(nameof(Restart), gameRestartDelay);
    }

    void Restart()
    {
        SceneManager.LoadScene("__Scene_0");
    }

    static public void HERO_DIED()
    {
        S.DelayedRestart();
    }

    static public WeaponDefinition GET_WEAPON_DEFINITION(eWeaponType wt)
    {
        if (WEAP_DICT.ContainsKey(wt))
        {
            return WEAP_DICT[wt];
        }
        return new WeaponDefinition();
    }

    static public void SHIP_DESTROYED(Enemy e)
    {
        if (Random.value <= e.powerUpDropChance)
        {
            int ndx = Random.Range(0, S.powerUpFrequency.Length);
            eWeaponType pUpType = S.powerUpFrequency[ndx];

            GameObject go = Instantiate(S.prefabPowerUp);
            PowerUp pUp = go.GetComponent<PowerUp>();
            pUp.SetType(pUpType);
            pUp.transform.position = e.transform.position;
        }
    }

    public static Main Get()
    {
        return S;
    }
}