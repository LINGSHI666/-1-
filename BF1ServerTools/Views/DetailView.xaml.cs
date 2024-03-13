using BF1ServerTools.RES;
using BF1ServerTools.API;
using BF1ServerTools.API.Requ;
using BF1ServerTools.API.RespJson;
using BF1ServerTools.Data;
using BF1ServerTools.Utils;
using BF1ServerTools.Models;
using BF1ServerTools.Helper;
using BF1ServerTools.Windows;

namespace BF1ServerTools.Views;

/// <summary>
/// DetailView.xaml 的交互逻辑
/// </summary>
public partial class DetailView : UserControl
{
    /// <summary>
    /// 数据模型绑定
    /// </summary>
    public DetailModel DetailModel { get; set; } = new();

    private List<MapInfo> ListBox_MapList = new();

    private List<RSPInfo> ListBox_AdminList = new();
    private List<RSPInfo> ListBox_VIPList = new();
    private List<RSPInfo> ListBox_BANList = new();

    /// <summary>
    /// 服务器设置详情Json数据模型
    /// </summary>
    private ServerDetails _serverDetails;
    /// <summary>
    /// 是否成功获取服务器设置详情
    /// </summary>
    private bool _isGetServerDetailsOK = false;

    public DetailView()
    {
        InitializeComponent();
        this.DataContext = this;
        MainWindow.WindowClosingEvent += MainWindow_WindowClosingEvent;

        new Thread(UpdateServerDetilsThread)
        {
            Name = "UpdateServerDetilsThread",
            IsBackground = true
        }.Start();
    }

    private void MainWindow_WindowClosingEvent()
    {

    }

    /// <summary>
    /// 智能更新服务器详情线程
    /// </summary>
    private async void UpdateServerDetilsThread()
    {
        bool isClear = true;

        while (MainWindow.IsAppRunning)
        {
            if (Globals.GameId != 0)
            {
                if (Globals.ServerAdmins_PID.Count == 0 &&
                    DetailModel.ServerName != "OFFICIAL" &&
                    DetailModel.ServerOwnerName != "NULL")
                {
                    if (await GetFullServerDetails())
                    {
                        ListBox_MapList.ForEach(map =>
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                            {
                                ListBox_Map.Items.Add(map);
                            });
                        });

                        ListBox_AdminList.ForEach(map =>
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                            {
                                ListBox_Admin.Items.Add(map);
                            });
                        });

                        ListBox_VIPList.ForEach(map =>
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                            {
                                ListBox_VIP.Items.Add(map);
                            });
                        });

                        ListBox_BANList.ForEach(map =>
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                            {
                                ListBox_BAN.Items.Add(map);
                            });
                        });
                    }
                }
                else
                {
                    isClear = false;
                }
            }
            else
            {
                if (!isClear)
                {
                    isClear = true;

                    DetailModel.ServerName = string.Empty;
                    DetailModel.ServerDescription = string.Empty;
                    DetailModel.ServerGameId = string.Empty;
                    DetailModel.ServerGuid = string.Empty;
                    DetailModel.ServerId = string.Empty;
                    DetailModel.ServerBookmark = string.Empty;
                    DetailModel.ServerOwnerName = string.Empty;
                    DetailModel.ServerOwnerPersonaId = string.Empty;
                    DetailModel.ServerOwnerImage = string.Empty;

                    Globals.ServerAdmins_PID.Clear();
                    Globals.ServerVIPs_PID.Clear();

                    Globals.ServerId = 0;
                    Globals.PersistedGameId = string.Empty;

                    ListBox_MapList.Clear();
                    ListBox_AdminList.Clear();
                    ListBox_VIPList.Clear();
                    ListBox_BANList.Clear();

                    this.Dispatcher.Invoke(DispatcherPriority.Background, () =>
                    {
                        ListBox_Map.Items.Clear();
                        ListBox_Admin.Items.Clear();
                        ListBox_VIP.Items.Clear();
                        ListBox_BAN.Items.Clear();
                    });
                }
            }

            Thread.Sleep(1000);
        }
    }

    /// <summary>
    /// 获取当前服务器详情数据
    /// </summary>
    private async Task<bool> GetFullServerDetails()
    {
        if (string.IsNullOrEmpty(Globals.SessionId))
            return false;

        DetailModel.ServerName = "获取中...";
        DetailModel.ServerDescription = "获取中...";
        DetailModel.ServerGameId = "获取中...";
        DetailModel.ServerGuid = "获取中...";
        DetailModel.ServerId = "获取中...";
        DetailModel.ServerBookmark = "获取中...";
        DetailModel.ServerOwnerName = "获取中...";
        DetailModel.ServerOwnerPersonaId = "获取中...";
        DetailModel.ServerOwnerImage = string.Empty;

        ListBox_MapList.Clear();
        ListBox_AdminList.Clear();
        ListBox_VIPList.Clear();
        ListBox_BANList.Clear();

        Globals.ServerAdmins_PID.Clear();
        Globals.ServerVIPs_PID.Clear();

        Globals.ServerId = 0;
        Globals.PersistedGameId = string.Empty;

        /////////////////////////////////////////////////////////////////////////////////

        var result = await BF1API.GetFullServerDetails(Globals.SessionId, Globals.GameId);
        if (result.IsSuccess)
        {
            var fullServerDetails = JsonHelper.JsonDese<FullServerDetails>(result.Content);
            if (fullServerDetails.result.serverInfo.serverType == "OFFICIAL")
            {
                DetailModel.ServerName = "OFFICIAL";
                DetailModel.ServerDescription = "当前进入的是官方服务器，操作取消";
                DetailModel.ServerGameId = string.Empty;
                DetailModel.ServerGuid = string.Empty;
                DetailModel.ServerId = string.Empty;
                DetailModel.ServerBookmark = string.Empty;
                DetailModel.ServerOwnerName = string.Empty;
                DetailModel.ServerOwnerPersonaId = string.Empty;
                DetailModel.ServerOwnerImage = string.Empty;

                LoggerHelper.Warn("当前进入的是官方服务器，操作取消");

                return false;
            }

            Globals.ServerId = int.Parse(fullServerDetails.result.rspInfo.server.serverId);
            Globals.PersistedGameId = fullServerDetails.result.rspInfo.server.persistedGameId;

            DetailModel.ServerName = fullServerDetails.result.serverInfo.name;
            DetailModel.ServerDescription = fullServerDetails.result.serverInfo.description;

            DetailModel.ServerGameId = Globals.GameId.ToString();
            DetailModel.ServerGuid = Globals.PersistedGameId;
            DetailModel.ServerId = Globals.ServerId.ToString();

            int index = 0;
            if (fullServerDetails.result.rspInfo.owner != null)
            {
                DetailModel.ServerOwnerName = fullServerDetails.result.rspInfo.owner.displayName;
                DetailModel.ServerOwnerPersonaId = fullServerDetails.result.rspInfo.owner.personaId;
                DetailModel.ServerOwnerImage = fullServerDetails.result.rspInfo.owner.avatar;

                // 服主
                ListBox_AdminList.Add(new()
                {
                    Index = index++,
                    Avatar = fullServerDetails.result.rspInfo.owner.avatar,
                    DisplayName = fullServerDetails.result.rspInfo.owner.displayName,
                    PersonaId = long.Parse(fullServerDetails.result.rspInfo.owner.personaId)
                });
                Globals.ServerAdmins_PID.Add(long.Parse(fullServerDetails.result.rspInfo.owner.personaId));
            }
            else
            {
                DetailModel.ServerOwnerName = "NULL";
                DetailModel.ServerOwnerPersonaId = "NULL";

                LoggerHelper.Warn("检测到Bug服务器，工具可能会出现异常");
            }

            DetailModel.ServerBookmark = $"★ {fullServerDetails.result.serverInfo.serverBookmarkCount}";

            // 地图列表
            foreach (var item in fullServerDetails.result.serverInfo.rotation)
            {
                ListBox_MapList.Add(new MapInfo()
                {
                    MapImage = ClientHelper.GetTempImagePath(item.mapImage, "map"),
                    MapName = ChsUtil.ToSimplified(item.mapPrettyName),
                    MapMode = ChsUtil.ToSimplified(item.modePrettyName)
                });
            }

            // 管理员列表
            foreach (var item in fullServerDetails.result.rspInfo.adminList)
            {
                ListBox_AdminList.Add(new RSPInfo()
                {
                    Index = index++,
                    Avatar = item.avatar,
                    DisplayName = item.displayName,
                    PersonaId = long.Parse(item.personaId)
                });

                Globals.ServerAdmins_PID.Add(long.Parse(item.personaId));
            }

            // VIP列表
            index = 1;
            foreach (var item in fullServerDetails.result.rspInfo.vipList)
            {
                ListBox_VIPList.Add(new RSPInfo()
                {
                    Index = index++,
                    Avatar = item.avatar,
                    DisplayName = item.displayName,
                    PersonaId = long.Parse(item.personaId)
                });

                Globals.ServerVIPs_PID.Add(long.Parse(item.personaId));
            }

            // BAN列表
            index = 1;
            foreach (var item in fullServerDetails.result.rspInfo.bannedList)
            {
                ListBox_BANList.Add(new RSPInfo()
                {
                    Index = index++,
                    Avatar = item.avatar,
                    DisplayName = item.displayName,
                    PersonaId = long.Parse(item.personaId)
                });
            }

            return true;
        }
        else
        {
            DetailModel.ServerName = string.Empty;
            DetailModel.ServerDescription = string.Empty;
            DetailModel.ServerGameId = string.Empty;
            DetailModel.ServerGuid = string.Empty;
            DetailModel.ServerId = string.Empty;
            DetailModel.ServerBookmark = string.Empty;
            DetailModel.ServerOwnerName = string.Empty;
            DetailModel.ServerOwnerPersonaId = string.Empty;
            DetailModel.ServerOwnerImage = string.Empty;

            return false;
        }
    }

    /// <summary>
    /// 切换当前服务器地图事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ListBox_Map_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListBox_Map.SelectedIndex == -1)
            return;

        if (!PlayerUtil.CheckAuth())
        {
            // 使ListBox能够响应重复点击
            ListBox_Map.SelectedIndex = -1;
            return;
        }

        if (string.IsNullOrEmpty(Globals.PersistedGameId))
        {
            NotifierHelper.Show(NotifierType.Warning, "PersistedGameId为空，请重新获取服务器详细信息");
            // 使ListBox能够响应重复点击
            ListBox_Map.SelectedIndex = -1;
            return;
        }

        if (ListBox_Map.SelectedItem is MapInfo item)
        {
            var mapInfo = item.MapMode + " - " + item.MapName;
            var changeMapWindow = new ChangeMapWindow(mapInfo, item.MapImage)
            {
                Owner = MainWindow.MainWindowInstance
            };
            if (changeMapWindow.ShowDialog() == true)
            {
                NotifierHelper.Show(NotifierType.Information, $"正在更换服务器 {Globals.GameId} 地图为 {item.MapName} 中...");

                var result = await BF1API.RSPChooseLevel(Globals.SessionId, Globals.PersistedGameId, ListBox_Map.SelectedIndex);
                if (result.IsSuccess)
                    NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  更换服务器 {Globals.GameId} 地图为 {item.MapName} 成功");
                else
                    NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  更换服务器 {Globals.GameId} 地图为 {item.MapName} 失败\n{result.Content}");
            }
        }

        // 使ListBox能够响应重复点击
        ListBox_Map.SelectedIndex = -1;
    }

    /// <summary>
    /// 从Admin列表移除选中玩家
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_RemoveSelectedAdmin_Click(object sender, RoutedEventArgs e)
    {
        if (!PlayerUtil.CheckAuth2())
            return;

        if (ListBox_Admin.SelectedItem is RSPInfo item)
        {
            NotifierHelper.Show(NotifierType.Information, $"正在移除服务器管理员 {item.DisplayName} 中...");

            var result = await BF1API.RemoveServerAdmin(Globals.SessionId, Globals.ServerId, item.PersonaId);
            if (result.IsSuccess)
                NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  移除服务器管理员 {item.DisplayName} 成功");
            else
                NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  移除服务器管理员 {item.DisplayName} 失败\n{result.Content}");
        }
    }

    /// <summary>
    /// 添加玩家到Admin列表
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_AddNewAdmin_Click(object sender, RoutedEventArgs e)
    {
        if (!PlayerUtil.CheckAuth2())
            return;

        var name = TextBox_NewAdminName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            NotifierHelper.Show(NotifierType.Warning, "请输入正确的玩家名称");
            return;
        }

        NotifierHelper.Show(NotifierType.Information, $"正在添加服务器管理员 {name} 中...");

        var result = await BF1API.AddServerAdmin(Globals.SessionId, Globals.ServerId, name);
        if (result.IsSuccess)
        {
            TextBox_NewAdminName.Clear();
            NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  添加服务器管理员 {name} 成功");
        }
        else
        {
            NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  添加服务器管理员 {name} 失败\n{result.Content}");
        }
    }

    /// <summary>
    /// 从VIP列表移除选中玩家
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_RemoveSelectedVIP_Click(object sender, RoutedEventArgs e)
    {
        if (!PlayerUtil.CheckAuth2())
            return;

        if (ListBox_VIP.SelectedItem is RSPInfo item)
        {
            NotifierHelper.Show(NotifierType.Information, $"正在移除服务器VIP {item.DisplayName} 中...");

            var result = await BF1API.RemoveServerVip(Globals.SessionId, Globals.ServerId, item.PersonaId);
            if (result.IsSuccess)
                NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  移除服务器VIP {item.DisplayName} 成功");
            else
                NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  移除服务器VIP {item.DisplayName} 失败\n{result.Content}");
        }
    }

    /// <summary>
    /// 添加玩家到VIP列表
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_AddNewVIP_Click(object sender, RoutedEventArgs e)
    {
        if (!PlayerUtil.CheckAuth2())
            return;

        var name = TextBox_NewVIPName.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            NotifierHelper.Show(NotifierType.Error, "请输入正确的玩家名称");
            return;
        }

        NotifierHelper.Show(NotifierType.Information, $"正在添加服务器VIP {name} 中...");

        var result = await BF1API.AddServerVip(Globals.SessionId, Globals.ServerId, name);
        if (result.IsSuccess)
        {
            TextBox_NewVIPName.Clear();
            NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  添加服务器VIP {name} 成功");
        }
        else
        {
            NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  添加服务器VIP {name} 失败\n{result.Content}");
        }
    }

    /// <summary>
    /// 从BAN列表移除选中玩家
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_RemoveSelectedBAN_Click(object sender, RoutedEventArgs e)
    {
        if (!PlayerUtil.CheckAuth2())
            return;

        if (ListBox_BAN.SelectedItem is RSPInfo item)
        {
            NotifierHelper.Show(NotifierType.Information, $"正在移除服务器BAN {item.DisplayName} 中...");

            var result = await BF1API.RemoveServerBan(Globals.SessionId, Globals.ServerId, item.PersonaId);
            if (result.IsSuccess)
                NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  移除服务器BAN {item.DisplayName} 成功");
            else
                NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  移除服务器BAN {item.DisplayName} 失败\n{result.Content}");
        }
    }

    /// <summary>
    /// 添加玩家到BAN列表
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_AddNewBAN_Click(object sender, RoutedEventArgs e)
    {
        if (!PlayerUtil.CheckAuth2())
            return;

        var name = TextBox_NewBANName.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            NotifierHelper.Show(NotifierType.Error, "请输入正确的玩家名称");
            return;
        }

        NotifierHelper.Show(NotifierType.Information, $"正在添加服务器BAN {name} 中...");

        var result = await BF1API.AddServerBan(Globals.SessionId, Globals.ServerId, name);
        if (result.IsSuccess)
        {
            TextBox_NewVIPName.Clear();
            NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  添加服务器BAN {name} 成功");
        }
        else
        {
            NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  添加服务器BAN {name} 失败\n{result.Content}");
        }
    }

    #region 操作服务器高级信息
    /// <summary>
    /// 获取服务器信息（修改前需要重新获取）
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_GetServerAdvancedInfo_Click(object sender, RoutedEventArgs e)
    {
        if (!PlayerUtil.CheckAuth2())
            return;

        NotifierHelper.Show(NotifierType.Information, $"正在获取服务器 {Globals.ServerId} 数据中...");

        var result = await BF1API.GetServerDetails(Globals.SessionId, Globals.ServerId);
        if (result.IsSuccess)
        {
            _serverDetails = JsonHelper.JsonDese<ServerDetails>(result.Content);

            TextBox_ServerName.Text = _serverDetails.result.serverSettings.name;
            TextBox_ServerDescription.Text = _serverDetails.result.serverSettings.description;

            _isGetServerDetailsOK = true;

            NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  获取服务器 {Globals.ServerId} 数据成功");
        }
        else
        {
            NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  获取服务器 {Globals.ServerId} 数据失败\n{result.Content}");
        }
    }

    /// <summary>
    /// 更新服务器信息（需要服主权限才能修改）
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_UpdateServerAdvancedInfo_Click(object sender, RoutedEventArgs e)
    {
        if (!_isGetServerDetailsOK)
        {
            NotifierHelper.Show(NotifierType.Warning, "请先获取服务器信息后，再执行本操作");
            return;
        }

        var serverName = TextBox_ServerName.Text.Trim();
        var serverDescription = TextBox_ServerDescription.Text.Trim();
        serverDescription = ChsUtil.ToTraditional(serverDescription);

        if (!PlayerUtil.CheckAuth2())
            return;

        if (string.IsNullOrEmpty(serverName))
        {
            NotifierHelper.Show(NotifierType.Warning, "服务器名称不能为空");
            return;
        }

        NotifierHelper.Show(NotifierType.Information, $"正在更新服务器 {Globals.ServerId} 数据中...");

        UpdateServer reqBody = new()
        {
            jsonrpc = "2.0",
            method = "RSP.updateServer"
        };

        var tempParams = new UpdateServer.Params
        {
            deviceIdMap = new UpdateServer.Params.DeviceIdMap()
            {
                machash = Guid.NewGuid().ToString()
            },
            game = "tunguska",
            serverId = Globals.ServerId.ToString(),
            bannerSettings = new UpdateServer.Params.BannerSettings()
            {
                bannerUrl = "",
                clearBanner = true
            }
        };

        var tempMapRotation = new UpdateServer.Params.MapRotation();
        var temp = _serverDetails.result.mapRotations[0];
        var tempMaps = new List<UpdateServer.Params.MapRotation.MapsItem>();
        foreach (var item in temp.maps)
        {
            tempMaps.Add(new UpdateServer.Params.MapRotation.MapsItem()
            {
                gameMode = item.gameMode,
                mapName = item.mapName
            });
        }
        tempMapRotation.maps = tempMaps;
        tempMapRotation.rotationType = temp.rotationType;
        tempMapRotation.mod = temp.mod;
        tempMapRotation.name = temp.name;
        tempMapRotation.description = temp.description;
        tempMapRotation.id = "100";

        tempParams.mapRotation = tempMapRotation;

        tempParams.serverSettings = new UpdateServer.Params.ServerSettings()
        {
            name = serverName,
            description = serverDescription,

            message = _serverDetails.result.serverSettings.message,
            password = _serverDetails.result.serverSettings.password,
            bannerUrl = _serverDetails.result.serverSettings.bannerUrl,
            mapRotationId = _serverDetails.result.serverSettings.mapRotationId,
            customGameSettings = _serverDetails.result.serverSettings.customGameSettings
        };

        reqBody.@params = tempParams;
        reqBody.id = Guid.NewGuid().ToString();

        var result = await BF1API.UpdateServer(Globals.SessionId, reqBody);
        if (result.IsSuccess)
            NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  更新服务器 {Globals.ServerId} 数据成功");
        else
            NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  更新服务器 {Globals.ServerId} 数据失败\n{result.Content}");

        _isGetServerDetailsOK = false;
    }

    /// <summary>
    /// 转换服务器描述文本为简体中文
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_ToSimplified_Click(object sender, RoutedEventArgs e)
    {
        var serverDescription = TextBox_ServerDescription.Text.Trim();

        if (string.IsNullOrEmpty(serverDescription))
        {
            NotifierHelper.Show(NotifierType.Warning, "服务器描述不能为空");
            return;
        }

        TextBox_ServerDescription.Text = ChsUtil.ToSimplified(serverDescription);
        NotifierHelper.Show(NotifierType.Success, "转换服务器描述文本为简体中文成功");
    }

    /// <summary>
    /// 转换服务器描述文本为繁体中文
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_ToTraditional_Click(object sender, RoutedEventArgs e)
    {
        var serverDescription = TextBox_ServerDescription.Text.Trim();

        if (string.IsNullOrEmpty(serverDescription))
        {
            NotifierHelper.Show(NotifierType.Warning, "服务器描述不能为空");
            return;
        }

        TextBox_ServerDescription.Text = ChsUtil.ToTraditional(serverDescription);
        NotifierHelper.Show(NotifierType.Success, "转换服务器描述文本为繁体中文成功");
    }
    #endregion

    /// <summary>
    /// 刷新当前服务器详情
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_RefreshFullServerDetails_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Globals.SessionId))
        {
            NotifierHelper.Show(NotifierType.Warning, "请先获取玩家SessionId");
            return;
        }

        if (Globals.GameId == 0)
        {
            NotifierHelper.Show(NotifierType.Warning, "GameId为空，请先进入服务器后再操作");
            return;
        }

        Task.Run(async () =>
        {
            if (await GetFullServerDetails())
            {
                DetailModel.ServerName = string.Empty;
                DetailModel.ServerDescription = string.Empty;
                DetailModel.ServerGameId = string.Empty;
                DetailModel.ServerGuid = string.Empty;
                DetailModel.ServerId = string.Empty;
                DetailModel.ServerBookmark = string.Empty;
                DetailModel.ServerOwnerName = string.Empty;
                DetailModel.ServerOwnerPersonaId = string.Empty;
                DetailModel.ServerOwnerImage = string.Empty;

                Globals.ServerAdmins_PID.Clear();
                Globals.ServerVIPs_PID.Clear();

                Globals.ServerId = 0;
                Globals.PersistedGameId = string.Empty;

                ListBox_MapList.Clear();
                ListBox_AdminList.Clear();
                ListBox_VIPList.Clear();
                ListBox_BANList.Clear();

                this.Dispatcher.Invoke(DispatcherPriority.Background, () =>
                {
                    ListBox_Map.Items.Clear();
                    ListBox_Admin.Items.Clear();
                    ListBox_VIP.Items.Clear();
                    ListBox_BAN.Items.Clear();
                });

                ListBox_MapList.ForEach(map =>
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                    {
                        ListBox_Map.Items.Add(map);
                    });
                });

                ListBox_AdminList.ForEach(map =>
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                    {
                        ListBox_Admin.Items.Add(map);
                    });
                });

                ListBox_VIPList.ForEach(map =>
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                    {
                        ListBox_VIP.Items.Add(map);
                    });
                });

                ListBox_BANList.ForEach(map =>
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                    {
                        ListBox_BAN.Items.Add(map);
                    });
                });

                this.Dispatcher.Invoke(() =>
                {
                    NotifierHelper.Show(NotifierType.Success, "刷新当前服务器详情成功");
                });
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    NotifierHelper.Show(NotifierType.Error, "刷新当前服务器详情失败");
                });
            }
        });

        Task.Run(async () =>
        {
            int count = 5;
            while (count-- > 0)
            {
                this.Dispatcher.Invoke(() =>
                {
                    Button_RefreshFullServerDetails.IsEnabled = false;
                    Button_RefreshFullServerDetails.Content = $"刷新当前服务器详情 {count}s";
                });

                await Task.Delay(1000);
            }

            this.Dispatcher.Invoke(() =>
            {
                Button_RefreshFullServerDetails.IsEnabled = true;
                Button_RefreshFullServerDetails.Content = "刷新当前服务器详情";
            });
        });
    }

    /// <summary>
    /// 离开当前服务器
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_LeaveCurrentGame_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Globals.SessionId))
        {
            NotifierHelper.Show(NotifierType.Warning, "请先获取玩家SessionId");
            return;
        }

        if (Globals.GameId == 0)
        {
            NotifierHelper.Show(NotifierType.Warning, "请先进入服务器获取GameID");
            return;
        }

        NotifierHelper.Show(NotifierType.Information, $"正在离开服务器 {Globals.GameId} 中...");

        var result = await BF1API.LeaveGame(Globals.SessionId, Globals.GameId);
        if (result.IsSuccess)
            NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  离开服务器 {Globals.GameId} 成功");
        else
            NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  离开服务器 {Globals.GameId} 失败\n{result.Content}");
    }
}
