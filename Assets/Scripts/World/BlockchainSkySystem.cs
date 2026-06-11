using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Procedural blockchain network sky layer — floating labels, nodes, blocks, and links.
/// World Space Canvas only; visual atmosphere, no gameplay.
/// </summary>
public class BlockchainSkySystem : MonoBehaviour
{
    static readonly string[] Keywords = { "BLOCK", "HASH", "DAO", "TOKEN", "CONSENSUS" };
    static readonly Color GlowCyan = new Color(0.45f, 0.82f, 0.95f, 0.38f);
    static readonly Color GlowBlue = new Color(0.58f, 0.72f, 0.96f, 0.34f);
    static readonly Color GlowGold = new Color(0.92f, 0.78f, 0.38f, 0.32f);
    static readonly Color LineColor = new Color(0.52f, 0.86f, 1f, 0.22f);

    [SerializeField] float skyCenterHeight = 92f;
    [SerializeField] float domeRadius = 300f;
    [SerializeField] float canvasScale = 0.09f;
    [SerializeField] int nodeCount = 18;
    [SerializeField] int lineCount = 22;
    [SerializeField] int blockClusterCount = 5;
    [SerializeField] int keywordLabelCount = 14;
    [SerializeField] int randomSeed = 20480;

    Transform _skyRoot;
    Camera _camera;
    System.Random _rng;
    Font _font;
    Sprite _circleSprite;
    Sprite _squareSprite;
    readonly List<Transform> _billboards = new List<Transform>();
    readonly List<Transform> _nodePoints = new List<Transform>();

    void Start()
    {
        _rng = new System.Random(randomSeed);
        _camera = Camera.main;
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        EnsureSprites();
        BuildSky();
    }

    void LateUpdate()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }

        if (_camera == null)
        {
            return;
        }

        for (int i = 0; i < _billboards.Count; i++)
        {
            Transform target = _billboards[i];
            if (target == null)
            {
                continue;
            }

            Vector3 toCamera = target.position - _camera.transform.position;
            if (toCamera.sqrMagnitude > 0.01f)
            {
                target.rotation = Quaternion.LookRotation(toCamera, Vector3.up);
            }
        }
    }

    void BuildSky()
    {
        _skyRoot = new GameObject("BlockchainSky").transform;
        _skyRoot.SetParent(transform, false);
        _skyRoot.position = new Vector3(0f, skyCenterHeight, -0.72f);

        for (int i = 0; i < nodeCount; i++)
        {
            Vector3 pos = RandomDomePosition();
            Transform node = CreateNode($"SkyNode_{i:00}", pos, NextFloat(0.85f, 1.25f));
            _nodePoints.Add(node);
        }

        for (int i = 0; i < blockClusterCount; i++)
        {
            CreateBlockCluster($"SkyBlock_{i:00}", RandomDomePosition(), NextInt(2, 4), NextInt(2, 4));
        }

        for (int i = 0; i < keywordLabelCount; i++)
        {
            string word = Keywords[NextInt(0, Keywords.Length)];
            CreateKeywordLabel($"SkyLabel_{word}_{i:00}", word, RandomDomePosition());
        }

        for (int i = 0; i < lineCount; i++)
        {
            if (_nodePoints.Count < 2)
            {
                break;
            }

            Transform a = _nodePoints[NextInt(0, _nodePoints.Count)];
            Transform b = _nodePoints[NextInt(0, _nodePoints.Count)];
            if (a == b)
            {
                continue;
            }

            CreateConnectionLine($"SkyLine_{i:00}", a.position, b.position);
        }

        CreateNodeRingPattern();
    }

    void CreateNodeRingPattern()
    {
        const int ringNodes = 8;
        float ringRadius = domeRadius * 0.72f;
        float ringHeight = skyCenterHeight + domeRadius * 0.55f;

        for (int i = 0; i < ringNodes; i++)
        {
            float angle = i * (360f / ringNodes) * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * ringRadius,
                ringHeight,
                Mathf.Sin(angle) * ringRadius - 0.72f);

            Transform node = CreateNode($"SkyRingNode_{i:00}", pos, 0.75f);
            _nodePoints.Add(node);

            int next = (i + 1) % ringNodes;
            float nextAngle = next * (360f / ringNodes) * Mathf.Deg2Rad;
            Vector3 nextPos = new Vector3(
                Mathf.Cos(nextAngle) * ringRadius,
                ringHeight,
                Mathf.Sin(nextAngle) * ringRadius - 0.72f);
            CreateConnectionLine($"SkyRingLine_{i:00}", pos, nextPos);
        }
    }

    Transform CreateNode(string name, Vector3 worldPosition, float sizeMul)
    {
        Transform root = CreateSkyCanvasRoot(name, worldPosition, 80f);
        float size = 26f * sizeMul;

        Image ring = CreateImage("Ring", root, _circleSprite, PickGlowColor(), new Vector2(size * 1.6f, size * 1.6f));
        ring.raycastTarget = false;

        Image core = CreateImage("Core", root, _circleSprite, PickGlowColor() * 1.15f, new Vector2(size, size));
        core.raycastTarget = false;

        Image pulse = CreateImage("Pulse", root, _circleSprite, new Color(0.7f, 0.9f, 1f, 0.12f), new Vector2(size * 2.2f, size * 2.2f));
        pulse.raycastTarget = false;

        AddDrift(root.gameObject, NextFloat(0.08f, 0.2f), NextFloat(0.3f, 0.8f));
        return root;
    }

    void CreateBlockCluster(string name, Vector3 worldPosition, int cols, int rows)
    {
        Transform root = CreateSkyCanvasRoot(name, worldPosition, 120f);
        float cell = 18f;
        Color blockColor = Color.Lerp(GlowBlue, GlowGold, NextFloat());

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Image block = CreateImage(
                    $"Cell_{x}_{y}",
                    root,
                    _squareSprite,
                    blockColor * NextFloat(0.85f, 1.1f),
                    new Vector2(cell, cell));
                block.rectTransform.anchoredPosition = new Vector2(
                    (x - (cols - 1) * 0.5f) * (cell + 4f),
                    (y - (rows - 1) * 0.5f) * (cell + 4f));
                block.raycastTarget = false;
            }
        }

        Text hash = CreateText("Hash", root, NextHashSnippet(), 11, new Vector2(0f, -rows * cell * 0.65f));
        hash.color = new Color(0.75f, 0.88f, 1f, 0.28f);

        AddDrift(root.gameObject, NextFloat(0.05f, 0.14f), NextFloat(0.25f, 0.6f));
    }

    void CreateKeywordLabel(string name, string word, Vector3 worldPosition)
    {
        Transform root = CreateSkyCanvasRoot(name, worldPosition, 160f);
        Text label = CreateText("Label", root, word, 20, Vector2.zero);
        label.color = PickGlowColor() * 1.1f;
        label.fontStyle = FontStyle.Bold;

        Image underline = CreateImage("Underline", root, _squareSprite, label.color * 0.65f, new Vector2(word.Length * 9f, 2f));
        underline.rectTransform.anchoredPosition = new Vector2(0f, -16f);

        AddDrift(root.gameObject, NextFloat(0.06f, 0.16f), NextFloat(0.35f, 0.75f));
    }

    void CreateConnectionLine(string name, Vector3 from, Vector3 to)
    {
        Vector3 mid = (from + to) * 0.5f;
        float length = Vector3.Distance(from, to);
        Vector3 dir = (to - from).normalized;

        GameObject lineObject = new GameObject(name);
        lineObject.transform.SetParent(_skyRoot, false);
        lineObject.transform.position = mid;
        if (dir.sqrMagnitude > 0.001f)
        {
            lineObject.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        Canvas canvas = lineObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = _camera;

        RectTransform rect = lineObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(length, 2.5f);
        rect.localScale = Vector3.one * canvasScale * 0.85f;

        Image image = lineObject.AddComponent<Image>();
        image.sprite = _squareSprite;
        image.color = LineColor;
        image.raycastTarget = false;

        AddDrift(lineObject, NextFloat(0.03f, 0.08f), NextFloat(0.2f, 0.5f));
    }

    Transform CreateSkyCanvasRoot(string name, Vector3 worldPosition, float canvasSize)
    {
        GameObject rootObject = new GameObject(name);
        rootObject.transform.SetParent(_skyRoot, false);
        rootObject.transform.position = worldPosition;

        Canvas canvas = rootObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = _camera;

        CanvasGroup group = rootObject.AddComponent<CanvasGroup>();
        group.alpha = NextFloat(0.55f, 0.85f);
        group.interactable = false;
        group.blocksRaycasts = false;

        RectTransform rect = rootObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(canvasSize, canvasSize);
        rect.localScale = Vector3.one * canvasScale;

        _billboards.Add(rootObject.transform);
        return rootObject.transform;
    }

    static Image CreateImage(string name, Transform parent, Sprite sprite, Color color, Vector2 size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        Image image = obj.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        return image;
    }

    Text CreateText(string name, Transform parent, string content, int fontSize, Vector2 anchoredPos)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        Text text = obj.AddComponent<Text>();
        text.font = _font;
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(220f, 36f);
        rect.anchoredPosition = anchoredPos;
        return text;
    }

    Vector3 RandomDomePosition()
    {
        float theta = NextFloat(0f, Mathf.PI * 2f);
        float elevation = NextFloat(18f, 58f) * Mathf.Deg2Rad;
        float radius = domeRadius * NextFloat(0.75f, 1.05f);
        float horizontal = Mathf.Cos(elevation) * radius;
        float y = skyCenterHeight + Mathf.Sin(elevation) * radius;

        return new Vector3(
            Mathf.Cos(theta) * horizontal,
            y,
            Mathf.Sin(theta) * horizontal - 0.72f);
    }

    Color PickGlowColor()
    {
        return NextFloat() > 0.25f ? GlowCyan : GlowBlue;
    }

    string NextHashSnippet()
    {
        return $"0x{NextHex(4)}...{NextHex(3)}";
    }

    string NextHex(int chars)
    {
        const string hex = "0123456789abcdef";
        char[] buffer = new char[chars];
        for (int i = 0; i < chars; i++)
        {
            buffer[i] = hex[NextInt(0, hex.Length)];
        }

        return new string(buffer);
    }

    static void AddDrift(GameObject target, float speed, float amplitude)
    {
        BlockchainSkyDrift drift = target.AddComponent<BlockchainSkyDrift>();
        drift.Configure(speed, amplitude, Random.Range(0f, Mathf.PI * 2f));
    }

    void EnsureSprites()
    {
        _squareSprite = Sprite.Create(
            CreateSolidTexture(8, 8, Color.white),
            new Rect(0f, 0f, 8f, 8f),
            new Vector2(0.5f, 0.5f),
            8f);

        _circleSprite = Sprite.Create(
            CreateCircleTexture(32),
            new Rect(0f, 0f, 32f, 32f),
            new Vector2(0.5f, 0.5f),
            32f);
    }

    static Texture2D CreateSolidTexture(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    static Texture2D CreateCircleTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float radius = size * 0.5f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - (dist - radius + 1.5f) / 2f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return texture;
    }

    float NextFloat()
    {
        return (float)_rng.NextDouble();
    }

    float NextFloat(float min, float max)
    {
        return min + (max - min) * NextFloat();
    }

    int NextInt(int minInclusive, int maxExclusive)
    {
        return _rng.Next(minInclusive, maxExclusive);
    }
}

/// <summary> Gentle sky drift for blockchain UI elements. </summary>
public class BlockchainSkyDrift : MonoBehaviour
{
    Vector3 _origin;
    float _speed;
    float _amplitude;
    float _phase;
    Vector3 _driftAxis;

    public void Configure(float speed, float amplitude, float phase)
    {
        _speed = speed;
        _amplitude = amplitude;
        _phase = phase;
        _driftAxis = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-0.15f, 0.35f),
            Random.Range(-1f, 1f)).normalized;
    }

    void Start()
    {
        _origin = transform.position;
    }

    void Update()
    {
        float wave = Mathf.Sin(Time.time * _speed + _phase) * _amplitude;
        transform.position = _origin + _driftAxis * wave;
    }
}
