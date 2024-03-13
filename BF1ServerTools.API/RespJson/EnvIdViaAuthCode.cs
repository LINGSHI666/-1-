namespace BF1ServerTools.API.RespJson;

public class EnvIdViaAuthCode
{
    public string jsonrpc { get; set; }
    public string id { get; set; }
    public Result result { get; set; }
    public class Result
    {
        public string envId { get; set; }
        public Parameters parameters { get; set; }
        public string sessionId { get; set; }
        public string personaId { get; set; }
        public class Parameters
        {
            public string bbPrefix { get; set; }
            public bool supportsFilterState { get; set; }
            public bool supportsCampaignOperations { get; set; }
            public List<string> featureFlags { get; set; }
            public string currentUtcTimestamp { get; set; }
            public bool hasOnlineAccess { get; set; }
            [JsonIgnore]
            public string background { get; set; }
        }
    }
}