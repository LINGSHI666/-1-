namespace BF1ServerTools.API.RespJson;

public class DetailedStats
{
    public string jsonrpc { get; set; }
    public string id { get; set; }
    public Result result { get; set; }
    public class Result
    {
        public BasicStats basicStats { get; set; }
        public string favoriteClass { get; set; }
        public List<KitStatsItem> kitStats { get; set; }
        public float awardScore { get; set; }
        public float bonusScore { get; set; }
        public float squadScore { get; set; }
        public int avengerKills { get; set; }
        public int saviorKills { get; set; }
        public int highestKillStreak { get; set; }
        public int dogtagsTaken { get; set; }
        public int roundsPlayed { get; set; }
        public int flagsCaptured { get; set; }
        public int flagsDefended { get; set; }
        public float accuracyRatio { get; set; }
        public int headShots { get; set; }
        public float longestHeadShot { get; set; }
        public float nemesisKills { get; set; }
        public float nemesisKillStreak { get; set; }
        public float revives { get; set; }
        public float heals { get; set; }
        public float repairs { get; set; }
        public float suppressionAssist { get; set; }
        public float kdr { get; set; }
        public float killAssists { get; set; }
        [JsonIgnore]
        public List<GameModeStatsItem> gameModeStats { get; set; }
        [JsonIgnore]
        public List<VehicleStatsItem> vehicleStats { get; set; }
        public string detailedStatType { get; set; }
        public class BasicStats
        {
            public int timePlayed { get; set; }
            public int wins { get; set; }
            public int losses { get; set; }
            public int kills { get; set; }
            public int deaths { get; set; }
            public float kpm { get; set; }
            public float spm { get; set; }
            public float skill { get; set; }
            public string soldierImageUrl { get; set; }
            [JsonIgnore]
            public string rank { get; set; }
            [JsonIgnore]
            public string rankProgress { get; set; }
            [JsonIgnore]
            public string freemiumRank { get; set; }
            [JsonIgnore]
            public List<string> completion { get; set; }
            [JsonIgnore]
            public string highlights { get; set; }
            [JsonIgnore]
            public string highlightsByType { get; set; }
            [JsonIgnore]
            public string equippedDogtags { get; set; }
        }
        public class KitStatsItem
        {
        }
        public class GameModeStatsItem
        {
        }
        public class VehicleStatsItem
        {
        }
    }
}
