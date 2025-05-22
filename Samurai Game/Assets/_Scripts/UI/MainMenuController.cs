using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text highestWaveText;

    private MainMenuInputActions mainMenuInputActions;
    private InputAction playAction;
    private InputAction exitAction;

    private void Awake()
    {
        mainMenuInputActions = new MainMenuInputActions();
    }

    private void OnEnable()
    {
        playAction = mainMenuInputActions.UI.Play;
        playAction.Enable();
        playAction.started += OnPlayGame;

        exitAction = mainMenuInputActions.UI.Quit;
        exitAction.Enable();
        exitAction.started += OnQuitGame;
    }

    private void OnDisable()
    {
        playAction.Disable();
        playAction.started -= OnPlayGame;

        exitAction.Disable();
        exitAction.started -= OnQuitGame;
    }

    private void Start()
    {
        // INFO: Load Highest Wave Count from Saving System
        GameData gameData = SavingSystem.LoadGameData();

        if (highestWaveText != null)
        {
            highestWaveText.text = "Highest Wave: " + gameData.highestWave.ToString();
        }
    }

    private void OnPlayGame(InputAction.CallbackContext context)
    {
        // INFO: Index 1 is the game scene
        SceneManager.LoadScene(1);
    }

    private void OnQuitGame(InputAction.CallbackContext context)
    {
        QuitGame();
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
