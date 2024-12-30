using UnityEngine;


public class DifficultyReader : Singleton<DifficultyReader>
{
    private DifficultySettings _settings;


    public override void Awake()
    {
        base.Awake();
        LoadDifficultySettings();
    }

    private void LoadDifficultySettings()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("difficultySettings");
        if (jsonFile != null)
        {
            _settings = JsonUtility.FromJson<DifficultySettings>(jsonFile.text);
            Debug.Log("DifficultySetting settings loaded successfully!");
        }
        else
        {
            Debug.LogError("DifficultySetting settings file not found!");
        }
    }


    public DifficultySettings Settings
    {
        get { return _settings; }
    }
}
