using System.Collections.Generic;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
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
    bool _welcomeShown;
    bool _hasPlayerInputSnapshot;
    bool _previousCursorInputForLook;
    float _previousTimeScale = 1f;
    CursorLockMode _previousLockState;
    bool _previousCursorVisible;
    StarterAssetsInputs _playerInputs;

    static TMP_FontAsset _cjkFallbackFont;
    const string BundledChineseFontResource = "Fonts/NotoSansSC-VF";
    const string CommonChineseCharacters =
        "\u65c5\u8005\u9760\u8fd1\u706b\u5149\u4f60\u53ef\u4ee5\u95ee\u6211" +
        "\u58f0\u671b\u94b1\u5305\u6587\u660e\u89c4\u5219\u4e0e\u5bf9\u8bdd\u53d1\u9001\u5173\u95ed" +
        "\u5411\u63d0\u95ee\u7cfb\u7edf\u6b63\u5728\u56de\u5e94\u8bf7\u7a0d\u7b49\u8fd8\u6ca1\u6709" +
        "\u8fde\u63a5\u5230\u6838\u5fc3\u9519\u8bef\u8bf7\u6c42\u5931\u8d25\u53c2\u6570\u975e\u6cd5" +
        "\u9650\u5236\u5c0f\u6570\u70b9\u56de\u7b54\u83b7\u5f97\u901a\u884c\u8bc1\u4efb\u52a1\u5b8c\u6210" +
        "\u8fdb\u5165\u533a\u57df\u80cc\u5305\u5730\u56fe\u65b9\u5411\u79fb\u52a8\u8f6c\u5411" +
        "\u3002\uff0c\uff1a\uff1b\uff01\uff1f\u3010\u3011\uff08\uff09";

    readonly List<string> _history = new List<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureEventSystem();

        if (chatLog != null && chatLog.font != null)
        {
            Font legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (legacyFont != null)
            {
                TMP_FontAsset fallbackFont = TMP_FontAsset.CreateFontAsset(legacyFont);
                if (fallbackFont != null)
                {
                    if (chatLog.font.fallbackFontAssetTable == null)
                    {
                        chatLog.font.fallbackFontAssetTable = new List<TMP_FontAsset>();
                    }
                    if (!chatLog.font.fallbackFontAssetTable.Contains(fallbackFont))
                    {
                        chatLog.font.fallbackFontAssetTable.Add(fallbackFont);
                    }
                }
            }
        }

        if (inputField != null && inputField.textComponent != null && inputField.textComponent.font != null)
        {
            Font legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (legacyFont != null)
            {
                TMP_FontAsset fallbackFont = TMP_FontAsset.CreateFontAsset(legacyFont);
                if (fallbackFont != null)
                {
                    if (inputField.textComponent.font.fallbackFontAssetTable == null)
                    {
                        inputField.textComponent.font.fallbackFontAssetTable = new List<TMP_FontAsset>();
                    }
                    if (!inputField.textComponent.font.fallbackFontAssetTable.Contains(fallbackFont))
                    {
                        inputField.textComponent.font.fallbackFontAssetTable.Add(fallbackFont);
                    }
                }
            }
        }

        ApplyChineseFallbacksToAllText();
    }

    void ApplyChineseFallbacksToAllText()
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            AddChineseFallback(texts[i]);
        }
    }

    static void AddChineseFallback(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        TMP_FontAsset fallback = GetChineseFallbackFont();
        if (fallback == null)
        {
            return;
        }

        text.font = fallback;
    }

    static TMP_FontAsset GetChineseFallbackFont()
    {
        if (_cjkFallbackFont != null)
        {
            return _cjkFallbackFont;
        }

        Font font = Resources.Load<Font>(BundledChineseFontResource);
        if (font == null)
        {
            Debug.LogError($"[AgentUI] Missing bundled Chinese font at Resources/{BundledChineseFontResource}.");
            return null;
        }

        _cjkFallbackFont = TMP_FontAsset.CreateFontAsset(font);
        if (_cjkFallbackFont != null)
        {
            _cjkFallbackFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            _cjkFallbackFont.isMultiAtlasTexturesEnabled = true;
            _cjkFallbackFont.TryAddCharacters(CommonChineseCharacters);
        }

        return _cjkFallbackFont;
    }

    void Start()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSend);
        }

        CloseImmediate();
    }

    void OnDisable()
    {
        if (_isOpen)
        {
            Close();
        }
    }

    void OnDestroy()
    {
        if (_isOpen)
        {
            Close();
        }

        if (sendButton != null)
        {
            sendButton.onClick.RemoveListener(OnSend);
        }

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

        Instance.Open();
    }

    public static void CloseUI()
    {
        if (Instance == null)
        {
            return;
        }

        Instance.Close();
    }

    void Open()
    {
        if (_isOpen)
        {
            return;
        }

        _previousTimeScale = Time.timeScale;
        _previousLockState = Cursor.lockState;
        _previousCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CacheAndDisablePlayerLook();

        _isOpen = true;
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        ApplyChineseFallbacksToAllText();

        if (!_welcomeShown)
        {
            AppendLog("<color=#88DDFF>[Priest Z.AI]</color> \u65c5\u8005\uff0c\u9760\u8fd1\u706b\u5149\u3002\u4f60\u53ef\u4ee5\u95ee\u6211 DAO\u3001Token\u3001\u58f0\u671b\u3001\u94b1\u5305\u6216\u6587\u660e\u89c4\u5219\u3002");
            _welcomeShown = true;
        }

        SetInputEnabled(true);
        if (inputField != null)
        {
            inputField.text = "";
            inputField.ActivateInputField();
        }

        Time.timeScale = 0f;
    }

    void Close()
    {
        if (!_isOpen)
        {
            CloseImmediate();
            return;
        }

        _isOpen = false;
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        RestorePlayerLook();
        Cursor.lockState = _previousLockState;
        Cursor.visible = _previousCursorVisible;
        Time.timeScale = _previousTimeScale;
    }

    void CloseImmediate()
    {
        _isOpen = false;
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    void OnSend()
    {
        if (inputField == null)
        {
            return;
        }

        if (agent == null)
        {
            AppendLog("<color=#FFB86B>[\u7cfb\u7edf]</color> Priest Z.AI \u8fd8\u6ca1\u6709\u8fde\u63a5\u5230\u5bf9\u8bdd\u6838\u5fc3\u3002");
            return;
        }

        if (agent.IsBusy)
        {
            AppendLog("<color=#FFB86B>[\u7cfb\u7edf]</color> Priest Z.AI \u6b63\u5728\u56de\u5e94\uff0c\u8bf7\u7a0d\u7b49\u3002");
            return;
        }

        string text = inputField.text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        AppendLog($"<color=#FFE066>[\u4f60]</color> {text}");
        inputField.text = "";
        SetInputEnabled(false);

        agent.SendMessage(text, reply =>
        {
            AppendLog($"<color=#88DDFF>[Priest Z.AI]</color> {reply}");
            SetInputEnabled(true);
            if (_isOpen && inputField != null)
            {
                inputField.ActivateInputField();
            }
        });
    }

    void SetInputEnabled(bool enabled)
    {
        if (sendButton != null)
        {
            sendButton.interactable = enabled;
        }

        if (inputField != null)
        {
            inputField.interactable = enabled;
        }
    }

    void CacheAndDisablePlayerLook()
    {
        if (_playerInputs == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                _playerInputs = player.GetComponent<StarterAssetsInputs>();
            }
        }

        if (_playerInputs == null)
        {
            _hasPlayerInputSnapshot = false;
            return;
        }

        _previousCursorInputForLook = _playerInputs.cursorInputForLook;
        _hasPlayerInputSnapshot = true;
        _playerInputs.cursorInputForLook = false;
        _playerInputs.LookInput(Vector2.zero);
    }

    void RestorePlayerLook()
    {
        if (_playerInputs == null || !_hasPlayerInputSnapshot)
        {
            return;
        }

        _playerInputs.cursorInputForLook = _previousCursorInputForLook;
        _hasPlayerInputSnapshot = false;
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

    static void EnsureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            eventSystem = Object.FindFirstObjectByType<EventSystem>();
        }

        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            eventSystem = eventSystemObject.GetComponent<EventSystem>();
        }

#if ENABLE_INPUT_SYSTEM
        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        if (inputModule.actionsAsset == null)
        {
            inputModule.AssignDefaultActions();
        }

        StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (legacyModule != null)
        {
            legacyModule.enabled = false;
        }
#else
        if (eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
#endif
    }
}
