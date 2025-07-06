using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ActionType
{
    MoveLeft = 0,
    MoveRight,
    Jump,
    Attack,
    Interact,
    Dash,

    None
}

public struct KeybindingData
{
    public TMPro.TMP_Text textComponent;
    public string previousText;

    public KeybindingData(TMPro.TMP_Text _text)
    {
        textComponent = _text;
        previousText = _text.text;
    }
}

public class MainMenuController : MonoBehaviour
{
    [Header("Menus:")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject levelSelectorMenu;
    [SerializeField] private GameObject settingsMenu;

    [Space(10)]

    [SerializeField] private Button duskMountainButton;
    [SerializeField] private GameObject duskMountainInfo;
    [SerializeField] private GameObject duskMountainLockedOverlay;
    [SerializeField] private int duskMountainWaveRequirement = 25;

    [Space(10)]

    [SerializeField] private List<TMPro.TMP_Text> highestWavePerLevel = new();
    [SerializeField] private TMPro.TMP_Text highestWaveText;
    [SerializeField] private List<string> levelNames = new();

    [Space(10)]

    [SerializeField] private List<TMPro.TMP_Text> keybindTexts = new();

    private MainMenuInputActions mainMenuInputActions;
    private InputAction playAction;
    private InputAction exitAction;
    private InputAction selectLevel1Action;
    private InputAction selectLevel2Action;
    private ActionType currentAction = ActionType.None;

    private void Awake()
    {
        mainMenuInputActions = new MainMenuInputActions();
    }

    private void OnEnable()
    {
        playAction = mainMenuInputActions.UI.Play;
        playAction.Enable();
        playAction.started += OnShowLevelSelector;

        exitAction = mainMenuInputActions.UI.Quit;
        exitAction.Enable();

        selectLevel1Action = mainMenuInputActions.UI.SelectLevel1;
        selectLevel1Action.Enable();

        selectLevel2Action = mainMenuInputActions.UI.SelectLevel2;
        selectLevel2Action.Enable();
    }

    private void OnDisable()
    {
        playAction.Disable();
        playAction.started -= OnShowLevelSelector;

        exitAction.Disable();

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

        // INFO: Associate each entry in keybindTexts with a key of type ActionTypes
        for (int i = 0; i < keybindTexts.Count; ++i)
        {
            if (keybindTexts[i] != null)
            {
                ActionType type = (ActionType)i;
                KeybindingData data = new(keybindTexts[i]);

                // INFO: Update current/previous text if an entry is already found in player prefs
                //       otherwise leave as default
                if (PlayerPrefs.HasKey(type.ToString()))
                {
                    string storedText = PlayerPrefs.GetString(type.ToString());
                    data.textComponent.text = storedText;
                    data.previousText = storedText;
                }

                GameManager.Instance.SetKeybindingToAction(type, data);
            }
        }
    }

    private void OnShowLevelSelector(InputAction.CallbackContext context)
    {
        ShowLevelSelector();
    }

    private void OnHideLevelSelector(InputAction.CallbackContext context)
    {
        HideLevelSelector();
    }

    private void OnShowSettingsMenu(InputAction.CallbackContext context)
    {
        ShowSettingsMenu();
    }

    private void OnHideSettingsMenu(InputAction.CallbackContext context)
    {
        HideSettingsMenu();
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
        playAction.started -= OnShowLevelSelector;
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
        playAction.started += OnShowLevelSelector;
    }

    public void ShowSettingsMenu()
    {

    }

    public void HideSettingsMenu()
    {

    }

    public void OnKeybindClicked(int action)
    {
        // INFO: Revert the selected action text to its previous state 
        GameManager.Instance.RevertKeybindText(currentAction);

        ActionType actionType = (ActionType)action;

        // INFO: Set the text of the corresponding text component to something
        GameManager.Instance.SetCurrentKeybindText(actionType, "Rebinding");

        // INFO: Set the new current action
        currentAction = actionType;
    }

    public void Yep()
    {
        Debug.Log("Yep");
    }

    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
}
