
[System.Serializable]
public class DifficultySettings
{
    public DifficultyLevel Easy;
    public DifficultyLevel Normal;
    public DifficultyLevel Hard;
    public DifficultyLevel Dynamic;
}

[System.Serializable]
public class DifficultyLevel
{
    public int PlayerHealth;
    public int GhostHealth;
    public int ShooterHealth;
    public int BossHealth;
    public int MeteoritesHealth;
    public Wave[] Waves;
}

[System.Serializable]
public class Wave
{
    public int WaveNumber;
    public int Shooter;
    public int Ghost;
    public int Meteor;
    public bool Boss;
}