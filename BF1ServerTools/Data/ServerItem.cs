namespace BF1ServerTools.Data;

public class ServerItem
{
    public string GameId { get; set; }
    public string Guid { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Region { get; set; }
    public string Country { get; set; }
    public bool Ranked { get; set; }
    public int QueryCurrent { get; set; }
    public int QueryMax { get; set; }
    public int SoldierCurrent { get; set; }
    public int SoldierMax { get; set; }
    public int SpectatorCurrent { get; set; }
    public int SpectatorMax { get; set; }
    public string MapName { get; set; }
    public string MapNamePretty { get; set; }
    public string MapMode { get; set; }
    public string MapModePretty { get; set; }
    public string MapImageUrl { get; set; }
    public bool PasswordProtected { get; set; }
    public string PingSiteAlias { get; set; }
    public bool IsFavorite { get; set; }
    public bool Custom { get; set; }
    public int TickRate { get; set; }
    public string ServerType { get; set; }
    public string OfficialExperienceId { get; set; }
}
