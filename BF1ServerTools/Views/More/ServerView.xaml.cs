using BF1ServerTools.API;
using BF1ServerTools.API.RespJson;
using BF1ServerTools.RES;
using BF1ServerTools.Data;
using BF1ServerTools.Utils;
using BF1ServerTools.Helper;
using BF1ServerTools.Models;

using CommunityToolkit.Mvvm.Input;

namespace BF1ServerTools.Views.More;

/// <summary>
/// ServerView.xaml 的交互逻辑
/// </summary>
public partial class ServerView : UserControl
{
    /// <summary>
    /// 数据模型绑定
    /// </summary>
    public ServerModel ServerModel { get; set; } = new();
    /// <summary>
    /// 动态集合
    /// </summary>
    public ObservableCollection<ServerItem> ListBox_ServersItems { get; set; } = new();

    public ServerView()
    {
        InitializeComponent();
        this.DataContext = this;

        ServerModel.IsLoading = false;
    }

    /// <summary>
    /// 搜索服务器
    /// </summary>
    [RelayCommand]
    private async void QueryServer()
    {
        if (string.IsNullOrEmpty(ServerModel.ServerName))
        {
            NotifierHelper.Show(NotifierType.Warning, "请输入正确的服务器名称");
            return;
        }

        if (!string.IsNullOrEmpty(Globals.SessionId))
        {
            ListBox_ServersItems.Clear();
            ServerModel.IsLoading = true;
            ServerModel.ServerName = ServerModel.ServerName.Trim();

            NotifierHelper.Show(NotifierType.Information, $"正在查询服务器 {ServerModel.ServerName} 数据中...");

            var result = await BF1API.SearchServers(Globals.SessionId, ServerModel.ServerName);
            if (result.IsSuccess)
            {
                var searchServers = JsonHelper.JsonDese<SearchServers>(result.Content);

                var gameservers = searchServers.result.gameservers;
                gameservers = gameservers.OrderByDescending(s => s.slots.Soldier.current).ThenByDescending(s => s.slots.Queue.current).ToList();

                ServerModel.IsLoading = false;

                foreach (var item in gameservers)
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Background, () =>
                    {
                        ListBox_ServersItems.Add(new()
                        {
                            GameId = item.gameId,
                            Guid = item.guid,
                            Name = item.name,
                            Description = item.description,
                            Region = item.region,
                            Country = item.country,
                            Ranked = item.ranked,
                            QueryCurrent = item.slots.Queue.current,
                            QueryMax = item.slots.Queue.max,
                            SoldierCurrent = item.slots.Soldier.current,
                            SoldierMax = item.slots.Soldier.max,
                            SpectatorCurrent = item.slots.Spectator.current,
                            SpectatorMax = item.slots.Spectator.max,
                            MapName = item.mapName,
                            MapNamePretty = ChsUtil.ToSimplified(item.mapNamePretty),
                            MapMode = item.mapName,
                            MapModePretty = ChsUtil.ToSimplified(item.mapModePretty),
                            MapImageUrl = ClientHelper.GetTempImagePath(item.mapImageUrl, "map"),
                            PasswordProtected = item.passwordProtected,
                            PingSiteAlias = item.pingSiteAlias,
                            IsFavorite = item.isFavorite,
                            Custom = item.custom,
                            TickRate = item.tickRate,
                            ServerType = item.serverType,
                            OfficialExperienceId = item.operationIndex
                        });
                    });
                }

                NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  服务器 {ServerModel.ServerName} 数据查询成功");
            }
            else
            {
                NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  服务器 {ServerModel.ServerName} 数据查询失败\n{result.Content}");
            }
        }
        else
        {
            NotifierHelper.Show(NotifierType.Error, "玩家SessionId为空，操作取消");
        }

        ServerModel.IsLoading = false;
    }

    /// <summary>
    /// 服务器详情
    /// </summary>
    /// <param name="gameid"></param>
    [RelayCommand]
    private void ServerInfo(string gameid)
    {
        NotifierHelper.Show(NotifierType.Notification, "功能开发中...");
    }
}
