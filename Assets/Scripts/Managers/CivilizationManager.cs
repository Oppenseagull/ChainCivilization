using System;
using UnityEngine;

/// <summary>
/// In-memory civilization selection (session only, no persistence).
/// </summary>
public static class CivilizationManager
{
    public static CivilizationType selectedCivilizationType { get; private set; } = CivilizationType.None;

    public static bool HasSelectedCivilization => selectedCivilizationType != CivilizationType.None;

    public static event Action<CivilizationType> OnCivilizationSelected;

    public static void SelectCivilization(CivilizationType type)
    {
        if (type == CivilizationType.None)
        {
            return;
        }

        selectedCivilizationType = type;
        Debug.Log($"CivilizationManager: selectedCivilizationType = {selectedCivilizationType}");
        OnCivilizationSelected?.Invoke(selectedCivilizationType);
    }

    public static void ResetSelection()
    {
        selectedCivilizationType = CivilizationType.None;
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
                return "开放协作";
            case CivilizationType.TradeDAO:
                return "自由贸易";
            case CivilizationType.KnowledgeDAO:
                return "知识共享";
            default:
                return string.Empty;
        }
    }

    public static string GetFirstRuleText(CivilizationType type)
    {
        switch (type)
        {
            case CivilizationType.OpenDAO:
                return "开放协作高于封闭垄断";
            case CivilizationType.TradeDAO:
                return "自由交换创造价值";
            case CivilizationType.KnowledgeDAO:
                return "知识属于所有旅者";
            default:
                return string.Empty;
        }
    }

    public static string GetCivilizationName(CivilizationType type)
    {
        switch (type)
        {
            case CivilizationType.OpenDAO:
                return "开放协作文明";
            case CivilizationType.TradeDAO:
                return "自由贸易文明";
            case CivilizationType.KnowledgeDAO:
                return "知识共享文明";
            default:
                return "暂无";
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
                return "暂无";
        }
    }

    static bool TryGetTypeFromLabel(string label, out CivilizationType type)
    {
        switch (label)
        {
            case "开放协作":
                type = CivilizationType.OpenDAO;
                return true;
            case "自由贸易":
                type = CivilizationType.TradeDAO;
                return true;
            case "知识共享":
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
