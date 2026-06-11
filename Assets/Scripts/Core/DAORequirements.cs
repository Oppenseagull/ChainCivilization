using UnityEngine;

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
            line1 = "Green Pass 已获得";
            line2 = "你已经拥有建立文明资格";
            return;
        }

        if (MeetsGreenDaoAccess(moonBalance, reputation))
        {
            line1 = "欢迎加入 Green DAO";
            line2 = "进入区域即可获得 Green Pass";
            return;
        }

        line1 = $"需要：\nMOON >= {GreenDaoRequiredMoon}\nREP >= {GreenDaoRequiredReputation}";
        line2 = $"当前：\nMOON {moonBalance}\nREP {reputation}";
    }
}
