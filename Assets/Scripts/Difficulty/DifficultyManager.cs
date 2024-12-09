using UnityEngine;

public class DifficultyManager : Singleton<DifficultyManager>
{
    private GameObject _difficultySettings;
    private DifficultySettings _difficulty;
    private static DifficultyLevel _selectedDifficulty;

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

        _difficulty = _difficultySettings.GetComponent<DifficultyReader>().Settings;
    }

    public void SetDifficulty(string difficulty)
    {
        switch (difficulty.ToLower())
        {
            case "easy":

                _selectedDifficulty = _difficulty.Easy;
                Debug.Log("Easy");
                break;
            case "normal":

                _selectedDifficulty = _difficulty.Normal;
                Debug.Log("Normal");
                break;
            case "hard":

                _selectedDifficulty = _difficulty.Hard;
                Debug.Log("Hard");
                break;
            default:
                throw new System.ArgumentException($"Invalid difficulty level: {difficulty}");
        }
    }

    public DifficultyLevel GetDifficultyLevel()
    {
        if (_selectedDifficulty == null)
        {
            throw new System.InvalidOperationException("Difficulty level has not been set.");
        }
        return _selectedDifficulty;
    }
}
