/// <summary>
/// Active civilization rule bonuses. Take effect immediately after civilization creation.
/// </summary>
public static class CivilizationBonuses
{
    public const int OpenDaoMoonCrystalBonus = 20;
    public const int DefaultRedDonationReputation = 20;
    public const int TradeDaoRedDonationReputation = 40;

    public static int GetMoonCrystalBonus(CivilizationType type)
    {
        return type == CivilizationType.OpenDAO ? OpenDaoMoonCrystalBonus : 0;
    }

    public static int GetRedDonationReputationReward(CivilizationType type)
    {
        return type == CivilizationType.TradeDAO
            ? TradeDaoRedDonationReputation
            : DefaultRedDonationReputation;
    }

    public static string GetBonusDescription(CivilizationType type)
    {
        string bonusLine = GetRuleBonusLine(type);
        return string.IsNullOrEmpty(bonusLine) ? string.Empty : bonusLine;
    }

    public static string GetRuleTagline(CivilizationType type)
    {
        switch (type)
        {
            case CivilizationType.OpenDAO:
                return "Open Entry";
            case CivilizationType.TradeDAO:
                return "Free Exchange";
            case CivilizationType.KnowledgeDAO:
                return "Shared Building";
            default:
                return string.Empty;
        }
    }

    public static string GetRuleBonusLine(CivilizationType type)
    {
        switch (type)
        {
            case CivilizationType.OpenDAO:
                return "+20 MOON from each Moon Crystal";
            case CivilizationType.TradeDAO:
                return "+40 Reputation from Red DAO donation";
            case CivilizationType.KnowledgeDAO:
                return "Builder civilization has construction focus";
            default:
                return string.Empty;
        }
    }

    public static string GetAddressPanelCivilizationName(CivilizationType type)
    {
        switch (type)
        {
            case CivilizationType.OpenDAO:
                return "Open DAO";
            case CivilizationType.TradeDAO:
                return "Trade DAO";
            case CivilizationType.KnowledgeDAO:
                return "Builder DAO";
            default:
                return "Not Created";
        }
    }
}
