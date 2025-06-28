using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Menus:")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelSelectorMenu;

    [Space(10)]

    [SerializeField] private Button duskMountainButton;
    [SerializeField] private GameObject duskMountainInfo;
    [SerializeField] private GameObject duskMountainLockedOverlay;
    [SerializeField] private int duskMountainWaveRequirement = 25;

    [Space(10)]

    [SerializeField] private List<TMPro.TMP_Text> highestWavePerLevel = new();
    [SerializeField] private TMPro.TMP_Text highestWaveText;
    [SerializeField] private List<string> levelNames = new();

    private MainMenuInputActions mainMenuInputActions;
    private InputAction playAction;
    private InputAction exitAction;
    private InputAction selectLevel1Action;
    private InputAction selectLevel2Action;

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

        selectLevel1Action = mainMenuInputActions.UI.SelectLevel1;
        selectLevel1Action.Enable();

        selectLevel2Action = mainMenuInputActions.UI.SelectLevel2;
        selectLevel2Action.Enable();
    }

    private void OnDisable()
    {
        playAction.Disable();
        playAction.started -= OnPlayGame;

        exitAction.Disable();
        exitAction.started -= OnQuitGame;

        selectLevel1Action.Disable();
        selectLevel1Action.started -= OnSelectLevel1;

        selectLevel2Action.Disable();
        selectLevel2Action.started -= OnSelectLevel2;
    }

    private void Start()
    {
        // INFO: Load Highest Wave Count from Saving System
        GameData gameData = SavingSystem.LoadGameData();

        if (highestWaveText != null)
        {
            highestWaveText.text = "Highest Wave: " + gameData.highestWave.ToString();
        }

        // INFO: Initial saving of values to new GameData if it doesn't exist
        if (gameData.levelNames.Count == 0)
        {
            gameData.levelNames = levelNames;

            for (int i = 0; i < levelNames.Count; i++)
            {
                gameData.highestWavePerLevel.Add(0);
            }
        }

        SavingSystem.SaveGameData(gameData);

        // INFO: Set the wave requirement for Dusk Mountain
        if (duskMountainLockedOverlay != null)
        {
            TMPro.TMP_Text overlayText = duskMountainLockedOverlay.GetComponentInChildren<TMPro.TMP_Text>();

            if (overlayText != null)
            {
                overlayText.text = duskMountainWaveRequirement.ToString();
            }
        }
    }

    private void OnPlayGame(InputAction.CallbackContext context)
    {
        ShowLevelSelector();
    }

    private void OnQuitGame(InputAction.CallbackContext context)
    {
        QuitGame();
    }

    private void OnHideLevelSelector(InputAction.CallbackContext context)
    {
        HideLevelSelector();
    }

    private void OnSelectLevel1(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(1);
    }

    private void OnSelectLevel2(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(2);
    }

    public void ShowLevelSelector()
    {
        if (levelSelectorMenu != null && mainMenu != null)
        {
            mainMenu.SetActive(false);
            levelSelectorMenu.SetActive(true);
        }

        // INFO: Verify if Dusk Mountain is locked or not
        GameData gameData = SavingSystem.LoadGameData();

        if (duskMountainButton != null && duskMountainInfo != null && duskMountainLockedOverlay != null)
        {
            if (gameData.highestWave >= duskMountainWaveRequirement)
            {
                duskMountainButton.interactable = true;
                duskMountainInfo.SetActive(true);
                duskMountainLockedOverlay.SetActive(false);

                selectLevel2Action.started += OnSelectLevel2;
            }
            else
            {
                duskMountainButton.interactable = false;
                duskMountainInfo.SetActive(false);
                duskMountainLockedOverlay.SetActive(true);
            }
        }

        // INFO: Go through the list of highest waves per level and set the text accordingly
        if (highestWavePerLevel.Count != gameData.highestWavePerLevel.Count)
        {
            Debug.LogWarning("Mismatch between highestWavePerLevel UI elements and gameData.highestWavePerLevel count.");
            return;
        }

        for (int i = 0; i < highestWavePerLevel.Count; i++)
        {
            highestWavePerLevel[i].text = gameData.highestWavePerLevel[i].ToString();
        }

        // INFO: Switch action calls for gamepad
        playAction.started -= OnPlayGame;

        exitAction.started -= OnQuitGame;
        exitAction.started += OnHideLevelSelector;

        selectLevel1Action.started += OnSelectLevel1;
    }

    public void HideLevelSelector()
    {
        if (levelSelectorMenu != null && mainMenu != null)
        {
            levelSelectorMenu.SetActive(false);
            mainMenu.SetActive(true);
        }

        // INFO: Switch action calls for gamepad
        selectLevel1Action.started -= OnSelectLevel1;
        selectLevel2Action.started -= OnSelectLevel2;

        exitAction.started -= OnHideLevelSelector;
        exitAction.started += OnQuitGame;

        playAction.started += OnPlayGame;
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
