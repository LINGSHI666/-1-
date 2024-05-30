namespace BF1ServerTools.Data;

public class ServerRule
{
    public int MaxKill { get; set; } = 0;

    public int FlagKD { get; set; } = 0;
    public float MaxKD { get; set; } = 0.00f;

    public int FlagKPM { get; set; } = 0;
    public float MaxKPM { get; set; } = 0.00f;

    public int MinRank { get; set; } = 0;
    public int MaxRank { get; set; } = 0;

    public float LifeMaxKD { get; set; } = 0.00f;
    public float LifeMaxKPM { get; set; } = 0.00f;
    public int LifeMaxWeaponStar { get; set; } = 0;
    public int LifeMaxVehicleStar { get; set; } = 0;

    public int MaxScout { get; set; } = 0;

}
