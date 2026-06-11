using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Chat UI for the AI NPC. References TMP components in the scene.
/// Attach to the AgentChat Canvas root object.
/// </summary>
public class AgentUI : MonoBehaviour
{
    public static AgentUI Instance { get; private set; }
    public static bool IsOpen => Instance != null && Instance._isOpen;

    [Header("UI References")]
    [SerializeField] TMP_Text chatLog;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Button sendButton;
    [SerializeField] GameObject panelRoot;

    [Header("NPC Reference")]
    [SerializeField] GLMAgent agent;

    bool _isOpen;

    readonly List<string> _history = new List<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSend);
        }

        CloseUI();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Update()
    {
        if (!_isOpen)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseUI();
            return;
        }

        bool enterPressed = Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame;
#else
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseUI();
            return;
        }

        bool enterPressed = Input.GetKeyDown(KeyCode.Return);
#endif

        if (enterPressed && inputField != null && inputField.isFocused)
        {
            OnSend();
        }
    }

    public static void OpenUI()
    {
        if (Instance == null)
        {
            return;
        }

        Instance._isOpen = true;
        if (Instance.panelRoot != null)
        {
            Instance.panelRoot.SetActive(true);
        }

        if (Instance.inputField != null)
        {
            Instance.inputField.text = "";
            Instance.inputField.ActivateInputField();
        }

        Time.timeScale = 0f;
    }

    public static void CloseUI()
    {
        if (Instance == null)
        {
            return;
        }

        Instance._isOpen = false;
        if (Instance.panelRoot != null)
        {
            Instance.panelRoot.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    void OnSend()
    {
        if (agent == null || agent.IsBusy || inputField == null)
        {
            return;
        }

        string text = inputField.text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        AppendLog($"<color=#FFE066>[你]</color> {text}");
        inputField.text = "";
        inputField.ActivateInputField();

        agent.SendMessage(text, reply =>
        {
            AppendLog($"<color=#88DDFF>[智者]</color> {reply}");
        });
    }

    void AppendLog(string line)
    {
        _history.Add(line);
        if (_history.Count > 50)
        {
            _history.RemoveAt(0);
        }

        if (chatLog != null)
        {
            chatLog.text = string.Join("\n", _history);
        }
    }
}
