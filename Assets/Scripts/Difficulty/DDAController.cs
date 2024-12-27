using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDAController : Singleton<DDAController>
{
    private int _shotsFired;
    private int _shotsHit;

    private int _enemiesDefeated;
    public int EnemiesDefeated
    {
        get
        {
            return _enemiesDefeated;
        }
    }

    private int _damageTaken;
    public int DamageTaken
    {
        get
        {
            return _damageTaken;
        }
    }

    private DifficultySettings _settings;
    private DifficultyLevel _currentDifficulty;

    private int starthealth;


    // Singleton initialize
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Load the dynamic difficulty level at the start
        _settings = DifficultyManager.Instance.Difficulty;
        _currentDifficulty = DifficultyManager.Instance.GetDifficultyLevel();

        starthealth = _currentDifficulty.PlayerHealth;
    }


    // Shooting accuracy
    public void RecordPlayerShoot()
    {
        ++_shotsFired;

        AdjustDifficulty();
    }
    public void RecordEnemyHit()
    {
        ++_shotsHit;

        AdjustDifficulty();
    }
    public void RecordEnemyDefeated()
    {
        ++_enemiesDefeated;

        AdjustDifficulty();
    }
    public float GetAccuracy()
    {
        return _shotsFired > 0 ? (float)_shotsHit / _shotsFired : 0f;
    }


    // Dodging accuracy
    public void RecordPlayerHit(int damage)
    {
        _damageTaken += damage;

        AdjustDifficulty();
    }
    private float GetDamageTakenPercentage()
    {
        return starthealth > 0 ? (float)_damageTaken / starthealth : 0f;
    }


    // TODO: look into making this all beter
    private void AdjustDifficulty()
    {
        float accuracy = GetAccuracy();
        float damageTakenPercentage = GetDamageTakenPercentage();

        // Adjust difficulty based on player performance
        if (accuracy > 0.8f && damageTakenPercentage < 0.1f)
        {
            IncreaseDifficulty();
        }
        else if (accuracy < 0.5f && damageTakenPercentage > 0.2f)
        {
            DecreaseDifficulty();
        }
        else
        {
            MaintainDifficulty();
        }
    }

    private void IncreaseDifficulty()
    {
        Debug.Log("Increasing difficulty");
        _currentDifficulty.PlayerHealth = Mathf.Max(_currentDifficulty.PlayerHealth - 1, 1); // Decrease player health
        starthealth = _currentDifficulty.PlayerHealth; // Update start health
    }

    private void DecreaseDifficulty()
    {
        Debug.Log("Decreasing difficulty");
        _currentDifficulty.PlayerHealth = Mathf.Min(_currentDifficulty.PlayerHealth + 1, 15); // Increase player health
        starthealth = _currentDifficulty.PlayerHealth; // Update start health
    }

    private void MaintainDifficulty()
    {
        Debug.Log("Maintaining current difficulty");
        // Implement logic to maintain current difficulty
    }


    public void DestroyDDAController()
    {
        Destroy(gameObject);
    }
}
