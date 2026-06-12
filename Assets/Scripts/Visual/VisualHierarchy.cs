using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual readability tiers so players can tell interactables from scenery at a glance.
/// </summary>
public enum VisualHierarchyTier
{
    Static,
    DaoBuilding,
    Boundary,
    CivilizationSeed,
    Interactive
}

/// <summary>
/// Optional per-object settings when applying <see cref="VisualHierarchyTier.Interactive"/> or landmark tiers.
/// </summary>
public struct VisualHierarchyOptions
{
    public string LabelText;
    public Color AccentColor;
    public float SpinSpeed;
    public float FloatAmplitude;
    public float FloatSpeed;
    public float LabelHeight;
    public bool EnableParticles;
    public bool EnableFloat;
    public bool EnableSpin;
    public bool EnableGlowRing;

    public static VisualHierarchyOptions ForInteractive(string label, Color accent)
    {
        return new VisualHierarchyOptions
        {
            LabelText = label,
            AccentColor = accent,
            SpinSpeed = 36f,
            FloatAmplitude = 0.28f,
            FloatSpeed = 0.9f,
            LabelHeight = 2.4f,
            EnableParticles = true,
            EnableFloat = true,
            EnableSpin = true,
            EnableGlowRing = true
        };
    }

    public static VisualHierarchyOptions ForCivilizationSeed()
    {
        VisualHierarchyOptions options = ForInteractive("CIVILIZATION SEED", new Color(1f, 0.92f, 0.42f));
        options.SpinSpeed = 42f;
        options.FloatAmplitude = 0.38f;
        options.LabelHeight = 3.2f;
        return options;
    }
}

/// <summary>
/// Central visual hierarchy: emission strength, motion, particles, and floating labels.
/// </summary>
public static class VisualHierarchy
{
    const string HierarchyRootName = "VisualHierarchy_FX";

    public static float EmissionStrength(VisualHierarchyTier tier)
    {
        switch (tier)
        {
            case VisualHierarchyTier.DaoBuilding:
                return 0.18f;
            case VisualHierarchyTier.Boundary:
                return 0.55f;
            case VisualHierarchyTier.CivilizationSeed:
                return 1.15f;
            case VisualHierarchyTier.Interactive:
                return 0.72f;
            default:
                return 0f;
        }
    }

    public static void ApplyInteractiveFxOnly(GameObject root, VisualHierarchyOptions options)
    {
        if (root == null)
        {
            return;
        }

        ApplyInteractiveFx(root, options);
    }

    public static void Apply(GameObject root, VisualHierarchyTier tier, VisualHierarchyOptions? options = null)
    {
        if (root == null)
        {
            return;
        }

        switch (tier)
        {
            case VisualHierarchyTier.Static:
                ApplyStatic(root);
                break;
            case VisualHierarchyTier.DaoBuilding:
                ApplyEmissionTier(root, tier);
                break;
            case VisualHierarchyTier.Boundary:
                ApplyEmissionTier(root, tier);
                break;
            case VisualHierarchyTier.CivilizationSeed:
                ApplyEmissionTier(root, tier);
                ApplyInteractiveFx(root, options ?? VisualHierarchyOptions.ForCivilizationSeed());
                break;
            case VisualHierarchyTier.Interactive:
                ApplyEmissionTier(root, tier);
                ApplyInteractiveFx(root, options ?? VisualHierarchyOptions.ForInteractive("INTERACT", Color.white));
                break;
        }
    }

    public static void ApplyStatic(GameObject root)
    {
        StripMotion(root);
        SetEmissionOnRenderers(root, 0f);
    }

    static void ApplyEmissionTier(GameObject root, VisualHierarchyTier tier)
    {
        StripMotion(root);
        SetEmissionOnRenderers(root, EmissionStrength(tier));
    }

    static void ApplyInteractiveFx(GameObject root, VisualHierarchyOptions options)
    {
        Transform fxRoot = EnsureFxRoot(root);

        if (options.EnableSpin && root.GetComponent<VisualHierarchySpin>() == null)
        {
            root.AddComponent<VisualHierarchySpin>().Configure(options.SpinSpeed);
        }

        if (options.EnableFloat && root.GetComponent<VisualHierarchyFloat>() == null)
        {
            root.AddComponent<VisualHierarchyFloat>().Configure(options.FloatAmplitude, options.FloatSpeed);
        }

        if (options.EnableParticles && fxRoot.Find("InteractParticles") == null)
        {
            CreateParticleAura(fxRoot, options.AccentColor);
        }

        if (!string.IsNullOrEmpty(options.LabelText) && fxRoot.Find("InteractLabel") == null)
        {
            CreateFloatingLabel(fxRoot, options.LabelText, options.AccentColor, options.LabelHeight);
        }

        if (options.EnableGlowRing)
        {
            CreateGlowRing(fxRoot, options.AccentColor, VisualHierarchyTier.Interactive);
        }
        else
        {
            Transform existingRing = fxRoot.Find("GlowRing");
            if (existingRing != null)
            {
                Object.Destroy(existingRing.gameObject);
            }
        }
    }

    static Transform EnsureFxRoot(GameObject root)
    {
        Transform existing = root.transform.Find(HierarchyRootName);
        if (existing != null)
        {
            return existing;
        }

        GameObject fxRoot = new GameObject(HierarchyRootName);
        fxRoot.transform.SetParent(root.transform, false);
        return fxRoot.transform;
    }

    static void StripMotion(GameObject root)
    {
        EnvironmentFloatMotion[] envFloats = root.GetComponentsInChildren<EnvironmentFloatMotion>(true);
        for (int i = 0; i < envFloats.Length; i++)
        {
            Object.Destroy(envFloats[i]);
        }

        EnvironmentSlowSpin[] envSpins = root.GetComponentsInChildren<EnvironmentSlowSpin>(true);
        for (int i = 0; i < envSpins.Length; i++)
        {
            Object.Destroy(envSpins[i]);
        }

        VisualHierarchyFloat[] floats = root.GetComponentsInChildren<VisualHierarchyFloat>(true);
        for (int i = 0; i < floats.Length; i++)
        {
            if (floats[i].gameObject != root)
            {
                Object.Destroy(floats[i]);
            }
        }

        VisualHierarchySpin[] spins = root.GetComponentsInChildren<VisualHierarchySpin>(true);
        for (int i = 0; i < spins.Length; i++)
        {
            Object.Destroy(spins[i]);
        }
    }

    public static void SetEmissionOnRenderers(GameObject root, float strength)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            ApplyEmissionStrength(renderers[i], strength);
        }
    }

    public static void ApplyEmissionStrength(Renderer renderer, float strength)
    {
        if (renderer == null)
        {
            return;
        }

        Material[] materials = renderer.materials;
        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];
            if (mat == null)
            {
                continue;
            }

            Color baseColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
            if (strength <= 0f)
            {
                mat.DisableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", Color.black);
                }
            }
            else
            {
                Color emission = baseColor * strength;
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", emission);
                }
            }

            materials[i] = mat;
        }

        renderer.materials = materials;
    }

    public static Material CreateLitMaterial(Color color, Color emission, float smoothness)
    {
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

        if (emission != Color.black && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
        }

        return material;
    }

    static void CreateGlowRing(Transform parent, Color accent, VisualHierarchyTier tier)
    {
        if (parent.Find("GlowRing") != null)
        {
            return;
        }

        float scale = tier == VisualHierarchyTier.Interactive ? 1.35f : 1.1f;
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "GlowRing";
        ring.transform.SetParent(parent, false);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localScale = new Vector3(scale, 0.05f, scale);

        Collider collider = ring.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }

        Renderer renderer = ring.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color emission = accent * EmissionStrength(VisualHierarchyTier.Interactive);
            renderer.sharedMaterial = CreateLitMaterial(accent * 0.85f, emission, 0.2f);
        }

        ring.AddComponent<VisualHierarchyGlowPulse>().Configure(0.9f, 1.12f, 1.3f);
    }

    static void CreateParticleAura(Transform parent, Color accent)
    {
        GameObject particleObject = new GameObject("InteractParticles");
        particleObject.transform.SetParent(parent, false);
        particleObject.transform.localPosition = Vector3.up * 0.5f;

        ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = 1.2f;
        main.startSpeed = 0.35f;
        main.startSize = 0.12f;
        main.maxParticles = 24;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startColor = new ParticleSystem.MinMaxGradient(accent * 0.9f);

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 10f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.55f;

        ParticleSystemRenderer renderer = particleObject.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateLitMaterial(accent, accent * 0.5f, 0.1f);
    }

    static void CreateFloatingLabel(Transform parent, string text, Color accent, float height)
    {
        GameObject canvasObject = new GameObject("InteractLabel");
        canvasObject.transform.SetParent(parent, false);
        canvasObject.transform.localPosition = Vector3.up * height;
        canvasObject.transform.localScale = Vector3.one * 0.012f;

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(360f, 72f);

        GameObject textObject = new GameObject("LabelText", typeof(RectTransform));
        textObject.transform.SetParent(canvasObject.transform, false);

        Text label = textObject.AddComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 34;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = accent;
        label.raycastTarget = false;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        canvasObject.AddComponent<BillboardLabel>();
    }
}

/// <summary> Applies a visual hierarchy tier on Start for scene-authored objects. </summary>
public class VisualHierarchyObject : MonoBehaviour
{
    [SerializeField] VisualHierarchyTier tier = VisualHierarchyTier.Static;
    [SerializeField] bool useCustomInteractiveOptions;
    [SerializeField] string labelText = "INTERACT";
    [SerializeField] Color accentColor = Color.white;
    [SerializeField] float spinSpeed = 36f;
    [SerializeField] float floatAmplitude = 0.28f;
    [SerializeField] float labelHeight = 2.4f;

    void Start()
    {
        VisualHierarchyOptions? options = null;
        if (tier == VisualHierarchyTier.Interactive || tier == VisualHierarchyTier.CivilizationSeed)
        {
            if (useCustomInteractiveOptions)
            {
                options = new VisualHierarchyOptions
                {
                    LabelText = labelText,
                    AccentColor = accentColor,
                    SpinSpeed = spinSpeed,
                    FloatAmplitude = floatAmplitude,
                    FloatSpeed = 0.9f,
                    LabelHeight = labelHeight,
                    EnableParticles = true,
                    EnableFloat = tier == VisualHierarchyTier.CivilizationSeed,
                    EnableSpin = true
                };
            }
            else if (tier == VisualHierarchyTier.CivilizationSeed)
            {
                options = VisualHierarchyOptions.ForCivilizationSeed();
            }
        }

        VisualHierarchy.Apply(gameObject, tier, options);
    }
}

public class VisualHierarchyFloat : MonoBehaviour
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

public class VisualHierarchySpin : MonoBehaviour
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

public class VisualHierarchyGlowPulse : MonoBehaviour
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
