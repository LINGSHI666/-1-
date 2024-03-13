using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1ServerTools.Data;

public partial class RuleWeaponModel : ObservableObject
{
    /// <summary>
    /// 种类
    /// </summary>
    [ObservableProperty]
    private string kind;

    /// <summary>
    /// 中文名称
    /// </summary>
    [ObservableProperty]
    private string name;

    /// <summary>
    /// 英文名称
    /// </summary>
    [ObservableProperty]
    private string english;

    /// <summary>
    /// 图片
    /// </summary>
    [ObservableProperty]
    private string image;

    /// <summary>
    /// 队伍1限制
    /// </summary>
    [ObservableProperty]
    private bool team1;

    /// <summary>
    /// 队伍2限制
    /// </summary>
    [ObservableProperty]
    private bool team2;
}
