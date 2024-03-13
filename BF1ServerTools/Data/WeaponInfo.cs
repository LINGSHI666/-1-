namespace BF1ServerTools.Data;

public class WeaponInfo
{
    public string name { get; set; }
    public string imageUrl { get; set; }
    public string star { get; set; }

    public int kills { get; set; }
    public string killsPerMinute { get; set; }

    public int headshots { get; set; }
    public string headshotsVKills { get; set; }

    public int shots { get; set; }
    public int hits { get; set; }
    public string hitsVShots { get; set; }

    public string hitVKills { get; set; }
    public string time { get; set; }
}
