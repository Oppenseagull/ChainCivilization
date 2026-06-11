using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Unified TAB menu (Sims / Stardew style). Tabs: Map, Inventory, Journal, Worldview, Chain Civilization, Demo Guide.
/// </summary>
public class TabMenuUI : MonoBehaviour
{
    public static TabMenuUI Instance { get; private set; }
    public static bool IsOpen => Instance != null && Instance._isOpen;

    enum MenuTab
    {
        Map = 0,
        Inventory = 1,
        Journal = 2,
        Worldview = 3,
        ChainCivilization = 4,
        DemoGuide = 5
    }

    static readonly string[] TabLabels = { "地图", "背包", "文明日志", "世界观说明", "Chain Civilization", "Demo Guide" };

    bool _isOpen;
    MenuTab _activeTab = MenuTab.Map;

    Canvas _canvas;
    GameObject _root;
    GameObject[] _pages = new GameObject[6];
    Image[] _tabBackgrounds = new Image[6];
    Text[] _tabLabels = new Text[6];

    ExplorationMapView.MapWidget _mapWidget;
    InventoryPanelView.InventoryWidget _inventoryWidget;
    DemoFlowPanelView.DemoFlowWidget _demoFlowWidget;
    RectTransform _journalContent;

    MainQuestManager _questManager;
    Transform _player;
    ThirdPersonController _playerController;
    StarterAssetsInputs _playerInputs;

    bool _movementLocked;
    CursorLockMode _previousLockState;
    bool _previousCursorVisible;

    static Font _font;

    const float TabPageMinHeight = 420f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _questManager = GetComponent<MainQuestManager>();
        if (_questManager == null)
        {
            _questManager = FindFirstObjectByType<MainQuestManager>();
        }

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            _player = player.transform;
        }

        BuildUi();
        CloseImmediate();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            UnlockPlayerMovement();
            Instance = null;
        }
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            Toggle();
        }

        if (_isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
#else
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Toggle();
        }

        if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
#endif
    }

    void LateUpdate()
    {
        if (!_isOpen)
        {
            return;
        }

        if (_activeTab == MenuTab.Map && _mapWidget != null)
        {
            ExplorationMapView.Refresh(_mapWidget, _questManager, _player);
        }

        if (_activeTab == MenuTab.DemoGuide && _demoFlowWidget != null)
        {
            DemoFlowPanelView.Refresh(_demoFlowWidget, _questManager);
        }
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
        LockPlayerMovement();
        _previousLockState = Cursor.lockState;
        _previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        RefreshActivePage();
        UpdateTabVisuals();
    }

    void Close()
    {
        _isOpen = false;
        _root.SetActive(false);
        UnlockPlayerMovement();
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

    void SelectTab(MenuTab tab)
    {
        _activeTab = tab;
        RefreshActivePage();
        UpdateTabVisuals();
    }

    void RefreshActivePage()
    {
        for (int i = 0; i < _pages.Length; i++)
        {
            _pages[i].SetActive(i == (int)_activeTab);
        }

        switch (_activeTab)
        {
            case MenuTab.Map:
                RefreshMapPage();
                break;
            case MenuTab.Inventory:
                RefreshInventoryPage();
                break;
            case MenuTab.Journal:
                RefreshJournalPage();
                break;
            case MenuTab.DemoGuide:
                RefreshDemoGuidePage();
                break;
        }

        FinalizeActivePageLayout();
    }

    void UpdateTabVisuals()
    {
        for (int i = 0; i < _tabBackgrounds.Length; i++)
        {
            bool active = i == (int)_activeTab;
            _tabBackgrounds[i].color = active
                ? new Color(0.42f, 0.34f, 0.14f, 1f)
                : new Color(0.18f, 0.2f, 0.16f, 0.95f);
            _tabLabels[i].color = active
                ? new Color(1f, 0.92f, 0.45f)
                : new Color(0.72f, 0.76f, 0.68f);
            _tabLabels[i].fontStyle = active ? FontStyle.Bold : FontStyle.Normal;
        }
    }

    void RefreshMapPage()
    {
        ExplorationMapView.Refresh(_mapWidget, _questManager, _player);
    }

    void RefreshInventoryPage()
    {
        InventoryPanelView.Refresh(_inventoryWidget);
    }

    void RefreshDemoGuidePage()
    {
        DemoFlowPanelView.Refresh(_demoFlowWidget, _questManager);
    }

    void RefreshJournalPage()
    {
        if (_journalContent == null)
        {
            return;
        }

        ClearChildren(_journalContent);
        IReadOnlyList<CivilizationJournalLog.Entry> entries = CivilizationJournalLog.Entries;

        if (entries.Count == 0)
        {
            CreateBodyLine(_journalContent, "暂无记录", 22, FontStyle.Bold, new Color(0.7f, 0.78f, 0.86f));
            CreateBodyLine(_journalContent, "探索世界后自动写入日志", 18, FontStyle.Italic, new Color(0.62f, 0.72f, 0.82f));
            RebuildScrollContentLayout(_journalContent);
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            CivilizationJournalLog.Entry entry = entries[i];
            CreateBodyLine(_journalContent, entry.DayLabel, 22, FontStyle.Bold, new Color(1f, 0.92f, 0.45f));
            for (int line = 0; line < entry.Lines.Count; line++)
            {
                CreateBodyLine(_journalContent, entry.Lines[line], 18, FontStyle.Normal, new Color(0.88f, 0.95f, 1f));
            }

            if (i < entries.Count - 1)
            {
                CreateSpacer(_journalContent, 14f);
            }
        }

        RebuildScrollContentLayout(_journalContent);
    }

    void FinalizeActivePageLayout()
    {
        GameObject activePage = _pages[(int)_activeTab];
        if (activePage == null)
        {
            return;
        }

        RectTransform pageRect = activePage.GetComponent<RectTransform>();
        if (pageRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(pageRect);
        }

        ScrollRect scroll = activePage.GetComponent<ScrollRect>();
        if (scroll == null)
        {
            scroll = activePage.GetComponentInChildren<ScrollRect>(true);
        }

        if (scroll != null)
        {
            if (scroll.content != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            }

            scroll.verticalNormalizedPosition = 1f;
        }
    }

    static void RebuildScrollContentLayout(RectTransform content)
    {
        if (content == null)
        {
            return;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        ScrollRect scroll = content.GetComponentInParent<ScrollRect>();
        if (scroll != null)
        {
            RectTransform scrollRoot = scroll.GetComponent<RectTransform>();
            if (scrollRoot != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRoot);
            }
        }
    }

    void LockPlayerMovement()
    {
        if (_movementLocked)
        {
            return;
        }

        if (_playerController == null)
        {
            _playerController = FindFirstObjectByType<ThirdPersonController>();
        }

        if (_playerInputs == null)
        {
            _playerInputs = FindFirstObjectByType<StarterAssetsInputs>();
        }

        if (_playerController != null)
        {
            _playerController.enabled = false;
        }

        if (_playerInputs != null)
        {
            _playerInputs.enabled = false;
        }

        _movementLocked = true;
    }

    void UnlockPlayerMovement()
    {
        if (!_movementLocked)
        {
            return;
        }

        if (_playerController != null)
        {
            _playerController.enabled = true;
        }

        if (_playerInputs != null)
        {
            _playerInputs.enabled = true;
        }

        _movementLocked = false;
    }

    void BuildUi()
    {
        EnsureEventSystem();

        GameObject canvasObject = new GameObject("TabMenuCanvas");
        canvasObject.transform.SetParent(transform, false);

        _canvas = canvasObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 220;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        _root = CreateUiObject("TabMenuRoot", canvasObject.transform);
        StretchFull(_root.GetComponent<RectTransform>());

        Image dim = _root.AddComponent<Image>();
        dim.color = new Color(0.02f, 0.04f, 0.08f, 0.72f);

        GameObject panel = CreateUiObject("MenuPanel", _root.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(1040f, 720f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.12f, 0.09f, 0.97f);

        VerticalLayoutGroup panelLayout = panel.AddComponent<VerticalLayoutGroup>();
        panelLayout.padding = new RectOffset(28, 28, 24, 20);
        panelLayout.spacing = 14f;
        panelLayout.childAlignment = TextAnchor.UpperCenter;
        panelLayout.childControlWidth = true;
        panelLayout.childControlHeight = false;
        panelLayout.childForceExpandWidth = true;
        panelLayout.childForceExpandHeight = false;

        CreateTitle(panel.transform, "旅行者菜单");
        CreateTabBar(panel.transform);
        CreateContentArea(panel.transform);
        CreateFooter(panel.transform, "TAB / ESC 关闭");
    }

    void CreateTabBar(Transform parent)
    {
        GameObject bar = CreateUiObject("TabBar", parent);
        LayoutElement barLayout = bar.AddComponent<LayoutElement>();
        barLayout.minHeight = 52f;
        barLayout.preferredHeight = 52f;

        HorizontalLayoutGroup layout = bar.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 6f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        for (int i = 0; i < TabLabels.Length; i++)
        {
            MenuTab tab = (MenuTab)i;
            CreateTabButton(bar.transform, TabLabels[i], tab);
        }
    }

    void CreateTabButton(Transform parent, string label, MenuTab tab)
    {
        GameObject buttonObject = CreateUiObject($"Tab_{label}", parent);
        Image background = buttonObject.AddComponent<Image>();
        background.color = new Color(0.18f, 0.2f, 0.16f, 0.95f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(() => SelectTab(tab));

        GameObject textObject = CreateUiObject("Label", buttonObject.transform);
        StretchFull(textObject.GetComponent<RectTransform>());
        Text text = textObject.AddComponent<Text>();
        text.text = label;
        text.font = _font;
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.72f, 0.76f, 0.68f);
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        int index = (int)tab;
        _tabBackgrounds[index] = background;
        _tabLabels[index] = text;
    }

    void CreateContentArea(Transform parent)
    {
        GameObject contentShell = CreateUiObject("ContentShell", parent);
        LayoutElement shellLayout = contentShell.AddComponent<LayoutElement>();
        shellLayout.minHeight = 500f;
        shellLayout.preferredHeight = 500f;
        shellLayout.flexibleHeight = 1f;

        Image shellImage = contentShell.AddComponent<Image>();
        shellImage.color = new Color(0.06f, 0.08f, 0.06f, 0.88f);

        VerticalLayoutGroup shellLayoutGroup = contentShell.AddComponent<VerticalLayoutGroup>();
        shellLayoutGroup.padding = new RectOffset(16, 16, 16, 16);
        shellLayoutGroup.childAlignment = TextAnchor.UpperLeft;
        shellLayoutGroup.childControlWidth = true;
        shellLayoutGroup.childControlHeight = true;
        shellLayoutGroup.childForceExpandWidth = true;
        shellLayoutGroup.childForceExpandHeight = true;

        _pages[(int)MenuTab.Map] = CreateMapPage(contentShell.transform);
        _pages[(int)MenuTab.Inventory] = CreateInventoryPage(contentShell.transform);
        _pages[(int)MenuTab.Journal] = CreateScrollPage(contentShell.transform, "JournalPage", out _journalContent);
        _pages[(int)MenuTab.Worldview] = CreateWorldviewPage(contentShell.transform);
        _pages[(int)MenuTab.ChainCivilization] = WorldIntroductionPanelView.Build(contentShell.transform);
        _pages[(int)MenuTab.DemoGuide] = CreateDemoGuidePage(contentShell.transform);
    }

    GameObject CreateMapPage(Transform parent)
    {
        GameObject page = CreateUiObject("MapPage", parent);
        StretchFull(page.GetComponent<RectTransform>());
        ApplyTabPageLayout(page);

        VerticalLayoutGroup layout = page.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        _mapWidget = ExplorationMapView.Build(page.transform, 820f, 470f);
        return page;
    }

    GameObject CreateInventoryPage(Transform parent)
    {
        GameObject page = CreateUiObject("InventoryPage", parent);
        StretchFull(page.GetComponent<RectTransform>());
        ApplyTabPageLayout(page);

        VerticalLayoutGroup layout = page.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        _inventoryWidget = InventoryPanelView.Build(page.transform, 820f);
        return page;
    }

    GameObject CreateDemoGuidePage(Transform parent)
    {
        GameObject page = CreateUiObject("DemoGuidePage", parent);
        StretchFull(page.GetComponent<RectTransform>());
        ApplyTabPageLayout(page);

        VerticalLayoutGroup layout = page.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        _demoFlowWidget = DemoFlowPanelView.Build(page.transform, 860f);
        return page;
    }

    GameObject CreateScrollPage(Transform parent, string name, out RectTransform content,
        RectOffset contentPadding = null, TextAnchor contentAlignment = TextAnchor.UpperCenter, float contentSpacing = 6f)
    {
        GameObject page = CreateUiObject(name, parent);
        StretchFull(page.GetComponent<RectTransform>());
        ApplyTabPageLayout(page);

        ScrollRect scroll = page.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 24f;

        GameObject viewport = CreateUiObject("Viewport", page.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        StretchFull(viewportRect);
        viewport.AddComponent<RectMask2D>();

        GameObject contentObject = CreateUiObject("Content", viewport.transform);
        content = contentObject.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup contentLayout = contentObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = contentPadding ?? new RectOffset(12, 12, 12, 12);
        contentLayout.spacing = contentSpacing;
        contentLayout.childAlignment = contentAlignment;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewportRect;
        scroll.content = content;
        return page;
    }

    GameObject CreateWorldviewPage(Transform parent)
    {
        GameObject page = CreateScrollPage(parent, "WorldviewPage", out RectTransform content,
            new RectOffset(20, 20, 16, 16), TextAnchor.UpperLeft, 8f);

        CreateBodyLine(content, "Chain Civilization", 30, FontStyle.Bold, new Color(0.95f, 0.82f, 0.45f));
        CreateBodyLine(content, "一个关于 Web3 文明的实验世界。", 18, FontStyle.Italic, new Color(0.82f, 0.76f, 0.62f));
        CreateSpacer(content, 8f);
        CreateBodyLine(content, "在这里", 16, FontStyle.Italic, new Color(0.7f, 0.62f, 0.5f));
        CreateBodyLine(content, "没有统一国家。", 16, FontStyle.Normal, new Color(0.88f, 0.84f, 0.76f));
        CreateBodyLine(content, "没有统一规则。", 16, FontStyle.Normal, new Color(0.88f, 0.84f, 0.76f));
        CreateBodyLine(content, "每个 DAO 拥有自己的价值观。", 16, FontStyle.Normal, new Color(0.88f, 0.84f, 0.76f));
        CreateBodyLine(content, "每个文明拥有自己的共识。", 16, FontStyle.Normal, new Color(0.88f, 0.84f, 0.76f));
        CreateSpacer(content, 10f);
        CreateBodyLine(content, "Token 代表激励。", 16, FontStyle.Italic, new Color(0.78f, 0.88f, 0.82f));
        CreateBodyLine(content, "Pass 代表身份。", 16, FontStyle.Italic, new Color(0.78f, 0.88f, 0.82f));
        CreateBodyLine(content, "规则决定文明的发展方向。", 16, FontStyle.Italic, new Color(0.78f, 0.88f, 0.82f));
        CreateSpacer(content, 12f);
        CreateBodyLine(content, "玩家目标", 18, FontStyle.Bold, new Color(0.92f, 0.78f, 0.42f));
        CreateBodyLine(content, "· 探索文明", 17, FontStyle.Normal, new Color(0.9f, 0.86f, 0.78f));
        CreateBodyLine(content, "· 理解规则", 17, FontStyle.Normal, new Color(0.9f, 0.86f, 0.78f));
        CreateBodyLine(content, "· 获得身份", 17, FontStyle.Normal, new Color(0.9f, 0.86f, 0.78f));
        CreateBodyLine(content, "· 建立共识", 17, FontStyle.Normal, new Color(0.9f, 0.86f, 0.78f));
        CreateBodyLine(content, "· 创造属于自己的文明", 17, FontStyle.Normal, new Color(0.9f, 0.86f, 0.78f));

        RebuildScrollContentLayout(content);
        return page;
    }

    static void ApplyTabPageLayout(GameObject page)
    {
        LayoutElement pageLayout = page.AddComponent<LayoutElement>();
        pageLayout.minHeight = TabPageMinHeight;
        pageLayout.preferredHeight = TabPageMinHeight;
        pageLayout.flexibleHeight = 1f;
    }

    void CreateTitle(Transform parent, string text)
    {
        GameObject titleObject = CreateUiObject("MenuTitle", parent);
        LayoutElement layout = titleObject.AddComponent<LayoutElement>();
        layout.minHeight = 44f;
        layout.preferredHeight = 44f;

        Text title = titleObject.AddComponent<Text>();
        title.text = text;
        title.font = _font;
        title.fontSize = 32;
        title.fontStyle = FontStyle.Bold;
        title.alignment = TextAnchor.MiddleCenter;
        title.color = new Color(1f, 0.9f, 0.5f);
    }

    void CreateFooter(Transform parent, string text)
    {
        GameObject footerObject = CreateUiObject("Footer", parent);
        LayoutElement layout = footerObject.AddComponent<LayoutElement>();
        layout.minHeight = 24f;
        layout.preferredHeight = 24f;

        Text footer = footerObject.AddComponent<Text>();
        footer.text = text;
        footer.font = _font;
        footer.fontSize = 16;
        footer.fontStyle = FontStyle.Italic;
        footer.alignment = TextAnchor.MiddleCenter;
        footer.color = new Color(0.68f, 0.76f, 0.66f, 0.9f);
    }

    void CreateSectionHeader(Transform parent, string text)
    {
        CreateBodyLine(parent, text, 16, FontStyle.BoldAndItalic, new Color(0.62f, 0.72f, 0.84f));
    }

    void CreateBodyLine(Transform parent, string text, int fontSize, FontStyle fontStyle, Color color)
    {
        GameObject lineObject = CreateUiObject("Line", parent);
        LayoutElement layout = lineObject.AddComponent<LayoutElement>();
        layout.minHeight = fontSize + 10f;
        layout.preferredHeight = fontSize + 10f;

        Text line = lineObject.AddComponent<Text>();
        line.text = text;
        line.font = _font;
        line.fontSize = fontSize;
        line.fontStyle = fontStyle;
        line.color = color;
        line.alignment = TextAnchor.MiddleLeft;
        line.horizontalOverflow = HorizontalWrapMode.Wrap;
        line.verticalOverflow = VerticalWrapMode.Overflow;
    }

    void CreateSpacer(Transform parent, float height)
    {
        GameObject spacer = CreateUiObject("Spacer", parent);
        LayoutElement layout = spacer.AddComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;
    }

    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
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
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        eventSystem.AddComponent<InputSystemUIInputModule>();
#else
        eventSystem.AddComponent<StandaloneInputModule>();
#endif
    }
}
