using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


[System.Serializable]
public class PlayerReport
{
    public string PlayerName;
    public string ActiveTimePlayed; // in minutes and seconds
    public List<DifficultyReport> DifficultiesPlayed;
}

[System.Serializable]
public class DifficultyReport
{
    public string Difficulty;
    public List<PerformanceReport> PerformanceReports;
}

[System.Serializable]
public class PerformanceReport
{
    public float Accuracy;
    public float DamageTaken;
    public string Comment;
}

public class PlayerReportData : SingletonNetwork<PlayerReportData>
{
    public string playerName;
    public List<DifficultyReport> difficultiesPlayed = new List<DifficultyReport>();


    private void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    public void SaveReportToJson()
    {
        // Get the active playtime
        string activeTimePlayed = GameplayManager.Instance.GetActivePlayTimeInMinutes();

        PlayerReport report = new PlayerReport
        {
            PlayerName = playerName,
            ActiveTimePlayed = activeTimePlayed,
            DifficultiesPlayed = difficultiesPlayed
        };

        string json = JsonUtility.ToJson(report, true);
        string folderPath = Path.Combine(Application.persistentDataPath, "PlayerReports");


        // Create the folder if it doesn't exist
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "PlayerReport.json");
        File.WriteAllText(filePath, json);

        Debug.Log("Report saved to " + filePath);

        // Send the JSON file via email
        DataMailer.Instance.SendEmail(filePath);
    }

    public void AddDifficultyReport(DifficultyReport difficultyReport)
    {
        difficultiesPlayed.Add(difficultyReport);
    }
}
