using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDAController : Singleton<DDAController>
{
    // Player stats
    private int _shotsFired;
    private int _shotsHit;

    private int _damageTaken;
    public int DamageTaken
    {
        get
        {
            return _damageTaken;
        }
    }

    private int _startingPlayerHealth;


    // Enemy stats
    private int _enemiesDefeated;
    public int EnemiesDefeated
    {
        get
        {
            return _enemiesDefeated;
        }
    }


    // General difficulty settings
    private DifficultySettings _settings;
    public DifficultyLevel _currentDifficulty;


    // Adjustment times
    private float _adjustmentInterval = 10f; // Adjust difficulty every 10 seconds
    private float _nextAdjustmentTime;
    private float _adjustmentTimer;


    // Singleton initialize
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Load the dynamic difficulty level at the start
        _settings = DifficultyManager.Instance.DifficultySetting;
        _currentDifficulty = DifficultyManager.Instance.GetDifficultyLevel();

        _startingPlayerHealth = _currentDifficulty.PlayerHealth;
        _nextAdjustmentTime = Time.deltaTime + _adjustmentInterval;
    }


    // Shooting accuracy
    public void RecordShotFired()
    {
        ++_shotsFired;
    }
    public void RecordShotHit()
    {
        ++_shotsHit;
    }
    public void RecordEnemyDefeated()
    {
        ++_enemiesDefeated;
    }
    public float GetAccuracy()
    {
        // TODO: Implement logic to calculate player accuracy based on shots fired and shots hit and enemies killd
        return _shotsFired > 0 ? (float)_shotsHit / _shotsFired : 0f;
    }


    // Dodging accuracy
    public void RecordPlayerHit(int damage)
    {
        _damageTaken += damage;
    }
    private float GetDamageTakenPercentage()
    {
        return _startingPlayerHealth > 0 ? (float)_damageTaken / _startingPlayerHealth : 0f;
    }


    private void Update()
    {
        // Accumulate time since last adjustment
        _adjustmentTimer += Time.deltaTime;

        if (_adjustmentTimer >= _adjustmentInterval)
        {
            AdjustDifficulty();
            _adjustmentTimer = 0f; // Reset the timer
        }
    }



    private void AdjustDifficulty()
    {
        float accuracy = GetAccuracy();
        float damageTakenPercentage = GetDamageTakenPercentage();

        Debug.Log($"Adjusting difficulty... Accuracy: {accuracy}, Damage Taken: {damageTakenPercentage}");



        // Adjust difficulty based on player metrics
        if (accuracy > 0.8f && damageTakenPercentage < 0.1f)
        {
            IncreaseDifficulty();
        }
        else if (accuracy < 0.5f || damageTakenPercentage > 0.3f)
        {
            DecreaseDifficulty();
        }
        else
        {
            MaintainDifficulty();
        }


        // Reset counters to focus on recent performance
        _shotsFired = 0;
        _shotsHit = 0;
        _damageTaken = 0;
    }

    private void IncreaseDifficulty()
    {
        // Enemies / boss
        EnemyAdjustgments(1, 5, 1, 6);
        BossAdjustments(1, 25);

        // Meteor
        MeteorAdjustments(1, 6);

        // Spawning
        WaveAdjustments(1, 10, 1, 5, 1, 10);


        // Player
        _startingPlayerHealth = _currentDifficulty.PlayerHealth;
    }

    private void DecreaseDifficulty()
    {
        // Enemies / boss
        EnemyAdjustgments(-1, 1, -1, 2);
        BossAdjustments(-1, 15);

        // Meteor
        MeteorAdjustments(-1, 2);

        // Spawning
        WaveAdjustments(-1, 2,-1, 0, -1, 2);


        // Player
        _startingPlayerHealth = _currentDifficulty.PlayerHealth;
    }

    private void MaintainDifficulty()
    {
        // No adjustments needed
    }


    // Adjustment helpers
    private void EnemyAdjustgments(int minHealthShooter, int maxHealthShooter, int minHealthGhost, int maxhealthGhost)
    {
        // Ghost health
        _currentDifficulty.GhostHealth = Mathf.Clamp(
            _currentDifficulty.GhostHealth + minHealthGhost,
            minHealthGhost,
            maxhealthGhost
        );

        // Shooter health
        _currentDifficulty.ShooterHealth = Mathf.Clamp(
            _currentDifficulty.ShooterHealth + minHealthShooter,
            minHealthShooter,
            maxHealthShooter
        );
    }

    private void BossAdjustments(int minHealth, int maxHealth)
    {
        _currentDifficulty.BossHealth = Mathf.Clamp(
            _currentDifficulty.BossHealth + minHealth,
            minHealth,
            maxHealth
        );
    }

    private void MeteorAdjustments(int minHealth, int maxHealth)
    {
        _currentDifficulty.MeteoritesHealth = Mathf.Clamp(
            _currentDifficulty.MeteoritesHealth + minHealth,
            minHealth,
            maxHealth
        );
    }


    private void WaveAdjustments(int minGhostSpawnAmount, int maxGhostSpawnAmount, int minShootSpawnAmount, int maxShootSpawnAmount, int minMeteorSpawnAmount, int maxMeteorSpawnAmount)
    {
        EnemySpawner spawner = FindObjectOfType(typeof(EnemySpawner)) as EnemySpawner;
        int index = spawner.CurrentWaveIndex;

        if (index > 5)
        {
            return;
        }


        Wave wave = _currentDifficulty.Waves[index];


        wave.Ghost = Mathf.Clamp(wave.Ghost + minGhostSpawnAmount, 0, maxGhostSpawnAmount);
        wave.Shooter = Mathf.Clamp(wave.Shooter + minShootSpawnAmount, 0, maxShootSpawnAmount);
        wave.Meteor = Mathf.Clamp(wave.Meteor + minMeteorSpawnAmount, 0, maxMeteorSpawnAmount);


        Debug.Log($"Spawning Wave {index}: Ghosts={wave.Ghost}, Shooters={wave.Shooter}, Meteors={wave.Meteor}");
    }


    public void DestroyDDAController()
    {
        Destroy(gameObject);
    }

}
