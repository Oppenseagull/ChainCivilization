using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Simple Canvas panel for choosing the first civilization rule after activating the seed.
/// </summary>
public class CivilizationSeedRulePanel : MonoBehaviour
{
    public static CivilizationSeedRulePanel Instance { get; private set; }

    [SerializeField] string panelTitle = "选择你的第一条文明规则";
    [SerializeField] string[] ruleOptions = { "开放协作", "自由贸易", "知识共享" };

    enum PanelPhase
    {
        ChooseCivilization,
        ChooseJoinRule
    }

    Canvas _canvas;
    GameObject _panelRoot;
    bool _isOpen;
    CursorLockMode _previousLockState;
    bool _previousCursorVisible;
    PanelPhase _phase = PanelPhase.ChooseCivilization;
    CivilizationType _pendingCivilizationType = CivilizationType.None;
    string _pendingCivilizationLabel;

    public bool IsOpen => _isOpen;
    public event Action<string> OnRuleSelected;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildUi();
        Close();
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
            Close();
        }
#else
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
#endif
    }

    public void Open()
    {
        if (_panelRoot == null)
        {
            BuildUi();
        }

        _phase = PanelPhase.ChooseCivilization;
        _pendingCivilizationType = CivilizationType.None;
        _pendingCivilizationLabel = null;
        RebuildPanelContents();

        _isOpen = true;
        _panelRoot.SetActive(true);

        _previousLockState = Cursor.lockState;
        _previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        _isOpen = false;
        if (_panelRoot != null)
        {
            _panelRoot.SetActive(false);
        }

        Cursor.lockState = _previousLockState;
        Cursor.visible = _previousCursorVisible;
    }

    void BuildUi()
    {
        EnsureEventSystem();

        GameObject canvasObject = new GameObject("CivilizationSeedRuleCanvas");
        canvasObject.transform.SetParent(transform, false);

        _canvas = canvasObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 200;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        _panelRoot = CreatePanel(canvasObject.transform);
    }

    GameObject CreatePanel(Transform parent)
    {
        GameObject panel = CreateUiObject("RulePanel", parent);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(720f, 520f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.06f, 0.1f, 0.18f, 0.94f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(36, 36, 36, 36);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateTitle(panel.transform, panelTitle);
        CreateHint(panel.transform, "按 ESC 关闭");
        return panel;
    }

    void RebuildPanelContents()
    {
        if (_panelRoot == null)
        {
            return;
        }

        // Keep panel root object and layout, rebuild children.
        for (int i = _panelRoot.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_panelRoot.transform.GetChild(i).gameObject);
        }

        if (_phase == PanelPhase.ChooseCivilization)
        {
            CreateTitle(_panelRoot.transform, "请选择你的文明方向：");

            for (int i = 0; i < ruleOptions.Length; i++)
            {
                string rule = ruleOptions[i];
                CreateButton(_panelRoot.transform, rule, () => SelectCivilizationCandidate(rule));
            }

            CreateHint(_panelRoot.transform, "先选方向，再制定规则（ESC 关闭）");
            return;
        }

        CreateTitle(_panelRoot.transform, "请选择你的第一条文明规则：");

        CreateButton(_panelRoot.transform, "A 任何人都可以加入", () => SelectJoinRule(CivilizationRuleSelection.JoinRule.AnyoneCanJoin));
        CreateButton(_panelRoot.transform, "B 需要Pass才能加入", () => SelectJoinRule(CivilizationRuleSelection.JoinRule.PassRequired));
        CreateButton(_panelRoot.transform, "C 需要贡献才能加入", () => SelectJoinRule(CivilizationRuleSelection.JoinRule.ContributionRequired));

        if (!string.IsNullOrEmpty(_pendingCivilizationLabel))
        {
            CreateHint(_panelRoot.transform, $"当前文明方向：{_pendingCivilizationLabel}（ESC 关闭）");
        }
        else
        {
            CreateHint(_panelRoot.transform, "按 ESC 关闭");
        }
    }

    void CreateTitle(Transform parent, string text)
    {
        GameObject titleObject = CreateUiObject("Title", parent);
        LayoutElement layout = titleObject.AddComponent<LayoutElement>();
        layout.minHeight = 72f;
        layout.preferredHeight = 72f;

        Text title = titleObject.AddComponent<Text>();
        title.text = text;
        title.alignment = TextAnchor.MiddleCenter;
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 34;
        title.fontStyle = FontStyle.Bold;
        title.color = new Color(1f, 0.92f, 0.45f);
    }

    Button CreateButton(Transform parent, string label, Action onClick)
    {
        GameObject buttonObject = CreateUiObject(label + "Button", parent);
        LayoutElement layout = buttonObject.AddComponent<LayoutElement>();
        layout.minHeight = 64f;
        layout.preferredHeight = 64f;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.14f, 0.24f, 0.38f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.14f, 0.24f, 0.38f, 1f);
        colors.highlightedColor = new Color(0.22f, 0.38f, 0.58f, 1f);
        colors.pressedColor = new Color(0.28f, 0.48f, 0.72f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.onClick.AddListener(() => onClick());

        GameObject labelObject = CreateUiObject("Label", buttonObject.transform);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        StretchFull(labelRect);

        Text text = labelObject.AddComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 26;
        text.color = new Color(0.85f, 0.95f, 1f);

        return button;
    }

    void CreateHint(Transform parent, string text)
    {
        GameObject hintObject = CreateUiObject("EscHint", parent);
        LayoutElement layout = hintObject.AddComponent<LayoutElement>();
        layout.minHeight = 36f;
        layout.preferredHeight = 36f;

        Text hint = hintObject.AddComponent<Text>();
        hint.text = text;
        hint.alignment = TextAnchor.MiddleCenter;
        hint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hint.fontSize = 18;
        hint.fontStyle = FontStyle.Italic;
        hint.color = new Color(0.75f, 0.85f, 0.95f, 0.85f);
    }

    void SelectCivilizationCandidate(string rule)
    {
        if (!CivilizationManager.TryParseTypeFromLabel(rule, out CivilizationType type) || type == CivilizationType.None)
        {
            Debug.LogWarning($"CivilizationSeed: unknown civilization label '{rule}'");
            return;
        }

        _pendingCivilizationType = type;
        _pendingCivilizationLabel = rule;
        _phase = PanelPhase.ChooseJoinRule;
        RebuildPanelContents();
    }

    void SelectJoinRule(CivilizationRuleSelection.JoinRule joinRule)
    {
        if (_pendingCivilizationType == CivilizationType.None)
        {
            _phase = PanelPhase.ChooseCivilization;
            RebuildPanelContents();
            return;
        }

        CivilizationRuleSelection.Select(joinRule);
        CivilizationManager.SelectCivilization(_pendingCivilizationType);
        CivilizationJournalLog.RecordCivilizationCreated(_pendingCivilizationType, joinRule);

        Debug.Log($"CivilizationSeed: JOIN RULE SELECTED -> {CivilizationRuleSelection.GetDisplayLine(joinRule)}; CIV -> {_pendingCivilizationType}");
        OnRuleSelected?.Invoke(CivilizationRuleSelection.GetDisplayLine(joinRule));
        Close();
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
