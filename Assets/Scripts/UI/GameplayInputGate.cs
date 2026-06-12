using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Shared guard for gameplay hotkeys while the player is typing in UI.
/// </summary>
public static class GameplayInputGate
{
    public static bool BlocksGameplayShortcuts => AgentUI.IsOpen || IsTextInputFocused();

    public static bool IsTextInputFocused()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null || eventSystem.currentSelectedGameObject == null)
        {
            return false;
        }

        GameObject selected = eventSystem.currentSelectedGameObject;
        TMP_InputField tmpInput = selected.GetComponent<TMP_InputField>();
        if (tmpInput != null && tmpInput.isFocused)
        {
            return true;
        }

        InputField legacyInput = selected.GetComponent<InputField>();
        return legacyInput != null && legacyInput.isFocused;
    }
}
