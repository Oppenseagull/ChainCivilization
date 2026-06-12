using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Inventory panel toggled with I. Sims-style card UI for MOON, passes, seeds, and future slots.
/// </summary>
public class InventoryPanelUI : MonoBehaviour
{
    public static InventoryPanelUI Instance { get; private set; }
    public static bool IsOpen => Instance != null && Instance._isOpen;

    bool _isOpen;
    Canvas _canvas;
    GameObject _root;
    InventoryPanelView.InventoryWidget _inventoryWidget;

    CursorLockMode _previousLockState;
    bool _previousCursorVisible;

    static Font _font;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUi();
        CloseImmediate();
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
        if (TabMenuUI.IsOpen)
        {
            if (_isOpen)
            {
                Close();
            }

            return;
        }

        if (GameplayInputGate.BlocksGameplayShortcuts)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            Toggle();
        }

        if (_isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
#else
        if (Input.GetKeyDown(KeyCode.I))
        {
            Toggle();
        }

        if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
#endif
    }

    public static void CloseIfOpen()
    {
        if (Instance != null && Instance._isOpen)
        {
            Instance.Close();
        }
    }

    void Toggle()
    {
        if (_isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    void Open()
    {
        _isOpen = true;
        _root.SetActive(true);
        _previousLockState = Cursor.lockState;
        _previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        InventoryPanelView.Refresh(_inventoryWidget);
    }

    void Close()
    {
        _isOpen = false;
        _root.SetActive(false);
        Cursor.lockState = _previousLockState;
        Cursor.visible = _previousCursorVisible;
    }

    void CloseImmediate()
    {
        _isOpen = false;
        if (_root != null)
        {
            _root.SetActive(false);
        }
    }

    void BuildUi()
    {
        EnsureEventSystem();

        GameObject canvasObject = new GameObject("InventoryPanelCanvas");
        canvasObject.transform.SetParent(transform, false);

        _canvas = canvasObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 196;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        _root = CreateUiObject("InventoryPanelRoot", canvasObject.transform);
        StretchFull(_root.GetComponent<RectTransform>());

        Image dim = _root.AddComponent<Image>();
        dim.color = new Color(0.08f, 0.12f, 0.18f, 0.45f);

        GameObject panel = CreateUiObject("InventoryPanel", _root.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(700f, 640f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.24f, 0.34f, 0.44f, 0.97f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(28, 28, 24, 20);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateTitle(panel.transform, "背包");
        _inventoryWidget = InventoryPanelView.Build(panel.transform, 620f);
        CreateFooter(panel.transform, "I / ESC 关闭");
    }

    static void CreateTitle(Transform parent, string text)
    {
        GameObject titleObject = CreateUiObject("Title", parent);
        LayoutElement titleLayout = titleObject.AddComponent<LayoutElement>();
        titleLayout.minHeight = 40f;
        titleLayout.preferredHeight = 40f;

        Text title = titleObject.AddComponent<Text>();
        title.text = text;
        title.font = _font;
        title.fontSize = 30;
        title.fontStyle = FontStyle.Bold;
        title.alignment = TextAnchor.MiddleCenter;
        title.color = new Color(0.92f, 0.96f, 1f);
    }

    static void CreateFooter(Transform parent, string text)
    {
        GameObject footerObject = CreateUiObject("Footer", parent);
        LayoutElement footerLayout = footerObject.AddComponent<LayoutElement>();
        footerLayout.minHeight = 24f;
        footerLayout.preferredHeight = 24f;

        Text footer = footerObject.AddComponent<Text>();
        footer.text = text;
        footer.font = _font;
        footer.fontSize = 16;
        footer.fontStyle = FontStyle.Italic;
        footer.alignment = TextAnchor.MiddleCenter;
        footer.color = new Color(0.72f, 0.82f, 0.9f, 0.9f);
    }

    static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void EnsureEventSystem()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
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
