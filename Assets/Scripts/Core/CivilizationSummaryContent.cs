/// <summary>
/// Civilization summary text shown after the player creates their civilization.
/// </summary>
public static class CivilizationSummaryContent
{
    public struct SummaryData
    {
        public string ChoiceLabel;
        public string[] WorldEvaluation;
        public string Advantage;
        public string Risk;
    }

    public static SummaryData GetSummary(CivilizationType type)
    {
        switch (type)
        {
            case CivilizationType.OpenDAO:
                return new SummaryData
                {
                    ChoiceLabel = "Open DAO",
                    WorldEvaluation = new[]
                    {
                        "This is an open civilization.",
                        "Anyone can join and contribute."
                    },
                    Advantage = "Fast growth.",
                    Risk = "Consensus may be fragile."
                };
            case CivilizationType.TradeDAO:
                return new SummaryData
                {
                    ChoiceLabel = "Trade DAO",
                    WorldEvaluation = new[]
                    {
                        "This is a market civilization.",
                        "Resources flow through free exchange."
                    },
                    Advantage = "High resource efficiency.",
                    Risk = "Wealth may concentrate."
                };
            case CivilizationType.KnowledgeDAO:
                return new SummaryData
                {
                    ChoiceLabel = "Builder DAO",
                    WorldEvaluation = new[]
                    {
                        "This is a builder civilization.",
                        "Travelers gather to create shared knowledge."
                    },
                    Advantage = "Strong construction ability.",
                    Risk = "Expansion may be slower."
                };
            default:
                return default;
        }
    }
}
