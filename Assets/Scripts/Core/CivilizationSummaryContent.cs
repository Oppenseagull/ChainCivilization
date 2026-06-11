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
                        "这是一个开放文明。",
                        "任何人都可以加入。"
                    },
                    Advantage = "增长速度快。",
                    Risk = "共识较弱。"
                };
            case CivilizationType.TradeDAO:
                return new SummaryData
                {
                    ChoiceLabel = "Trade DAO",
                    WorldEvaluation = new[]
                    {
                        "这是一个市场文明。",
                        "资源在旅者之间自由流通。"
                    },
                    Advantage = "资源流通效率高。",
                    Risk = "容易形成财富集中。"
                };
            case CivilizationType.KnowledgeDAO:
                return new SummaryData
                {
                    ChoiceLabel = "Builder DAO",
                    WorldEvaluation = new[]
                    {
                        "这是一个建设文明。",
                        "号召旅者共同建造家园。"
                    },
                    Advantage = "建设能力强。",
                    Risk = "扩张速度慢。"
                };
            default:
                return default;
        }
    }
}
