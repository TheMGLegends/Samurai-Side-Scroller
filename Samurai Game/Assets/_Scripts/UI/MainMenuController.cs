using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text highestWaveText;

    private void Start()
    {
        // INFO: Load Highest Wave Count from Saving System
        GameData gameData = SavingSystem.LoadGameData();

        if (highestWaveText != null)
        {
            highestWaveText.text = "Highest Wave: " + gameData.highestWave.ToString();
        }
    }

    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
