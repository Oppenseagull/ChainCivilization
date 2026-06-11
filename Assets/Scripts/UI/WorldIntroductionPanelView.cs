using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hackathon demo world introduction. Large-type auto layout for TAB menu.
/// </summary>
public static class WorldIntroductionPanelView
{
    static readonly string[] BodyLines =
    {
        "你是一名链上流浪者。",
        "探索不同 DAO 文明。",
        "理解不同规则。",
        "获得 DAO Pass。",
        "穿越文明边界。",
        "建立属于自己的文明。"
    };

    const string ClosingLine = "这是一个关于协作与共识的世界。";

    static Font _font;

    public static GameObject Build(Transform parent)
    {
        EnsureResources();

        GameObject page = CreateUiObject("ChainCivilizationPage", parent);
        StretchFull(page.GetComponent<RectTransform>());

        LayoutElement pageLayout = page.AddComponent<LayoutElement>();
        pageLayout.minHeight = 420f;
        pageLayout.preferredHeight = 420f;
        pageLayout.flexibleHeight = 1f;

        Image pageBackground = page.AddComponent<Image>();
        pageBackground.color = new Color(0.08f, 0.14f, 0.22f, 0.92f);

        ScrollRect scroll = page.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;

        GameObject viewport = CreateUiObject("Viewport", page.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        StretchFull(viewportRect);
        viewport.AddComponent<RectMask2D>();

        GameObject contentObject = CreateUiObject("Content", viewport.transform);
        RectTransform content = contentObject.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup contentLayout = contentObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(48, 48, 40, 40);
        contentLayout.spacing = 18f;
        contentLayout.childAlignment = TextAnchor.MiddleCenter;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        CreateHeroTitle(content, "Chain Civilization");
        CreateDivider(content);
        CreateIntroLine(content, BodyLines[0], 28, FontStyle.Bold, new Color(1f, 0.94f, 0.62f));

        for (int i = 1; i < BodyLines.Length; i++)
        {
            CreateIntroLine(content, BodyLines[i], 26, FontStyle.Normal, new Color(0.9f, 0.96f, 1f));
        }

        CreateSpacer(content, 12f);
        CreateDivider(content);
        CreateIntroLine(content, ClosingLine, 24, FontStyle.Italic, new Color(0.72f, 0.88f, 0.95f));

        scroll.viewport = viewportRect;
        scroll.content = content;
        return page;
    }

    static void CreateHeroTitle(Transform parent, string text)
    {
        GameObject titleObject = CreateUiObject("HeroTitle", parent);
        LayoutElement layout = titleObject.AddComponent<LayoutElement>();
        layout.minHeight = 56f;
        layout.preferredHeight = 56f;

        Text title = titleObject.AddComponent<Text>();
        title.text = text;
        title.font = _font;
        title.fontSize = 40;
        title.fontStyle = FontStyle.Bold;
        title.alignment = TextAnchor.MiddleCenter;
        title.color = new Color(1f, 0.88f, 0.42f);
        title.horizontalOverflow = HorizontalWrapMode.Wrap;
        title.verticalOverflow = VerticalWrapMode.Overflow;
    }

    static void CreateIntroLine(Transform parent, string text, int fontSize, FontStyle fontStyle, Color color)
    {
        GameObject lineObject = CreateUiObject("IntroLine", parent);
        LayoutElement layout = lineObject.AddComponent<LayoutElement>();
        layout.minHeight = fontSize + 16f;
        layout.preferredHeight = fontSize + 16f;

        Text line = lineObject.AddComponent<Text>();
        line.text = text;
        line.font = _font;
        line.fontSize = fontSize;
        line.fontStyle = fontStyle;
        line.alignment = TextAnchor.MiddleCenter;
        line.color = color;
        line.lineSpacing = 1.1f;
        line.horizontalOverflow = HorizontalWrapMode.Wrap;
        line.verticalOverflow = VerticalWrapMode.Overflow;
    }

    static void CreateDivider(Transform parent)
    {
        GameObject dividerObject = CreateUiObject("Divider", parent);
        LayoutElement layout = dividerObject.AddComponent<LayoutElement>();
        layout.minHeight = 20f;
        layout.preferredHeight = 20f;

        Text divider = dividerObject.AddComponent<Text>();
        divider.text = "—  —  —";
        divider.font = _font;
        divider.fontSize = 18;
        divider.fontStyle = FontStyle.Italic;
        divider.alignment = TextAnchor.MiddleCenter;
        divider.color = new Color(0.45f, 0.62f, 0.78f, 0.75f);
    }

    static void CreateSpacer(Transform parent, float height)
    {
        GameObject spacer = CreateUiObject("Spacer", parent);
        LayoutElement layout = spacer.AddComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;
    }

    static void EnsureResources()
    {
        if (_font == null)
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
}
