using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Stardew-style simplified 2D exploration map. Landmarks, route paths, glowing player dot.
/// </summary>
public class ExplorationMapView
{
    public enum LandmarkState
    {
        Completed,
        Current,
        Upcoming
    }

    public struct Landmark
    {
        public string Label;
        public Vector2 WorldXZ;
        public int QuestStepIndex;
    }

    public sealed class MapWidget
    {
        public RectTransform MapArea;
        public RectTransform PlayerDotRoot;
        public Image PlayerGlow;
        public Image PlayerCore;
        public LandmarkWidget[] Landmarks;
    }

    public sealed class LandmarkWidget
    {
        public Image Marker;
        public Image Ring;
        public Text Label;
        public int QuestStepIndex;
    }

    static readonly Landmark[] Landmarks =
    {
        new Landmark { Label = "出生地", WorldXZ = new Vector2(0f, 0f), QuestStepIndex = -1 },
        new Landmark { Label = "Blue DAO", WorldXZ = new Vector2(-200f, -200f), QuestStepIndex = 0 },
        new Landmark { Label = "Red DAO", WorldXZ = new Vector2(200f, 200f), QuestStepIndex = 1 },
        new Landmark { Label = "Green DAO", WorldXZ = new Vector2(280f, -280f), QuestStepIndex = 4 },
        new Landmark { Label = "Boundary Stone", WorldXZ = new Vector2(420f, -420f), QuestStepIndex = 5 },
        new Landmark { Label = "Civilization Seed", WorldXZ = new Vector2(400f, -400f), QuestStepIndex = 6 }
    };

    static readonly Vector2 WorldMin = new Vector2(-240f, -460f);
    static readonly Vector2 WorldMax = new Vector2(460f, 240f);
    static readonly Color MapBackground = new Color(0.12f, 0.24f, 0.38f, 0.98f);
    static readonly Color LandFill = new Color(0.18f, 0.34f, 0.44f, 0.92f);
    static readonly Color LandBorder = new Color(0.38f, 0.62f, 0.78f, 0.55f);
    static readonly Color PathColor = new Color(0.52f, 0.74f, 0.9f, 0.45f);

    static Font _font;
    static Texture2D _circleTexture;
    static Texture2D _glowTexture;

    public static MapWidget Build(Transform parent, float width, float height)
    {
        EnsureResources();

        GameObject root = CreateUiObject("ExplorationMap", parent);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(width, height);

        LayoutElement layout = root.AddComponent<LayoutElement>();
        layout.minWidth = width;
        layout.preferredWidth = width;
        layout.minHeight = height;
        layout.preferredHeight = height;

        GameObject mapAreaObject = CreateUiObject("MapArea", root.transform);
        MapWidget widget = new MapWidget();
        widget.MapArea = mapAreaObject.GetComponent<RectTransform>();
        StretchFull(widget.MapArea);

        Image mapBackground = mapAreaObject.AddComponent<Image>();
        mapBackground.color = MapBackground;

        Vector2 mapSize = new Vector2(width, height);
        CreateLandMass(widget.MapArea);
        CreateRoutePaths(widget.MapArea, mapSize);
        widget.Landmarks = CreateLandmarks(widget.MapArea, mapSize);
        CreatePlayerDot(widget.MapArea, out widget.PlayerDotRoot, out widget.PlayerGlow, out widget.PlayerCore);
        mapAreaObject.AddComponent<ExplorationMapPulse>();

        return widget;
    }

    public static void Refresh(MapWidget widget, MainQuestManager questManager, Transform player)
    {
        if (widget == null || widget.MapArea == null)
        {
            return;
        }

        LandmarkState[] states = BuildLandmarkStates(questManager);

        for (int i = 0; i < widget.Landmarks.Length; i++)
        {
            ApplyLandmarkVisual(widget.Landmarks[i], states[i]);
        }

        if (player != null)
        {
            Vector2 mapSize = widget.MapArea.rect.size;
            if (mapSize.x <= 1f || mapSize.y <= 1f)
            {
                mapSize = widget.MapArea.sizeDelta;
            }

            Vector2 mapPos = WorldToMapLocal(player.position.x, player.position.z, mapSize);
            widget.PlayerDotRoot.anchoredPosition = mapPos;
            widget.PlayerDotRoot.gameObject.SetActive(true);
        }
        else
        {
            widget.PlayerDotRoot.gameObject.SetActive(false);
        }
    }

    static LandmarkState[] BuildLandmarkStates(MainQuestManager questManager)
    {
        LandmarkState[] states = new LandmarkState[Landmarks.Length];
        bool currentAssigned = false;

        for (int i = 0; i < Landmarks.Length; i++)
        {
            if (IsLandmarkCompleted(questManager, Landmarks[i].QuestStepIndex))
            {
                states[i] = LandmarkState.Completed;
                continue;
            }

            if (!currentAssigned)
            {
                states[i] = LandmarkState.Current;
                currentAssigned = true;
            }
            else
            {
                states[i] = LandmarkState.Upcoming;
            }
        }

        if (questManager != null && questManager.IsAllQuestsComplete)
        {
            for (int i = 0; i < states.Length; i++)
            {
                states[i] = LandmarkState.Completed;
            }
        }

        return states;
    }

    static bool IsLandmarkCompleted(MainQuestManager questManager, int questStepIndex)
    {
        if (questManager == null)
        {
            return questStepIndex < 0;
        }

        return questManager.IsDemoMilestoneComplete(questStepIndex);
    }

    static void ApplyLandmarkVisual(LandmarkWidget landmark, LandmarkState state)
    {
        Color markerColor;
        Color ringColor;
        Color labelColor;

        switch (state)
        {
            case LandmarkState.Completed:
                markerColor = new Color(0.45f, 0.88f, 0.95f);
                ringColor = new Color(0.35f, 0.72f, 0.82f, 0.75f);
                labelColor = new Color(0.78f, 0.95f, 1f);
                break;
            case LandmarkState.Current:
                markerColor = new Color(1f, 0.9f, 0.45f);
                ringColor = new Color(1f, 0.82f, 0.35f, 0.85f);
                labelColor = new Color(1f, 0.94f, 0.62f);
                break;
            default:
                markerColor = new Color(0.45f, 0.55f, 0.64f);
                ringColor = new Color(0.35f, 0.45f, 0.55f, 0.55f);
                labelColor = new Color(0.62f, 0.72f, 0.82f);
                break;
        }

        landmark.Marker.color = markerColor;
        landmark.Ring.color = ringColor;
        landmark.Label.color = labelColor;
        landmark.Label.fontStyle = state == LandmarkState.Current ? FontStyle.Bold : FontStyle.Normal;
    }

    static void CreateLandMass(RectTransform mapArea)
    {
        GameObject land = CreateUiObject("LandMass", mapArea);
        RectTransform landRect = land.GetComponent<RectTransform>();
        landRect.anchorMin = new Vector2(0.5f, 0.5f);
        landRect.anchorMax = new Vector2(0.5f, 0.5f);
        landRect.pivot = new Vector2(0.5f, 0.5f);
        landRect.sizeDelta = new Vector2(560f, 420f);
        landRect.anchoredPosition = new Vector2(10f, -8f);

        Image landImage = land.AddComponent<Image>();
        landImage.color = LandFill;
        landImage.sprite = CreateRoundedSprite();
        landImage.type = Image.Type.Sliced;

        GameObject border = CreateUiObject("LandBorder", land.transform);
        RectTransform borderRect = border.GetComponent<RectTransform>();
        StretchFull(borderRect);
        borderRect.offsetMin = new Vector2(-6f, -6f);
        borderRect.offsetMax = new Vector2(6f, 6f);

        Image borderImage = border.AddComponent<Image>();
        borderImage.color = LandBorder;
        borderImage.sprite = CreateRoundedSprite();
        borderImage.type = Image.Type.Sliced;
    }

    static void CreateRoutePaths(RectTransform mapArea, Vector2 mapSize)
    {
        for (int i = 0; i < Landmarks.Length - 1; i++)
        {
            Vector2 from = WorldToMapLocal(Landmarks[i].WorldXZ.x, Landmarks[i].WorldXZ.y, mapSize);
            Vector2 to = WorldToMapLocal(Landmarks[i + 1].WorldXZ.x, Landmarks[i + 1].WorldXZ.y, mapSize);
            CreatePathSegment(mapArea, from, to);
        }
    }

    static void CreatePathSegment(RectTransform parent, Vector2 from, Vector2 to)
    {
        Vector2 delta = to - from;
        float length = delta.magnitude;
        if (length < 1f)
        {
            return;
        }

        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        GameObject path = CreateUiObject("RoutePath", parent);
        RectTransform rect = path.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.sizeDelta = new Vector2(length, 3f);
        rect.anchoredPosition = from;
        rect.localRotation = Quaternion.Euler(0f, 0f, angle);

        Image image = path.AddComponent<Image>();
        image.color = PathColor;
    }

    static LandmarkWidget[] CreateLandmarks(RectTransform mapArea, Vector2 mapSize)
    {
        LandmarkWidget[] widgets = new LandmarkWidget[Landmarks.Length];

        for (int i = 0; i < Landmarks.Length; i++)
        {
            Landmark landmark = Landmarks[i];
            Vector2 mapPos = WorldToMapLocal(landmark.WorldXZ.x, landmark.WorldXZ.y, mapSize);

            GameObject markerRoot = CreateUiObject($"Landmark_{landmark.Label}", mapArea);
            RectTransform markerRect = markerRoot.GetComponent<RectTransform>();
            markerRect.anchorMin = new Vector2(0.5f, 0.5f);
            markerRect.anchorMax = new Vector2(0.5f, 0.5f);
            markerRect.pivot = new Vector2(0.5f, 0.5f);
            markerRect.sizeDelta = new Vector2(120f, 56f);
            markerRect.anchoredPosition = mapPos;

            GameObject ringObject = CreateUiObject("Ring", markerRoot.transform);
            RectTransform ringRect = ringObject.GetComponent<RectTransform>();
            ringRect.anchorMin = new Vector2(0.5f, 1f);
            ringRect.anchorMax = new Vector2(0.5f, 1f);
            ringRect.pivot = new Vector2(0.5f, 0.5f);
            ringRect.sizeDelta = new Vector2(18f, 18f);
            ringRect.anchoredPosition = new Vector2(0f, -10f);
            Image ring = ringObject.AddComponent<Image>();
            ring.sprite = CreateCircleSprite();
            ring.type = Image.Type.Simple;

            GameObject dotObject = CreateUiObject("Dot", markerRoot.transform);
            RectTransform dotRect = dotObject.GetComponent<RectTransform>();
            dotRect.anchorMin = ringRect.anchorMin;
            dotRect.anchorMax = ringRect.anchorMax;
            dotRect.pivot = ringRect.pivot;
            dotRect.sizeDelta = new Vector2(10f, 10f);
            dotRect.anchoredPosition = ringRect.anchoredPosition;
            Image dot = dotObject.AddComponent<Image>();
            dot.sprite = CreateCircleSprite();
            dot.type = Image.Type.Simple;

            GameObject labelObject = CreateUiObject("Label", markerRoot.transform);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0f);
            labelRect.anchorMax = new Vector2(0.5f, 0f);
            labelRect.pivot = new Vector2(0.5f, 1f);
            labelRect.sizeDelta = new Vector2(120f, 34f);
            labelRect.anchoredPosition = new Vector2(0f, 0f);

            Text label = labelObject.AddComponent<Text>();
            label.text = landmark.Label;
            label.font = _font;
            label.fontSize = 13;
            label.alignment = TextAnchor.UpperCenter;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;

            widgets[i] = new LandmarkWidget
            {
                Marker = dot,
                Ring = ring,
                Label = label,
                QuestStepIndex = landmark.QuestStepIndex
            };
        }

        return widgets;
    }

    static void CreatePlayerDot(RectTransform mapArea, out RectTransform root, out Image glow, out Image core)
    {
        GameObject playerRoot = CreateUiObject("PlayerDot", mapArea);
        root = playerRoot.GetComponent<RectTransform>();
        root.anchorMin = new Vector2(0.5f, 0.5f);
        root.anchorMax = new Vector2(0.5f, 0.5f);
        root.pivot = new Vector2(0.5f, 0.5f);
        root.sizeDelta = new Vector2(40f, 40f);

        GameObject glowObject = CreateUiObject("Glow", playerRoot.transform);
        RectTransform glowRect = glowObject.GetComponent<RectTransform>();
        StretchCenter(glowRect, 34f);
        glow = glowObject.AddComponent<Image>();
        glow.sprite = CreateGlowSprite();
        glow.type = Image.Type.Simple;
        glow.color = new Color(0.55f, 0.88f, 1f, 0.75f);

        GameObject coreObject = CreateUiObject("Core", playerRoot.transform);
        RectTransform coreRect = coreObject.GetComponent<RectTransform>();
        StretchCenter(coreRect, 12f);
        core = coreObject.AddComponent<Image>();
        core.sprite = CreateCircleSprite();
        core.type = Image.Type.Simple;
        core.color = new Color(0.92f, 0.98f, 1f);

        playerRoot.transform.SetAsLastSibling();
    }

    static Vector2 WorldToMapLocal(float worldX, float worldZ, Vector2 mapSize)
    {
        const float padding = 36f;
        float drawWidth = mapSize.x - padding * 2f;
        float drawHeight = mapSize.y - padding * 2f;

        float normalizedX = Mathf.InverseLerp(WorldMin.x, WorldMax.x, worldX);
        float normalizedZ = Mathf.InverseLerp(WorldMin.y, WorldMax.y, worldZ);

        float x = -drawWidth * 0.5f + normalizedX * drawWidth;
        float y = -drawHeight * 0.5f + (1f - normalizedZ) * drawHeight;
        return new Vector2(x, y);
    }

    static void EnsureResources()
    {
        if (_font == null)
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }

    static Sprite CreateRoundedSprite()
    {
        return Sprite.Create(CreateSolidTexture(8, 8, Color.white), new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f), 8f);
    }

    static Sprite CreateCircleSprite()
    {
        if (_circleTexture == null)
        {
            _circleTexture = CreateRadialTexture(32, false);
        }

        return Sprite.Create(_circleTexture, new Rect(0f, 0f, 32f, 32f), new Vector2(0.5f, 0.5f), 32f);
    }

    static Sprite CreateGlowSprite()
    {
        if (_glowTexture == null)
        {
            _glowTexture = CreateRadialTexture(48, true);
        }

        return Sprite.Create(_glowTexture, new Rect(0f, 0f, 48f, 48f), new Vector2(0.5f, 0.5f), 48f);
    }

    static Texture2D CreateSolidTexture(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    static Texture2D CreateRadialTexture(int size, bool softGlow)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float radius = size * 0.5f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / radius;
                float alpha = softGlow
                    ? Mathf.Clamp01(1f - distance)
                    : distance <= 1f ? 1f : 0f;
                if (!softGlow && distance > 1f)
                {
                    alpha = 0f;
                }

                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha));
            }
        }

        texture.Apply();
        return texture;
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

    static void StretchCenter(RectTransform rect, float size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(size, size);
        rect.anchoredPosition = Vector2.zero;
    }
}

/// <summary>
/// Pulses the player glow dot on the exploration map.
/// </summary>
public class ExplorationMapPulse : MonoBehaviour
{
    Image _glow;
    float _phase;

    void Awake()
    {
        Transform playerDot = transform.Find("PlayerDot");
        if (playerDot == null)
        {
            return;
        }

        Transform glow = playerDot.Find("Glow");
        if (glow != null)
        {
            _glow = glow.GetComponent<Image>();
        }
    }

    void Update()
    {
        if (_glow == null)
        {
            return;
        }

        _phase += Time.unscaledDeltaTime * 2.6f;
        float pulse = 0.55f + Mathf.Sin(_phase) * 0.25f;
        float scale = 0.9f + Mathf.Sin(_phase) * 0.18f;
        Color color = _glow.color;
        color.a = pulse;
        _glow.color = color;
        _glow.rectTransform.localScale = Vector3.one * scale;
    }
}
