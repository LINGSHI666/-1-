using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1ServerTools.Data;

public partial class RuleTeamModel : ObservableObject
{
    /// <summary>
    /// 最大击杀
    /// </summary>
    [ObservableProperty]
    private int maxKill;

    /// <summary>
    /// 计算KD标志
    /// </summary>
    [ObservableProperty]
    private int flagKD;

    /// <summary>
    /// 最大KD
    /// </summary>
    [ObservableProperty]
    private float maxKD;

    /// <summary>
    /// 计算KPM标志
    /// </summary>
    [ObservableProperty]
    private int flagKPM;

    /// <summary>
    /// 最大KPM
    /// </summary>
    [ObservableProperty]
    private float maxKPM;

    /// <summary>
    /// 最低等级
    /// </summary>
    [ObservableProperty]
    private int minRank;

    /// <summary>
    /// 最低等级
    /// </summary>
    [ObservableProperty]
    private int maxRank;

    /// <summary>
    /// 最大生涯KD
    /// </summary>
    [ObservableProperty]
    private float lifeMaxKD;

    /// <summary>
    /// 最大生涯KPM
    /// </summary>
    [ObservableProperty]
    private float lifeMaxKPM;

    /// <summary>
    /// 最大生涯武器星数
    /// </summary>
    [ObservableProperty]
    private int lifeMaxWeaponStar;

    /// <summary>
    /// 最大生涯坦克星数
    /// </summary>
    [ObservableProperty]
    private int lifeMaxVehicleStar;
    /// <summary>
    /// 最大生涯飞机星数
    /// </summary>
    [ObservableProperty]
    private int lifeMaxPlaneStar;
    /// <summary>
    /// 最多侦察数
    /// </summary>
    [ObservableProperty]
    private int maxScout;
}
