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
        if (_currentMarker != null)
        {
            Destroy(_currentMarker);
        }

        Color accent = GetAccentColor(type);
        string typeName = CivilizationBonuses.GetAddressPanelCivilizationName(type);

        _currentMarker = new GameObject("My Civilization");
        _currentMarker.transform.SetParent(transform, false);
        _currentMarker.transform.localPosition = spawnLocalPosition;
        _currentMarker.transform.localRotation = Quaternion.identity;

        GameObject basePlatform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        basePlatform.name = "Marker_Base";
        basePlatform.transform.SetParent(_currentMarker.transform, false);
        basePlatform.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        basePlatform.transform.localScale = new Vector3(baseRadius * 2f, 0.12f, baseRadius * 2f);
        ApplyMaterial(basePlatform, new Color(0.66f, 0.62f, 0.5f));
        RemoveCollider(basePlatform);

        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pillar.name = "Marker_Pillar";
        pillar.transform.SetParent(_currentMarker.transform, false);
        pillar.transform.localPosition = new Vector3(0f, pillarHeight * 0.5f + 0.2f, 0f);
        pillar.transform.localScale = new Vector3(1.2f, pillarHeight, 1.2f);
        ApplyMaterial(pillar, accent);
        RemoveCollider(pillar);
        _glowRenderer = pillar.GetComponent<Renderer>();
        _glowBaseColor = accent;

        CreateLabelCanvas(_currentMarker.transform, typeName, accent);

        GroundSnapUtility.SnapTransform(_currentMarker.transform, 0f);
        VisualHierarchy.Apply(_currentMarker, VisualHierarchyTier.DaoBuilding);

        Debug.Log($"CivilizationMarkerSpawner: spawned My Civilization ({typeName}) at {_currentMarker.transform.position}");
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

    void CreateLabelCanvas(Transform parent, string typeName, Color accent)
    {
        GameObject canvasObject = new GameObject("Marker_LabelCanvas");
        canvasObject.transform.SetParent(parent, false);
        canvasObject.transform.localPosition = new Vector3(0f, pillarHeight + 1.8f, 0f);
        canvasObject.transform.localRotation = Quaternion.identity;
        canvasObject.transform.localScale = Vector3.one * 0.01f;

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(420f, 160f);

        GameObject panel = CreateUiObject("MarkerPanel", canvasObject.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        StretchFull(panelRect);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.06f, 0.1f, 0.16f, 0.92f);

        GameObject titleObject = CreateUiObject("MarkerTitle", panel.transform);
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.52f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        Text title = titleObject.AddComponent<Text>();
        title.text = "My Civilization";
        title.alignment = TextAnchor.MiddleCenter;
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 34;
        title.fontStyle = FontStyle.Bold;
        title.color = new Color(1f, 0.92f, 0.45f);

        GameObject typeObject = CreateUiObject("MarkerType", panel.transform);
        RectTransform typeRect = typeObject.GetComponent<RectTransform>();
        typeRect.anchorMin = new Vector2(0f, 0f);
        typeRect.anchorMax = new Vector2(1f, 0.48f);
        typeRect.offsetMin = Vector2.zero;
        typeRect.offsetMax = Vector2.zero;

        Text typeText = typeObject.AddComponent<Text>();
        typeText.text = typeName;
        typeText.alignment = TextAnchor.MiddleCenter;
        typeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        typeText.fontSize = 28;
        typeText.color = accent;

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
