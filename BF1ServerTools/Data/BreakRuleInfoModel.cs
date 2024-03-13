using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1ServerTools.Data;

public partial class BreakRuleInfoModel : ObservableObject
{
    /// <summary>
    /// 序号
    /// </summary>
    [ObservableProperty]
    private int index;

    /// <summary>
    /// 违规玩家等级
    /// </summary>
    [ObservableProperty]
    private int rank;

    /// <summary>
    /// 违规玩家ID
    /// </summary>
    [ObservableProperty]
    private string name;

    /// <summary>
    /// 违规玩家数字ID
    /// </summary>
    [ObservableProperty]
    private long personaId;

    ///////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 管理员
    /// </summary>
    [ObservableProperty]
    private bool admin;

    /// <summary>
    /// 白名单
    /// </summary>
    [ObservableProperty]
    private bool white;

    ///////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 违规原因
    /// </summary>
    [ObservableProperty]
    private string reason;

    /// <summary>
    /// 违规数量
    /// </summary>
    [ObservableProperty]
    private int count;

    /// <summary>
    /// 全部违规原因
    /// </summary>
    [ObservableProperty]
    private string allReason;
}
