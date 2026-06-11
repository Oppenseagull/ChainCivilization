using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// High-visibility floating airdrop coin. Readable from ~50m; interact prompt within 20m.
/// </summary>
public class AirdropCoin : MonoBehaviour
{
    const string CollectedKeyPrefix = "airdrop_collected_";
    const float PromptRadius = 20f;

    static readonly Color GoldBase = new Color(0.96f, 0.82f, 0.28f);
    static readonly Color GoldEmission = new Color(1f, 0.78f, 0.18f);
    static readonly Color RingEmission = new Color(1f, 0.72f, 0.12f);

    [SerializeField] string coinId = "airdrop_00";
    [SerializeField] float interactRadius = 4f;
    [SerializeField] int moonReward = 10;
    [SerializeField] float hoverHeight = 1.8f;

    Transform _player;
    bool _playerInPromptRange;
    bool _playerInInteractRange;
    public void Initialize(string id)
    {
        coinId = id;
    }

    void Start()
    {
        if (IsCollected())
        {
            Destroy(gameObject);
            return;
        }

        BuildVisual();
        GroundSnapUtility.SnapTransform(transform, hoverHeight);

        gameObject.AddComponent<AirdropFloatMotion>().Configure(0.55f, 0.95f);
    }

    void Update()
    {
        if (_player == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                _player = player.transform;
            }
        }

        if (_player == null || IsCollected())
        {
            _playerInPromptRange = false;
            _playerInInteractRange = false;
            return;
        }

        float distance = Vector3.Distance(_player.position, transform.position);
        _playerInPromptRange = distance <= PromptRadius;
        _playerInInteractRange = distance <= interactRadius;

        if (!_playerInInteractRange)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryCollect();
        }
#else
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryCollect();
        }
#endif
    }

    void LateUpdate()
    {
        if (!GameHUDCanvas.IsActive)
        {
            HUDPromptChannel.Clear(this);
            return;
        }

        if (!_playerInPromptRange || IsCollected())
        {
            HUDPromptChannel.Clear(this);
            return;
        }

        float priority = -Vector3.Distance(_player.position, transform.position);
        HUDPromptChannel.Set(this, "Press E to Claim", null, priority);
    }

    void TryCollect()
    {
        if (IsCollected())
        {
            return;
        }

        TokenManager tokens = TokenManager.Instance;
        if (tokens == null)
        {
            return;
        }

        Vector3 claimPosition = transform.position + Vector3.up * 1.6f;
        tokens.AddMoon(moonReward);
        AirdropClaimFloater.Spawn(claimPosition, $"+{moonReward} MOON");
        MarkCollected();
        Destroy(gameObject);
    }

    public static void ClearAllCollectedPrefs()
    {
        for (int i = 0; i < 20; i++)
        {
            PlayerPrefs.DeleteKey(CollectedKeyPrefix + $"airdrop_{i:00}");
        }

        PlayerPrefs.Save();
    }

    bool IsCollected()
    {
        return PlayerPrefs.GetInt(CollectedKeyPrefix + coinId, 0) == 1;
    }

    void MarkCollected()
    {
        PlayerPrefs.SetInt(CollectedKeyPrefix + coinId, 1);
        PlayerPrefs.Save();
    }

    void BuildVisual()
    {
        GameObject spinRoot = new GameObject("AirdropSpinRoot");
        spinRoot.transform.SetParent(transform, false);
        spinRoot.AddComponent<AirdropSpin>().Configure(52f);

        GameObject coin = CreatePart(spinRoot.transform, PrimitiveType.Cylinder, "Coin", Vector3.zero, new Vector3(1.65f, 0.18f, 1.65f));
        ApplyEmissive(coin, GoldBase, GoldEmission * 0.55f, 0.28f);

        GameObject innerRing = CreatePart(spinRoot.transform, PrimitiveType.Cylinder, "InnerRing", new Vector3(0f, -0.12f, 0f), new Vector3(2.05f, 0.06f, 2.05f));
        ApplyEmissive(innerRing, GoldBase * 0.92f, GoldEmission * 0.45f, 0.22f);

        GameObject outerRing = CreatePart(spinRoot.transform, PrimitiveType.Cylinder, "GlowRing", new Vector3(0f, -0.2f, 0f), new Vector3(2.85f, 0.1f, 2.85f));
        ApplyEmissive(outerRing, GoldBase * 0.85f, RingEmission, 0.18f);
        outerRing.AddComponent<AirdropGlowPulse>().Configure(0.88f, 1.12f, 1.4f);

        GameObject beaconRing = CreatePart(spinRoot.transform, PrimitiveType.Cylinder, "BeaconRing", new Vector3(0f, -0.28f, 0f), new Vector3(3.6f, 0.05f, 3.6f));
        ApplyEmissive(beaconRing, new Color(1f, 0.9f, 0.45f, 0.35f), RingEmission * 0.65f, 0.12f);
        beaconRing.AddComponent<AirdropGlowPulse>().Configure(0.75f, 1.2f, 0.9f);

        GameObject shard = CreatePart(spinRoot.transform, PrimitiveType.Cube, "ChainShard", new Vector3(0f, 0.35f, 0f), new Vector3(0.5f, 0.5f, 0.1f));
        shard.transform.localRotation = Quaternion.Euler(12f, 35f, 8f);
        ApplyEmissive(shard, new Color(1f, 0.9f, 0.55f), GoldEmission * 0.35f, 0.24f);

        CreateFloatingLabel("AIRDROP", new Vector3(0f, 2.8f, 0f));
    }

    Transform CreateFloatingLabel(string text, Vector3 localPosition)
    {
        GameObject canvasObject = new GameObject("AirdropLabel");
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localPosition = localPosition;
        canvasObject.transform.localScale = Vector3.one * 0.014f;

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(360f, 80f);

        GameObject textObject = new GameObject("LabelText", typeof(RectTransform));
        textObject.transform.SetParent(canvasObject.transform, false);

        Text label = textObject.AddComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 42;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(1f, 0.92f, 0.42f);
        label.raycastTarget = false;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        canvasObject.AddComponent<BillboardLabel>();
        return canvasObject.transform;
    }

    static GameObject CreatePart(Transform parent, PrimitiveType type, string partName, Vector3 localPos, Vector3 scale)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPos;
        part.transform.localScale = scale;

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        return part;
    }

    static void ApplyEmissive(GameObject target, Color color, Color emission, float smoothness)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }

        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
        }

        renderer.sharedMaterial = material;
    }
}

/// <summary> Vertical bob for airdrop root. </summary>
public class AirdropFloatMotion : MonoBehaviour
{
    float _baseY;
    float _amplitude;
    float _speed;
    float _phase;

    public void Configure(float amplitude, float speed)
    {
        _amplitude = amplitude;
        _speed = speed;
        _phase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Start()
    {
        _baseY = transform.position.y;
    }

    void Update()
    {
        float y = _baseY + Mathf.Sin(Time.time * _speed + _phase) * _amplitude;
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, y, pos.z);
    }
}

/// <summary> Continuous yaw spin for the coin assembly. </summary>
public class AirdropSpin : MonoBehaviour
{
    float _degreesPerSecond;

    public void Configure(float degreesPerSecond)
    {
        _degreesPerSecond = degreesPerSecond;
    }

    void Update()
    {
        transform.Rotate(0f, _degreesPerSecond * Time.deltaTime, 0f, Space.Self);
    }
}

/// <summary> Pulses glow ring scale for long-range readability. </summary>
public class AirdropGlowPulse : MonoBehaviour
{
    Vector3 _baseScale;
    float _min = 0.9f;
    float _max = 1.1f;
    float _speed = 1.2f;

    public void Configure(float min, float max, float speed)
    {
        _min = min;
        _max = max;
        _speed = speed;
    }

    void Start()
    {
        _baseScale = transform.localScale;
    }

    void Update()
    {
        float pulse = Mathf.Lerp(_min, _max, (Mathf.Sin(Time.time * _speed) + 1f) * 0.5f);
        transform.localScale = _baseScale * pulse;
    }
}

/// <summary> World-space reward text that rises and fades after claim. </summary>
public class AirdropClaimFloater : MonoBehaviour
{
    const float Duration = 2.2f;
    const float RiseSpeed = 1.35f;

    Vector3 _startPosition;
    float _timer;
    CanvasGroup _group;
    Text _label;

    public static void Spawn(Vector3 worldPosition, string text)
    {
        GameObject floaterObject = new GameObject("AirdropClaimFloater");
        floaterObject.transform.position = worldPosition;
        AirdropClaimFloater floater = floaterObject.AddComponent<AirdropClaimFloater>();
        floater.Initialize(text);
    }

    void Initialize(string text)
    {
        _startPosition = transform.position;
        _timer = Duration;

        GameObject canvasObject = new GameObject("ClaimCanvas");
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localScale = Vector3.one * 0.016f;

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        _group = canvasObject.AddComponent<CanvasGroup>();
        _group.alpha = 1f;
        _group.interactable = false;
        _group.blocksRaycasts = false;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(420f, 90f);

        GameObject textObject = new GameObject("ClaimText", typeof(RectTransform));
        textObject.transform.SetParent(canvasObject.transform, false);

        _label = textObject.AddComponent<Text>();
        _label.text = text;
        _label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _label.fontSize = 48;
        _label.fontStyle = FontStyle.Bold;
        _label.alignment = TextAnchor.MiddleCenter;
        _label.color = new Color(1f, 0.88f, 0.32f);
        _label.raycastTarget = false;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        canvasObject.AddComponent<BillboardLabel>();
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        transform.position = _startPosition + Vector3.up * (Duration - _timer) * RiseSpeed;

        if (_group != null)
        {
            _group.alpha = Mathf.Clamp01(_timer / Duration);
        }

        if (_timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
