namespace BF1ServerTools.API.RespJson;

public class ServerDetails
{
    public string jsonrpc { get; set; }
    public string id { get; set; }
    public Result result { get; set; }
    public class Result
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
            public string mapRotationId { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string mod { get; set; }
            public string rotationType { get; set; }
            public List<MapsItem> maps { get; set; }
            public class MapsItem
            {
                public string gameMode { get; set; }
                public string mapName { get; set; }
            }
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
