using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1ServerTools.Models;

public partial class DetailModel : ObservableObject
{
    /// <summary>
    /// 服务器名称
    /// </summary>
    [ObservableProperty]
    private string serverName;

    /// <summary>
    /// 服务器描述
    /// </summary>
    [ObservableProperty]
    private string serverDescription;

    /// <summary>
    /// 服务器GameId
    /// </summary>
    [ObservableProperty]
    private string serverGameId;

    /// <summary>
    /// 服务器Guid
    /// </summary>
    [ObservableProperty]
    private string serverGuid;

    /// <summary>
    /// 服务器ServerId
    /// </summary>
    [ObservableProperty]
    private string serverId;

    /// <summary>
    /// 服务器收藏数
    /// </summary>
    [ObservableProperty]
    private string serverBookmark;

    /// <summary>
    /// 服主Id
    /// </summary>
    [ObservableProperty]
    private string serverOwnerName;

    /// <summary>
    /// 服主数字Id
    /// </summary>
    [ObservableProperty]
    private string serverOwnerPersonaId;

    /// <summary>
    /// 服主头像
    /// </summary>
    [ObservableProperty]
    private string serverOwnerImage;
}
