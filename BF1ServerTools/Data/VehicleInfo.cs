namespace BF1ServerTools.Data;

public class VehicleInfo
{
    public string name { get; set; }
    public string imageUrl { get; set; }
    public string star { get; set; }

    public int kills { get; set; }
    public string killsPerMinute { get; set; }

    public int destroyed { get; set; }
    public string time { get; set; }
}
