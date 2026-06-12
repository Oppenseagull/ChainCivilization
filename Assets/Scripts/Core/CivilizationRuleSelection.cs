using System;
using UnityEngine;

/// <summary>
/// Stores the player's first civilization join-rule choice.
/// </summary>
public static class CivilizationRuleSelection
{
    public enum JoinRule
    {
        None,
        AnyoneCanJoin,
        PassRequired,
        ContributionRequired
    }

    static bool _loaded;
    static JoinRule _selectedJoinRule = JoinRule.None;

    public static JoinRule SelectedJoinRule
    {
        get
        {
            EnsureLoaded();
            return _selectedJoinRule;
        }
        private set => _selectedJoinRule = value;
    }

    public static void Reset()
    {
        _loaded = true;
        SelectedJoinRule = JoinRule.None;
        PlayerPrefs.DeleteKey(GameSaveKeys.SelectedJoinRule);
        PlayerPrefs.Save();
    }

    public static void Select(JoinRule rule)
    {
        SelectedJoinRule = rule;
        PlayerPrefs.SetInt(GameSaveKeys.SelectedJoinRule, (int)rule);
        PlayerPrefs.Save();
    }

    public static string GetDisplayLine(JoinRule rule)
    {
        switch (rule)
        {
            case JoinRule.AnyoneCanJoin:
                return "Anyone can join";
            case JoinRule.PassRequired:
                return "A Pass is required to join";
            case JoinRule.ContributionRequired:
                return "Contribution is required to join";
            default:
                return "Not selected";
        }
    }

    static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        int saved = PlayerPrefs.GetInt(GameSaveKeys.SelectedJoinRule, (int)JoinRule.None);
        SelectedJoinRule = Enum.IsDefined(typeof(JoinRule), saved)
            ? (JoinRule)saved
            : JoinRule.None;
        _loaded = true;
    }
}
