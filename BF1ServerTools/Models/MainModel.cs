using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1ServerTools.Models;

public partial class MainModel : ObservableObject
{
    /// <summary>
    /// 程序版本号
    /// </summary>
    [ObservableProperty]
    private Version versionInfo;

    /// <summary>
    /// 程序运行时间
    /// </summary>
    [ObservableProperty]
    private string appRunTime;

    /// <summary>
    /// 是否使用模式1
    /// </summary>
    [ObservableProperty]
    private bool isUseMode1;

    ///////////////////////////////////////////////

    /// <summary>
    /// 模式1 显示名称
    /// </summary>
    [ObservableProperty]
    private string displayName1;

    /// <summary>
    /// 模式1 数字Id
    /// </summary>
    [ObservableProperty]
    private long personaId1;

    ///////////////////////////////////////////////

    /// <summary>
    /// 模式2 显示名称
    /// </summary>
    [ObservableProperty]
    private string displayName2;

    /// <summary>
    /// 模式2 数字Id
    /// </summary>
    [ObservableProperty]
    private long personaId2;
}
