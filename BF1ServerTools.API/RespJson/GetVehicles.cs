namespace BF1ServerTools.API.RespJson;

public class GetVehicles
{
    public string jsonrpc { get; set; }
    public string id { get; set; }
    public List<ResultItem> result { get; set; }
    public class ResultItem
    {
        public string name { get; set; }
        [JsonIgnore]
        public Star star { get; set; }
        public List<VehiclesItem> vehicles { get; set; }
        [JsonIgnore]
        public Stats stats { get; set; }
        [JsonIgnore]
        public List<string> accessories { get; set; }
        [JsonIgnore]
        public int sortOrder { get; set; }
        public class Star
        {
        }
        public class VehiclesItem
        {
            public string guid { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string imageUrl { get; set; }
            [JsonIgnore]
            public List<string> accessories { get; set; }
            public Stats stats { get; set; }
            [JsonIgnore]
            public Progression progression { get; set; }
            [JsonIgnore]
            public Images images { get; set; }
            [JsonIgnore]
            public string expansion { get; set; }
            [JsonIgnore]
            public string rank { get; set; }
            public class Stats
            {
                public Values values { get; set; }
                public class Values
                {
                    public float seconds { get; set; }
                    public float kills { get; set; }
                    public float destroyed { get; set; }
                }
            }
            public class Progression
            {
            }
            public class Images
            {
            }
        }
        public class Stats
        {
        }
    }
}
