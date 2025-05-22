[System.Serializable]
public class GameData
{
    public int highestWave;

    public GameData()
    {
        highestWave = 0;
    }

    public GameData(int highestWave)
    {
        this.highestWave = highestWave;
    }
}
