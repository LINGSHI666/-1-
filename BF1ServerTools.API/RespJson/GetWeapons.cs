namespace BF1ServerTools.API.RespJson;

public class GetWeapons
{
    public string jsonrpc { get; set; }
    public string id { get; set; }
    public List<ResultItem> result { get; set; }
    public class ResultItem
    {
        public string name { get; set; }
        public List<WeaponsItem> weapons { get; set; }
        public string categoryId { get; set; }
        public class WeaponsItem
        {
            public string guid { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string category { get; set; }
            public string imageUrl { get; set; }
            [JsonIgnore]
            public List<string> accessories { get; set; }
            [JsonIgnore]
            public Star star { get; set; }
            public Stats stats { get; set; }
            [JsonIgnore]
            public List<UnlockRequirementsItem> unlockRequirements { get; set; }
            [JsonIgnore]
            public Progression progression { get; set; }
            [JsonIgnore]
            public Info info { get; set; }
            [JsonIgnore]
            public int price { get; set; }
            [JsonIgnore]
            public Images images { get; set; }
            [JsonIgnore]
            public string svgImage { get; set; }
            [JsonIgnore]
            public Hires hires { get; set; }
            [JsonIgnore]
            public PurchaseInfo purchaseInfo { get; set; }
            [JsonIgnore]
            public List<string> kitShortcutLicenses { get; set; }
            [JsonIgnore]
            public string expansion { get; set; }
            [JsonIgnore]
            public string premium { get; set; }
            [JsonIgnore]
            public string rank { get; set; }
            [JsonIgnore]
            public string extendedStats { get; set; }
            public class Star
            {
            }
            public class Stats
            {
                public Values values { get; set; }
                public class Values
                {
                    public float kills { get; set; }
                    public float headshots { get; set; }
                    public float accuracy { get; set; }
                    public float seconds { get; set; }
                    public float hits { get; set; }
                    public float shots { get; set; }
                }
            }
            public class UnlockRequirementsItem
            {
            }
            public class Progression
            {
            }
            public class Info
            {
            }
            public class Images
            {
            }
            public class Hires
            {
            }
            public class PurchaseInfo
            {
            }
        }
    }
}
