using System;
using UnityEngine;

/// <summary>
/// Stores the player's selected civilization and persists it across scene reloads.
/// </summary>
public static class CivilizationManager
{
    static bool _loaded;
    static CivilizationType _selectedCivilizationType = CivilizationType.None;

    public static CivilizationType selectedCivilizationType
    {
        get
        {
            EnsureLoaded();
            return _selectedCivilizationType;
        }
        private set => _selectedCivilizationType = value;
    }

    public static bool HasSelectedCivilization
    {
        get
        {
            EnsureLoaded();
            return _selectedCivilizationType != CivilizationType.None;
        }
    }

    public static event Action<CivilizationType> OnCivilizationSelected;

    public static void SelectCivilization(CivilizationType type)
    {
        if (type == CivilizationType.None)
        {
            return;
        }

        EnsureLoaded();
        selectedCivilizationType = type;
        PlayerPrefs.SetInt(GameSaveKeys.SelectedCivilizationType, (int)type);
        PlayerPrefs.Save();
        Debug.Log($"CivilizationManager: selectedCivilizationType = {selectedCivilizationType}");
        OnCivilizationSelected?.Invoke(selectedCivilizationType);
    }

    public static void ResetSelection()
    {
        _loaded = true;
        selectedCivilizationType = CivilizationType.None;
        PlayerPrefs.DeleteKey(GameSaveKeys.SelectedCivilizationType);
        PlayerPrefs.Save();
    }

    public static bool TryParseTypeFromLabel(string label, out CivilizationType type)
    {
        return TryGetTypeFromLabel(label, out type);
    }

    public static bool TrySelectFromLabel(string label)
    {
        if (!TryGetTypeFromLabel(label, out CivilizationType type))
        {
            Debug.LogWarning($"CivilizationManager: unknown rule label '{label}'");
            return false;
        }

        SelectCivilization(type);
        return true;
    }

    public static string GetDisplayName(CivilizationType type)
    {
        switch (type)
        {
            case CivilizationType.OpenDAO:
                return "Open Collaboration";
            case CivilizationType.TradeDAO:
                return "Free Trade";
            case CivilizationType.KnowledgeDAO:
                return "Shared Knowledge";
            default:
                return string.Empty;
        }
    }

    public static string GetFirstRuleText(CivilizationType type)
    {
        switch (type)
        {
            case CivilizationType.OpenDAO:
                return "Open collaboration rises above closed walls.";
            case CivilizationType.TradeDAO:
                return "Free exchange creates shared value.";
            case CivilizationType.KnowledgeDAO:
                return "Knowledge belongs to every builder.";
            default:
                return string.Empty;
        }
    }

    public static string GetCivilizationName(CivilizationType type)
    {
        switch (type)
        {
            case CivilizationType.OpenDAO:
                return "Open Collaboration Civilization";
            case CivilizationType.TradeDAO:
                return "Free Trade Civilization";
            case CivilizationType.KnowledgeDAO:
                return "Shared Knowledge Civilization";
            default:
                return "None";
        }
    }

    public static string GetTypeName(CivilizationType type)
    {
        switch (type)
        {
            case CivilizationType.OpenDAO:
                return "OpenDAO";
            case CivilizationType.TradeDAO:
                return "TradeDAO";
            case CivilizationType.KnowledgeDAO:
                return "KnowledgeDAO";
            default:
                return "None";
        }
    }

    static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        int saved = PlayerPrefs.GetInt(GameSaveKeys.SelectedCivilizationType, (int)CivilizationType.None);
        _selectedCivilizationType = Enum.IsDefined(typeof(CivilizationType), saved)
            ? (CivilizationType)saved
            : CivilizationType.None;
        _loaded = true;
    }

    static bool TryGetTypeFromLabel(string label, out CivilizationType type)
    {
        switch (label)
        {
            case "Open Collaboration":
            case "Open DAO":
            case "\u5F00\u653E\u534F\u4F5C":
                type = CivilizationType.OpenDAO;
                return true;
            case "Free Trade":
            case "Trade DAO":
            case "\u81EA\u7531\u8D38\u6613":
                type = CivilizationType.TradeDAO;
                return true;
            case "Shared Knowledge":
            case "Knowledge DAO":
            case "\u77E5\u8BC6\u5171\u4EAB":
                type = CivilizationType.KnowledgeDAO;
                return true;
            default:
                type = CivilizationType.None;
                return false;
        }
    }
}

public enum CivilizationType
{
    None,
    OpenDAO,
    TradeDAO,
    KnowledgeDAO
}
