using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1ServerTools.Models;

public partial class ServerModel : ObservableObject
{
    /// <summary>
    /// 服务器名称
    /// </summary>
    [ObservableProperty]
    private string serverName;

    /// <summary>
    /// 是否显示加载动画
    /// </summary>
    [ObservableProperty]
    private bool isLoading;
}
