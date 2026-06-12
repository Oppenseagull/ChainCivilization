using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Civilization Journal UI toggled with J. Shows auto-recorded timeline entries.
/// </summary>
public class CivilizationJournalUI : MonoBehaviour
{
    Canvas _canvas;
    GameObject _panelRoot;
    GameObject _scrollRoot;
    RectTransform _scrollContent;
    Text _emptyLabel;

    bool _isOpen;
    CursorLockMode _previousLockState;
    bool _previousCursorVisible;

    void Awake()
    {
        BuildUi();
        Close();
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
        if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
        {
            Toggle();
        }

        if (_isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
#else
        if (Input.GetKeyDown(KeyCode.J))
        {
            Toggle();
        }

        if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
#endif
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
        RefreshContent();
        _isOpen = true;
        _panelRoot.SetActive(true);

        _previousLockState = Cursor.lockState;
        _previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Close()
    {
        _isOpen = false;
        if (_panelRoot != null)
        {
            _panelRoot.SetActive(false);
        }

        Cursor.lockState = _previousLockState;
        Cursor.visible = _previousCursorVisible;
    }

    public void ResetJournal()
    {
        Close();
        RefreshContent();
    }

    void RefreshContent()
    {
        if (_scrollContent == null)
        {
            return;
        }

        for (int i = _scrollContent.childCount - 1; i >= 0; i--)
        {
            Destroy(_scrollContent.GetChild(i).gameObject);
        }

        IReadOnlyList<CivilizationJournalLog.Entry> entries = CivilizationJournalLog.Entries;
        bool hasEntries = entries.Count > 0;
        _scrollRoot.SetActive(hasEntries);
        _emptyLabel.gameObject.SetActive(!hasEntries);

        if (!hasEntries)
        {
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            CreateEntryBlock(_scrollContent, entries[i], i < entries.Count - 1);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollContent);
    }

    void CreateEntryBlock(Transform parent, CivilizationJournalLog.Entry entry, bool addSpacer)
    {
        GameObject block = CreateUiObject("JournalEntry", parent);
        VerticalLayoutGroup blockLayout = block.AddComponent<VerticalLayoutGroup>();
        blockLayout.spacing = 4f;
        blockLayout.childAlignment = TextAnchor.UpperLeft;
        blockLayout.childControlWidth = true;
        blockLayout.childControlHeight = true;
        blockLayout.childForceExpandWidth = true;
        blockLayout.childForceExpandHeight = false;

        LayoutElement blockElement = block.AddComponent<LayoutElement>();
        blockElement.minWidth = 620f;
        blockElement.preferredWidth = 620f;

        CreateLine(block.transform, entry.DayLabel, 22, FontStyle.Bold, new Color(1f, 0.92f, 0.45f));

        for (int i = 0; i < entry.Lines.Count; i++)
        {
            CreateLine(block.transform, entry.Lines[i], 20, FontStyle.Normal, new Color(0.88f, 0.95f, 1f));
        }

        if (addSpacer)
        {
            GameObject spacer = CreateUiObject("Spacer", block.transform);
            LayoutElement spacerLayout = spacer.AddComponent<LayoutElement>();
            spacerLayout.minHeight = 16f;
            spacerLayout.preferredHeight = 16f;
        }
    }

    void CreateLine(Transform parent, string text, int fontSize, FontStyle fontStyle, Color color)
    {
        GameObject lineObject = CreateUiObject("Line", parent);
        LayoutElement layout = lineObject.AddComponent<LayoutElement>();
        layout.minHeight = fontSize + 8f;
        layout.preferredHeight = fontSize + 8f;

        Text line = lineObject.AddComponent<Text>();
        line.text = text;
        line.alignment = TextAnchor.MiddleLeft;
        line.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        line.fontSize = fontSize;
        line.fontStyle = fontStyle;
        line.color = color;
        line.horizontalOverflow = HorizontalWrapMode.Wrap;
        line.verticalOverflow = VerticalWrapMode.Overflow;
    }

    void BuildUi()
    {
        EnsureEventSystem();

        GameObject canvasObject = new GameObject("CivilizationJournalCanvas");
        canvasObject.transform.SetParent(transform, false);

        _canvas = canvasObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 180;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        _panelRoot = CreatePanel(canvasObject.transform);
    }

    GameObject CreatePanel(Transform parent)
    {
        GameObject panel = CreateUiObject("JournalPanel", parent);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(760f, 560f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.09f, 0.16f, 0.95f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(32, 32, 28, 24);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateTitle(panel.transform, "文明日志");
        CreateScrollArea(panel.transform);
        _emptyLabel = CreateEmptyLabel(panel.transform);
        CreateHint(panel.transform, "按 J 或 ESC 关闭");

        return panel;
    }

    void CreateScrollArea(Transform parent)
    {
        GameObject scrollObject = CreateUiObject("JournalScroll", parent);
        _scrollRoot = scrollObject;
        LayoutElement scrollLayout = scrollObject.AddComponent<LayoutElement>();
        scrollLayout.minHeight = 420f;
        scrollLayout.preferredHeight = 420f;
        scrollLayout.flexibleHeight = 1f;

        Image scrollBackground = scrollObject.AddComponent<Image>();
        scrollBackground.color = new Color(0.03f, 0.06f, 0.11f, 0.55f);

        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 24f;

        GameObject viewport = CreateUiObject("Viewport", scrollObject.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        StretchFull(viewportRect);
        viewport.AddComponent<RectMask2D>();
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = Color.clear;

        GameObject content = CreateUiObject("Content", viewport.transform);
        _scrollContent = content.GetComponent<RectTransform>();
        _scrollContent.anchorMin = new Vector2(0f, 1f);
        _scrollContent.anchorMax = new Vector2(1f, 1f);
        _scrollContent.pivot = new Vector2(0.5f, 1f);
        _scrollContent.anchoredPosition = Vector2.zero;
        _scrollContent.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(16, 16, 12, 12);
        contentLayout.spacing = 0f;
        contentLayout.childAlignment = TextAnchor.UpperLeft;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = _scrollContent;
    }

    Text CreateEmptyLabel(Transform parent)
    {
        GameObject emptyObject = CreateUiObject("EmptyLabel", parent);
        LayoutElement layout = emptyObject.AddComponent<LayoutElement>();
        layout.minHeight = 420f;
        layout.preferredHeight = 420f;

        Text empty = emptyObject.AddComponent<Text>();
        empty.text = "暂无记录\n探索世界后自动写入日志";
        empty.alignment = TextAnchor.MiddleCenter;
        empty.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        empty.fontSize = 20;
        empty.fontStyle = FontStyle.Italic;
        empty.color = new Color(0.65f, 0.75f, 0.85f, 0.9f);
        empty.gameObject.SetActive(true);
        return empty;
    }

    void CreateTitle(Transform parent, string text)
    {
        GameObject titleObject = CreateUiObject("JournalTitle", parent);
        LayoutElement layout = titleObject.AddComponent<LayoutElement>();
        layout.minHeight = 48f;
        layout.preferredHeight = 48f;

        Text title = titleObject.AddComponent<Text>();
        title.text = text;
        title.alignment = TextAnchor.MiddleCenter;
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 34;
        title.fontStyle = FontStyle.Bold;
        title.color = new Color(1f, 0.92f, 0.45f);
    }

    void CreateHint(Transform parent, string text)
    {
        GameObject hintObject = CreateUiObject("JournalHint", parent);
        LayoutElement layout = hintObject.AddComponent<LayoutElement>();
        layout.minHeight = 24f;
        layout.preferredHeight = 24f;

        Text hint = hintObject.AddComponent<Text>();
        hint.text = text;
        hint.alignment = TextAnchor.MiddleCenter;
        hint.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hint.fontSize = 18;
        hint.fontStyle = FontStyle.Italic;
        hint.color = new Color(0.7f, 0.8f, 0.9f, 0.85f);
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
