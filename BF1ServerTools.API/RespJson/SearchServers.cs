namespace BF1ServerTools.API.RespJson;

public class SearchServers
{
    public string jsonrpc { get; set; }
    public string id { get; set; }
    public Result result { get; set; }
    public class Result
    {
        public List<GameserversItem> gameservers { get; set; }
        public bool hasMoreResults { get; set; }
        public class GameserversItem
        {
            public string gameId { get; set; }
            public string guid { get; set; }
            public string protocolVersion { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string region { get; set; }
            public string country { get; set; }
            public bool ranked { get; set; }
            public Slots slots { get; set; }
            public string mapName { get; set; }
            public string mapNamePretty { get; set; }
            public string mapMode { get; set; }
            public string mapModePretty { get; set; }
            public string mapImageUrl { get; set; }
            public MapExpansion mapExpansion { get; set; }
            public List<ExpansionsItem> expansions { get; set; }
            public string game { get; set; }
            public string platform { get; set; }
            public bool passwordProtected { get; set; }
            public string ip { get; set; }
            public string pingSiteAlias { get; set; }
            public bool isFavorite { get; set; }
            public bool custom { get; set; }
            public string preset { get; set; }
            public int tickRate { get; set; }
            public string serverType { get; set; }
            public string experience { get; set; }
            public string officialExperienceId { get; set; }
            [JsonIgnore]
            public string operationIndex { get; set; }
            [JsonIgnore]
            public string mixId { get; set; }
            [JsonIgnore]
            public string serverMode { get; set; }
            [JsonIgnore]
            public string ownerId { get; set; }
            [JsonIgnore]
            public string playgroundId { get; set; }
            [JsonIgnore]
            public string overallGameMode { get; set; }
            [JsonIgnore]
            public List<string> mapRotation { get; set; }
            public string secret { get; set; }
            public Settings settings { get; set; }
            public class Slots
            {
                public Queue1 Queue { get; set; }
                public Soldier1 Soldier { get; set; }
                public Spectator1 Spectator { get; set; }
                public class Queue1
                {
                    public int current { get; set; }
                    public int max { get; set; }
                }
                public class Soldier1
                {
                    public int current { get; set; }
                    public int max { get; set; }
                }
                public class Spectator1
                {
                    public int current { get; set; }
                    public int max { get; set; }
                }
            }
            public class MapExpansion
            {
                public string name { get; set; }
                public int mask { get; set; }
                public string license { get; set; }
                public string prettyName { get; set; }
            }
            public class ExpansionsItem
            {
                public string name { get; set; }
                public int mask { get; set; }
                public string license { get; set; }
                public string prettyName { get; set; }
            }
            public class Settings
            {
            }
        }
    }
}