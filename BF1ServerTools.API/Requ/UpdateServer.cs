namespace BF1ServerTools.API.Requ;

public class UpdateServer
{
    public string jsonrpc { get; set; }
    public string method { get; set; }
    public Params @params { get; set; }
    public string id { get; set; }
    public class Params
    {
        public DeviceIdMap deviceIdMap { get; set; }
        public string game { get; set; }
        public string serverId { get; set; }
        public BannerSettings bannerSettings { get; set; }
        public MapRotation mapRotation { get; set; }
        public ServerSettings serverSettings { get; set; }
        public class DeviceIdMap
        {
            public string machash { get; set; }
        }
        public class BannerSettings
        {
            public string bannerUrl { get; set; }
            public bool clearBanner { get; set; }
        }
        public class MapRotation
        {
            public List<MapsItem> maps { get; set; }
            public string rotationType { get; set; }
            public string mod { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string id { get; set; }
            public class MapsItem
            {
                public string gameMode { get; set; }
                public string mapName { get; set; }
            }
        }
        public class ServerSettings
        {
            public string name { get; set; }
            public string description { get; set; }
            public string message { get; set; }
            public string password { get; set; }
            public string bannerUrl { get; set; }
            public string mapRotationId { get; set; }
            public string customGameSettings { get; set; }
        }
    }
}
