using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    public GameObject spaceGhostEnemyPrefabToSpawn;
    public GameObject spaceShooterEnemyPrefabToSpawn;

    [Header("Boss")]
    [SerializeField]
    BossUI m_bossUI;

    [SerializeField]
    private GameObject m_bossPrefabToSpawn;

    [SerializeField]
    private Transform m_bossPosition;

    [Header("Meteors")]
    [SerializeField]
    private GameObject m_meteorPrefab;

    [SerializeField]
    AudioClip m_warningClip;

    [SerializeField]
    GameObject m_warningUI;

    private Vector3 m_CurrentNewEnemyPosition = new Vector3();
    private Vector3 m_CurrentNewMeteorPosition = new Vector3();
    private float m_CurrentEnemySpawnTime = 0f;
    private float m_CurrentMeteorSpawnTime = 0f;
    private bool m_IsSpawning = true;

    private Wave[] waves;
    private int currentWaveIndex = 0;
    private int currentGhostCount = 0;
    private int currentShooterCount = 0;
    private int currentMeteorCount = 0;




    private void Start()
    {
        // Initialize spawn positions
        m_CurrentNewEnemyPosition.x = transform.position.x;
        m_CurrentNewEnemyPosition.z = 0f;

        m_CurrentNewMeteorPosition.x = transform.position.x;
        m_CurrentNewMeteorPosition.z = 0f;

        // Load wave data from DifficultyManager
        if (DifficultyManager.Instance.DifficultySetting.ToString() != "Dynamic")
        {
            waves = DifficultyManager.Instance.GetDifficultyLevel().Waves;
        }
        else
        {
            waves = DDAController.Instance._currentDifficulty.Waves;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!(IsServer && m_IsSpawning))
            return;

        if (currentWaveIndex < waves.Length)
        {
            UpdateEnemySpawning();
            UpdateMeteorSpawning();
        }
    }


    private void StartWave()
    {
        if (currentWaveIndex >= waves.Length)
        {
            m_IsSpawning = false;
            return;
        }

        var currentWave = waves[currentWaveIndex];
        currentGhostCount = currentWave.Ghost;
        currentShooterCount = currentWave.Shooter;
        currentMeteorCount = currentWave.Meteor;

        Debug.Log($"Spawning Wave {currentWaveIndex}");

        if (currentWave.Boss)
        {
            StartCoroutine(BossAppear());
        }
    }



    // Spawning enemies
    private void UpdateEnemySpawning()
    {
        m_CurrentEnemySpawnTime += Time.deltaTime;

        if (m_CurrentEnemySpawnTime >= 1.0f) // Spawn interval for enemies
        {
            if (currentGhostCount > 0)
            {
                SpawnEnemy(spaceGhostEnemyPrefabToSpawn);
                currentGhostCount--;
            }
            
            if (currentShooterCount > 0)
            {
                SpawnEnemy(spaceShooterEnemyPrefabToSpawn);
                currentShooterCount--;
            }

            m_CurrentEnemySpawnTime = 0f;

            if (currentGhostCount == 0 && currentShooterCount == 0 && currentMeteorCount == 0)
            {
                StartWave();
                ++currentWaveIndex;
            }
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        m_CurrentNewEnemyPosition.y = UnityEngine.Random.Range(-5f, 5f);
        NetworkObjectSpawner.SpawnNewNetworkObject(enemyPrefab, m_CurrentNewEnemyPosition);
    }



    // Spawning meteors
    private void UpdateMeteorSpawning()
    {
        m_CurrentMeteorSpawnTime += Time.deltaTime;

        if (m_CurrentMeteorSpawnTime >= 2.5f && currentMeteorCount > 0) // Spawn interval for meteors
        {
            SpawnMeteor();
            currentMeteorCount--;
            m_CurrentMeteorSpawnTime = 0f;
        }
    }

    private void SpawnMeteor()
    {
        m_CurrentNewMeteorPosition.y = UnityEngine.Random.Range(-5f, 6f);
        NetworkObjectSpawner.SpawnNewNetworkObject(m_meteorPrefab, m_CurrentNewMeteorPosition);
    }

    

    // Spawning the boss
    private IEnumerator BossAppear()
    {
        m_IsSpawning = false;

        // Warning title and sound
        PlayWarnnigClientRpc();
        m_warningUI.SetActive(true);
        AudioManager.Instance.PlaySoundEffect(m_warningClip);

        yield return new WaitForSeconds(m_warningClip.length);

        StopWarnnigClientRpc();
        m_warningUI.SetActive(false);

        GameObject boss = NetworkObjectSpawner.SpawnNewNetworkObject(m_bossPrefabToSpawn, transform.position);

        BossController bossController = boss.GetComponent<BossController>();
        bossController.StartBoss(m_bossPosition.position);
        bossController.SetUI(m_bossUI);
        boss.name = "BOSS";

        m_IsSpawning = true;
    }

    [ClientRpc]
    void PlayWarnnigClientRpc()
    {
        if (IsServer)
            return;

        m_warningUI.SetActive(true);

        AudioManager.Instance.PlaySoundEffect(m_warningClip);
    }

    [ClientRpc]
    void StopWarnnigClientRpc()
    {
        if (IsServer)
            return;

        m_warningUI.SetActive(false);
    }
}
