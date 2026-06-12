using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spawns a "My Civilization" world marker near BoundaryStone after selection.
/// </summary>
public class CivilizationMarkerSpawner : MonoBehaviour
{
    [SerializeField] Vector3 spawnLocalPosition = new Vector3(-22f, 0f, 18f);
    [SerializeField] float baseRadius = 2.2f;
    [SerializeField] float pillarHeight = 4.5f;

    GameObject _currentMarker;
    Renderer _glowRenderer;
    Color _glowBaseColor;
    Coroutine _glowRoutine;

    void OnEnable()
    {
        CivilizationManager.OnCivilizationSelected += HandleCivilizationSelected;

        if (CivilizationManager.HasSelectedCivilization)
        {
            HandleCivilizationSelected(CivilizationManager.selectedCivilizationType);
        }
    }

    void OnDisable()
    {
        CivilizationManager.OnCivilizationSelected -= HandleCivilizationSelected;
    }

    void HandleCivilizationSelected(CivilizationType type)
    {
        if (type == CivilizationType.None)
        {
            return;
        }

        SpawnMarker(type);
    }

    void SpawnMarker(CivilizationType type)
    {
        StartCoroutine(CinematicRoutine(type));
    }

    IEnumerator CinematicRoutine(CivilizationType type)
    {
        if (_currentMarker != null)
        {
            Destroy(_currentMarker);
        }

        Color accent = GetAccentColor(type);
        string typeName = CivilizationBonuses.GetAddressPanelCivilizationName(type);

        Transform player = GameObject.Find("Player")?.transform;
        Vector3 spawnPos = player != null ? player.position + player.forward * 30f : transform.position + spawnLocalPosition;

        _currentMarker = new GameObject("My Civilization");
        _currentMarker.transform.position = spawnPos;
        GroundSnapUtility.SnapTransform(_currentMarker.transform, 0f);

        float originalAmbient = RenderSettings.ambientIntensity;
        for (float t = 0; t < 1f; t += Time.deltaTime)
        {
            RenderSettings.ambientIntensity = Mathf.Lerp(originalAmbient, 0.1f, t);
            yield return null;
        }

        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pillar.name = "Marker_Pillar";
        pillar.transform.SetParent(_currentMarker.transform, false);
        pillar.transform.localPosition = new Vector3(0f, 50f, 0f);
        pillar.transform.localScale = new Vector3(15f, 100f, 15f);
        ApplyMaterial(pillar, accent);
        RemoveCollider(pillar);
        _glowRenderer = pillar.GetComponent<Renderer>();
        _glowBaseColor = accent;

        CreateCinematicLabel(_currentMarker.transform, typeName, accent);

        VisualHierarchy.Apply(_currentMarker, VisualHierarchyTier.DaoBuilding);
        _glowRoutine = StartCoroutine(PulseGlow());

        for (float t = 0; t < 1f; t += Time.deltaTime * 0.5f)
        {
            RenderSettings.ambientIntensity = Mathf.Lerp(0.1f, originalAmbient * 0.4f, t);
            yield return null;
        }
    }

    IEnumerator PulseGlow()
    {
        if (_glowRenderer == null)
        {
            yield break;
        }

        const float duration = 2.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float pulse = 0.55f + 0.45f * Mathf.Sin(elapsed * 4.5f);
            Color glow = Color.Lerp(_glowBaseColor, Color.white, pulse * 0.35f);
            if (_glowRenderer.material != null)
            {
                _glowRenderer.material.color = glow;
                if (_glowRenderer.material.HasProperty("_BaseColor"))
                {
                    _glowRenderer.material.SetColor("_BaseColor", glow);
                }

                if (_glowRenderer.material.HasProperty("_EmissionColor"))
                {
                    _glowRenderer.material.EnableKeyword("_EMISSION");
                    _glowRenderer.material.SetColor("_EmissionColor", glow * (1.2f + pulse));
                }
            }

            yield return null;
        }

        if (_glowRenderer != null && _glowRenderer.material != null)
        {
            _glowRenderer.material.color = _glowBaseColor;
            if (_glowRenderer.material.HasProperty("_BaseColor"))
            {
                _glowRenderer.material.SetColor("_BaseColor", _glowBaseColor);
            }
        }

        _glowRoutine = null;
    }

    void CreateCinematicLabel(Transform parent, string typeName, Color accent)
    {
        GameObject canvasObject = new GameObject("CinematicLabel");
        canvasObject.transform.SetParent(parent, false);
        canvasObject.transform.localPosition = new Vector3(0f, 115f, 0f);
        canvasObject.transform.localScale = Vector3.one * 0.2f;

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1000f, 400f);

        GameObject titleObj = CreateUiObject("TitleText", canvasObject.transform);
        StretchFull(titleObj.GetComponent<RectTransform>());
        Text title = titleObj.AddComponent<Text>();
        title.text = typeName.ToUpperInvariant() + "\nCIVILIZATION\nHAS RISEN";
        title.alignment = TextAnchor.MiddleCenter;
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 80;
        title.fontStyle = FontStyle.Bold;
        title.color = accent;
        
        BillboardLabel billboard = canvasObject.AddComponent<BillboardLabel>();
        billboard.enabled = true;
    }

    static Color GetAccentColor(CivilizationType type)
    {
        switch (type)
        {
            case CivilizationType.OpenDAO:
                return new Color(0.58f, 0.76f, 0.98f);
            case CivilizationType.TradeDAO:
                return new Color(0.96f, 0.62f, 0.44f);
            case CivilizationType.KnowledgeDAO:
                return new Color(0.56f, 0.88f, 0.54f);
            default:
                return Color.white;
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

    static void ApplyMaterial(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        renderer.material = material;
    }

    static void RemoveCollider(GameObject target)
    {
        Collider collider = target.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }
    }
}

/// <summary>
/// Keeps a world-space label facing the main camera.
/// </summary>
public class BillboardLabel : MonoBehaviour
{
    Camera _camera;

    void LateUpdate()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                return;
            }
        }

        transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position);
    }
}
