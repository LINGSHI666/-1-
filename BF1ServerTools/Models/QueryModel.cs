using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1ServerTools.Models;

public partial class QueryModel : ObservableObject
{
    /// <summary>
    /// 玩家名称
    /// </summary>
    [ObservableProperty]
    private string playerName;

    /// <summary>
    /// 是否显示加载动画
    /// </summary>
    [ObservableProperty]
    private bool isLoading;

    //////////////////////////////////////

    /// <summary>
    /// 玩家头像
    /// </summary>
    [ObservableProperty]
    private string avatar;

    /// <summary>
    /// 玩家图章
    /// </summary>
    [ObservableProperty]
    private string emblem;

    /// <summary>
    /// 玩家显示名称
    /// </summary>
    [ObservableProperty]
    private string displayName;

    /// <summary>
    /// 玩家数字ID
    /// </summary>
    [ObservableProperty]
    private string personaId;

    /// <summary>
    /// 玩家等级
    /// </summary>
    [ObservableProperty]
    private string rank;

    /// <summary>
    /// 玩家游玩时间
    /// </summary>
    [ObservableProperty]
    private string playTime;

    //////////////////////////////////////

    /// <summary>
    /// 玩家正在游玩服务器
    /// </summary>
    [ObservableProperty]
    private string playingServer;
}
