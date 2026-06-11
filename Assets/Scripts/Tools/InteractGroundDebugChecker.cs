using System.Collections;
using UnityEngine;

/// <summary>
/// Logs world position and ground gap for key interact objects after play starts.
/// Warns when interactables or triggers float more than 2m above ground.
/// </summary>
public class InteractGroundDebugChecker : MonoBehaviour
{
    const string LogPrefix = "[InteractGroundDebug]";

    [SerializeField] float checkDelay = 0.25f;
    [SerializeField] float warnHeightThreshold = InteractPlacementRules.DebugWarnHeightThreshold;
    [SerializeField] bool runOnStart = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreateChecker()
    {
        if (FindAnyObjectByType<InteractGroundDebugChecker>() != null)
        {
            return;
        }

        new GameObject("InteractGroundDebugChecker").AddComponent<InteractGroundDebugChecker>();
    }

    IEnumerator Start()
    {
        if (!runOnStart)
        {
            yield break;
        }

        yield return new WaitForSeconds(checkDelay);
        RunCheck();
    }

    [ContextMenu("Run Interact Ground Check")]
    public void RunCheck()
    {
        int warnCount = 0;

        Debug.Log($"{LogPrefix} ===== Interact Ground Check =====");

        warnCount += CheckComponent<CivilizationSeedInteract>(InteractCategory.Interactable);
        warnCount += CheckComponent<MoonCrystalInteract>(InteractCategory.Interactable);
        warnCount += CheckComponent<BlueDAOSteleInteract>(InteractCategory.Interactable);
        warnCount += CheckComponent<RedDAOSteleInteract>(InteractCategory.Interactable);
        warnCount += CheckComponent<GreenDAOSteleInteract>(InteractCategory.Interactable);
        warnCount += CheckComponent<BoundaryTrigger>(InteractCategory.Trigger);
        warnCount += CheckComponent<DAOPassEntryZone>(InteractCategory.Trigger);

        CheckNamedObject("BoundaryStone", InteractCategory.Landmark, ref warnCount);
        CheckNamedObject("CivilizationFlag_Blue", InteractCategory.Interactable, ref warnCount);
        CheckNamedObject("CivilizationFlag_Red", InteractCategory.Interactable, ref warnCount);
        CheckNamedObject("CivilizationFlag_Green", InteractCategory.Interactable, ref warnCount);
        CheckNamedObject("My Civilization", InteractCategory.Interactable, ref warnCount);

        if (warnCount == 0)
        {
            Debug.Log($"{LogPrefix} Check complete. No floating interact warnings.");
            return;
        }

        Debug.LogWarning($"{LogPrefix} Check complete with {warnCount} warning(s).");
    }

    enum InteractCategory
    {
        Interactable,
        Trigger,
        Landmark
    }

    int CheckComponent<T>(InteractCategory category) where T : Component
    {
        int warnCount = 0;
        T[] items = FindObjectsByType<T>(FindObjectsSortMode.None);

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                continue;
            }

            LogTransform(items[i].transform, category, ref warnCount);
        }

        return warnCount;
    }

    void CheckNamedObject(string objectName, InteractCategory category, ref int warnCount)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null)
        {
            return;
        }

        LogTransform(target.transform, category, ref warnCount);
    }

    void LogTransform(Transform target, InteractCategory category, ref int warnCount)
    {
        Vector3 worldPosition = target.position;
        string categoryLabel = GetCategoryLabel(category);

        if (!GroundSnapUtility.TryGetHeightAboveGround(worldPosition, out float heightAboveGround))
        {
            Debug.LogWarning(
                $"{LogPrefix}[WARN] {target.name} | Category: {categoryLabel} | World: {FormatVector(worldPosition)} | GroundGap: N/A (no ground hit)");
            warnCount++;
            return;
        }

        string message = BuildMessage(target.name, categoryLabel, worldPosition, heightAboveGround);
        Debug.Log(message);

        if (ShouldWarn(category, heightAboveGround))
        {
            Debug.LogWarning($"{LogPrefix}[WARN] {message} | Expected: {GetExpectedRange(category)}");
            warnCount++;
        }
    }

    static bool ShouldWarn(InteractCategory category, float heightAboveGround)
    {
        switch (category)
        {
            case InteractCategory.Interactable:
            case InteractCategory.Trigger:
                return heightAboveGround > InteractPlacementRules.DebugWarnHeightThreshold;

            case InteractCategory.Landmark:
                return false;

            default:
                return false;
        }
    }

    static string GetCategoryLabel(InteractCategory category)
    {
        switch (category)
        {
            case InteractCategory.Interactable:
                return "Interactable";
            case InteractCategory.Trigger:
                return "Trigger";
            case InteractCategory.Landmark:
                return "Landmark";
            default:
                return "Unknown";
        }
    }

    static string GetExpectedRange(InteractCategory category)
    {
        switch (category)
        {
            case InteractCategory.Interactable:
                return $"{InteractPlacementRules.InteractableMinHeight}~{InteractPlacementRules.InteractableMaxHeight}m above ground";
            case InteractCategory.Trigger:
                return $"on ground (<= {InteractPlacementRules.TriggerMaxHeightAboveGround}m center height)";
            case InteractCategory.Landmark:
                return "any height";
            default:
                return "n/a";
        }
    }

    static string BuildMessage(string objectName, string categoryLabel, Vector3 worldPosition, float heightAboveGround)
    {
        return $"{objectName} | Category: {categoryLabel} | World: {FormatVector(worldPosition)} | GroundGap: {heightAboveGround:F2}m";
    }

    static string FormatVector(Vector3 value)
    {
        return $"({value.x:F1}, {value.y:F1}, {value.z:F1})";
    }
}
