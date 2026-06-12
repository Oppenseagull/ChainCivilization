using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// New Game / Restart: ESC menu, clears all PlayerPrefs, reloads scene from spawn.
/// </summary>
[DisallowMultipleComponent]
public class GameResetManager : MonoBehaviour
{
    [SerializeField] Vector3 spawnPosition = new Vector3(0f, 1f, 0f);

    bool _menuOpen;
    bool _isRestarting;

    GUIStyle _overlayStyle;
    GUIStyle _panelStyle;
    GUIStyle _titleStyle;
    GUIStyle _buttonStyle;
    GUIStyle _hintStyle;
    Texture2D _overlayTexture;
    Texture2D _panelTexture;
    bool _stylesReady;

    float _previousTimeScale = 1f;

    void Update()
    {
        if (_isRestarting)
        {
            return;
        }

        if (!_menuOpen && GameplayInputGate.BlocksGameplayShortcuts)
        {
            return;
        }

        if (IsRestartShortcutPressed())
        {
            RestartGame();
            return;
        }

        if (!WasEscapePressedThisFrame())
        {
            return;
        }

        if (IsAnyGamePanelOpen())
        {
            return;
        }

        ToggleMenu();
    }

    void ToggleMenu()
    {
        _menuOpen = !_menuOpen;

        if (_menuOpen)
        {
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            return;
        }

        Time.timeScale = _previousTimeScale;
    }

    void CloseMenu()
    {
        if (!_menuOpen)
        {
            return;
        }

        _menuOpen = false;
        Time.timeScale = _previousTimeScale;
    }

    public void RestartGame()
    {
        if (_isRestarting)
        {
            return;
        }

        _isRestarting = true;
        _menuOpen = false;
        Debug.Log("GameResetManager: restarting game and clearing saved demo state.");

        Time.timeScale = 1f;
        ResetSessionState();
        GameSaveKeys.DeleteAllSavedData();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    static void ResetSessionState()
    {
        CivilizationManager.ResetSelection();
        CivilizationRuleSelection.Reset();
        QuestSignals.ResetSession();
        CivilizationJournalLog.ResetSession();
        HUDPromptChannel.ClearAll();
        TabMenuUI.CloseIfOpen();
        MapPanelUI.CloseIfOpen();
        InventoryPanelUI.CloseIfOpen();

        TokenManager tokens = TokenManager.Instance;
        if (tokens != null)
        {
            tokens.ResetDemoState();
        }

        ReputationManager reputation = ReputationManager.Instance;
        if (reputation != null)
        {
            reputation.ResetDemoState();
        }

        DAOPassManager passes = DAOPassManager.Instance;
        if (passes != null)
        {
            passes.ResetDemoState();
        }

        MoonCrystalInteract.ClearAllCollectedPrefs();
        AirdropCoin.ClearAllCollectedPrefs();

        MainQuestManager quest = Object.FindFirstObjectByType<MainQuestManager>();
        if (quest != null)
        {
            quest.ResetProgress();
        }

        CivilizationJournalUI journal = Object.FindFirstObjectByType<CivilizationJournalUI>();
        if (journal != null)
        {
            journal.ResetJournal();
        }
    }

    void OnGUI()
    {
        if (!_menuOpen || _isRestarting)
        {
            return;
        }

        EnsureStyles();

        GUI.color = new Color(0.02f, 0.04f, 0.1f, 0.72f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _overlayTexture);
        GUI.color = Color.white;

        const float width = 320f;
        const float height = 200f;
        float x = (Screen.width - width) * 0.5f;
        float y = (Screen.height - height) * 0.5f;

        GUI.Box(new Rect(x, y, width, height), GUIContent.none, _panelStyle);
        GUI.Label(new Rect(x, y + 16f, width, 32f), "Menu", _titleStyle);

        float buttonWidth = width - 48f;
        float buttonX = x + 24f;

        if (GUI.Button(new Rect(buttonX, y + 64f, buttonWidth, 40f), "Restart Game", _buttonStyle))
        {
            RestartGame();
            return;
        }

        if (GUI.Button(new Rect(buttonX, y + 116f, buttonWidth, 40f), "Continue", _buttonStyle))
        {
            CloseMenu();
        }

        GUI.Label(new Rect(x, y + height - 30f, width, 22f), "Ctrl+R restart / ESC close", _hintStyle);
    }

    static bool IsAnyGamePanelOpen()
    {
        return TabMenuUI.IsOpen
            || MapPanelUI.IsOpen
            || InventoryPanelUI.IsOpen
            || AgentUI.IsOpen
            || (CivilizationSeedRulePanel.Instance != null && CivilizationSeedRulePanel.Instance.IsOpen);
    }

    static bool WasEscapePressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    static bool IsRestartShortcutPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return false;
        }

        bool ctrlHeld = keyboard.ctrlKey.isPressed
            || keyboard.leftCtrlKey.isPressed
            || keyboard.rightCtrlKey.isPressed;
        bool ctrlPressedThisFrame = keyboard.ctrlKey.wasPressedThisFrame
            || keyboard.leftCtrlKey.wasPressedThisFrame
            || keyboard.rightCtrlKey.wasPressedThisFrame;
        bool rHeld = keyboard.rKey.isPressed;
        bool rPressedThisFrame = keyboard.rKey.wasPressedThisFrame;

        return (ctrlHeld && rPressedThisFrame) || (rHeld && ctrlPressedThisFrame);
#else
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool ctrlPressedThisFrame = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
        bool rHeld = Input.GetKey(KeyCode.R);
        bool rPressedThisFrame = Input.GetKeyDown(KeyCode.R);

        return (ctrlHeld && rPressedThisFrame) || (rHeld && ctrlPressedThisFrame);
#endif
    }

    void EnsureStyles()
    {
        if (_stylesReady)
        {
            return;
        }

        _overlayTexture = MakeTexture(2, 2, Color.white);
        _panelTexture = MakeTexture(2, 2, new Color(0.07f, 0.1f, 0.16f, 0.96f));

        _panelStyle = new GUIStyle(GUI.skin.box);
        _panelStyle.normal.background = _panelTexture;

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _titleStyle.normal.textColor = new Color(0.9f, 0.94f, 1f);

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 17,
            fontStyle = FontStyle.Bold
        };

        _hintStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 13,
            fontStyle = FontStyle.Italic
        };
        _hintStyle.normal.textColor = new Color(0.62f, 0.72f, 0.82f, 0.9f);

        _stylesReady = true;
    }

    static Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
