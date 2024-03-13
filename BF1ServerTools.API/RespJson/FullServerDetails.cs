namespace BF1ServerTools.API.RespJson;

public class FullServerDetails
{
    public string jsonrpc { get; set; }
    public string id { get; set; }
    public Result result { get; set; }
    public class Result
    {
        public ServerInfo serverInfo { get; set; }
        public RspInfo rspInfo { get; set; }
        [JsonIgnore]
        public string platoonInfo { get; set; }
        public class ServerInfo
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
            [JsonIgnore]
            public int operationIndex { get; set; }
            public string ip { get; set; }
            public string pingSiteAlias { get; set; }
            public bool isFavorite { get; set; }
            public bool custom { get; set; }
            public string preset { get; set; }
            public int tickRate { get; set; }
            public string serverType { get; set; }
            public Settings settings { get; set; }
            public List<RotationItem> rotation { get; set; }
            public bool punkbusterEnabled { get; set; }
            public bool fairfightEnabled { get; set; }
            public string experience { get; set; }
            public string officialExperienceId { get; set; }
            public string serverBookmarkCount { get; set; }
            [JsonIgnore]
            public string mixId { get; set; }
            [JsonIgnore]
            public string serverMode { get; set; }
            [JsonIgnore]
            public List<string> mapRotation { get; set; }
            public string secret { get; set; }
            public class Slots
            {
            }
            public class MapExpansion
            {
            }
            public class ExpansionsItem
            {
            }
            public class Settings
            {
            }
            public class RotationItem
            {
                public string mapPrettyName { get; set; }
                public string mapImage { get; set; }
                public string modePrettyName { get; set; }
            }
        }
        public class RspInfo
        {
            public List<AdminListItem> adminList { get; set; }
            public List<VipListItem> vipList { get; set; }
            public List<BannedListItem> bannedList { get; set; }
            public List<MapRotationsItem> mapRotations { get; set; }
            public Owner owner { get; set; }
            public Server server { get; set; }
            public ServerSettings serverSettings { get; set; }
            public class AdminListItem
            {
                public string platform { get; set; }
                public string nucleusId { get; set; }
                public string personaId { get; set; }
                public string platformId { get; set; }
                public string displayName { get; set; }
                public string avatar { get; set; }
                public string accountId { get; set; }
            }
            public class VipListItem
            {
                public string platform { get; set; }
                public string nucleusId { get; set; }
                public string personaId { get; set; }
                public string platformId { get; set; }
                public string displayName { get; set; }
                public string avatar { get; set; }
                public string accountId { get; set; }
            }
            public class BannedListItem
            {
                public string platform { get; set; }
                public string nucleusId { get; set; }
                public string personaId { get; set; }
                public string platformId { get; set; }
                public string displayName { get; set; }
                public string avatar { get; set; }
                public string accountId { get; set; }
            }
            public class MapRotationsItem
            {
            }
            public class Owner
            {
                public string platform { get; set; }
                public string nucleusId { get; set; }
                public string personaId { get; set; }
                public string platformId { get; set; }
                public string displayName { get; set; }
                public string avatar { get; set; }
                public string accountId { get; set; }
            }
            public class Server
            {
                public string serverId { get; set; }
                public string persistedGameId { get; set; }
                public string createdDate { get; set; }
                public string expirationDate { get; set; }
                public string updatedDate { get; set; }
                public string updatedBy { get; set; }
                public string ownerId { get; set; }
                public Status status { get; set; }
                public string pingSiteAlias { get; set; }
                public string gameProtocolVersionString { get; set; }
                public string name { get; set; }
                public string bannerUrl { get; set; }
                public bool isFree { get; set; }
                public class Status
                {
                    public int value { get; set; }
                    public string name { get; set; }
                    public string originalName { get; set; }
                }
            }
            public class ServerSettings
            {
                public string name { get; set; }
                public string description { get; set; }
                public string message { get; set; }
                public string password { get; set; }
                public string mapRotationId { get; set; }
                public string bannerUrl { get; set; }
                public string customGameSettings { get; set; }
            }
        }
    }
}
