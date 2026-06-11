using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sims-style inventory cards. Reads MOON, Green DAO Pass, Civilization Seed from existing managers.
/// </summary>
public static class InventoryPanelView
{
    public struct ItemEntry
    {
        public string Name;
        public string Description;
        public string CountLabel;
        public Color IconColor;
        public Color IconAccent;
        public bool IsOwned;
        public bool IsPlaceholder;
    }

    public struct ItemSection
    {
        public string Title;
        public List<ItemEntry> Items;
    }

    public sealed class InventoryWidget
    {
        public RectTransform Content;
    }

    static readonly Color CardBackground = new Color(0.96f, 0.95f, 0.92f, 0.98f);
    static readonly Color CardBorder = new Color(0.78f, 0.76f, 0.72f, 0.9f);
    static readonly Color CardLocked = new Color(0.88f, 0.89f, 0.9f, 0.85f);
    static readonly Color SectionTitle = new Color(0.38f, 0.58f, 0.74f);
    static readonly Color TitleText = new Color(0.18f, 0.22f, 0.28f);
    static readonly Color BodyText = new Color(0.42f, 0.46f, 0.5f);
    static readonly Color PlaceholderText = new Color(0.58f, 0.6f, 0.64f, 0.85f);

    static Font _font;
    static Sprite _cardSprite;
    static Sprite _iconSprite;

    public static InventoryWidget Build(Transform parent, float width)
    {
        EnsureResources();

        GameObject root = CreateUiObject("InventoryView", parent);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(width, 0f);

        LayoutElement rootLayout = root.AddComponent<LayoutElement>();
        rootLayout.minWidth = width;
        rootLayout.preferredWidth = width;
        rootLayout.flexibleWidth = 1f;

        GameObject scrollObject = CreateUiObject("InventoryScroll", root.transform);
        RectTransform scrollRect = scrollObject.GetComponent<RectTransform>();
        StretchFull(scrollRect);

        LayoutElement scrollLayout = scrollObject.AddComponent<LayoutElement>();
        scrollLayout.minHeight = 420f;
        scrollLayout.preferredHeight = 420f;
        scrollLayout.flexibleHeight = 1f;

        ScrollRect scroll = scrollObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 24f;

        GameObject viewport = CreateUiObject("Viewport", scrollObject.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        StretchFull(viewportRect);
        viewport.AddComponent<RectMask2D>();

        GameObject content = CreateUiObject("Content", viewport.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(8, 8, 8, 8);
        contentLayout.spacing = 14f;
        contentLayout.childAlignment = TextAnchor.UpperLeft;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewportRect;
        scroll.content = contentRect;

        return new InventoryWidget { Content = contentRect };
    }

    public static void Refresh(InventoryWidget widget)
    {
        if (widget == null || widget.Content == null)
        {
            return;
        }

        ClearChildren(widget.Content);
        List<ItemSection> sections = BuildSections();

        for (int i = 0; i < sections.Count; i++)
        {
            CreateSection(widget.Content, sections[i]);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(widget.Content);
    }

    public static List<ItemSection> BuildSections()
    {
        TokenManager tokens = TokenManager.Instance;
        DAOPassManager passes = DAOPassManager.Instance;
        MainQuestManager questManager = Object.FindFirstObjectByType<MainQuestManager>();

        int moon = tokens != null ? tokens.MoonBalance : 0;
        bool hasPass = passes != null && passes.HasGreenPass;
        bool hasCivilization = CivilizationManager.HasSelectedCivilization;
        bool boundaryReached = questManager != null && questManager.IsDemoMilestoneComplete(5);

        List<ItemSection> sections = new List<ItemSection>();

        sections.Add(new ItemSection
        {
            Title = "资源",
            Items = new List<ItemEntry>
            {
                new ItemEntry
                {
                    Name = "MOON",
                    Description = "链上激励代币。用于 DAO 互动、资源积累与文明准入。",
                    CountLabel = $"x{moon}",
                    IconColor = new Color(1f, 0.86f, 0.32f),
                    IconAccent = new Color(0.92f, 0.68f, 0.12f),
                    IsOwned = true
                }
            }
        });

        sections.Add(new ItemSection
        {
            Title = "DAO Pass",
            Items = new List<ItemEntry>
            {
                new ItemEntry
                {
                    Name = "Green DAO Pass",
                    Description = hasPass
                        ? "Green DAO 准入凭证。代表你在开放协作网络中的身份。"
                        : "尚未获得。满足 MOON 与 REP 条件后，可在 Green DAO 申领。",
                    CountLabel = hasPass ? "x1" : string.Empty,
                    IconColor = hasPass ? new Color(0.42f, 0.88f, 0.55f) : new Color(0.62f, 0.68f, 0.66f),
                    IconAccent = hasPass ? new Color(0.2f, 0.62f, 0.38f) : new Color(0.48f, 0.52f, 0.5f),
                    IsOwned = hasPass
                }
            }
        });

        string seedDescription;
        bool seedOwned = hasCivilization;
        bool seedDiscovered = boundaryReached || hasCivilization;

        if (hasCivilization)
        {
            string civName = CivilizationBonuses.GetAddressPanelCivilizationName(CivilizationManager.selectedCivilizationType);
            seedDescription = $"已用于创造文明：{civName}。";
        }
        else if (boundaryReached)
        {
            seedDescription = "已在边界发现。前往 Civilization Seed 植入你的第一条规则。";
        }
        else
        {
            seedDescription = "尚未发现。抵达文明边界后可取得。";
        }

        sections.Add(new ItemSection
        {
            Title = "文明",
            Items = new List<ItemEntry>
            {
                new ItemEntry
                {
                    Name = "Civilization Seed",
                    Description = seedDescription,
                    CountLabel = seedOwned ? "x1" : string.Empty,
                    IconColor = seedDiscovered ? new Color(0.45f, 0.82f, 0.92f) : new Color(0.62f, 0.68f, 0.72f),
                    IconAccent = seedDiscovered ? new Color(0.18f, 0.55f, 0.72f) : new Color(0.48f, 0.52f, 0.56f),
                    IsOwned = seedOwned || seedDiscovered
                }
            }
        });

        sections.Add(new ItemSection
        {
            Title = "未来道具",
            Items = new List<ItemEntry>
            {
                new ItemEntry
                {
                    Name = "预留栏位",
                    Description = "更多文明道具将在后续版本加入。",
                    CountLabel = string.Empty,
                    IconColor = new Color(0.72f, 0.74f, 0.76f),
                    IconAccent = new Color(0.58f, 0.6f, 0.62f),
                    IsOwned = false,
                    IsPlaceholder = true
                },
                new ItemEntry
                {
                    Name = "预留栏位",
                    Description = "探索、身份与共识相关物品会显示在这里。",
                    CountLabel = string.Empty,
                    IconColor = new Color(0.72f, 0.74f, 0.76f),
                    IconAccent = new Color(0.58f, 0.6f, 0.62f),
                    IsOwned = false,
                    IsPlaceholder = true
                }
            }
        });

        return sections;
    }

    static void CreateSection(Transform parent, ItemSection section)
    {
        GameObject sectionRoot = CreateUiObject($"Section_{section.Title}", parent);
        VerticalLayoutGroup sectionLayout = sectionRoot.AddComponent<VerticalLayoutGroup>();
        sectionLayout.spacing = 8f;
        sectionLayout.childAlignment = TextAnchor.UpperLeft;
        sectionLayout.childControlWidth = true;
        sectionLayout.childControlHeight = true;
        sectionLayout.childForceExpandWidth = true;
        sectionLayout.childForceExpandHeight = false;

        LayoutElement sectionElement = sectionRoot.AddComponent<LayoutElement>();
        sectionElement.flexibleWidth = 1f;

        CreateSectionHeader(sectionRoot.transform, section.Title);

        for (int i = 0; i < section.Items.Count; i++)
        {
            CreateItemCard(sectionRoot.transform, section.Items[i]);
        }
    }

    static void CreateSectionHeader(Transform parent, string title)
    {
        GameObject headerObject = CreateUiObject("SectionHeader", parent);
        LayoutElement layout = headerObject.AddComponent<LayoutElement>();
        layout.minHeight = 28f;
        layout.preferredHeight = 28f;

        Text header = headerObject.AddComponent<Text>();
        header.text = title;
        header.font = _font;
        header.fontSize = 18;
        header.fontStyle = FontStyle.Bold;
        header.alignment = TextAnchor.MiddleLeft;
        header.color = SectionTitle;
    }

    static void CreateItemCard(Transform parent, ItemEntry entry)
    {
        GameObject card = CreateUiObject($"Card_{entry.Name}", parent);
        LayoutElement cardLayout = card.AddComponent<LayoutElement>();
        cardLayout.minHeight = 92f;
        cardLayout.preferredHeight = 92f;

        Image cardImage = card.AddComponent<Image>();
        cardImage.sprite = _cardSprite;
        cardImage.type = Image.Type.Sliced;
        cardImage.color = entry.IsPlaceholder || !entry.IsOwned ? CardLocked : CardBackground;

        HorizontalLayoutGroup row = card.AddComponent<HorizontalLayoutGroup>();
        row.padding = new RectOffset(14, 14, 12, 12);
        row.spacing = 14f;
        row.childAlignment = TextAnchor.MiddleLeft;
        row.childControlWidth = false;
        row.childControlHeight = true;
        row.childForceExpandWidth = false;
        row.childForceExpandHeight = true;

        CreateIconBlock(card.transform, entry);
        CreateTextBlock(card.transform, entry);

        if (!entry.IsPlaceholder)
        {
            GameObject border = CreateUiObject("Border", card.transform);
            RectTransform borderRect = border.GetComponent<RectTransform>();
            StretchFull(borderRect);
            borderRect.offsetMin = new Vector2(1f, 1f);
            borderRect.offsetMax = new Vector2(-1f, -1f);
            Image borderImage = border.AddComponent<Image>();
            borderImage.sprite = _cardSprite;
            borderImage.type = Image.Type.Sliced;
            borderImage.color = CardBorder;
            border.transform.SetAsFirstSibling();
        }
    }

    static void CreateIconBlock(Transform parent, ItemEntry entry)
    {
        GameObject iconRoot = CreateUiObject("Icon", parent);
        LayoutElement iconLayout = iconRoot.AddComponent<LayoutElement>();
        iconLayout.minWidth = 64f;
        iconLayout.preferredWidth = 64f;
        iconLayout.minHeight = 64f;
        iconLayout.preferredHeight = 64f;

        Image iconBackground = iconRoot.AddComponent<Image>();
        iconBackground.sprite = _iconSprite;
        iconBackground.type = Image.Type.Sliced;
        iconBackground.color = entry.IconAccent;

        GameObject iconInner = CreateUiObject("IconInner", iconRoot.transform);
        RectTransform innerRect = iconInner.GetComponent<RectTransform>();
        innerRect.anchorMin = new Vector2(0.5f, 0.5f);
        innerRect.anchorMax = new Vector2(0.5f, 0.5f);
        innerRect.pivot = new Vector2(0.5f, 0.5f);
        innerRect.sizeDelta = new Vector2(46f, 46f);
        innerRect.anchoredPosition = Vector2.zero;

        Image innerImage = iconInner.AddComponent<Image>();
        innerImage.sprite = _iconSprite;
        innerImage.type = Image.Type.Sliced;
        innerImage.color = entry.IconColor;

        GameObject symbolObject = CreateUiObject("Symbol", iconInner.transform);
        StretchFull(symbolObject.GetComponent<RectTransform>());
        Text symbol = symbolObject.AddComponent<Text>();
        symbol.text = GetIconSymbol(entry.Name);
        symbol.font = _font;
        symbol.fontSize = entry.Name == "MOON" ? 22 : 18;
        symbol.fontStyle = FontStyle.Bold;
        symbol.alignment = TextAnchor.MiddleCenter;
        symbol.color = new Color(0.12f, 0.16f, 0.2f, 0.85f);
    }

    static void CreateTextBlock(Transform parent, ItemEntry entry)
    {
        GameObject textRoot = CreateUiObject("TextBlock", parent);
        LayoutElement textLayout = textRoot.AddComponent<LayoutElement>();
        textLayout.flexibleWidth = 1f;
        textLayout.minWidth = 220f;

        VerticalLayoutGroup column = textRoot.AddComponent<VerticalLayoutGroup>();
        column.spacing = 4f;
        column.childAlignment = TextAnchor.UpperLeft;
        column.childControlWidth = true;
        column.childControlHeight = true;
        column.childForceExpandWidth = true;
        column.childForceExpandHeight = false;

        GameObject titleRow = CreateUiObject("TitleRow", textRoot.transform);
        LayoutElement titleRowLayout = titleRow.AddComponent<LayoutElement>();
        titleRowLayout.minHeight = 24f;
        titleRowLayout.preferredHeight = 24f;

        HorizontalLayoutGroup titleGroup = titleRow.AddComponent<HorizontalLayoutGroup>();
        titleGroup.spacing = 8f;
        titleGroup.childAlignment = TextAnchor.MiddleLeft;
        titleGroup.childControlWidth = true;
        titleGroup.childControlHeight = true;
        titleGroup.childForceExpandWidth = false;
        titleGroup.childForceExpandHeight = true;

        GameObject titleObject = CreateUiObject("Name", titleRow.transform);
        LayoutElement titleLayout = titleObject.AddComponent<LayoutElement>();
        titleLayout.flexibleWidth = 1f;
        Text title = titleObject.AddComponent<Text>();
        title.text = entry.Name;
        title.font = _font;
        title.fontSize = 18;
        title.fontStyle = FontStyle.Bold;
        title.alignment = TextAnchor.MiddleLeft;
        title.color = entry.IsPlaceholder ? PlaceholderText : TitleText;

        if (!string.IsNullOrEmpty(entry.CountLabel))
        {
            GameObject countObject = CreateUiObject("Count", titleRow.transform);
            LayoutElement countLayout = countObject.AddComponent<LayoutElement>();
            countLayout.minWidth = 48f;
            countLayout.preferredWidth = 48f;
            Text count = countObject.AddComponent<Text>();
            count.text = entry.CountLabel;
            count.font = _font;
            count.fontSize = 16;
            count.fontStyle = FontStyle.Bold;
            count.alignment = TextAnchor.MiddleRight;
            count.color = new Color(0.28f, 0.52f, 0.68f);
        }

        GameObject descObject = CreateUiObject("Description", textRoot.transform);
        LayoutElement descLayout = descObject.AddComponent<LayoutElement>();
        descLayout.minHeight = 40f;
        descLayout.preferredHeight = 40f;
        descLayout.flexibleHeight = 1f;

        Text description = descObject.AddComponent<Text>();
        description.text = entry.Description;
        description.font = _font;
        description.fontSize = 14;
        description.fontStyle = FontStyle.Italic;
        description.alignment = TextAnchor.UpperLeft;
        description.color = entry.IsPlaceholder ? PlaceholderText : BodyText;
        description.horizontalOverflow = HorizontalWrapMode.Wrap;
        description.verticalOverflow = VerticalWrapMode.Overflow;
    }

    static string GetIconSymbol(string itemName)
    {
        switch (itemName)
        {
            case "MOON":
                return "M";
            case "Green DAO Pass":
                return "P";
            case "Civilization Seed":
                return "S";
            default:
                return "?";
        }
    }

    static void EnsureResources()
    {
        if (_font == null)
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        if (_cardSprite == null)
        {
            _cardSprite = CreateRoundedSprite();
        }

        if (_iconSprite == null)
        {
            _iconSprite = CreateRoundedSprite();
        }
    }

    static Sprite CreateRoundedSprite()
    {
        Texture2D texture = new Texture2D(12, 12, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[12 * 12];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 12f, 12f), new Vector2(0.5f, 0.5f), 12f);
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
