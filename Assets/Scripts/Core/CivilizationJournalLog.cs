using System.Collections.Generic;

/// <summary>
/// Session-only civilization journal timeline. Auto-records DAO visits, rewards,
/// passes, and civilization creation.
/// </summary>
public static class CivilizationJournalLog
{
    public readonly struct Entry
    {
        public readonly string DayLabel;
        public readonly IReadOnlyList<string> Lines;

        public Entry(string dayLabel, IReadOnlyList<string> lines)
        {
            DayLabel = dayLabel;
            Lines = lines;
        }
    }

    const string DemoDayLabel = "Day 1";

    static readonly List<Entry> _entries = new List<Entry>();
    static readonly HashSet<string> _recordedKeys = new HashSet<string>();

    public static IReadOnlyList<Entry> Entries => _entries;

    public static void ResetSession()
    {
        _entries.Clear();
        _recordedKeys.Clear();
    }

    public enum DaoVisitKind
    {
        Blue,
        Red,
        Green
    }

    public static void RecordDaoVisited(DaoVisitKind kind)
    {
        string key = $"dao_{kind}";
        if (!_recordedKeys.Add(key))
        {
            return;
        }

        GetDaoVisitLines(kind, out string daoName, out string learnLine);
        AddEntry(DemoDayLabel, daoName, learnLine);
    }

    public static void RecordMoonReward(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        string key = $"moon_{amount}_{_entries.Count}";
        if (!_recordedKeys.Add(key))
        {
            return;
        }

        AddEntry(DemoDayLabel, $"Gained {amount} MOON");
    }

    public static void RecordMoonRewardOnce(string uniqueKey, int amount)
    {
        if (amount <= 0 || !_recordedKeys.Add(uniqueKey))
        {
            return;
        }

        AddEntry(DemoDayLabel, $"Gained {amount} MOON");
    }

    public static void RecordReputationReward(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        string key = $"rep_{amount}_{_entries.Count}";
        if (!_recordedKeys.Add(key))
        {
            return;
        }

        AddEntry(DemoDayLabel, $"Gained {amount} Reputation");
    }

    public static void RecordReputationRewardOnce(string uniqueKey, int amount)
    {
        if (amount <= 0 || !_recordedKeys.Add(uniqueKey))
        {
            return;
        }

        AddEntry(DemoDayLabel, $"Gained {amount} Reputation");
    }

    public static void RecordPass(string passId, string displayName)
    {
        string key = $"pass_{passId}";
        if (!_recordedKeys.Add(key))
        {
            return;
        }

        string label = FormatPassLabel(passId, displayName);
        AddEntry(DemoDayLabel, $"Received {label}");
    }

    public static void RecordCivilizationCreated(CivilizationType type, CivilizationRuleSelection.JoinRule joinRule)
    {
        if (!_recordedKeys.Add($"civ_{type}"))
        {
            return;
        }

        string civName = CivilizationBonuses.GetAddressPanelCivilizationName(type);
        string ruleLine = CivilizationRuleSelection.GetDisplayLine(joinRule);
        AddEntry(DemoDayLabel, $"Created {civName}", "Rule:", ruleLine);
    }

    static void GetDaoVisitLines(DaoVisitKind kind, out string daoName, out string learnLine)
    {
        switch (kind)
        {
            case DaoVisitKind.Blue:
                daoName = "Discovered Blue DAO";
                learnLine = "Learned open collaboration";
                break;
            case DaoVisitKind.Red:
                daoName = "Discovered Red DAO";
                learnLine = "Learned market civilization";
                break;
            case DaoVisitKind.Green:
                daoName = "Discovered Green DAO";
                learnLine = "Learned access civilization";
                break;
            default:
                daoName = "Discovered DAO";
                learnLine = string.Empty;
                break;
        }
    }

    static string FormatPassLabel(string passId, string displayName)
    {
        if (!string.IsNullOrEmpty(displayName))
        {
            if (displayName.Contains("Green"))
            {
                return "Green Pass";
            }

            return displayName;
        }

        if (passId == DAOPassManager.GreenPassId)
        {
            return "Green Pass";
        }

        return passId;
    }

    static void AddEntry(string dayLabel, params string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            return;
        }

        var cleaned = new List<string>(lines.Length);
        for (int i = 0; i < lines.Length; i++)
        {
            if (!string.IsNullOrEmpty(lines[i]))
            {
                cleaned.Add(lines[i]);
            }
        }

        if (cleaned.Count == 0)
        {
            return;
        }

        _entries.Add(new Entry(dayLabel, cleaned));
    }
}
