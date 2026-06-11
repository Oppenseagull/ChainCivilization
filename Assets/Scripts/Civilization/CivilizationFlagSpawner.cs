using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns a civilization flag near BoundaryStone when the player selects a rule.
/// </summary>
public class CivilizationFlagSpawner : MonoBehaviour
{
    [SerializeField] Vector3 spawnLocalPosition = new Vector3(22f, 0f, -22f);
    [SerializeField] float poleHeight = 10f;
    [SerializeField] float poleRadius = 0.18f;
    [SerializeField] Vector3 bannerScale = new Vector3(3.5f, 2f, 0.12f);
    [SerializeField] float bannerHeight = 8.5f;

    GameObject _currentFlag;
    Transform _bannerTransform;
    float _bannerTargetHeight;
    Coroutine _raiseRoutine;

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

        SpawnFlag(type);
    }

    void SpawnFlag(CivilizationType type)
    {
        if (_currentFlag != null)
        {
            Destroy(_currentFlag);
        }

        string flagName = GetFlagName(type);
        Color bannerColor = GetFlagColor(type);

        _currentFlag = new GameObject(flagName);
        _currentFlag.transform.SetParent(transform, false);
        _currentFlag.transform.localPosition = spawnLocalPosition;
        _currentFlag.transform.localRotation = Quaternion.Euler(0f, 135f, 0f);

        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "FlagPole";
        pole.transform.SetParent(_currentFlag.transform, false);
        pole.transform.localPosition = new Vector3(0f, poleHeight * 0.5f, 0f);
        pole.transform.localScale = new Vector3(poleRadius * 2f, poleHeight * 0.5f, poleRadius * 2f);
        ApplyMaterial(pole, new Color(0.72f, 0.74f, 0.78f));
        RemoveCollider(pole);

        GameObject banner = GameObject.CreatePrimitive(PrimitiveType.Cube);
        banner.name = "FlagBanner";
        banner.transform.SetParent(_currentFlag.transform, false);
        _bannerTargetHeight = bannerHeight;
        banner.transform.localPosition = new Vector3(bannerScale.x * 0.45f, 0.5f, 0f);
        banner.transform.localScale = bannerScale;
        ApplyMaterial(banner, bannerColor);
        RemoveCollider(banner);
        _bannerTransform = banner.transform;

        GroundSnapUtility.SnapTransform(_currentFlag.transform, 0f);
        VisualHierarchy.Apply(_currentFlag, VisualHierarchyTier.DaoBuilding);

        if (_raiseRoutine != null)
        {
            StopCoroutine(_raiseRoutine);
        }

        _raiseRoutine = StartCoroutine(RaiseBanner());

        Debug.Log($"CivilizationFlagSpawner: spawned {flagName} at {_currentFlag.transform.position}");
    }

    IEnumerator RaiseBanner()
    {
        if (_bannerTransform == null)
        {
            yield break;
        }

        const float duration = 1.6f;
        float elapsed = 0f;
        Vector3 start = _bannerTransform.localPosition;
        Vector3 end = new Vector3(start.x, _bannerTargetHeight, start.z);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _bannerTransform.localPosition = Vector3.Lerp(start, end, t);
            yield return null;
        }

        _bannerTransform.localPosition = end;
        _raiseRoutine = null;
    }

    static string GetFlagName(CivilizationType type)
    {
        switch (type)
        {
            case CivilizationType.OpenDAO:
                return "CivilizationFlag_Blue";
            case CivilizationType.TradeDAO:
                return "CivilizationFlag_Red";
            case CivilizationType.KnowledgeDAO:
                return "CivilizationFlag_Green";
            default:
                return "CivilizationFlag";
        }
    }

    static Color GetFlagColor(CivilizationType type)
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

    static void ApplyMaterial(GameObject target, Color color)
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
