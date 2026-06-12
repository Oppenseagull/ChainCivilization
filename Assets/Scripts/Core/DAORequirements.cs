/// <summary>
/// Single source of truth for Green DAO access thresholds.
/// </summary>
public static class DAORequirements
{
    public const int GreenDaoRequiredMoon = 200;
    public const int GreenDaoRequiredReputation = 20;

    public static bool MeetsGreenDaoAccess(int moonBalance, int reputation)
    {
        return moonBalance >= GreenDaoRequiredMoon && reputation >= GreenDaoRequiredReputation;
    }

    public static bool MeetsGreenDaoMoonRequirement(TokenManager tokens)
    {
        return tokens != null && tokens.MoonBalance >= GreenDaoRequiredMoon;
    }

    public static bool MeetsGreenDaoReputationRequirement(ReputationManager reputation)
    {
        return reputation != null && reputation.GetReputation() >= GreenDaoRequiredReputation;
    }

    public static void GetGreenDaoStatusHud(int moonBalance, int reputation, bool hasPass, out string line1, out string line2)
    {
        if (hasPass)
        {
            line1 = "Green Pass acquired";
            line2 = "You can now reach the civilization boundary.";
            return;
        }

        if (MeetsGreenDaoAccess(moonBalance, reputation))
        {
            line1 = "Green DAO accepts you";
            line2 = "Enter the green zone to receive Green Pass.";
            return;
        }

        line1 = $"Need: MOON {moonBalance}/{GreenDaoRequiredMoon}";
        line2 = $"Need: REP {reputation}/{GreenDaoRequiredReputation}";
    }
}
