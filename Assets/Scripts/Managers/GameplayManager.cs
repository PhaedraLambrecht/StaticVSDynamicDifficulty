using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameplayManager : SingletonNetwork<GameplayManager>
{
    public static Action<ulong> OnPlayerDefeated;

    [SerializeField]
    private CharacterDataSO[] m_charactersData;

    [SerializeField]
    private PlayerUI[] m_playersUI;

    [SerializeField]
    private GameObject m_deathUI;

    [SerializeField]
    private Transform[] m_shipStartingPositions;

    private int m_numberOfPlayerConnected;
    private List<ulong> m_connectedClients = new List<ulong>();
    private List<PlayerShipController> m_playerShips = new List<PlayerShipController>();

    private float activePlayTime;
    private bool isPlaying;

    private string currentDifficulty;
    private List<PerformanceReport> currentPerformanceReports;


    // Time played
    private void Update()
    {
        if (isPlaying)
        {
            activePlayTime += Time.deltaTime;
        }
    }

    public void StartPlaying()
    {
        isPlaying = true;

        currentDifficulty = DifficultyManager.Instance._difficulty;
        currentPerformanceReports = new List<PerformanceReport>();

        GameObject playerReport = new GameObject("playerReport");
        PlayerReportData ataMailer = playerReport.AddComponent<PlayerReportData>();
    }

    public void StopPlaying()
    {
        isPlaying = false;
    }

    public string GetActivePlayTimeInMinutes()
    {
        int minutes = Mathf.FloorToInt(activePlayTime / 60);
        int seconds = Mathf.FloorToInt(activePlayTime % 60);
        return $"{minutes} minutes and {seconds} seconds";
    }

    public void AddPerformanceReport(float accuracy, float damageTaken, string comment)
    {
        currentPerformanceReports.Add(new PerformanceReport
        {
            Accuracy = accuracy,
            DamageTaken = damageTaken,
            Comment = comment
        });
    }

    public DifficultyReport FinalizeCurrentDifficulty()
    {
        DifficultyReport difficultyReport = new DifficultyReport
        {
            Difficulty = currentDifficulty,
            PerformanceReports = new List<PerformanceReport>(currentPerformanceReports)
        };
        currentDifficulty = null;
        currentPerformanceReports.Clear();
        return difficultyReport;
    }




    private void OnEnable()
    {
        if (!IsServer)
            return;

        OnPlayerDefeated += PlayerDeath;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnDisable()
    {
        if (!IsServer)
            return;

        OnPlayerDefeated -= PlayerDeath;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    public void PlayerDeath(ulong clientId)
    {
        StopPlaying();
        PlayerReportData.Instance.AddDifficultyReport(FinalizeCurrentDifficulty());

        m_numberOfPlayerConnected--;

        if (m_numberOfPlayerConnected <= 0)
        {
            LoadClientRpc();
            LoadingSceneManager.Instance.LoadScene(SceneName.Defeat);
        }
        else
        {
            ActivateDeathUIClientRpc(clientId);
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        foreach (var player in m_playerShips)
        {
            if (player != null && player.characterData.clientId == clientId)
            {
                player.Hit(999); // Do critical damage
            }
        }
    }

    [ClientRpc]
    private void ActivateDeathUIClientRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            m_deathUI.SetActive(true);
        }
    }

    [ClientRpc]
    private void LoadClientRpc()
    {
        if (IsServer)
            return;

        LoadingFadeEffect.Instance.FadeAll();
    }

    [ClientRpc]
    private void SetPlayerUIClientRpc(int charIndex, string playerShipName)
    {
        GameObject playerSpaceship = GameObject.Find(playerShipName);

        PlayerShipController playerShipController =
            playerSpaceship.GetComponent<PlayerShipController>();

        m_playersUI[m_charactersData[charIndex].playerId].SetUI(
            m_charactersData[charIndex].playerId,
            m_charactersData[charIndex].iconSprite,
            m_charactersData[charIndex].iconDeathSprite,
            playerShipController.health.Value,
            m_charactersData[charIndex].darkColor);

        playerShipController.playerUI = m_playersUI[m_charactersData[charIndex].playerId];
    }

    private IEnumerator HostShutdown()
    {
        ShutdownClientRpc();
        yield return new WaitForSeconds(0.5f);
        Shutdown();
    }

    private void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
        LoadingSceneManager.Instance.LoadScene(SceneName.Menu, false);
    }

    [ClientRpc]
    private void ShutdownClientRpc()
    {
        if (IsServer)
            return;

        Shutdown();
    }

    public void BossDefeat()
    {
        StopPlaying();
        PlayerReportData.Instance.AddDifficultyReport(FinalizeCurrentDifficulty());

        LoadClientRpc();
        LoadingSceneManager.Instance.LoadScene(SceneName.Victory);
    }

    public void ExitToMenu()
    {
        PlayerReportData.Instance.SaveReportToJson();

        if (IsServer)
        {
            StartCoroutine(HostShutdown());
        }
        else
        {
            NetworkManager.Singleton.Shutdown();
            LoadingSceneManager.Instance.LoadScene(SceneName.Menu, false);
        }
    }

    public void ServerSceneInit(ulong clientId)
    {
        StartPlaying();

        m_connectedClients.Add(clientId);

        if (m_connectedClients.Count < NetworkManager.Singleton.ConnectedClients.Count)
            return;

        foreach (var client in m_connectedClients)
        {
            int index = 0;

            foreach (CharacterDataSO data in m_charactersData)
            {
                if (data.isSelected && data.clientId == client)
                {
                    GameObject playerSpaceship =
                        NetworkObjectSpawner.SpawnNewNetworkObjectAsPlayerObject(
                            data.spaceshipPrefab,
                            m_shipStartingPositions[m_numberOfPlayerConnected].position,
                            data.clientId,
                            true);

                    PlayerShipController playerShipController =
                        playerSpaceship.GetComponent<PlayerShipController>();
                    playerShipController.characterData = data;
                    playerShipController.gameplayManager = this;

                    m_playerShips.Add(playerShipController);
                    SetPlayerUIClientRpc(index, playerSpaceship.name);

                    m_numberOfPlayerConnected++;
                }

                index++;
            }
        }
    }
}
