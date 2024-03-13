using BF1ServerTools.API;
using BF1ServerTools.API.RespJson;
using BF1ServerTools.SDK;
using BF1ServerTools.SDK.Data;
using BF1ServerTools.RES;
using BF1ServerTools.Data;
using BF1ServerTools.Utils;
using BF1ServerTools.Helper;
using BF1ServerTools.Windows;

namespace BF1ServerTools.Views;

/// <summary>
/// MonitView.xaml 的交互逻辑
/// </summary>
public partial class MonitView : UserControl
{
    private List<PlayerData> PlayerList_Team1 = new();
    private List<PlayerData> PlayerList_Team2 = new();

    ///////////////////////////////////////////////////////

    /// <summary>
    /// 用于检测玩家换边数据
    /// </summary>
    public static List<PlayerData> PlayerDatas_Team1 = new();
    public static List<PlayerData> PlayerDatas_Team2 = new();

    /// <summary>
    /// 正在执行踢人请求的玩家列表，保留指定时间秒数
    /// </summary>
    private List<AutoKickInfo> Kicking_PlayerList = new();

    /// <summary>
    /// 绑定UI动态数据集合，用于更新违规玩家列表
    /// </summary>
    public ObservableCollection<BreakRuleInfoModel> ListView_PlayerList_BreakRuleInfo { get; set; } = new();

    /// <summary>
    /// 绑定UI动态数据集合，用于缓存生涯玩家数据
    /// </summary>
    public ObservableCollection<LifeDataInfo> LifeDataInfos { get; set; } = new();

    ///////////////////////////////////////////////////////

    /// <summary>
    /// 当前服务器信息
    /// </summary>
    private ServerData _serverData = new();

    /// <summary>
    /// 踢人成功延迟时间，单位秒
    /// </summary>
    private const int KickDelay = 10;

    /// <summary>
    /// 生涯数据缓存时间，单位分钟
    /// </summary>
    private const int CacheTime = 30;

    /// <summary>
    /// 生涯数据缓存
    /// </summary>
    private string F_LifeCache_Path = FileUtil.D_Data_Path + @"\LifeCache.json";

    /// <summary>
    /// 自定义踢出非白名单玩家理由
    /// </summary>
    public string KickNoWhitesReason { get; set; } = "请加QQ群";

    ///////////////////////////////////////////////////////

    public MonitView()
    {
        InitializeComponent();
        this.DataContext = this;
        MainWindow.WindowClosingEvent += MainWindow_WindowClosingEvent;

        // 读取生涯数据缓存
        if (File.Exists(F_LifeCache_Path))
        {
            using var streamReader = new StreamReader(F_LifeCache_Path);
            Globals.LifePlayerCacheDatas = JsonHelper.JsonDese<List<LifePlayerData>>(streamReader.ReadToEnd());
        }

        new Thread(CacheLifePlayerDataThread)
        {
            Name = "CacheLifePlayerDataThread",
            IsBackground = true
        }.Start();

        new Thread(CheckBreakPlayerThread)
        {
            Name = "CheckBreakPlayerThread",
            IsBackground = true
        }.Start();

        new Thread(CheckAutoKickStateThread)
        {
            Name = "CheckAutoKickStateThread",
            IsBackground = true
        }.Start();
    }

    /// <summary>
    /// 主窗口关闭事件
    /// </summary>
    private void MainWindow_WindowClosingEvent()
    {
        // 保存生涯数据缓存
        File.WriteAllText(F_LifeCache_Path, JsonHelper.JsonSeri(Globals.LifePlayerCacheDatas));
    }

    /// <summary>
    /// 缓存玩家生涯数据
    /// </summary>
    private async void CacheLifePlayerDataThread()
    {
        while (MainWindow.IsAppRunning)
        {
            // 移除超过预设时间的玩家
            // 倒叙删除，因为每次删除list的下标号会改变，倒叙就不存在这个问题
            for (int i = Globals.LifePlayerCacheDatas.Count - 1; i >= 0; i--)
            {
                if (!Globals.LifePlayerCacheDatas[i].IsWeaponOK || !Globals.LifePlayerCacheDatas[i].IsVehicleOK)
                {
                    Globals.LifePlayerCacheDatas.RemoveAt(i);
                }
                else if (MiscUtil.DiffMinutes(Globals.LifePlayerCacheDatas[i].Date, DateTime.Now) > CacheTime)
                {
                    Globals.LifePlayerCacheDatas.RemoveAt(i);
                }
            }

            // 移除踢人缓存CD超时玩家
            for (int i = Globals.KickCoolDownInfos.Count - 1; i >= 0; i--)
            {
                if (MiscUtil.DiffMinutes(Globals.KickCoolDownInfos[i].Date, DateTime.Now) > CacheTime)
                {
                    Globals.KickCoolDownInfos.RemoveAt(i);
                }
            }

            // SessionId不为空 且 GameId不为0
            if (!string.IsNullOrEmpty(Globals.SessionId) && Globals.GameId != 0)
            {
                // 遍历玩家列表
                foreach (var item in Player.GetPlayerCache())
                {
                    // 先判断这个玩家是否在玩家生涯缓存数据中
                    var index = Globals.LifePlayerCacheDatas.FindIndex(var => var.PersonaId == item.PersonaId);
                    if (index == -1)
                    {
                        // 缓存玩家生涯KD、KPM
                        var result = await BF1API.DetailedStatsByPersonaId(Globals.SessionId, item.PersonaId);
                        if (result.IsSuccess)
                        {
                            var detailedStats = JsonHelper.JsonDese<DetailedStats>(result.Content);

                            int kills = detailedStats.result.basicStats.kills;
                            int deaths = detailedStats.result.basicStats.deaths;
                            float kd = PlayerUtil.GetPlayerKD(kills, deaths);
                            float kpm = detailedStats.result.basicStats.kpm;
                            int time = PlayerUtil.GetPlayHours(detailedStats.result.basicStats.timePlayed);

                            Globals.LifePlayerCacheDatas.Add(new()
                            {
                                Date = DateTime.Now,
                                Name = item.Name,
                                PersonaId = item.PersonaId,
                                KD = kd,
                                KPM = kpm,
                                Time = time,
                                WeaponInfos = new(),
                                VehicleInfos = new()
                            });

                            // 拿到当前生涯索引
                            var lifeIndex = Globals.LifePlayerCacheDatas.FindIndex(var => var.PersonaId == item.PersonaId);

                            // 缓存玩家生涯武器数据
                            result = await BF1API.GetWeaponsByPersonaId(Globals.SessionId, item.PersonaId);
                            if (result.IsSuccess)
                            {
                                var getWeapons = JsonHelper.JsonDese<GetWeapons>(result.Content);
                                Globals.LifePlayerCacheDatas[lifeIndex].IsWeaponOK = true;
                                if (getWeapons.result.Count != 0)
                                {
                                    foreach (var res in getWeapons.result)
                                    {
                                        foreach (var wea in res.weapons)
                                        {
                                            var star = (int)wea.stats.values.kills / 100;
                                            if (star == 0)
                                                continue;

                                            Globals.LifePlayerCacheDatas[lifeIndex].WeaponInfos.Add(new()
                                            {
                                                Name = ChsUtil.ToSimplified(wea.name),
                                                Kill = (int)wea.stats.values.kills,
                                                Star = star
                                            });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // 获取失败则移除队列
                                Globals.LifePlayerCacheDatas[lifeIndex].IsWeaponOK = false;
                                Globals.LifePlayerCacheDatas.RemoveAt(lifeIndex);
                                continue;
                            }

                            // 缓存玩家生涯载具数据
                            result = await BF1API.GetVehiclesByPersonaId(Globals.SessionId, item.PersonaId);
                            if (result.IsSuccess)
                            {
                                var getVehicles = JsonHelper.JsonDese<GetVehicles>(result.Content);
                                Globals.LifePlayerCacheDatas[lifeIndex].IsVehicleOK = true;
                                if (getVehicles.result.Count != 0)
                                {
                                    foreach (var res in getVehicles.result)
                                    {
                                        foreach (var veh in res.vehicles)
                                        {
                                            var star = (int)veh.stats.values.kills / 100;
                                            if (star == 0)
                                                continue;

                                            Globals.LifePlayerCacheDatas[lifeIndex].VehicleInfos.Add(new()
                                            {
                                                Name = ChsUtil.ToSimplified(veh.name),
                                                Kill = (int)veh.stats.values.kills,
                                                Star = star
                                            });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // 获取失败则移除队列
                                Globals.LifePlayerCacheDatas[lifeIndex].IsVehicleOK = false;
                                Globals.LifePlayerCacheDatas.RemoveAt(lifeIndex);
                                continue;
                            }

                            // 按击杀数降序排序
                            Globals.LifePlayerCacheDatas[lifeIndex].WeaponInfos.Sort((a, b) => b.Kill.CompareTo(a.Kill));
                            Globals.LifePlayerCacheDatas[lifeIndex].VehicleInfos.Sort((a, b) => b.Kill.CompareTo(a.Kill));
                        }
                    }
                }
            }

            Thread.Sleep(5000);
        }
    }

    /// <summary>
    /// 检查违规玩家线程
    /// </summary>
    private void CheckBreakPlayerThread()
    {
        while (MainWindow.IsAppRunning)
        {
            //////////////////////////////// 数据初始化 ////////////////////////////////

            PlayerList_Team1.Clear();
            PlayerList_Team2.Clear();

            Globals.BreakRuleInfo_PlayerList.Clear();

            //////////////////////////////// 服务器数据获取 ////////////////////////////////

            // 服务器地图名称
            _serverData.MapName = Server.GetMapName();
            _serverData.Team1Name = ClientHelper.GetTeamChsName(_serverData.MapName, 1);
            _serverData.Team2Name = ClientHelper.GetTeamChsName(_serverData.MapName, 2);

            _serverData.MapName = string.IsNullOrEmpty(_serverData.MapName) ? "未知" : _serverData.MapName;
            _serverData.MapName = ClientHelper.GetMapChsName(_serverData.MapName);

            // 服务器游戏模式
            _serverData.GameMode = Server.GetGameMode();
            _serverData.GameMode = ClientHelper.GetGameMode(_serverData.GameMode);

            //////////////////////////////// 队伍数据整理 ////////////////////////////////

            var time = PlayerUtil.SecondsToMinute(Server.GetServerTime());

            foreach (var item in Player.GetPlayerList())
            {
                item.Kd = PlayerUtil.GetPlayerKD(item.Kill, item.Dead);
                item.Kpm = PlayerUtil.GetPlayerKPM(item.Kill, time);

                item.LifeKd = PlayerUtil.GetLifeKD(item.PersonaId);
                item.LifeKpm = PlayerUtil.GetLifeKPM(item.PersonaId);

                item.Admin = PlayerUtil.IsAdminVIP(item.PersonaId, Globals.ServerAdmins_PID);
                item.White = PlayerUtil.IsWhite(item.Name, Globals.CustomWhites_Name);

                // 踢人CD
                for (int i = 0; i < Globals.KickCoolDownInfos.Count; i++)
                {
                    if (item.PersonaId == Globals.KickCoolDownInfos[i].PersonaId)
                    {
                        AddBreakRulePlayerInfo(item, BreakType.CD, "Server Kick CD 30 Minute");
                    }
                }

                // 黑名单
                for (int i = 0; i < Globals.CustomBlacks_Name.Count; i++)
                {
                    if (item.Name == Globals.CustomBlacks_Name[i])
                    {
                        AddBreakRulePlayerInfo(item, BreakType.Black, "Server Black List");
                    }
                }

                // 踢出非白名单玩家
                if (Globals.IsEnableKickNoWhites)
                {
                    AddBreakRulePlayerInfo(item, BreakType.NoWhite, $"{KickNoWhitesReason}");
                }

                switch (item.TeamId)
                {
                    case 0:
                        // 检查队伍0违规玩家
                        if (item.Spectator == 0x01)
                            CheckTeam0PlayerIsBreakRule(item);
                        break;
                    case 1:
                        PlayerList_Team1.Add(item);
                        // 检查队伍1违规玩家
                        CheckTeam12PlayerIsBreakRule(item, Globals.ServerRule_Team1, Globals.CustomWeapons_Team1);
                        break;
                    case 2:
                        PlayerList_Team2.Add(item);
                        // 检查队伍2违规玩家
                        CheckTeam12PlayerIsBreakRule(item, Globals.ServerRule_Team2, Globals.CustomWeapons_Team2);
                        break;
                }
            }

            // 填充默认规则
            foreach (var item in Globals.BreakRuleInfo_PlayerList)
            {
                item.Reason = item.BreakInfos[0].Reason;
            }

            ////////////////////////////////////////////////////////////////////////////////

            this.Dispatcher.BeginInvoke(() =>
            {
                UpdateListViewBreakRule();
            });

            ////////////////////////////////////////////////////////////////////////////////

            // 自动踢出违规玩家
            AutoKickBreakRulePlayer();

            ////////////////////////////////////////////////////////////////////////////////

            // 检测换边玩家
            CheckPlayerChangeTeam();

            /////////////////////////////////////////////////////////////////////////

            Thread.Sleep(1000);
        }
    }

    /// <summary>
    /// 检查自动踢人状态线程
    /// </summary>
    private void CheckAutoKickStateThread()
    {
        bool isRun = false;

        while (MainWindow.IsAppRunning)
        {
            if (Globals.GameId == 0 || !Globals.AutoKickBreakRulePlayer)
            {
                if (!isRun)
                {
                    isRun = true;

                    this.Dispatcher.Invoke(() =>
                    {
                        if (ToggleButton_RunAutoKick.IsChecked == true)
                        {
                            Globals.AutoKickBreakRulePlayer = false;
                            ToggleButton_RunAutoKick.IsChecked = false;
                        }
                    });
                }
            }
            else
            {
                isRun = false;
            }

            Thread.Sleep(1000);
        }
    }

    /// <summary>
    /// 增加违规玩家信息
    /// </summary>
    /// <param name="playerData"></param>
    /// <param name="breakType"></param>
    /// <param name="reason"></param>
    private void AddBreakRulePlayerInfo(PlayerData playerData, BreakType breakType, string reason)
    {
        var index = Globals.BreakRuleInfo_PlayerList.FindIndex(val => val.PersonaId == playerData.PersonaId);
        if (index == -1)
        {
            Globals.BreakRuleInfo_PlayerList.Add(new()
            {
                Rank = playerData.Rank,
                Name = playerData.Name,
                PersonaId = playerData.PersonaId,
                Admin = playerData.Admin,
                White = playerData.White,
                BreakInfos = new()
                {
                    new()
                    {
                        BreakType = breakType,
                        Reason = reason
                    }
                }
            });
        }
        else
        {
            Globals.BreakRuleInfo_PlayerList[index].BreakInfos.Add(new()
            {
                BreakType = breakType,
                Reason = reason
            });
        }
    }

    /// <summary>
    /// 检查队伍0玩家是否违规
    /// </summary>
    /// <param name="playerData"></param>
    private void CheckTeam0PlayerIsBreakRule(PlayerData playerData)
    {
        // 限制观战
        if (Globals.IsAutoKickSpectator)
        {
            playerData.Rank = -1;
            AddBreakRulePlayerInfo(playerData, BreakType.Spectator, "Server BAN Spectator");
        }
    }

    /// <summary>
    /// 检查队伍12玩家是否违规
    /// </summary>
    /// <param name="playerData"></param>
    private void CheckTeam12PlayerIsBreakRule(PlayerData playerData, ServerRule serverRule, List<string> customWeapons)
    {
        // 先判断这个玩家是否在玩家生涯缓存数据中
        var lifeIndex = Globals.LifePlayerCacheDatas.FindIndex(var => var.PersonaId == playerData.PersonaId);
        if (lifeIndex != -1)
        {
            // 限制玩家生涯KD
            if (serverRule.LifeMaxKD != 0 &&
                Globals.LifePlayerCacheDatas[lifeIndex].KD > serverRule.LifeMaxKD)
            {
                AddBreakRulePlayerInfo(playerData, BreakType.LifeKD, $"Life KD Limit {serverRule.LifeMaxKD:0.00}");
            }

            // 限制玩家生涯KPM
            if (serverRule.LifeMaxKPM != 0 &&
                Globals.LifePlayerCacheDatas[lifeIndex].KPM > serverRule.LifeMaxKPM)
            {
                AddBreakRulePlayerInfo(playerData, BreakType.LifeKPM, $"Life KPM Limit {serverRule.LifeMaxKPM:0.00}");
            }

            var tempData = new List<string>
            {
                playerData.WeaponS0,
                playerData.WeaponS1,
                playerData.WeaponS2,
                playerData.WeaponS3,
                playerData.WeaponS4,
                playerData.WeaponS5,
                playerData.WeaponS6,
                playerData.WeaponS7
            };

            foreach (var item in tempData)
            {
                // 限制玩家武器最高星数
                if (serverRule.LifeMaxWeaponStar != 0)
                {
                    var name = ClientHelper.GetWeaponChsName(item);
                    var weaponIndex = Globals.LifePlayerCacheDatas[lifeIndex].WeaponInfos.FindIndex(var => name.Contains(var.Name, StringComparison.OrdinalIgnoreCase));
                    if (weaponIndex != -1)
                    {
                        if (Globals.LifePlayerCacheDatas[lifeIndex].WeaponInfos[weaponIndex].Star > serverRule.LifeMaxWeaponStar)
                        {
                            AddBreakRulePlayerInfo(playerData, BreakType.LifeWeaponStar, $"Life Weapon Star Limit {serverRule.LifeMaxWeaponStar:0}");
                        }
                    }
                }
            }

            // 限制玩家载具最高星数
            if (serverRule.LifeMaxVehicleStar != 0)
            {
                var name = ClientHelper.GetWeaponChsName(playerData.WeaponS0);
                var vehicleIndex = Globals.LifePlayerCacheDatas[lifeIndex].VehicleInfos.FindIndex(var => name.Contains(var.Name, StringComparison.OrdinalIgnoreCase));
                if (vehicleIndex != -1)
                {
                    if (Globals.LifePlayerCacheDatas[lifeIndex].VehicleInfos[vehicleIndex].Star > serverRule.LifeMaxVehicleStar)
                    {
                        AddBreakRulePlayerInfo(playerData, BreakType.LifeVehicleStar, $"Life Vehicle Star Limit {serverRule.LifeMaxVehicleStar:0}");
                    }
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        // 限制玩家击杀
        if (playerData.Kill > serverRule.MaxKill &&
            serverRule.MaxKill != 0)
        {
            AddBreakRulePlayerInfo(playerData, BreakType.Kill, $"Kill Limit {serverRule.MaxKill:0}");
        }

        // 计算玩家KD最低击杀数
        if (playerData.Kill > serverRule.FlagKD &&
            serverRule.FlagKD != 0)
        {
            // 限制玩家KD
            if (playerData.Kd > serverRule.MaxKD &&
                serverRule.MaxKD != 0.00f)
            {
                AddBreakRulePlayerInfo(playerData, BreakType.KD, $"KD Limit {serverRule.MaxKD:0.00}");
            }
        }

        // 计算玩家KPM比条件
        if (playerData.Kill > serverRule.FlagKPM &&
            serverRule.FlagKPM != 0)
        {
            // 限制玩家KPM
            if (playerData.Kpm > serverRule.MaxKPM &&
                serverRule.MaxKPM != 0.00f)
            {
                AddBreakRulePlayerInfo(playerData, BreakType.KPM, $"KPM Limit {serverRule.MaxKPM:0.00}");
            }
        }

        // 限制玩家最低等级
        if (playerData.Rank < serverRule.MinRank &&
            serverRule.MinRank != 0 &&
            playerData.Rank != 0)
        {
            AddBreakRulePlayerInfo(playerData, BreakType.Rank, $"Min Rank Limit {serverRule.MinRank:0}");
        }

        // 限制玩家最高等级
        if (playerData.Rank > serverRule.MaxRank &&
            serverRule.MaxRank != 0 &&
            playerData.Rank != 0)
        {
            AddBreakRulePlayerInfo(playerData, BreakType.Rank, $"Max Rank Limit {serverRule.MaxRank:0}");
        }

        // 从武器规则里遍历限制武器名称
        for (int i = 0; i < customWeapons.Count; i++)
        {
            var item = customWeapons[i];

            // K 弹
            if (item == "_KBullet")
            {
                if (playerData.WeaponS2.Contains("_KBullet") ||
                    playerData.WeaponS5.Contains("_KBullet"))
                {
                    AddBreakRulePlayerInfo(playerData, BreakType.Weapon, "Weapon Limit K Bullet");
                }
            }

            // 步枪手榴弹（破片）
            if (item == "_RGL_Frag")
            {
                if (playerData.WeaponS2.Contains("_RGL_Frag") ||
                    playerData.WeaponS5.Contains("_RGL_Frag"))
                {
                    AddBreakRulePlayerInfo(playerData, BreakType.Weapon, "Weapon Limit RGL Frag");
                }
            }

            // 步枪手榴弹（烟雾）
            if (item == "_RGL_Smoke")
            {
                if (playerData.WeaponS2.Contains("_RGL_Smoke") ||
                    playerData.WeaponS5.Contains("_RGL_Smoke"))
                {
                    AddBreakRulePlayerInfo(playerData, BreakType.Weapon, "Weapon Limit RGL Smoke");
                }
            }

            // 步枪手榴弹（高爆）
            if (item == "_RGL_HE")
            {
                if (playerData.WeaponS2.Contains("_RGL_HE") ||
                    playerData.WeaponS5.Contains("_RGL_HE"))
                {
                    AddBreakRulePlayerInfo(playerData, BreakType.Weapon, "Weapon Limit RGL HE");
                }
            }

            // 其他违规武器
            if (playerData.WeaponS0 == item ||
                playerData.WeaponS1 == item ||
                playerData.WeaponS2 == item ||
                playerData.WeaponS3 == item ||
                playerData.WeaponS4 == item ||
                playerData.WeaponS5 == item ||
                playerData.WeaponS6 == item ||
                playerData.WeaponS7 == item)
            {
                AddBreakRulePlayerInfo(playerData, BreakType.Weapon, $"Weapon Limit {ClientHelper.GetWeaponShortTxt(item)}");
            }
        }
    }

    /// <summary>
    /// 动态更新 ListView 违规玩家列表
    /// </summary>
    private void UpdateListViewBreakRule()
    {
        if (Globals.BreakRuleInfo_PlayerList.Count == 0)
        {
            ListView_PlayerList_BreakRuleInfo.Clear();
        }

        // 更新ListView中现有的玩家数据，并把ListView中已经不在服务器的玩家清除
        for (int i = 0; i < ListView_PlayerList_BreakRuleInfo.Count; i++)
        {
            int index = Globals.BreakRuleInfo_PlayerList.FindIndex(val => val.PersonaId == ListView_PlayerList_BreakRuleInfo[i].PersonaId);
            if (index != -1)
            {
                ListView_PlayerList_BreakRuleInfo[i].Rank = Globals.BreakRuleInfo_PlayerList[index].Rank;
                ListView_PlayerList_BreakRuleInfo[i].Name = Globals.BreakRuleInfo_PlayerList[index].Name;
                ListView_PlayerList_BreakRuleInfo[i].PersonaId = Globals.BreakRuleInfo_PlayerList[index].PersonaId;
                ListView_PlayerList_BreakRuleInfo[i].Admin = Globals.BreakRuleInfo_PlayerList[index].Admin;
                ListView_PlayerList_BreakRuleInfo[i].White = Globals.BreakRuleInfo_PlayerList[index].White;
                ListView_PlayerList_BreakRuleInfo[i].Reason = Globals.BreakRuleInfo_PlayerList[index].Reason;
                ListView_PlayerList_BreakRuleInfo[i].Count = Globals.BreakRuleInfo_PlayerList[index].BreakInfos.Count;

                StringBuilder builder = new();
                foreach (var item in Globals.BreakRuleInfo_PlayerList[index].BreakInfos)
                {
                    builder.Append($"{item.BreakType}, ");
                }
                ListView_PlayerList_BreakRuleInfo[i].AllReason = builder.ToString();
            }
            else
            {
                ListView_PlayerList_BreakRuleInfo.RemoveAt(i);
            }
        }

        // 增加ListView没有的玩家数据
        for (int i = 0; i < Globals.BreakRuleInfo_PlayerList.Count; i++)
        {
            int index = ListView_PlayerList_BreakRuleInfo.ToList().FindIndex(val => val.PersonaId == Globals.BreakRuleInfo_PlayerList[i].PersonaId);
            if (index == -1)
            {
                StringBuilder builder = new();
                foreach (var item in Globals.BreakRuleInfo_PlayerList[i].BreakInfos)
                {
                    builder.Append($"{item.BreakType}, ");
                }

                ListView_PlayerList_BreakRuleInfo.Add(new()
                {
                    Rank = Globals.BreakRuleInfo_PlayerList[i].Rank,
                    Name = Globals.BreakRuleInfo_PlayerList[i].Name,
                    PersonaId = Globals.BreakRuleInfo_PlayerList[i].PersonaId,
                    Admin = Globals.BreakRuleInfo_PlayerList[i].Admin,
                    White = Globals.BreakRuleInfo_PlayerList[i].White,
                    Reason = Globals.BreakRuleInfo_PlayerList[i].Reason,
                    Count = Globals.BreakRuleInfo_PlayerList[i].BreakInfos.Count,
                    AllReason = builder.ToString()
                });
            }
        }

        // 修正序号
        for (int i = 0; i < ListView_PlayerList_BreakRuleInfo.Count; i++)
        {
            ListView_PlayerList_BreakRuleInfo[i].Index = i + 1;
        }
    }

    /// <summary>
    /// 自动踢出违规玩家
    /// </summary>
    private void AutoKickBreakRulePlayer()
    {
        // 自动踢出违规玩家开关
        if (Globals.AutoKickBreakRulePlayer)
        {
            // 遍历违规玩家列表
            foreach (var item in Globals.BreakRuleInfo_PlayerList)
            {
                // 跳过管理员玩家
                if (item.Admin)
                    continue;

                // 跳过白名单玩家
                if (item.White)
                {
                    if (CheckWhiteBreakRule(item))
                        continue;
                }

                // 先检查踢出玩家是否在 正在踢人 列表中
                var index = Kicking_PlayerList.FindIndex(var => var.PersonaId == item.PersonaId);
                if (index == -1)
                {
                    // 该玩家不在 正在踢人 列表中
                    Kicking_PlayerList.Add(new()
                    {
                        Rank = item.Rank,
                        Name = item.Name,
                        PersonaId = item.PersonaId,
                        Reason = item.Reason,
                        Flag = KickFlag.Default,
                        State = "准备踢人",
                        Time = DateTime.Now
                    });
                }
            }

            // 执行踢人请求
            foreach (var item in Kicking_PlayerList)
            {
                if (item.Flag == KickFlag.Default)
                {
                    AutoKickPlayer(item);
                }
            }

            // 动态修正踢人列表数据
            // 倒叙删除，因为每次删除list的下标号会改变，倒叙就不存在这个问题
            for (int i = Kicking_PlayerList.Count - 1; i >= 0; i--)
            {
                if (Kicking_PlayerList.Count != 0)
                {
                    // 如果踢人失败，立刻移除列表（准备重新踢出）
                    if (Kicking_PlayerList[i].Flag == KickFlag.Faild)
                    {
                        Kicking_PlayerList.RemoveAt(i);
                        break;
                    }

                    // 如果踢人成功，KickDelay 秒后移除列表（避免重复踢出）
                    if (Kicking_PlayerList[i].Flag == KickFlag.Success && MiscUtil.DiffSeconds(Kicking_PlayerList[i].Time, DateTime.Now) > KickDelay)
                    {
                        Kicking_PlayerList.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 自动踢出玩家
    /// </summary>
    /// <param name="info"></param>
    private async void AutoKickPlayer(AutoKickInfo info)
    {
        info.Flag = KickFlag.Kicking;
        info.State = "正在踢人中...";
        info.Time = DateTime.Now;

        var result = await BF1API.RSPKickPlayer(Globals.SessionId, Globals.GameId, info.PersonaId, info.Reason);
        if (result.IsSuccess)
        {
            info.Flag = KickFlag.Success;
            info.State = "踢出成功";
            info.Time = DateTime.Now;

            LogView.ActionAddKickOKLog(info);
        }
        else
        {
            info.Flag = KickFlag.Faild;
            info.State = $"踢出失败  {result.Content}";
            info.Time = DateTime.Now;

            LogView.ActionAddKickNOLog(info);
        }
    }

    /// <summary>
    /// List深复制
    /// </summary>
    /// <param name="originalList"></param>
    /// <returns></returns>
    private List<PlayerData> CopyPlayerDataList(List<PlayerData> originalList)
    {
        List<PlayerData> list = new();
        foreach (var item in originalList)
        {
            PlayerData data = new()
            {
                Rank = item.Rank,
                Name = item.Name,
                PersonaId = item.PersonaId
            };
            list.Add(data);
        }
        return list;
    }

    /// <summary>
    /// 检测换边玩家
    /// </summary>
    private void CheckPlayerChangeTeam()
    {
        // 如果玩家没有进入服务器，不检测换边情况
        if (Globals.GameId == 0)
            return;

        // 如果双方玩家人数都为0，不检测换边情况
        if (PlayerList_Team1.Count == 0 && PlayerList_Team2.Count == 0)
        {
            PlayerDatas_Team1.Clear();
            PlayerDatas_Team2.Clear();
            return;
        }

        // 第一次初始化
        if (PlayerDatas_Team1.Count == 0 && PlayerDatas_Team2.Count == 0)
        {
            PlayerDatas_Team1 = CopyPlayerDataList(PlayerList_Team1);
            PlayerDatas_Team2 = CopyPlayerDataList(PlayerList_Team2);
            return;
        }

        // 变量保存的队伍1玩家列表
        foreach (var item in PlayerDatas_Team1)
        {
            // 查询这个玩家是否在目前的队伍2中
            var index = PlayerList_Team2.FindIndex(var => var.PersonaId == item.PersonaId);
            if (index != -1)
            {
                LogView.ActionAddChangeTeamInfoLog(new ChangeTeamInfo()
                {
                    Rank = item.Rank,
                    Name = item.Name,
                    PersonaId = item.PersonaId,
                    GameMode = _serverData.GameMode,
                    MapName = _serverData.MapName,
                    Team1Name = _serverData.Team1Name,
                    Team2Name = _serverData.Team2Name,
                    State = $"{_serverData.Team1Name} >>> {_serverData.Team2Name}",
                    Time = DateTime.Now
                });
                break;
            }
        }

        // 变量保存的队伍2玩家列表
        foreach (var item in PlayerDatas_Team2)
        {
            // 查询这个玩家是否在目前的队伍1中
            var index = PlayerList_Team1.FindIndex(var => var.PersonaId == item.PersonaId);
            if (index != -1)
            {
                LogView.ActionAddChangeTeamInfoLog(new ChangeTeamInfo()
                {
                    Rank = item.Rank,
                    Name = item.Name,
                    PersonaId = item.PersonaId,
                    GameMode = _serverData.GameMode,
                    MapName = _serverData.MapName,
                    Team1Name = _serverData.Team1Name,
                    Team2Name = _serverData.Team2Name,
                    State = $"{_serverData.Team1Name} <<< {_serverData.Team2Name}",
                    Time = DateTime.Now
                });
                break;
            }
        }

        // 更新保存的数据
        PlayerDatas_Team1 = CopyPlayerDataList(PlayerList_Team1);
        PlayerDatas_Team2 = CopyPlayerDataList(PlayerList_Team2);
    }

    /// <summary>
    /// 检测白名单特权
    /// </summary>
    /// <param name="breakInfo"></param>
    private bool CheckWhiteBreakRule(BreakRuleInfo breakRuleInfo)
    {
        foreach (var item in breakRuleInfo.BreakInfos)
        {
            if (item.BreakType == BreakType.LifeKD)
            {
                if (Globals.WhiteLifeKD)
                    continue;
                else
                {
                    breakRuleInfo.Reason = item.Reason;
                    return false;
                }
            }

            if (item.BreakType == BreakType.LifeKPM)
            {
                if (Globals.WhiteLifeKPM)
                    continue;
                else
                {
                    breakRuleInfo.Reason = item.Reason;
                    return false;
                }
            }

            if (item.BreakType == BreakType.LifeWeaponStar)
            {
                if (Globals.WhiteLifeWeaponStar)
                    continue;
                else
                {
                    breakRuleInfo.Reason = item.Reason;
                    return false;
                }
            }

            if (item.BreakType == BreakType.LifeVehicleStar)
            {
                if (Globals.WhiteLifeVehicleStar)
                    continue;
                else
                {
                    breakRuleInfo.Reason = item.Reason;
                    return false;
                }
            }

            if (item.BreakType == BreakType.Kill)
            {
                if (Globals.WhiteKill)
                    continue;
                else
                {
                    breakRuleInfo.Reason = item.Reason;
                    return false;
                }
            }

            if (item.BreakType == BreakType.KD)
            {
                if (Globals.WhiteKD)
                    continue;
                else
                {
                    breakRuleInfo.Reason = item.Reason;
                    return false;
                }
            }

            if (item.BreakType == BreakType.KPM)
            {
                if (Globals.WhiteKPM)
                    continue;
                else
                {
                    breakRuleInfo.Reason = item.Reason;
                    return false;
                }
            }

            if (item.BreakType == BreakType.Rank)
            {
                if (Globals.WhiteRank)
                    continue;
                else
                {
                    breakRuleInfo.Reason = item.Reason;
                    return false;
                }
            }

            if (item.BreakType == BreakType.Weapon)
            {
                if (Globals.WhiteWeapon)
                    continue;
                else
                {
                    breakRuleInfo.Reason = item.Reason;
                    return false;
                }
            }
        }

        return true;
    }

    ////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 打印规则日志
    /// </summary>
    /// <param name="log"></param>
    private void AddRuleLog(string log = "")
    {
        TextBox_RuleLogger.AppendText($"{log}\n");
    }

    /// <summary>
    /// 清空规则日志
    /// </summary>
    private void ClearRuleLog()
    {
        TextBox_RuleLogger.Clear();

        AddRuleLog("【操作时间】");
        AddRuleLog($"{DateTime.Now:yyyy/MM/dd HH:mm:ss}");
        AddRuleLog();
    }

    /////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 检查自动踢人环境是否合格
    /// </summary>
    /// <returns></returns>
    private async Task<bool> CheckAutoKickEnv()
    {
        ClearRuleLog();

        NotifierHelper.Show(NotifierType.Information, "正在检查自动踢人中...");

        AddRuleLog("【检查环境】");

        AddRuleLog("👉 正在检查 玩家是否应用当前规则...");
        if (!Globals.IsSetRuleOK)
        {
            AddRuleLog("❌ 玩家没有应用当前规则");
            NotifierHelper.Show(NotifierType.Warning, "环境检查未通过，操作取消");
            return false;
        }
        else
        {
            AddRuleLog("✔ 玩家已正确应用当前规则");
            AddRuleLog();
        }

        AddRuleLog("👉 正在检查 GameId是否正确...");
        if (Globals.GameId == 0)
        {
            AddRuleLog("❌ GameId为空，请先进入服务器");
            NotifierHelper.Show(NotifierType.Warning, "环境检查未通过，操作取消");
            return false;
        }
        else
        {
            AddRuleLog("✔ GameId检查正确");
            AddRuleLog();
        }

        AddRuleLog("👉 正在检查 服务器管理员列表是否为空...");
        if (Globals.ServerAdmins_PID.Count == 0)
        {
            AddRuleLog("❌ 服务器管理员列表为空，请先获取当前服务器详情");
            NotifierHelper.Show(NotifierType.Warning, "环境检查未通过，操作取消");
            return false;
        }
        else
        {
            AddRuleLog("✔ 服务器管理员列表检查正确");
            AddRuleLog();
        }

        AddRuleLog("👉 正在检查 玩家是否为当前服务器管理...");
        if (!Globals.LoginPlayerIsAdmin)
        {
            AddRuleLog("❌ 玩家不是当前服务器管理");
            NotifierHelper.Show(NotifierType.Warning, "环境检查未通过，操作取消");
            return false;
        }
        else
        {
            AddRuleLog("✔ 已确认玩家为当前服务器管理");
            AddRuleLog();
        }

        AddRuleLog("👉 正在检查 SessionId是否为空...");
        if (string.IsNullOrEmpty(Globals.SessionId))
        {
            AddRuleLog("❌ SessionId为空，请先获取SessionId");
            NotifierHelper.Show(NotifierType.Warning, "环境检查未通过，操作取消");
            return false;
        }
        else
        {
            AddRuleLog("✔ SessionId检查正确");
            AddRuleLog();
        }

        AddRuleLog("👉 正在检查 SessionId是否有效...");
        var result = await BF1API.GetWelcomeMessage(Globals.SessionId);
        if (!result.IsSuccess)
        {
            AddRuleLog("❌ SessionId已过期，请重新获取");
            NotifierHelper.Show(NotifierType.Warning, "环境检查未通过，操作取消");
            return false;
        }
        else
        {
            AddRuleLog("✔ SessionId检查有效");
            AddRuleLog();
        }

        AddRuleLog("恭喜，自动踢人环境检查已全部通过");

        return true;
    }

    /// <summary>
    /// 检查自动踢人环境
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_CheckAutoKickEnv_Click(object sender, RoutedEventArgs e)
    {
        if (await CheckAutoKickEnv())
        {
            NotifierHelper.Show(NotifierType.Success, "自动踢人环境检查完毕，可以开启自动踢人功能");
        }
    }

    /// <summary>
    /// 激活自动踢人
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ToggleButton_RunAutoKick_Click(object sender, RoutedEventArgs e)
    {
        if (ToggleButton_RunAutoKick.IsChecked == true)
        {
            // 检查自动踢人环境
            if (await CheckAutoKickEnv())
            {
                AddRuleLog();
                AddRuleLog("环境检查完毕，自动踢人开启成功");

                Globals.AutoKickBreakRulePlayer = true;
                NotifierHelper.Show(NotifierType.Success, "自动踢人开启成功");
            }
            else
            {
                Globals.AutoKickBreakRulePlayer = false;
                ToggleButton_RunAutoKick.IsChecked = false;
                NotifierHelper.Show(NotifierType.Warning, "自动踢人环境检查未通过");
            }
        }
        else
        {
            Globals.AutoKickBreakRulePlayer = false;
            NotifierHelper.Show(NotifierType.Notification, "自动踢人关闭成功");
        }
    }

    ////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 限制非管理员观战玩家
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox_IsAutoKickSpectator_Click(object sender, RoutedEventArgs e)
    {
        Globals.IsAutoKickSpectator = CheckBox_IsAutoKickSpectator.IsChecked == true;
    }

    /// <summary>
    /// 启用踢出玩家CD限制
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox_IsEnableKickCoolDown_Click(object sender, RoutedEventArgs e)
    {
        Globals.IsEnableKickCoolDown = CheckBox_IsEnableKickCoolDown.IsChecked == true;
    }

    /// <summary>
    /// 启用踢出非白名单玩家
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox_IsEnableKickNoWhites_Click(object sender, RoutedEventArgs e)
    {
        Globals.IsEnableKickNoWhites = CheckBox_IsEnableKickNoWhites.IsChecked == true;
    }

    /// <summary>
    /// 违规列表 ListView选中变更事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListView_BreakPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListView_BreakPlayer.SelectedItem is BreakRuleInfoModel item)
            MenuItem_Player.Header = $"[{item.Rank}] {item.Name}    {item.Reason}";
        else
            MenuItem_Player.Header = "当前未选中";
    }

    /// <summary>
    /// 手动踢出玩家
    /// </summary>
    /// <param name="rank"></param>
    /// <param name="displayName"></param>
    /// <param name="personaId"></param>
    /// <param name="reason"></param>
    private async void KickPlayer(int rank, string displayName, long personaId, string reason)
    {
        if (!PlayerUtil.CheckAuth())
            return;

        NotifierHelper.Show(NotifierType.Information, $"正在踢出玩家 {displayName} 中...");

        var result = await BF1API.RSPKickPlayer(Globals.SessionId, Globals.GameId, personaId, reason);
        if (result.IsSuccess)
        {
            var info = new AutoKickInfo()
            {
                Rank = rank,
                Name = displayName,
                PersonaId = personaId,
                Reason = PlayerUtil.GetDefaultChsReason(reason),
                State = "踢出成功"
            };
            LogView.ActionScoreKickLog(info);
            NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  踢出玩家 {displayName} 成功");
        }
        else
        {
            var info = new AutoKickInfo()
            {
                Rank = rank,
                Name = displayName,
                PersonaId = personaId,
                Reason = PlayerUtil.GetDefaultChsReason(reason),
                State = $"踢出失败  {result.Content}"
            };
            LogView.ActionScoreKickLog(info);
            NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  踢出玩家 {displayName} 失败\n{result.Content}");
        }
    }

    /// <summary>
    /// 复制数据到剪切板
    /// </summary>
    /// <param name="obj"></param>
    private void Copy2Clipboard(object obj)
    {
        Clipboard.SetDataObject(obj);
        NotifierHelper.Show(NotifierType.Success, $"复制 {obj} 到剪切板成功");
    }

    #region 右键菜单事件
    private void MenuItem_CopyPlayerName_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_BreakPlayer.SelectedItem is BreakRuleInfoModel item)
            Copy2Clipboard(item.Name);
        else
            NotifierHelper.Show(NotifierType.Warning, "当前未选中任何玩家，操作取消");
    }

    private void MenuItem_CopyPlayerPersonaId_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_BreakPlayer.SelectedItem is BreakRuleInfoModel item)
            Copy2Clipboard(item.PersonaId);
        else
            NotifierHelper.Show(NotifierType.Warning, "当前未选中任何玩家，操作取消");
    }

    private void MenuItem_KickPlayerCustom_Click(object sender, RoutedEventArgs e)
    {
        if (!PlayerUtil.CheckAuth())
            return;

        if (ListView_BreakPlayer.SelectedItem is BreakRuleInfoModel item)
        {
            var customKickWindow = new CustomKickWindow(item.Rank, item.Name, item.PersonaId)
            {
                Owner = MainWindow.MainWindowInstance
            };
            customKickWindow.ShowDialog();
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "当前未选中任何玩家，操作取消");
        }
    }

    private void MenuItem_KickPlayerOffensiveBehavior_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_BreakPlayer.SelectedItem is BreakRuleInfoModel item)
            KickPlayer(item.Rank, item.Name, item.PersonaId, "OFFENSIVEBEHAVIOR");
        else
            NotifierHelper.Show(NotifierType.Warning, "当前未选中任何玩家，操作取消");
    }

    private void MenuItem_KickPlayerLatency_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_BreakPlayer.SelectedItem is BreakRuleInfoModel item)
            KickPlayer(item.Rank, item.Name, item.PersonaId, "LATENCY");
        else
            NotifierHelper.Show(NotifierType.Warning, "当前未选中任何玩家，操作取消");
    }

    private void MenuItem_KickPlayerRuleViolation_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_BreakPlayer.SelectedItem is BreakRuleInfoModel item)
            KickPlayer(item.Rank, item.Name, item.PersonaId, "RULEVIOLATION");
        else
            NotifierHelper.Show(NotifierType.Warning, "当前未选中任何玩家，操作取消");
    }

    private void MenuItem_KickPlayerGeneral_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_BreakPlayer.SelectedItem is BreakRuleInfoModel item)
            KickPlayer(item.Rank, item.Name, item.PersonaId, "GENERAL");
        else
            NotifierHelper.Show(NotifierType.Warning, "当前未选中任何玩家，操作取消");
    }
    #endregion
}
