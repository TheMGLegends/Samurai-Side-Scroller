using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private Dictionary<ActionType, KeybindingData> keybindingsToActions = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
    }

    public void SetKeybindingToAction(ActionType actionType, KeybindingData keybindingData)
    {
        keybindingsToActions[actionType] = keybindingData;
    }

    public void SetCurrentKeybindText(ActionType actionType, string text, Color color)
    {
        if (keybindingsToActions.ContainsKey(actionType))
        {
            keybindingsToActions[actionType].textComponent.color = color;
            keybindingsToActions[actionType].textComponent.text = text;
        }
    }

    public void RevertKeybindText(ActionType actionType, Color color)
    {
        if (keybindingsToActions.ContainsKey(actionType))
        {
            keybindingsToActions[actionType].textComponent.color = color;
            keybindingsToActions[actionType].textComponent.text = keybindingsToActions[actionType].previousText;
        }    
    }    

    public void ModifyKeybindText(ActionType actionType, string text)
    {
        if (keybindingsToActions.ContainsKey(actionType))
        {
            keybindingsToActions[actionType].textComponent.text = text;
            keybindingsToActions[actionType].previousText = text;
        }
    }    
}
