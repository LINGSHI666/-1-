using BF1ServerTools.API;
using BF1ServerTools.API.RespJson;
using BF1ServerTools.SDK;
using BF1ServerTools.SDK.Data;
using BF1ServerTools.RES;
using BF1ServerTools.Data;
using BF1ServerTools.Utils;
using BF1ServerTools.Helper;
using BF1ServerTools.Windows;
using System.Numerics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System;
using System.Windows.Shapes;

namespace BF1ServerTools.Views;

/// <summary>
/// MonitView.xaml 的交互逻辑
/// </summary>
public partial class MonitView : UserControl
{
    private List<PlayerData> PlayerList_Team1 = new();
    private List<PlayerData> PlayerList_Team2 = new();
    public Queue<PlayerData> ScoutListTeacm1 = new Queue<PlayerData>();
    public Queue<PlayerData> ScoutListTeacm2 = new Queue<PlayerData>();// 使用队列来管理排除名单 
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
    /// 正在执行延时踢人请求的玩家列表，保留指定时间秒数
    /// </summary>
    private List<AutoKickInfo> DelayKicking_PlayerList = new();

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
    //滑条改变事件
    private void Slider_ValueChangedDelaykick(object sender, RoutedPropertyChangedEventArgs<double> e)
    {

        if (labelDelaykick != null) // 检查控件是否为null
        {
            double newValue = e.NewValue; // 获取滑块的当前值
            labelDelaykick.Text = $"延时时间为{newValue}s（请考虑观战15s延时）"; // 更新标签内容
        }
    }
    int scorediff = 0;
    private void Slider_ValueChangedKickInfiltration(object sender, RoutedPropertyChangedEventArgs<double> e)
    {

        if (labelKickInfiltration != null) // 检查控件是否为null
        {
            double newValue = e.NewValue; // 获取滑块的当前值
            scorediff = (int)newValue;
            labelKickInfiltration.Text = $"分差高于{newValue}允许劣势方偷家（设置为0以忽略）"; // 更新标签内容
        }
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
                        // 缓存玩家生涯KD、KPM、技巧值
                        var result = await BF1API.DetailedStatsByPersonaId(Globals.SessionId, item.PersonaId);
                        if (result.IsSuccess)
                        {
                            var detailedStats = JsonHelper.JsonDese<DetailedStats>(result.Content);

                            int kills = detailedStats.result.basicStats.kills;
                            int deaths = detailedStats.result.basicStats.deaths;
                            float kd = PlayerUtil.GetPlayerKD(kills, deaths);
                            float kpm = detailedStats.result.basicStats.kpm;
                            int time = PlayerUtil.GetPlayHours(detailedStats.result.basicStats.timePlayed);
                            float skill = detailedStats.result.basicStats.skill;
                            Globals.LifePlayerCacheDatas.Add(new()
                            {
                                Date = DateTime.Now,
                                Name = item.Name,
                                PersonaId = item.PersonaId,
                                KD = kd,
                                KPM = kpm,
                                Skill = skill,
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
    public static string Mapnameget()
    {   
        return Server.GetMapName();
    }
    /// <summary>
    /// 检查违规玩家线程
    /// </summary>
    private void CheckBreakPlayerThread()
    {
        while (MainWindow.IsAppRunning)

        {   if(Globals.IsEnableKickInfiltration && RobotView.monitoringcount <2)
            {
                Task.Run(() => RobotView.MonitorFlags());
                Thread.Sleep(6000);
                Task.Run(() => RobotView.MonitorFlags());
                
            }
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
            List<PlayerData> playerList = Player.GetPlayerList();
            // 同步等待异步方法完成并获取结果
            playerList = RobotView.GetPlayerListXYZ(playerList).Result;
            var user = playerList.FirstOrDefault(player => player.PersonaId == Globals.PersonaId);
            if (user != null)
            {
                Globals.userteamid = user.TeamId;
               
            }
            else
            {
                Globals.userteamid = -1;
            }

            ScoutListTeacm1 = RemoveDuplicates(ScoutListTeacm1);
            ScoutListTeacm2 = RemoveDuplicates(ScoutListTeacm2);

            // 将 List 转换为 HashSet 以优化查找效率
            HashSet<long> playerIdSet = new HashSet<long>(playerList.Select(p => p.PersonaId));

            // 将队列转换为数组进行迭代
            PlayerData[] scoutArray = ScoutListTeacm1.ToArray();
            ScoutListTeacm1.Clear();  // 清空原队列

            foreach (var player in scoutArray)
            {
                if (playerIdSet.Contains(player.PersonaId))
                {
                    PlayerData matchingPlayer = playerList.FirstOrDefault(p => p.PersonaId == player.PersonaId);
                    if (matchingPlayer.Kit == "ID_M_SCOUT" && matchingPlayer.TeamId == 1)
                    {
                        ScoutListTeacm1.Enqueue(player); // 重新将符合条件的元素加回队列
                    }
                  
                }
            }
            
            // 将队列转换为数组进行迭代
            scoutArray = ScoutListTeacm2.ToArray();
            ScoutListTeacm2.Clear();  // 清空原队列

            foreach (var player in scoutArray)
            {
                if (playerIdSet.Contains(player.PersonaId))
                {
                    PlayerData matchingPlayer = playerList.FirstOrDefault(p => p.PersonaId == player.PersonaId);
                    if (matchingPlayer.Kit == "ID_M_SCOUT" && matchingPlayer.TeamId == 2)
                    {
                        ScoutListTeacm2.Enqueue(player); // 重新将符合条件的元素加回队列
                    }

                }
            }
           
            foreach (var item in playerList)
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
            AddBreakRulePlayerInfo(playerData, BreakType.Kill, $"击杀限制{serverRule.MaxKill:0}");
        }

        // 计算玩家KD最低击杀数
        if (playerData.Kill > serverRule.FlagKD &&
            serverRule.FlagKD != 0)
        {
            // 限制玩家KD
            if (playerData.Kd > serverRule.MaxKD &&
                serverRule.MaxKD != 0.00f)
            {
                AddBreakRulePlayerInfo(playerData, BreakType.KD, $"KD限制{serverRule.MaxKD:0.00}");
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
                AddBreakRulePlayerInfo(playerData, BreakType.KPM, $"KPM限制{serverRule.MaxKPM:0.00}");
            }
        }

        // 限制玩家最低等级
        if (playerData.Rank < serverRule.MinRank &&
            serverRule.MinRank != 0 &&
            playerData.Rank != 0)
        {
            AddBreakRulePlayerInfo(playerData, BreakType.Rank, $"最低等级限制{serverRule.MinRank:0}");
        }

        // 限制玩家最高等级
        if (playerData.Rank > serverRule.MaxRank &&
            serverRule.MaxRank != 0 &&
            playerData.Rank != 0)
        {
            AddBreakRulePlayerInfo(playerData, BreakType.Rank, $"最高等级限制{serverRule.MaxRank:0}");
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
                    AddBreakRulePlayerInfo(playerData, BreakType.Weapon, "限制武器K弹");
                }
            }

            // 步枪手榴弹（破片）
            if (item == "_RGL_Frag")
            {
                if (playerData.WeaponS2.Contains("_RGL_Frag") ||
                    playerData.WeaponS5.Contains("_RGL_Frag"))
                {
                    AddBreakRulePlayerInfo(playerData, BreakType.Weapon, "限制步枪手榴弹 破片");
                }
            }

            // 步枪手榴弹（烟雾）
            if (item == "_RGL_Smoke")
            {
                if (playerData.WeaponS2.Contains("_RGL_Smoke") ||
                    playerData.WeaponS5.Contains("_RGL_Smoke"))
                {
                    AddBreakRulePlayerInfo(playerData, BreakType.Weapon, "限制步枪手榴弹 烟雾");
                }
            }

            // 步枪手榴弹（高爆）
            if (item == "_RGL_HE")
            {
                if (playerData.WeaponS2.Contains("_RGL_HE") ||
                    playerData.WeaponS5.Contains("_RGL_HE"))
                {
                    AddBreakRulePlayerInfo(playerData, BreakType.Weapon, "限制步枪手榴弹 高爆");
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
                AddBreakRulePlayerInfo(playerData, BreakType.Weapon, $"武器限制{ClientHelper.GetWeaponShortTxt(item)}");
            }
        }
        //限制侦察数
        if (playerData.TeamId == 1)
        {
            if (serverRule.MaxScout != 0 && playerData.Kit == "ID_M_SCOUT")
            {
                while (ScoutListTeacm1.Count > serverRule.MaxScout)
                {
                    ScoutListTeacm1.Dequeue(); // 移除队列前端的元素
                }
                if (!(ScoutListTeacm1.Any(p => p.PersonaId == playerData.PersonaId)))
                {
                   
                    ScoutListTeacm1 = RemoveDuplicates(ScoutListTeacm1);
                    if (ScoutListTeacm1.Count < serverRule.MaxScout || playerData.White)
                    {
                        if (ScoutListTeacm1.Count == serverRule.MaxScout)
                        {
                            ScoutListTeacm1.Dequeue(); // 移除队列前端的元素
                        }
                        ScoutListTeacm1.Enqueue(playerData);
                    }
                    else
                    {
                        AddBreakRulePlayerInfo(playerData, BreakType.Scout, $"最多侦察{serverRule.MaxScout:0}");
                    }
                }
            }
        }
        if (playerData.TeamId == 2)
        {    
            if (serverRule.MaxScout != 0 && playerData.Kit == "ID_M_SCOUT")

            {    
                while (ScoutListTeacm2.Count > serverRule.MaxScout)
                {
                    ScoutListTeacm2.Dequeue(); // 移除队列前端的元素
                }
                if (!(ScoutListTeacm2.Any(p => p.PersonaId == playerData.PersonaId)))
                {
                    ScoutListTeacm2 = RemoveDuplicates(ScoutListTeacm2);
                    if (ScoutListTeacm2.Count < serverRule.MaxScout || playerData.White)
                    {
                       if(ScoutListTeacm2.Count == serverRule.MaxScout)
                        {
                            ScoutListTeacm2.Dequeue(); // 移除队列前端的元素
                        }
                        ScoutListTeacm2.Enqueue(playerData);
                    }
                    else
                    {
                        AddBreakRulePlayerInfo(playerData, BreakType.Scout, $"最多侦察{serverRule.MaxScout:0}");
                    }
                }
            }
        }
        //偷家限制
        if (Globals.IsEnableKickInfiltration)
        {
           int flags = 0;//对手占领的旗帜数
            if (Globals.userteamid == 2)
            {
                if (playerData.TeamId == 2)
                {
                    flags = RobotView.team2Flags;//和user同队，对手是team2
                }
                else
                {
                    flags = RobotView.team1Flags;
                }
            }
            else {
                if (playerData.TeamId == 1)
                {
                    flags = RobotView.team2Flags;
                }
                else
                {
                    flags = RobotView.team1Flags;
                }


            }
            if (flags <= 1)
            {
                goto NEXT;//由于某种神奇原因，分数所属方是相对的
            }
            if (scorediff != 0)
            {
                int a = Server.GetTeam1Score() - Server.GetTeam2Score();
                if (Math.Abs(a) > scorediff)
                {

                   
                    a = a > 0 ? 2 : 1;
                    
                    if (playerData.TeamId ==a)
                    {
                        goto NEXT;
                    }
                }
            }
            var name = ClientHelper.GetWeaponChsName(playerData.WeaponS0);
            if (name.Contains("战斗机", StringComparison.OrdinalIgnoreCase) ||
       name.Contains("轰炸机", StringComparison.OrdinalIgnoreCase) ||
       name.Contains("攻击机", StringComparison.OrdinalIgnoreCase))
            {
                goto NEXT;
            }
            var square = new List<PointXZ>();
            if (PolygonStorage.Polygons.TryGetValue(ScoreView.mapname + playerData.TeamId, out square))
            {
                PointXZ point = new PointXZ(playerData.X, playerData.Z);
                if (Polygon.IsPointInPolygon(point, square))
                {
                    AddBreakRulePlayerInfo(playerData, BreakType.Infiltration, "偷家");
                }
                    
            }
            else
            {

                goto NEXT;
            }
        }
    NEXT:;
    }
    public static Queue<PlayerData> RemoveDuplicates(Queue<PlayerData> inputQueue)
    {
        HashSet<long> seenPersonaIds = new HashSet<long>();
        Queue<PlayerData> uniqueQueue = new Queue<PlayerData>();

        int count = inputQueue.Count;
        for (int i = 0; i < count; i++)
        {
            PlayerData player = inputQueue.Dequeue();
            if (seenPersonaIds.Add(player.PersonaId))
            {
                uniqueQueue.Enqueue(player);
            }
        }

        return uniqueQueue;
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
                // 跳过管理员玩家，测试时关闭
                if (item.Admin)
                    continue;

                // 跳过白名单玩家
                if (item.White)
                {
                    if (CheckWhiteBreakRule(item))
                        continue;
                }
                // 定义需要检查的 BreakType 集合
                var breakTypesToCheck = new HashSet<BreakType>
               {
            BreakType.Spectator,
            BreakType.Black,
            BreakType.CD,
            BreakType.NoWhite,
            BreakType.LifeKD,
            BreakType.LifeKPM,
            BreakType.LifeWeaponStar,
            BreakType.LifeVehicleStar,
            BreakType.Kill,
            BreakType.KD,
            BreakType.KPM,
            BreakType.Rank
                };
                // 检查是否有是其他的 BreakType 项
                bool hasSpecificViolation = item.BreakInfos.Any(b => breakTypesToCheck.Contains(b.BreakType));
                if (!hasSpecificViolation && Delaykick)
                {
                    // 先检查踢出玩家是否在 延时踢人 列表中
                    var index2 = DelayKicking_PlayerList.FindIndex(var => var.PersonaId == item.PersonaId);
                    if (index2 == -1)
                    {
                        DelayKicking_PlayerList.Add(new()
                        {
                            Rank = item.Rank,
                            Name = item.Name,
                            PersonaId = item.PersonaId,
                            Reason = item.Reason,
                            Flag = KickFlag.Default,
                            State = "准备踢人",
                            Time = DateTime.Now
                        });
                        ChatInputWindow.SendChsToBF1Chat($"玩家{item.Name}请注意，您已违规{item.Reason}");
                    }
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
            
                DelayKicking_PlayerList.RemoveAll(item =>
                {
                    var index = Globals.BreakRuleInfo_PlayerList.FindIndex(p => p.PersonaId == item.PersonaId);
                    
                    return index == -1; // 移除不再违规的玩家
                    
                });
                     
            var nowtime = DateTime.Now;
            // 执行踢人请求
            foreach (var item in Kicking_PlayerList)
            {
                if (item.Flag == KickFlag.Default)
                {
                    AutoKickPlayer(item);



                }
            }
            int delaytime = 40;
                Application.Current.Dispatcher.Invoke(() =>
                {
            delaytime = (int)(sliderDelaykick != null ? sliderDelaykick.Value : 40);
            });
            foreach (var item in DelayKicking_PlayerList)
            {
                if (item.Flag == KickFlag.Default && (nowtime - item.Time).TotalSeconds > delaytime)
                {
                    AutoKickPlayer(item);
                   // MessageBox.Show("踢人成功");
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
            for (int i = DelayKicking_PlayerList.Count - 1; i >= 0; i--)
            {
                if (DelayKicking_PlayerList.Count != 0)
                {
                    // 如果踢人失败，立刻移除列表（准备重新踢出）
                    if (DelayKicking_PlayerList[i].Flag == KickFlag.Faild)
                    {
                        DelayKicking_PlayerList.RemoveAt(i);
                        break;
                    }

                    // 如果踢人成功，KickDelay 秒后移除列表（避免重复踢出）
                    if (DelayKicking_PlayerList[i].Flag == KickFlag.Success && MiscUtil.DiffSeconds(DelayKicking_PlayerList[i].Time, DateTime.Now) > KickDelay)
                    {
                        DelayKicking_PlayerList.RemoveAt(i);
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

        var result = await BF1API.RSPKickPlayer(Globals.SessionId, Globals.GameId, info.PersonaId, ChsUtil.ToTraditional(info.Reason));
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
        if (!Globals.LoginPlayerIsAdmin)//测试时改为true
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
    private void CheckBox_IsEnableDelaykick_Click(object sender, RoutedEventArgs e)
    {
        Delaykick = CheckBox_IsEnableDelaykick.IsChecked == true;
    }
    private bool Delaykick = false;
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
    /// 启用踢出偷家玩家
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CheckBox_IsEnableKickInfiltration_Click(object sender, RoutedEventArgs e)
    {
        Globals.IsEnableKickInfiltration = CheckBox_IsEnableKickInfiltration.IsChecked == true;
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
public static class PolygonStorage
{
    // 在静态构造函数中初始化
    public static readonly Dictionary<string, List<PointXZ>> Polygons;

    static PolygonStorage()
    {
        Polygons = new Dictionary<string, List<PointXZ>>
        {
            { "亚眠1",new List<PointXZ>
{
    new PointXZ(-157.8593, -74.06298),
    new PointXZ(-204.1108, -74.19588),
    new PointXZ(-204.1108, -79.57462),
    new PointXZ(-210.9863, -85.59748),
    new PointXZ(-216.2681, -91.93426),
    new PointXZ(-218.4537, -98.49104),
    new PointXZ(-218.4102, -105.5275),
    new PointXZ(-215.4151, -114.2166),
    new PointXZ(-207.2409, -120.9942),
    new PointXZ(-197.4154, -122.5758),
    new PointXZ(-172.6279, -122.5293),
    new PointXZ(-166.8493, -120.3848),
    new PointXZ(-161.1016, -115.5021),
    new PointXZ(-157.5349, -110.6351),
    new PointXZ(-156.4903, -103.6711),
    new PointXZ(-158.182, -95.50179),
    new PointXZ(-157.8593, -78.78714)
}
            },
            { "亚眠2", new List<PointXZ>
                {
                  new PointXZ(134.5433, 78.76258),
                new PointXZ(127.4218, 72.04205),
                 new PointXZ(125.0461, 74.50394),
                 new PointXZ(117.8956, 68.51612),
                 new PointXZ(110.9195, 61.24934),
                  new PointXZ(119.6977, 53.05249),
                 new PointXZ(130.627, 41.6),
                 new PointXZ(137.2542, 34.62389),
                new PointXZ(151.5555, 47.87855),
            new PointXZ(158.4734, 54.56398),
            new PointXZ(151.4973, 61.07507),
            new PointXZ(144.2887, 68.45811)
                }
            },
            { "西奈沙漠1", new List<PointXZ>
                {
                  new PointXZ(127.4288, -291.3338),
    new PointXZ(129.3532, -303.6458),
    new PointXZ(135.2214, -309.7542),
    new PointXZ(143.4882, -310.3039),
    new PointXZ(158.4643, -304.5773),
    new PointXZ(173.4821, -292.5955),
    new PointXZ(213.1074, -253.9091),
    new PointXZ(212.6949, -238.0219),
    new PointXZ(195.8116, -208.4712),
    new PointXZ(185.1035, -193.9815),
    new PointXZ(168.777, -190.9601),
    new PointXZ(152.7021, -193.1383),
    new PointXZ(136.5691, -200.331),
    new PointXZ(120.1944, -216.4745),
    new PointXZ(110.3684, -234.182),
    new PointXZ(110.8479, -245.2134),
    new PointXZ(113.6361, -267.3323)
                }
            },
            { "西奈沙漠2", new List<PointXZ>
                {
    new PointXZ(-299.9632, 6.819366),
    new PointXZ(-309.1942, 22.22291),
    new PointXZ(-309.3345, 44.21414),
    new PointXZ(-310.1098, 56.25326),
    new PointXZ(-309.2767, 59.04176),
    new PointXZ(-307.3475, 70.4396),
    new PointXZ(-298.0879, 79.10294),
    new PointXZ(-284.5687, 82.06926),
    new PointXZ(-257.9952, 83.46057),
    new PointXZ(-242.3254, 79.21498),
    new PointXZ(-238.0269, 63.95376),
    new PointXZ(-239.6793, 44.80589),
    new PointXZ(-246.5193, 23.41382),
    new PointXZ(-260.8078, 5.778769),
    new PointXZ(-283.1823, 2.6236)
                }
            },

             { "流血宴厅1", new List<PointXZ>
                {
     new PointXZ(-43.34179, 281.0894),
            new PointXZ(-36.12308, 273.1068),
            new PointXZ(-33.536, 268.3414),
            new PointXZ(-32.14036, 263.8481),
            new PointXZ(-34.34614, 255.2076),
            new PointXZ(-39.45997, 249.811),
            new PointXZ(-47.4604, 247.2668),
            new PointXZ(-60.77138, 259.6927),
            new PointXZ(-74.34989, 244.7336),
            new PointXZ(-93.46057, 261.9744),
            new PointXZ(-86.32526, 269.8766),
            new PointXZ(-102.5865, 284.9587),
            new PointXZ(-96.31116, 291.9237),
            new PointXZ(-108.2275, 302.7136),
            new PointXZ(-94.39954, 317.4596),
            new PointXZ(-83.06418, 312.694),
            new PointXZ(-72.85217, 306.8051)
                }
            },
             { "流血宴厅2", new List<PointXZ>
                {
      new PointXZ(216.6636, 641.6022),
    new PointXZ(178.4074, 618.0181),
    new PointXZ(193.4844, 594.0626),
    new PointXZ(231.1473, 617.5275)
                }
            },
             { "法欧堡1", new List<PointXZ>
                {
     new PointXZ(-41.66341, -185.8102),
    new PointXZ(-52.39787, -185.0735),
    new PointXZ(-59.23846, -181.9163),
    new PointXZ(-63.0271, -176.549),
    new PointXZ(-64.71092, -170.5504),
    new PointXZ(-64.50046, -162.7626),
    new PointXZ(-62.92185, -156.6587),
    new PointXZ(-60.08036, -152.9753),
    new PointXZ(-55.55506, -150.4496),
    new PointXZ(-32.82327, -145.6085),
    new PointXZ(-15.03775, -143.3985),
    new PointXZ(-0.6198921, -145.2928),
    new PointXZ(9.588367, -150.8705),
    new PointXZ(15.06085, -157.7111),
    new PointXZ(17.57541, -168.2368),
    new PointXZ(15.72801, -188.9051),
    new PointXZ(11.7984, -194.6503),
    new PointXZ(4.115902, -198.9651),
    new PointXZ(-3.250881, -199.7018),
    new PointXZ(-10.72291, -196.7551),
    new PointXZ(-22.93072, -190.5459)
                }
            },
             { "法欧堡2", new List<PointXZ>
                {
     new PointXZ(-55.6541, 312.035),
    new PointXZ(-52.51234, 319.9722),
    new PointXZ(-48.32333, 324.0509),
    new PointXZ(-42.34209, 325.7734),
    new PointXZ(-12.593, 323.222),
    new PointXZ(1.439259, 320.8258),
    new PointXZ(9.2661, 316.0856),
    new PointXZ(14.83308, 309.1958),
    new PointXZ(16.92759, 300.6524),
    new PointXZ(13.09322, 283.2747),
    new PointXZ(9.496061, 274.72),
    new PointXZ(0.8424435, 268.9326),
    new PointXZ(-9.685209, 266.838),
    new PointXZ(-20.7799, 267.1519),
    new PointXZ(-50.58988, 270.8093),
    new PointXZ(-66.18832, 272.2309),
    new PointXZ(-72.12514, 274.4073),
    new PointXZ(-73.54498, 278.9317),
    new PointXZ(-73.01692, 284.0966),
    new PointXZ(-71.13187, 288.2905),
    new PointXZ(-68.77564, 289.2329),
    new PointXZ(-66.29198, 299.799)
                }
            },
             { "阿尔贡森林1", new List<PointXZ>
                {
     new PointXZ(-600.6421, -463.0871),
    new PointXZ(-597.0009, -463.02),
    new PointXZ(-597.0749, -457.6686),
    new PointXZ(-563.6693, -457.3614),
    new PointXZ(-563.6693, -475.2),
    new PointXZ(-569.92, -475.2),
    new PointXZ(-570.0037, -478.4),
    new PointXZ(-600.5983, -478.6252)
                }
            },
             { "阿尔贡森林2", new List<PointXZ>
                {
     new PointXZ(-713.11, -123.7588),
    new PointXZ(-739.0392, -123.2166),
    new PointXZ(-744.5962, -132.4395),
    new PointXZ(-743.9664, -150.1463),
    new PointXZ(-711.2338, -151.1031),
    new PointXZ(-702.9702, -138.7315),
    new PointXZ(-706.1181, -129.1171)
                }
            },
              { "帝国边境1", new List<PointXZ>
                {
     new PointXZ(-19.46804, -362.1578),
    new PointXZ(-17.53249, -371.585),
    new PointXZ(-9.164639, -377.4295),
    new PointXZ(-5.791021, -380.8425),
    new PointXZ(-1.352278, -383.0907),
    new PointXZ(3.893514, -383.4366),
    new PointXZ(8.556148, -382.5496),
    new PointXZ(14.50246, -373.6026),
    new PointXZ(27.4048, -370.7136),
    new PointXZ(28.47727, -376.1686),
    new PointXZ(36.01294, -375.0288),
    new PointXZ(40.12907, -372.3691),
    new PointXZ(43.04202, -369.2662),
    new PointXZ(49.5645, -359.9574),
    new PointXZ(37.40609, -351.7252),
    new PointXZ(19.48511, -355.2714),
    new PointXZ(17.56517, -349.3019),
    new PointXZ(-2.098161, -353.3492),
    new PointXZ(-1.510916, -358.5103)
                }
            },
              { "帝国边境2", new List<PointXZ>
                {
    new PointXZ(82.52895, 256.7959),
    new PointXZ(78.91, 256.6786),
    new PointXZ(71.68349, 254.5623),
    new PointXZ(66.08, 255.642),
    new PointXZ(60.65768, 254.5224),
    new PointXZ(56.90479, 248.737),
    new PointXZ(51.89681, 247.2261),
    new PointXZ(45.85271, 248.3918),
    new PointXZ(41.40598, 248.3486),
    new PointXZ(37.69318, 246.19),
    new PointXZ(32.94425, 241.2252),
    new PointXZ(29.83585, 234.9652),
    new PointXZ(27.59078, 226.9557),
    new PointXZ(26.70493, 219.6273),
    new PointXZ(26.86599, 215.3189),
    new PointXZ(28.07397, 212.9029),
    new PointXZ(34.54172, 209.623),
    new PointXZ(37.89895, 209.0775),
    new PointXZ(42.52954, 207.9098),
    new PointXZ(52.84669, 202.3271),
    new PointXZ(61.91283, 203.4064),
    new PointXZ(69.89968, 206.817),
    new PointXZ(75.85598, 212.1831),
    new PointXZ(79.52705, 218.5598),
    new PointXZ(81.03807, 224.6471),
    new PointXZ(80.52006, 229.3093),
    new PointXZ(79.31123, 233.756),
    new PointXZ(79.49586, 237.7435),
    new PointXZ(81.34853, 242.5525),
    new PointXZ(84.81708, 247.4218),
    new PointXZ(85.1395, 251.2407),
    new PointXZ(84.57826, 254.4354)
                }
            },
               { "格拉巴山1", new List<PointXZ>
                {
    new PointXZ(-71.09334, -14.88103),
            new PointXZ(-64.20732, -18.31889),
            new PointXZ(-52.53026, -25.91638),
            new PointXZ(-50.04155, -27.91756),
            new PointXZ(-46.93559, -28.64086),
            new PointXZ(-43.95726, -28.21539),
            new PointXZ(-32.16994, -11.74874),
            new PointXZ(-29.91636, -6.984195),
            new PointXZ(-26.123, -1.401833),
            new PointXZ(-32.04955, 1.83156),
            new PointXZ(-37.41314, 5.313141),
            new PointXZ(-40.89368, 0.9721856),
            new PointXZ(-45.55651, 1.082317),
            new PointXZ(-47.93929, 3.666055),
            new PointXZ(-51.46873, 6.067661),
            new PointXZ(-54.38233, 15.19916),
            new PointXZ(-72.49471, 8.971189),
            new PointXZ(-72.68684, 1.433404),
            new PointXZ(-75.38795, -6.060931),
            new PointXZ(-77.32745, -13.08007)
                }
            },
               { "格拉巴山2", new List<PointXZ>
                {
      new PointXZ(388.3286, 229.2269),
            new PointXZ(383.2146, 227.8949),
            new PointXZ(368.4135, 220.206),
            new PointXZ(359.8127, 215.179),
            new PointXZ(371.3225, 197.196),
            new PointXZ(371.3284, 193.3504),
            new PointXZ(373.3912, 188.3654),
            new PointXZ(377.4308, 185.701),
            new PointXZ(381.3845, 185.9588),
            new PointXZ(405.0777, 201.3277),
            new PointXZ(406.7671, 204.1273),
            new PointXZ(407.0567, 207.8923),
            new PointXZ(404.7398, 212.2847),
            new PointXZ(401.5541, 214.4568),
            new PointXZ(397.6927, 215.3256),
            new PointXZ(392.2866, 223.1451)
                }
            },
               { "圣康坦的伤痕1", new List<PointXZ>
                {
     new PointXZ(-413.9534, -32.05938),
    new PointXZ(-420.2072, -52.43597),
    new PointXZ(-421.6664, -58.16853),
    new PointXZ(-420.8326, -66.14198),
    new PointXZ(-416.6261, -71.00588),
    new PointXZ(-417.2072, -74.30483),
    new PointXZ(-397.2165, -81.27594),
    new PointXZ(-391.1445, -82.8429),
    new PointXZ(-384.7536, -81.54526),
    new PointXZ(-378.8527, -79.12125),
    new PointXZ(-369.01, -76.50134),
    new PointXZ(-363.3051, -73.48976),
    new PointXZ(-362.2277, -69.86598),
    new PointXZ(-364.4069, -63.89164),
    new PointXZ(-366.8799, -55.2974),
    new PointXZ(-366.2815, -45.77115),
    new PointXZ(-367.8909, -38.69941),
    new PointXZ(-370.339, -32.51805),
    new PointXZ(-374.3348, -27.4886),
    new PointXZ(-376.1016, -24.62221),
    new PointXZ(-377.7082, -22.86888),
    new PointXZ(-379.6572, -22.04614),
    new PointXZ(-384.504, -20.87023),
    new PointXZ(-390.3542, -19.82857),
    new PointXZ(-392.9048, -19.21335),
    new PointXZ(-395.8272, -18.96014),
    new PointXZ(-399.4502, -18.18139),
    new PointXZ(-402.4309, -17.79683),
    new PointXZ(-411.3398, -18.0666)
                }
            },
               { "圣康坦的伤痕2", new List<PointXZ>
                {
    new PointXZ(59.84, -230.88),
    new PointXZ(47.68, -242.72),
    new PointXZ(45.99462, -246.4107),
    new PointXZ(43.02742, -247.6432),
    new PointXZ(40.05703, -246.4165),
    new PointXZ(38.82513, -243.4478),
    new PointXZ(40.05502, -240.4798),
    new PointXZ(35.2, -234.08),
    new PointXZ(30.24, -235.52),
    new PointXZ(25.12, -234.88),
    new PointXZ(21.12, -232.48),
    new PointXZ(17.92, -228.48),
    new PointXZ(16.64, -223.52),
    new PointXZ(17.44, -218.56),
    new PointXZ(20, -214.24),
    new PointXZ(25.46226, -210.1822),
    new PointXZ(31.66772, -209.17),
    new PointXZ(49.88817, -195.8347),
    new PointXZ(56.18165, -194.5585),
    new PointXZ(62.03507, -195.0866),
    new PointXZ(65.99604, -197.5512),
    new PointXZ(68.16, -201.44),
    new PointXZ(68.16, -205.28),
    new PointXZ(71.84, -205.44),
    new PointXZ(71.84, -211.52),
    new PointXZ(68.32, -211.52),
    new PointXZ(68.16, -217.92),
    new PointXZ(66.88, -221.12),
    new PointXZ(64.89566, -224.2657),
    new PointXZ(62.38705, -226.7744),
    new PointXZ(62.72, -229.28)
                }
            },
                { "苏伊士1", new List<PointXZ>
                {
    new PointXZ(-12.39786, -185.6545),
    new PointXZ(-9.682695, -181.7202),
    new PointXZ(-2.150299, -175.297),
    new PointXZ(-7.571476, -148.3143),
    new PointXZ(-6.955433, -143.5708),
    new PointXZ(-4.842518, -140.4043),
    new PointXZ(-12.30751, -124.171),
    new PointXZ(-16.34512, -114.9133),
    new PointXZ(-19.85015, -110.4371),
    new PointXZ(-27.4644, -113.6058),
    new PointXZ(-30.82301, -113.5236),
    new PointXZ(-33.88384, -117.1477),
    new PointXZ(-34.11795, -119.109),
    new PointXZ(-38.41348, -125.5608),
    new PointXZ(-38.32906, -128.9402),
    new PointXZ(-40.67927, -131.0623),
    new PointXZ(-41.31458, -132.1726),
    new PointXZ(-41.43103, -133.1142),
    new PointXZ(-41.51739, -133.9572),
    new PointXZ(-48.34174, -138.6367),
    new PointXZ(-56.30471, -137.7816),
    new PointXZ(-57.40586, -149.3055),
    new PointXZ(-54.13529, -159.3825),
    new PointXZ(-47.12942, -172.3822),
    new PointXZ(-42.99389, -170.0604),
    new PointXZ(-17.84879, -187.5947)
                }
            },
                { "苏伊士2", new List<PointXZ>
                {
   new PointXZ(-310.5228, 169.3594),
    new PointXZ(-310.1339, 166.916),
    new PointXZ(-304.172, 168.092),
    new PointXZ(-303.4831, 164.8581),
    new PointXZ(-300.8362, 165.5969),
    new PointXZ(-301.8578, 170.3437),
    new PointXZ(-296.8073, 171.6908),
    new PointXZ(-285.1361, 181.3876),
    new PointXZ(-285.8797, 191.2593),
    new PointXZ(-286.6209, 196.1947),
    new PointXZ(-281.5636, 196.8071),
    new PointXZ(-282.4841, 212.2311),
    new PointXZ(-287.6035, 212.085),
    new PointXZ(-287.7664, 217.1622),
    new PointXZ(-292.5524, 216.875),
    new PointXZ(-292.6103, 212.5075),
    new PointXZ(-311.3927, 213.1142),
    new PointXZ(-311.3721, 220.9336),
    new PointXZ(-319.1409, 220.9812),
    new PointXZ(-318.5883, 218.2139),
    new PointXZ(-313.8645, 218.195),
    new PointXZ(-313.9977, 212.9506),
    new PointXZ(-315.3088, 213.1372),
    new PointXZ(-314.9917, 207.4445),
    new PointXZ(-313.5741, 207.6133),
    new PointXZ(-312.9563, 202.5136),
    new PointXZ(-324.0845, 199.7319),
    new PointXZ(-322.035, 191.1803),
    new PointXZ(-321.2929, 187.8521),
    new PointXZ(-319.7409, 182.825),
    new PointXZ(-307.747, 185.124),
    new PointXZ(-306.2829, 177.2881),
    new PointXZ(-304.9272, 170.2267)
                }
            },
                { "庞然暗影1", new List<PointXZ>
                {
     new PointXZ(38.2391, 468.5313),
            new PointXZ(38.2391, 455.8116),
            new PointXZ(34.60379, 455.7359),
            new PointXZ(34.58929, 447.9088),
            new PointXZ(34.14936, 427.2594),
            new PointXZ(38.08762, 427.3351),
            new PointXZ(38.17211, 414.4742),
            new PointXZ(45.38264, 414.5643),
            new PointXZ(45.29251, 406.6327),
            new PointXZ(53.67476, 406.6327),
            new PointXZ(53.76489, 410.7788),
            new PointXZ(58.18134, 410.7788),
            new PointXZ(58.18134, 409.7874),
            new PointXZ(78.91576, 409.9676),
            new PointXZ(78.95259, 479.8736),
            new PointXZ(45.81434, 479.6495),
            new PointXZ(45.58549, 468.5354)
                }
            },
                { "庞然暗影2", new List<PointXZ>
                {
     new PointXZ(-272.8592, -60.30238),
    new PointXZ(-269.6509, -39.65873),
    new PointXZ(-258.1925, -41.95834),
    new PointXZ(-258.3116, -43.22708),
    new PointXZ(-245.5845, -45.48706),
    new PointXZ(-245.0294, -42.71166),
    new PointXZ(-242.1657, -43.2778),
    new PointXZ(-239.6952, -43.30458),
    new PointXZ(-229.5447, -42.12303),
    new PointXZ(-224.3619, -41.37111),
    new PointXZ(-214.2194, -40.30646),
    new PointXZ(-211.5657, -39.70808),
    new PointXZ(-210.9352, -41.69334),
    new PointXZ(-208.3304, -40.91456),
    new PointXZ(-206.4776, -48.1381),
    new PointXZ(-204.2486, -58.0738),
    new PointXZ(-199.469, -78.34792),
    new PointXZ(-214.2114, -82.64445),
    new PointXZ(-224.362, -83.53066),
    new PointXZ(-239.6954, -81.75828),
    new PointXZ(-245.281, -80.63041),
    new PointXZ(-257.7142, -77.64965),
    new PointXZ(-273.2353, -75.66254),
    new PointXZ(-271.3019, -60.54413)
                }
            },

                { "苏瓦松1", new List<PointXZ>
                {
     new PointXZ(41.35382, 244.6657),
    new PointXZ(21.9041, 277.2108),
    new PointXZ(39.4849, 287.6525),
    new PointXZ(34.3854, 296.1001),
    new PointXZ(39.3623, 299.1168),
    new PointXZ(44.60684, 290.4081),
    new PointXZ(62.38516, 301.5926),
    new PointXZ(81.58546, 269.0146),
    new PointXZ(71.68, 255.68),
    new PointXZ(58.24, 247.68)
                }
            },
                { "苏瓦松2", new List<PointXZ>
                {
     new PointXZ(438.9762, 437.3568),
    new PointXZ(433.4582, 437.6076),
    new PointXZ(429.0688, 438.2974),
    new PointXZ(424.3032, 441.1191),
    new PointXZ(421.1681, 444.1917),
    new PointXZ(409.2542, 461.5615),
    new PointXZ(412.2891, 463.9972),
    new PointXZ(421.1055, 470.2787),
    new PointXZ(423.185, 467.927),
    new PointXZ(436.9164, 477.2486),
    new PointXZ(449.3231, 458.6143),
    new PointXZ(455.9698, 449.7729)
                }
            },
                { "凡尔登高地1", new List<PointXZ>
                {
     new PointXZ(-79.0471, -286.4107),
    new PointXZ(-76.61303, -280.6233),
    new PointXZ(-71.57761, -274.4096),
    new PointXZ(-54.99577, -258.5224),
    new PointXZ(-46.27412, -265.9547),
    new PointXZ(-39.76122, -272.9375),
    new PointXZ(-48.2881, -296.19),
    new PointXZ(-43.50587, -297.7731),
    new PointXZ(-47.16768, -307.8861),
    new PointXZ(-51.23514, -306.315),
    new PointXZ(-54.25474, -305.2069),
    new PointXZ(-53.12498, -301.8858),
    new PointXZ(-57.42794, -300.4148),
    new PointXZ(-64.10712, -300.5303),
    new PointXZ(-78.82099, -300.2572),
    new PointXZ(-78.33869, -296.4372),
    new PointXZ(-81.77566, -296.0291),
    new PointXZ(-82.82479, -296.5808),
    new PointXZ(-85.77998, -296.4368),
    new PointXZ(-87.95576, -294.3178),
    new PointXZ(-87.54083, -291.2265),
    new PointXZ(-85.23232, -289.1013),
    new PointXZ(-82.19446, -289.4032)
                }
            },
                 { "凡尔登高地2", new List<PointXZ>
                {
     new PointXZ(-102.0239, 17.67276),
    new PointXZ(-102.0644, 30.44587),
    new PointXZ(-102.0579, 44.62736),
    new PointXZ(-70.84513, 44.50038),
    new PointXZ(-70.38344, 39.0011),
    new PointXZ(-71.43312, 26.45836),
    new PointXZ(-72.48309, 26.33134),
    new PointXZ(-73.20654, 16.39371),
    new PointXZ(-80.80862, 17.10832),
    new PointXZ(-80.85754, 19.46895),
    new PointXZ(-89.45165, 19.47481),
    new PointXZ(-89.52676, 18.12625)
                }
            },
                 { "攻占托尔1排除", new List<PointXZ>
                {
    new PointXZ(617.9616, -201.1725),
    new PointXZ(617.9179, -190.0231),
    new PointXZ(618.0513, -187.5644),
    new PointXZ(617.277, -187.3708),
    new PointXZ(617.7057, -174.6967),
    new PointXZ(617.1245, -174.7104),
    new PointXZ(617.1729, -166.9969),
    new PointXZ(617.8813, -166.9772),
    new PointXZ(617.857, -161.5863),
    new PointXZ(621.3621, -155.9383),
    new PointXZ(624.6341, -152.5765),
    new PointXZ(624.6804, -149.0789),
    new PointXZ(637.4014, -149.1842),
    new PointXZ(637.3464, -152.2165),
    new PointXZ(655.7363, -152.1682),
    new PointXZ(655.8192, -179.8304),
    new PointXZ(657.2685, -179.8566),
    new PointXZ(657.135, -201.5626)
                }
            },
                  { "攻占托尔2排除", new List<PointXZ>
                {
    new PointXZ(399.3151, -135.7858),
            new PointXZ(410.3532, -136.1517),
            new PointXZ(420.3916, -136.1344),
            new PointXZ(425.5804, -137.1471),
            new PointXZ(425.6036, -121.9473),
            new PointXZ(429.6379, -122.0864),
            new PointXZ(429.1517, -112.6403),
            new PointXZ(436.1455, -113.0824),
            new PointXZ(435.1096, -87.62035),
            new PointXZ(425.3565, -87.72729),
            new PointXZ(424.4908, -90.47976),
            new PointXZ(414.3787, -90.31587),
            new PointXZ(413.9987, -88.54868),
            new PointXZ(399.8806, -88.34236),
            new PointXZ(397.4249, -97.24622),
            new PointXZ(398.9713, -109.6841),
            new PointXZ(399.0521, -129.8992)
                }
            },
                   { "阿尔比恩1", new List<PointXZ>
                {
    new PointXZ(239.3595, -162.1956),
            new PointXZ(238.5971, -77.34825),
            new PointXZ(244.9144, -73.42719),
            new PointXZ(298.72, -73.20936),
            new PointXZ(304, -83.2),
            new PointXZ(304.6014, -101.9638),
            new PointXZ(297.4128, -107.7365),
            new PointXZ(294.3631, -112.0932),
            new PointXZ(294.2542, -129.0844),
            new PointXZ(291.4223, -147.6005),
            new PointXZ(290.551, -165.681),
            new PointXZ(247.5283, -165.1364)
                }
            },
                   { "阿尔比恩2", new List<PointXZ>
                {
    new PointXZ(-197.0299, -227.9773),
            new PointXZ(-192.4307, -237.9144),
            new PointXZ(-192.6672, -258.1276),
            new PointXZ(-189.8194, -262.3661),
            new PointXZ(-187.6288, -267.3746),
            new PointXZ(-187.0678, -274.5209),
            new PointXZ(-191.7554, -278.2153),
            new PointXZ(-196.1648, -281.2344),
            new PointXZ(-208.2569, -264.3522),
            new PointXZ(-209.4093, -256.7708),
            new PointXZ(-218.3209, -237.6201)
                }
            },
                    { "武普库夫山口1", new List<PointXZ>
                {
     new PointXZ(171.1704, 76.21664),
            new PointXZ(162.8557, 71.69167),
            new PointXZ(145.5477, 77.17819),
            new PointXZ(144.4561, 77.48167),
            new PointXZ(142.4173, 78.77911),
            new PointXZ(141.4288, 80.97239),
            new PointXZ(141.4906, 83.07299),
            new PointXZ(143.1896, 90.11621),
            new PointXZ(148.255, 99.586),
            new PointXZ(161.4759, 110.4681),
            new PointXZ(178.9923, 123.8321),
            new PointXZ(194.0278, 103.8747),
            new PointXZ(177.419, 91.92274)
                }
            },
                     { "武普库夫山口2", new List<PointXZ>
                {
    new PointXZ(-217.9019, 422.9188),
    new PointXZ(-218.0191, 434.9912),
    new PointXZ(-216.261, 439.5037),
    new PointXZ(-213.5067, 442.5511),
    new PointXZ(-209.6637, 443.1751),
    new PointXZ(-201.1999, 444.6609),
    new PointXZ(-198.1525, 443.5474),
    new PointXZ(-195.6912, 441.1446),
    new PointXZ(-193.6401, 438.097),
    new PointXZ(-187.7211, 435.6356),
    new PointXZ(-184.0877, 425.3214),
    new PointXZ(-181.9778, 423.4463),
    new PointXZ(-181.1574, 420.1058),
    new PointXZ(-179.9853, 418.0547),
    new PointXZ(-180.1611, 416.0622),
    new PointXZ(-180.8645, 414.6555),
    new PointXZ(-183.6188, 411.9599),
    new PointXZ(-185.2011, 409.8502),
    new PointXZ(-185.963, 406.5684),
    new PointXZ(-186.9593, 398.5395),
    new PointXZ(-189.362, 396.723),
    new PointXZ(-192.761, 395.4923),
    new PointXZ(-197.3907, 395.0234),
    new PointXZ(-201.1999, 394.8476),
    new PointXZ(-209.6974, 396.4885),
    new PointXZ(-214.7373, 398.9499),
    new PointXZ(-218.4294, 401.6457),
    new PointXZ(-220.4805, 405.6308)
                }
            },
                     { "加利西亚1", new List<PointXZ>
                {
     new PointXZ(-355.9662, -248.0177),
    new PointXZ(-355.4675, -256.903),
    new PointXZ(-365.7208, -261.184),
    new PointXZ(-376.412, -259.7119),
    new PointXZ(-390.6589, -252.7884),
    new PointXZ(-399.4135, -239.0312),
    new PointXZ(-400.278, -232.3502),
    new PointXZ(-392.3291, -202.5456),
    new PointXZ(-385.1535, -201.0361),
    new PointXZ(-366.5647, -203.8874),
    new PointXZ(-358.86, -209.2437),
    new PointXZ(-354.9276, -216.9808),
    new PointXZ(-351.9857, -226.4999)
                }
            },
                     { "加利西亚2", new List<PointXZ>
                {
     new PointXZ(86.5972, -232.3171),
    new PointXZ(77.3459, -229.6835),
    new PointXZ(68.63165, -207.2883),
    new PointXZ(70.03009, -199.538),
    new PointXZ(84.38972, -190.9674),
    new PointXZ(97.56821, -194.1102),
    new PointXZ(101.4306, -205.3983),
    new PointXZ(110.1449, -210.8842),
    new PointXZ(114.4265, -219.8826),
    new PointXZ(97.13464, -229.4282)
                }
            },
                      { "阿奇巴巴2", new List<PointXZ>
                {
     new PointXZ(-456.3989, 411.8885),
    new PointXZ(-451.8279, 419.7776),
    new PointXZ(-442.2655, 426.003),
    new PointXZ(-430.5713, 431.136),
    new PointXZ(-428.3664, 433.2032),
    new PointXZ(-424.8178, 434.099),
    new PointXZ(-414.4476, 430.8949),
    new PointXZ(-411.6725, 427.9691),
    new PointXZ(-411.6168, 419.8232),
    new PointXZ(-416.9035, 418.8526),
    new PointXZ(-416.5406, 416.1446),
    new PointXZ(-418.6389, 410.9817),
    new PointXZ(-433.3482, 400.0781),
    new PointXZ(-445.1945, 395.5803),
    new PointXZ(-454.6343, 399.762)
                }
            },
                       { "阿奇巴巴1", new List<PointXZ>
                {
    new PointXZ(-285.445, 214.3831),
    new PointXZ(-291.9875, 219.1827),
    new PointXZ(-290.6288, 230.0232),
    new PointXZ(-287.6634, 235.0487),
    new PointXZ(-286.7854, 238.1922),
    new PointXZ(-278.5312, 251.274),
    new PointXZ(-268.4159, 251.8669),
    new PointXZ(-251.0144, 254.8147),
    new PointXZ(-244.9786, 255.4267),
    new PointXZ(-240.165, 253.6078),
    new PointXZ(-237.7343, 251.2586),
    new PointXZ(-236.1888, 247.9423),
    new PointXZ(-236.0761, 243.9662),
    new PointXZ(-237.3479, 239.7965),
    new PointXZ(-241.1022, 235.5812),
    new PointXZ(-242.9534, 230.9293),
    new PointXZ(-246.7683, 227.388),
    new PointXZ(-251.0502, 223.4923),
    new PointXZ(-255.1225, 222.3173),
    new PointXZ(-260.3699, 218.6632),
    new PointXZ(-260.8036, 215.5402),
    new PointXZ(-262.3971, 213.2224),
    new PointXZ(-266.7262, 207.3966),
    new PointXZ(-272.0531, 205.4326),
    new PointXZ(-277.059, 204.6761),
    new PointXZ(-280.4067, 209.6004)
                }
            },
                       { "帕斯尚尔1", new List<PointXZ>
                {
    new PointXZ(38.92105, -797.9609),
    new PointXZ(36.7062, -789.9873),
    new PointXZ(31.2299, -784.3867),
    new PointXZ(18.8181, -777.0086),
    new PointXZ(-12.10933, -793.9835),
    new PointXZ(-14.08606, -796.0796),
    new PointXZ(-14.71926, -802.56),
    new PointXZ(-2.913317, -818.4959),
    new PointXZ(4.532556, -817.7887),
    new PointXZ(7.617195, -818.3741),
    new PointXZ(11.87309, -820.5579),
    new PointXZ(21.14245, -817.5559),
    new PointXZ(31.01701, -812.8184),
    new PointXZ(32.66022, -810.1046),
    new PointXZ(35.79606, -807.3509),
    new PointXZ(39.08376, -806.4113)
                }
            },
                        { "帕斯尚尔2", new List<PointXZ>
                {
   new PointXZ(-106.8317, -549.345),
            new PointXZ(-97.16092, -544.2563),
            new PointXZ(-103.0847, -532.7611),
            new PointXZ(-97.92482, -529.6362),
            new PointXZ(-99.22354, -527.2775),
            new PointXZ(-97.45479, -521.2642),
            new PointXZ(-94.36625, -519.5118),
            new PointXZ(-88.32, -521.4146),
            new PointXZ(-84.01466, -528.8804),
            new PointXZ(-71.56582, -521.6586),
            new PointXZ(-61.44, -538.88),
            new PointXZ(-100.6034, -560.6501)
                }
            },
        };
    }
}
