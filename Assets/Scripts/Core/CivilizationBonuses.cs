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
        if (type == CivilizationType.OpenDAO)
        {
            return OpenDaoMoonCrystalBonus;
        }

        return 0;
    }

    public static int GetRedDonationReputationReward(CivilizationType type)
    {
        if (type == CivilizationType.TradeDAO)
        {
            return TradeDaoRedDonationReputation;
        }

        return DefaultRedDonationReputation;
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
                return "自由加入";
            case CivilizationType.TradeDAO:
                return "自由交换";
            case CivilizationType.KnowledgeDAO:
                return "共建文明";
            default:
                return string.Empty;
        }
    }

    public static string GetRuleBonusLine(CivilizationType type)
    {
        switch (type)
        {
            case CivilizationType.OpenDAO:
                return "采集额外获得20 MOON";
            case CivilizationType.TradeDAO:
                return "捐赠额外获得40 Reputation";
            case CivilizationType.KnowledgeDAO:
                return "Builder文明拥有建设加成";
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
                return "未创建";
        }
    }
}
