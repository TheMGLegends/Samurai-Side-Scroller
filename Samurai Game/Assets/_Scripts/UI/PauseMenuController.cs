using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private List<GameObject> pauseMenuObjects = new();

    private HUDInputActions hudInputActions;
    private InputAction pauseAction;
    private InputAction restartAction;
    private InputAction exitAction;

    private PlayerCharacter playerCharacter;
    private WaveManager waveManager;

    private bool isPaused = false;

    private void Awake()
    {
        hudInputActions = new HUDInputActions();
    }

    private void Start()
    {
        playerCharacter = FindFirstObjectByType<PlayerCharacter>();
        waveManager = FindFirstObjectByType<WaveManager>();

        // INFO: Disable all pause menu objects at the start
        foreach (GameObject button in pauseMenuObjects)
        {
            button.SetActive(false);
        }
    }

    private void OnEnable()
    {
        pauseAction = hudInputActions.UI.Pause;
        pauseAction.Enable();
        pauseAction.started += OnPaused;

        restartAction = hudInputActions.UI.Restart;
        restartAction.Enable();
        restartAction.started += OnRestartLevel;

        exitAction = hudInputActions.UI.Exit;
        exitAction.Enable();
        exitAction.started += OnExitLevel;
    }

    private void OnDisable()
    {
        pauseAction.Disable();
        pauseAction.started -= OnPaused;

        restartAction.Disable();
        restartAction.started -= OnRestartLevel;

        exitAction.Disable();
        exitAction.started -= OnExitLevel;
    }

    private void OnPaused(InputAction.CallbackContext context)
    {
        AudioManager.Instance.PlaySFX("Click");

        if (isPaused)
        {
            isPaused = false;

            foreach (GameObject button in pauseMenuObjects)
            {
                button.SetActive(false);
            }

            playerCharacter.PlayerInputActions.Player.Enable();
            Time.timeScale = 1.0f;
        }
        else
        {
            isPaused = true;

            foreach (GameObject button in pauseMenuObjects)
            {
                button.SetActive(true);
            }

            playerCharacter.PlayerInputActions.Player.Disable();
            Time.timeScale = 0.0f;
        }
    }

    private void OnRestartLevel(InputAction.CallbackContext context)
    {
        if (isPaused) { RestartLevel(); }
    }

    private void OnExitLevel(InputAction.CallbackContext context)
    {
        if (isPaused)
        {
            if (waveManager) { waveManager.SaveHighestWave(); }

            // INFO: Index 0 is the main menu
            SceneManager.LoadScene(0);
            Time.timeScale = 1.0f;
        }
    }

    public void ResumeGame()
    {
        isPaused = false;

        foreach (GameObject button in pauseMenuObjects)
        {
            button.SetActive(false);
        }

        playerCharacter.PlayerInputActions.Player.Enable();
        Time.timeScale = 1.0f;
    }

    public void RestartLevel()
    {
        if (waveManager) { waveManager.SaveHighestWave(); }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1.0f;
    }

    public void LoadLevel(string levelName)
    {
        if (waveManager) { waveManager.SaveHighestWave(); }

        SceneManager.LoadScene(levelName);
        Time.timeScale = 1.0f;
    }
}
