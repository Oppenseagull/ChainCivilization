/// <summary>
/// Stores the player's first civilization join-rule choice (demo-only, no governance system).
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

    public static JoinRule SelectedJoinRule { get; private set; } = JoinRule.None;

    public static void Reset()
    {
        SelectedJoinRule = JoinRule.None;
    }

    public static void Select(JoinRule rule)
    {
        SelectedJoinRule = rule;
    }

    public static string GetDisplayLine(JoinRule rule)
    {
        switch (rule)
        {
            case JoinRule.AnyoneCanJoin:
                return "任何人都可以加入";
            case JoinRule.PassRequired:
                return "需要Pass才能加入";
            case JoinRule.ContributionRequired:
                return "需要贡献才能加入";
            default:
                return "未选择";
        }
    }
}

