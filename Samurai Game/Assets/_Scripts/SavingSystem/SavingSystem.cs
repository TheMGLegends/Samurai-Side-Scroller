using System.IO;
using UnityEngine;

public static class SavingSystem
{
    private static readonly string saveDataPath = Application.persistentDataPath + "/gameData.json";

    public static bool debugPath = false;

    public static string GetSaveDataPath() => saveDataPath;

    public static void SaveGameData(GameData gameData)
    {
        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(saveDataPath, json);
    }

    public static GameData LoadGameData()
    {
        if (File.Exists(saveDataPath))
        {
#if UNITY_EDITOR
            if (debugPath)
            {
                Debug.Log(saveDataPath);
            }
#endif
            string json = File.ReadAllText(saveDataPath);
            return JsonUtility.FromJson<GameData>(json);
        }
        else
        {
            Debug.LogWarning("Save file not found. Returning default GameData.");
            return new GameData();
        }
    }
}
