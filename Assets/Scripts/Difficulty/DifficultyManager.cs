using System;
using UnityEngine;

public class DifficultyManager : Singleton<DifficultyManager>
{
    private GameObject _difficultySettings;
    private DifficultySettings _difficultySetting;
    private static DifficultyLevel _selectedDifficulty;

    public string _difficulty;

    public override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadDifficultySettings()
    {
        if (_difficultySettings == null)
        {
            // Initialize StaticDifficulty if it is null
            _difficultySettings = new GameObject("DifficultyReader");
            _difficultySettings.AddComponent<DifficultyReader>();
        }

        _difficultySetting = _difficultySettings.GetComponent<DifficultyReader>().Settings;
    }

    public void SetDifficulty(string difficulty)
    {
        _difficulty = difficulty;

        switch (difficulty.ToLower())
        {
            case "easy":

                _selectedDifficulty = _difficultySetting.Easy;
                Debug.Log("Easy");
                break;
            case "normal":

                _selectedDifficulty = _difficultySetting.Normal;
                Debug.Log("Normal");
                break;
            case "hard":

                _selectedDifficulty = _difficultySetting.Hard;
                Debug.Log("Hard");
                break;
            case "dynamic":

                _selectedDifficulty = _difficultySetting.Dynamic;
                Debug.Log("Dynamic");
                break;
            default:
                throw new System.ArgumentException($"Invalid difficulty level: {difficulty}");
        }
    }

    public DifficultyLevel GetDifficultyLevel()
    {
        if (_selectedDifficulty == null)
        {
            throw new System.InvalidOperationException("DifficultySetting level has not been set.");
        }
        return _selectedDifficulty;
    }

    public DifficultySettings DifficultySetting
    {
        get
        {
            return _difficultySetting;
        }
    }
}
