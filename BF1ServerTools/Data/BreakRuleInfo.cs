namespace BF1ServerTools.Data;

public class BreakRuleInfo
{
    /// <summary>
    /// 违规玩家等级
    /// </summary>
    public int Rank { get; set; }
    /// <summary>
    /// 违规玩家ID
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 违规玩家数字ID
    /// </summary>
    public long PersonaId { get; set; }
    /// <summary>
    /// 管理员
    /// </summary>
    public bool Admin { get; set; }
    /// <summary>
    /// 白名单
    /// </summary>
    public bool White { get; set; }
    /// <summary>
    /// 违规原因
    /// </summary>
    public string Reason { get; set; }
    /// <summary>
    /// 违规详情合集
    /// </summary>
    public List<BreakInfo> BreakInfos { get; set; }
}

public class BreakInfo
{
    /// <summary>
    /// 违规类型
    /// </summary>
    public BreakType BreakType { get; set; }
    /// <summary>
    /// 违规原因
    /// </summary>
    public string Reason { get; set; }
}

public enum BreakType
{
    Spectator,
    Black,
    CD,
    NoWhite,

    LifeKD,
    LifeKPM,
    LifeWeaponStar,
    LifeVehicleStar,
    Kill,
    KD,
    KPM,
    Rank,
    Weapon
}