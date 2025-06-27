using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public int highestWave;
    public List<string> levelNames;
    public List<int> highestWavePerLevel;

    public GameData()
    {
        highestWave = 0;
        levelNames = new List<string>();
        highestWavePerLevel = new List<int>();
    }
}
