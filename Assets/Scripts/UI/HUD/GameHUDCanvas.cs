using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unified screen-space HUD: minimap (top-left), status and quest tracker (right), controls (bottom-left), interact (bottom-center).
/// </summary>
public class GameHUDCanvas : MonoBehaviour
{
    public static GameHUDCanvas Instance { get; private set; }
    public static bool IsActive => Instance != null;

    MainQuestManager _questManager;
    Transform _player;

    Canvas _canvas;
    ExplorationMapView.MapWidget _miniMapWidget;

    GameObject _questRoot;
    Text _questHeaderText;
    Text _questTitleText;
    Text _questDistanceText;
    Text _questStepsText;
    Text _questNextHeaderText;
    Text _questNextActionText;

    Text _moonText;
    Text _repText;
    Text _passText;

    Text _controlsText;

    GameObject _interactRoot;
    Text _interactLine1Text;
    Text _interactLine2Text;

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

        BuildCanvas();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void LateUpdate()
    {
        if (_canvas != null)
        {
            _canvas.gameObject.SetActive(!TabMenuUI.IsOpen && !MapPanelUI.IsOpen && !InventoryPanelUI.IsOpen);
        }

        if (TabMenuUI.IsOpen || MapPanelUI.IsOpen || InventoryPanelUI.IsOpen)
        {
            return;
        }

        RefreshMiniMap();
        RefreshQuestPanel();
        RefreshStatusPanel();
        RefreshInteractionPanel();
    }

    void RefreshMiniMap()
    {
        if (_miniMapWidget == null)
        {
            return;
        }

        if (_player == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                _player = player.transform;
            }
        }

        ExplorationMapView.Refresh(_miniMapWidget, _questManager, _player);
    }

    void RefreshQuestPanel()
    {
        if (_questManager == null)
        {
            _questRoot.SetActive(false);
            return;
        }

        MainQuestManager.QuestHudSnapshot snapshot = _questManager.GetQuestHudSnapshot();
        _questRoot.SetActive(snapshot.IsVisible);

        if (!snapshot.IsVisible)
        {
            return;
        }

        _questHeaderText.text = "【当前目标】";
        _questTitleText.text = snapshot.Title;
        _questStepsText.text = snapshot.StepsText;

        bool hasDistance = !string.IsNullOrEmpty(snapshot.DistanceText);
        _questDistanceText.gameObject.SetActive(hasDistance);
        _questDistanceText.text = snapshot.DistanceText;

        bool hasNextAction = !string.IsNullOrEmpty(snapshot.NextActionText);
        _questNextHeaderText.gameObject.SetActive(hasNextAction);
        _questNextActionText.gameObject.SetActive(hasNextAction);
        _questNextActionText.text = hasNextAction ? snapshot.NextActionText : string.Empty;
    }

    void RefreshStatusPanel()
    {
        TokenManager tokens = TokenManager.Instance;
        ReputationManager reputation = ReputationManager.Instance;
        DAOPassManager passes = DAOPassManager.Instance;

        int moon = tokens != null ? tokens.MoonBalance : 0;
        int rep = reputation != null ? reputation.GetReputation() : 0;
        string pass = passes != null && passes.HasGreenPass ? "Green" : "None";

        _moonText.text = $"MOON  {moon}";
        _repText.text = $"REP  {rep}";
        _passText.text = $"DAO PASS  {pass}";
    }

    void RefreshInteractionPanel()
    {
        if (HUDPromptChannel.TryGetDisplay(out string line1, out string line2))
        {
            _interactRoot.SetActive(true);
            _interactLine1Text.text = line1;
            bool hasLine2 = !string.IsNullOrEmpty(line2);
            _interactLine2Text.gameObject.SetActive(hasLine2);
            _interactLine2Text.text = hasLine2 ? line2 : string.Empty;
            return;
        }

        _interactRoot.SetActive(false);
    }

    void BuildCanvas()
    {
        GameObject canvasObject = new GameObject("GameHUDCanvas");
        canvasObject.transform.SetParent(transform, false);

        _canvas = canvasObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        BuildMiniMapPanel(canvasObject.transform);
        BuildStatusPanel(canvasObject.transform);
        _questRoot = BuildQuestPanel(canvasObject.transform);
        BuildControlsPanel(canvasObject.transform);
        _interactRoot = BuildInteractionPanel(canvasObject.transform);
    }

    void BuildMiniMapPanel(Transform parent)
    {
        GameObject root = CreatePanel("TopLeft_Minimap", parent,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(16f, -16f), new Vector2(286f, 206f),
            new Color(0.03f, 0.06f, 0.1f, 0.42f));

        CanvasGroup group = root.AddComponent<CanvasGroup>();
        group.alpha = 0.78f;

        _miniMapWidget = ExplorationMapView.Build(root.transform, 258f, 178f, true);
    }

    GameObject BuildQuestPanel(Transform parent)
    {
        GameObject root = CreatePanel("Right_Quest", parent,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-16f, -136f), new Vector2(360f, 336f),
            new Color(0.05f, 0.08f, 0.14f, 0.54f));

        _questHeaderText = CreateTopAnchoredText(root.transform, "Header", 16, FontStyle.Bold,
            new Color(0.75f, 0.88f, 1f), TextAnchor.UpperLeft,
            12f, 10f, 336f, 22f);

        _questTitleText = CreateTopAnchoredText(root.transform, "Title", 19, FontStyle.Bold,
            new Color(1f, 0.92f, 0.45f), TextAnchor.UpperLeft,
            12f, 36f, 336f, 30f);

        _questDistanceText = CreateTopAnchoredText(root.transform, "Distance", 14, FontStyle.Normal,
            new Color(0.7f, 0.85f, 0.95f), TextAnchor.UpperLeft,
            12f, 70f, 336f, 20f);

        _questStepsText = CreateTopAnchoredText(root.transform, "Steps", 13, FontStyle.Normal,
            new Color(0.78f, 0.86f, 0.95f), TextAnchor.UpperLeft,
            12f, 98f, 336f, 110f);
        _questStepsText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _questStepsText.verticalOverflow = VerticalWrapMode.Overflow;
        _questStepsText.lineSpacing = 1.05f;

        _questNextHeaderText = CreateTopAnchoredText(root.transform, "NextHeader", 14, FontStyle.Bold,
            new Color(0.75f, 0.88f, 1f), TextAnchor.UpperLeft,
            12f, 222f, 336f, 20f);

        _questNextActionText = CreateTopAnchoredText(root.transform, "NextAction", 14, FontStyle.Bold,
            new Color(0.55f, 0.95f, 0.75f), TextAnchor.UpperLeft,
            12f, 248f, 336f, 76f);
        _questNextActionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _questNextActionText.verticalOverflow = VerticalWrapMode.Overflow;
        _questNextHeaderText.text = "【下一步】";

        return root;
    }

    void BuildStatusPanel(Transform parent)
    {
        GameObject root = CreatePanel("TopRight_Status", parent,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-16f, -16f), new Vector2(220f, 108f),
            new Color(0.06f, 0.08f, 0.12f, 0.56f));

        _moonText = CreateText(root.transform, "Moon", 17, FontStyle.Bold,
            new Color(1f, 0.88f, 0.35f), TextAnchor.UpperLeft,
            new Vector2(12f, -10f), new Vector2(-12f, -36f));

        _repText = CreateText(root.transform, "Rep", 17, FontStyle.Bold,
            new Color(0.35f, 0.95f, 0.78f), TextAnchor.UpperLeft,
            new Vector2(12f, -38f), new Vector2(-12f, -64f));

        _passText = CreateText(root.transform, "Pass", 16, FontStyle.Bold,
            new Color(0.55f, 0.95f, 0.55f), TextAnchor.UpperLeft,
            new Vector2(12f, -66f), new Vector2(-12f, -92f));
    }

    void BuildControlsPanel(Transform parent)
    {
        GameObject root = CreatePanel("BottomLeft_Controls", parent,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(16f, 16f), new Vector2(200f, 232f),
            new Color(0.04f, 0.06f, 0.1f, 0.48f));

        _controlsText = CreateText(root.transform, "Controls", 14, FontStyle.Normal,
            new Color(0.72f, 0.8f, 0.9f, 0.95f), TextAnchor.UpperLeft,
            new Vector2(12f, -10f), new Vector2(-12f, -10f));
        _controlsText.text = "操作提示\nWASD移动\nShift奔跑\nSpace跳跃\nE交互\nTAB菜单\nM地图 I背包 J日志\nCtrl+R重启\nESC菜单";
        _controlsText.lineSpacing = 1.05f;
        _controlsText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _controlsText.verticalOverflow = VerticalWrapMode.Overflow;
    }

    GameObject BuildInteractionPanel(Transform parent)
    {
        GameObject root = CreatePanel("BottomCenter_Interaction", parent,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 24f), new Vector2(600f, 168f),
            new Color(0.05f, 0.09f, 0.16f, 0.68f));

        _interactLine1Text = CreateText(root.transform, "Line1", 18, FontStyle.Bold,
            new Color(0.9f, 0.95f, 1f), TextAnchor.UpperCenter,
            new Vector2(16f, -84f), new Vector2(-16f, -10f));
        _interactLine1Text.horizontalOverflow = HorizontalWrapMode.Wrap;
        _interactLine1Text.verticalOverflow = VerticalWrapMode.Overflow;
        _interactLine1Text.lineSpacing = 1.05f;

        _interactLine2Text = CreateText(root.transform, "Line2", 15, FontStyle.Italic,
            new Color(0.7f, 0.85f, 0.95f), TextAnchor.UpperCenter,
            new Vector2(16f, -158f), new Vector2(-16f, -88f));
        _interactLine2Text.horizontalOverflow = HorizontalWrapMode.Wrap;
        _interactLine2Text.verticalOverflow = VerticalWrapMode.Overflow;
        _interactLine2Text.lineSpacing = 1.1f;

        root.SetActive(false);
        return root;
    }

    static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    static Text CreateText(Transform parent, string name, int fontSize, FontStyle fontStyle, Color color,
        TextAnchor alignment, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Text text = textObject.AddComponent<Text>();
        text.font = _font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.supportRichText = false;
        return text;
    }

    static Text CreateTopAnchoredText(Transform parent, string name, int fontSize, FontStyle fontStyle,
        Color color, TextAnchor alignment, float left, float top, float width, float height)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(left, -top);
        rect.sizeDelta = new Vector2(width, height);

        Text text = textObject.AddComponent<Text>();
        text.font = _font;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.supportRichText = false;
        return text;
    }
}
