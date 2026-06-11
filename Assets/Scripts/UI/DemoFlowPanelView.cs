using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hackathon demo flow checklist for TAB menu. Syncs with MainQuestManager quest steps.
/// </summary>
public static class DemoFlowPanelView
{
    public enum StepVisualState
    {
        Upcoming,
        Current,
        Completed
    }

    struct DemoStep
    {
        public string StepLabel;
        public string Title;
        public int QuestStepIndex;
    }

    public sealed class DemoFlowWidget
    {
        public RectTransform Content;
    }

    static readonly DemoStep[] Steps =
    {
        new DemoStep { StepLabel = "Step1", Title = "发现 Blue DAO", QuestStepIndex = 0 },
        new DemoStep { StepLabel = "Step2", Title = "发现 Red DAO", QuestStepIndex = 1 },
        new DemoStep { StepLabel = "Step3", Title = "收集 Moon", QuestStepIndex = 2 },
        new DemoStep { StepLabel = "Step4", Title = "获得 Green Pass", QuestStepIndex = 4 },
        new DemoStep { StepLabel = "Step5", Title = "抵达文明边界", QuestStepIndex = 5 },
        new DemoStep { StepLabel = "Step6", Title = "获得文明种子", QuestStepIndex = 5 },
        new DemoStep { StepLabel = "Step7", Title = "建立文明", QuestStepIndex = 6 }
    };

    static readonly Color CompletedCard = new Color(0.12f, 0.24f, 0.16f, 0.95f);
    static readonly Color CompletedBorder = new Color(0.32f, 0.82f, 0.48f, 0.9f);
    static readonly Color CompletedText = new Color(0.45f, 0.95f, 0.62f);
    static readonly Color CurrentCard = new Color(0.28f, 0.22f, 0.08f, 0.98f);
    static readonly Color CurrentBorder = new Color(1f, 0.82f, 0.32f, 0.95f);
    static readonly Color CurrentText = new Color(1f, 0.92f, 0.45f);
    static readonly Color UpcomingCard = new Color(0.14f, 0.15f, 0.17f, 0.9f);
    static readonly Color UpcomingBorder = new Color(0.34f, 0.36f, 0.4f, 0.75f);
    static readonly Color UpcomingText = new Color(0.52f, 0.56f, 0.6f);

    static Font _font;
    static Sprite _cardSprite;

    public static DemoFlowWidget Build(Transform parent, float width)
    {
        EnsureResources();

        GameObject root = CreateUiObject("DemoFlowView", parent);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        StretchFull(rootRect);

        LayoutElement rootLayout = root.AddComponent<LayoutElement>();
        rootLayout.minHeight = 420f;
        rootLayout.preferredHeight = 420f;
        rootLayout.flexibleHeight = 1f;

        ScrollRect scroll = root.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 24f;

        GameObject viewport = CreateUiObject("Viewport", root.transform);
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
        contentLayout.padding = new RectOffset(12, 12, 12, 12);
        contentLayout.spacing = 10f;
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewportRect;
        scroll.content = content;

        return new DemoFlowWidget { Content = content };
    }

    public static void Refresh(DemoFlowWidget widget, MainQuestManager questManager)
    {
        if (widget == null || widget.Content == null)
        {
            return;
        }

        ClearChildren(widget.Content);
        CreateHeader(widget.Content, questManager);
        CreateSpacer(widget.Content, 6f);

        for (int i = 0; i < Steps.Length; i++)
        {
            StepVisualState state = ResolveStepState(i, questManager);
            CreateStepCard(widget.Content, Steps[i], state);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(widget.Content);

        ScrollRect scroll = widget.Content.GetComponentInParent<ScrollRect>();
        if (scroll != null)
        {
            RectTransform scrollRoot = scroll.GetComponent<RectTransform>();
            if (scrollRoot != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRoot);
            }

            scroll.verticalNormalizedPosition = 1f;
        }
    }

    static void CreateHeader(Transform parent, MainQuestManager questManager)
    {
        GameObject header = CreateUiObject("DemoFlowHeader", parent);
        LayoutElement layout = header.AddComponent<LayoutElement>();
        layout.minHeight = 72f;
        layout.preferredHeight = 72f;

        VerticalLayoutGroup column = header.AddComponent<VerticalLayoutGroup>();
        column.spacing = 6f;
        column.childAlignment = TextAnchor.MiddleCenter;
        column.childControlWidth = true;
        column.childControlHeight = true;
        column.childForceExpandWidth = true;
        column.childForceExpandHeight = false;

        GameObject titleObject = CreateUiObject("Title", header.transform);
        LayoutElement titleLayout = titleObject.AddComponent<LayoutElement>();
        titleLayout.minHeight = 34f;
        titleLayout.preferredHeight = 34f;
        Text title = titleObject.AddComponent<Text>();
        title.text = "Demo Guide";
        title.font = _font;
        title.fontSize = 30;
        title.fontStyle = FontStyle.Bold;
        title.alignment = TextAnchor.MiddleCenter;
        title.color = new Color(0.55f, 0.88f, 1f);

        int completed = questManager != null ? questManager.GetDemoCompletedMilestoneCount() : 0;
        GameObject progressObject = CreateUiObject("Progress", header.transform);
        LayoutElement progressLayout = progressObject.AddComponent<LayoutElement>();
        progressLayout.minHeight = 24f;
        progressLayout.preferredHeight = 24f;
        Text progress = progressObject.AddComponent<Text>();
        progress.text = $"{completed} / {MainQuestManager.DemoMilestoneTotal} Completed";
        progress.font = _font;
        progress.fontSize = 18;
        progress.fontStyle = FontStyle.Italic;
        progress.alignment = TextAnchor.MiddleCenter;
        progress.color = new Color(0.42f, 0.9f, 0.68f);
    }

    static StepVisualState ResolveStepState(int stepIndex, MainQuestManager questManager)
    {
        if (questManager == null)
        {
            return stepIndex == 0 ? StepVisualState.Current : StepVisualState.Upcoming;
        }

        if (IsStepCompleted(stepIndex, questManager))
        {
            return StepVisualState.Completed;
        }

        if (IsStepCurrent(stepIndex, questManager))
        {
            return StepVisualState.Current;
        }

        return StepVisualState.Upcoming;
    }

    static bool IsStepCompleted(int stepIndex, MainQuestManager questManager)
    {
        const int goToBoundary = 5;
        const int createCivilization = 6;

        switch (stepIndex)
        {
            case 4:
                return questManager.IsDemoMilestoneComplete(goToBoundary);
            case 5:
                return questManager.CurrentQuestIndex >= createCivilization
                    || questManager.IsAllQuestsComplete;
            case 6:
                return questManager.IsDemoMilestoneComplete(createCivilization)
                    || CivilizationManager.HasSelectedCivilization
                    || questManager.IsAllQuestsComplete;
            default:
                return questManager.IsDemoMilestoneComplete(Steps[stepIndex].QuestStepIndex);
        }
    }

    static bool IsStepCurrent(int stepIndex, MainQuestManager questManager)
    {
        if (questManager.IsAllQuestsComplete)
        {
            return false;
        }

        const int goToBoundary = 5;
        const int createCivilization = 6;

        switch (stepIndex)
        {
            case 4:
                return questManager.IsDemoGuideRowCurrent(goToBoundary);
            case 5:
                return questManager.CurrentQuestIndex == goToBoundary;
            case 6:
                return questManager.CurrentQuestIndex == createCivilization
                    && !CivilizationManager.HasSelectedCivilization;
            default:
                return questManager.IsDemoGuideRowCurrent(Steps[stepIndex].QuestStepIndex);
        }
    }

    static void CreateStepCard(Transform parent, DemoStep step, StepVisualState state)
    {
        Color cardColor;
        Color borderColor;
        Color labelColor;
        Color titleColor;
        string statusMark;

        switch (state)
        {
            case StepVisualState.Completed:
                cardColor = CompletedCard;
                borderColor = CompletedBorder;
                labelColor = CompletedText;
                titleColor = CompletedText;
                statusMark = "\u2713";
                break;
            case StepVisualState.Current:
                cardColor = CurrentCard;
                borderColor = CurrentBorder;
                labelColor = CurrentText;
                titleColor = CurrentText;
                statusMark = "\u25b6";
                break;
            default:
                cardColor = UpcomingCard;
                borderColor = UpcomingBorder;
                labelColor = UpcomingText;
                titleColor = UpcomingText;
                statusMark = "\u25cb";
                break;
        }

        GameObject card = CreateUiObject($"Step_{step.StepLabel}", parent);
        LayoutElement cardLayout = card.AddComponent<LayoutElement>();
        cardLayout.minHeight = 64f;
        cardLayout.preferredHeight = 64f;
        cardLayout.minWidth = 760f;
        cardLayout.preferredWidth = 760f;

        Image cardImage = card.AddComponent<Image>();
        cardImage.sprite = _cardSprite;
        cardImage.type = Image.Type.Sliced;
        cardImage.color = cardColor;

        HorizontalLayoutGroup row = card.AddComponent<HorizontalLayoutGroup>();
        row.padding = new RectOffset(16, 16, 10, 10);
        row.spacing = 14f;
        row.childAlignment = TextAnchor.MiddleLeft;
        row.childControlWidth = false;
        row.childControlHeight = true;
        row.childForceExpandWidth = false;
        row.childForceExpandHeight = true;

        GameObject markObject = CreateUiObject("Mark", card.transform);
        LayoutElement markLayout = markObject.AddComponent<LayoutElement>();
        markLayout.minWidth = 28f;
        markLayout.preferredWidth = 28f;
        Text mark = markObject.AddComponent<Text>();
        mark.text = statusMark;
        mark.font = _font;
        mark.fontSize = 20;
        mark.fontStyle = FontStyle.Bold;
        mark.alignment = TextAnchor.MiddleCenter;
        mark.color = labelColor;

        GameObject textColumn = CreateUiObject("TextColumn", card.transform);
        LayoutElement textColumnLayout = textColumn.AddComponent<LayoutElement>();
        textColumnLayout.flexibleWidth = 1f;
        textColumnLayout.minWidth = 500f;

        VerticalLayoutGroup textLayout = textColumn.AddComponent<VerticalLayoutGroup>();
        textLayout.spacing = 2f;
        textLayout.childAlignment = TextAnchor.MiddleLeft;
        textLayout.childControlWidth = true;
        textLayout.childControlHeight = true;
        textLayout.childForceExpandWidth = true;
        textLayout.childForceExpandHeight = false;

        GameObject stepLabelObject = CreateUiObject("StepLabel", textColumn.transform);
        LayoutElement stepLabelLayout = stepLabelObject.AddComponent<LayoutElement>();
        stepLabelLayout.minHeight = 22f;
        stepLabelLayout.preferredHeight = 22f;
        Text stepLabel = stepLabelObject.AddComponent<Text>();
        stepLabel.text = step.StepLabel;
        stepLabel.font = _font;
        stepLabel.fontSize = 15;
        stepLabel.fontStyle = FontStyle.Bold;
        stepLabel.alignment = TextAnchor.MiddleLeft;
        stepLabel.color = labelColor;

        GameObject titleObject = CreateUiObject("Title", textColumn.transform);
        LayoutElement titleLayout = titleObject.AddComponent<LayoutElement>();
        titleLayout.minHeight = 26f;
        titleLayout.preferredHeight = 26f;
        Text title = titleObject.AddComponent<Text>();
        title.text = step.Title;
        title.font = _font;
        title.fontSize = state == StepVisualState.Current ? 22 : 20;
        title.fontStyle = state == StepVisualState.Current ? FontStyle.Bold : FontStyle.Normal;
        title.alignment = TextAnchor.MiddleLeft;
        title.color = titleColor;

        GameObject border = CreateUiObject("Border", card.transform);
        RectTransform borderRect = border.GetComponent<RectTransform>();
        StretchFull(borderRect);
        borderRect.offsetMin = new Vector2(2f, 2f);
        borderRect.offsetMax = new Vector2(-2f, -2f);
        Image borderImage = border.AddComponent<Image>();
        borderImage.sprite = _cardSprite;
        borderImage.type = Image.Type.Sliced;
        borderImage.color = borderColor;
        border.transform.SetAsFirstSibling();
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

        if (_cardSprite == null)
        {
            Texture2D texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[64];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            _cardSprite = Sprite.Create(texture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 8f);
        }
    }

    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(parent.GetChild(i).gameObject);
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
