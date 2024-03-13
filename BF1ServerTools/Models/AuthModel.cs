using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1ServerTools.Models;

public partial class AuthModel : ObservableObject
{
    /// <summary>
    /// 头像
    /// </summary>
    [ObservableProperty]
    private string avatar2;

    /// <summary>
    /// 显示名称
    /// </summary>
    [ObservableProperty]
    private string displayName2;

    /// <summary>
    /// 数字Id
    /// </summary>
    [ObservableProperty]
    private long personaId2;

    ///////////////////////////////////////////////

    /// <summary>
    /// Remid
    /// </summary>
    [ObservableProperty]
    private string remid;

    /// <summary>
    /// Sid
    /// </summary>
    [ObservableProperty]
    private string sid;

    /// <summary>
    /// AccessToken
    /// </summary>
    [ObservableProperty]
    private string accessToken;

    /// <summary>
    /// SessionId2
    /// </summary>
    [ObservableProperty]
    private string sessionId2;
}
