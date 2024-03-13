using BF1ServerTools.API;
using BF1ServerTools.SDK;
using BF1ServerTools.SDK.Data;
using BF1ServerTools.RES;
using BF1ServerTools.Data;
using BF1ServerTools.Utils;
using BF1ServerTools.Models;
using BF1ServerTools.Helper;
using BF1ServerTools.Windows;
using BF1ServerTools.Extensions;

namespace BF1ServerTools.Views;

/// <summary>
/// ScoreView.xaml 的交互逻辑
/// </summary>
public partial class ScoreView : UserControl
{
    /// <summary>
    /// 数据模型绑定
    /// </summary>
    public ScoreModel ScoreModel { get; set; } = new();

    ///////////////////////////////////////////////////////

    private List<PlayerData> PlayerList_Team01 = new();
    private List<PlayerData> PlayerList_Team02 = new();

    private List<PlayerData> PlayerList_Team1 = new();
    private List<PlayerData> PlayerList_Team2 = new();

    /// <summary>
    /// 服务器数据
    /// </summary>
    private ServerData _serverData = new();

    ///////////////////////////////////////////////////////

    /// <summary>
    /// 绑定UI队伍1动态数据集合，用于更新ListView
    /// </summary>
    public ObservableCollection<PlayerDataModel> ListView_PlayerList_Team1 { get; set; } = new();
    /// <summary>
    /// 绑定UI队伍2动态数据集合，用于更新ListView
    /// </summary>
    public ObservableCollection<PlayerDataModel> ListView_PlayerList_Team2 { get; set; } = new();

    /// <summary>
    /// 绑定UI队伍0动态数据集合，用于更新观战列表
    /// </summary>
    public ObservableCollection<SpectatorInfo> ListBox_PlayerList_Team01 { get; set; } = new();

    /// <summary>
    /// 绑定UI队伍0动态数据集合，用于更新载入中列表
    /// </summary>
    public ObservableCollection<SpectatorInfo> ListBox_PlayerList_Team02 { get; set; } = new();

    ///////////////////////////////////////////////////////

    public ScoreView()
    {
        InitializeComponent();
        this.DataContext = this;
        MainWindow.WindowClosingEvent += MainWindow_WindowClosingEvent;

        new Thread(UpdateServerInfoThread)
        {
            Name = "UpdateServerInfoThread",
            IsBackground = true
        }.Start();

        new Thread(UpdatePlayerListThread)
        {
            Name = "UpdatePlayerListThread",
            IsBackground = true
        }.Start();
    }

    private void MainWindow_WindowClosingEvent()
    {

    }

    /// <summary>
    /// 更新服务器信息线程
    /// </summary>
    private void UpdateServerInfoThread()
    {
        while (MainWindow.IsAppRunning)
        {
            // 服务器名称
            _serverData.Name = Server.GetServerName();
            _serverData.Name = string.IsNullOrEmpty(_serverData.Name) ? "未知" : _serverData.Name;

            // 服务器地图名称
            _serverData.MapName = Server.GetMapName();
            _serverData.MapName = string.IsNullOrEmpty(_serverData.MapName) ? "未知" : _serverData.MapName;

            // 服务器游戏模式
            _serverData.GameMode = Server.GetGameMode();

            // 服务器时间
            _serverData.Time = Server.GetServerTime();

            //////////////////////////////// 服务器数据整理 ////////////////////////////////

            ScoreModel.ServerName = _serverData.Name;
            ScoreModel.ServerTime = PlayerUtil.SecondsToMMSS(_serverData.Time);

            ScoreModel.ServerMapName = ClientHelper.GetMapChsName(_serverData.MapName);
            ScoreModel.ServerMapImg = ClientHelper.GetMapPrevImage(_serverData.MapName);

            if (_serverData.MapName == "未知" || ScoreModel.ServerMapName == "大厅菜单")
                ScoreModel.ServerGameMode = "未知";
            else
                ScoreModel.ServerGameMode = ClientHelper.GetGameMode(_serverData.GameMode);

            ScoreModel.Team1Img = ClientHelper.GetTeam1Image(_serverData.MapName);
            ScoreModel.Team2Img = ClientHelper.GetTeam2Image(_serverData.MapName);

            ScoreModel.Team1Name = ClientHelper.GetTeamChsName(_serverData.MapName, 1);
            ScoreModel.Team2Name = ClientHelper.GetTeamChsName(_serverData.MapName, 2);

            // 当服务器模式为征服时，下列数据才有效
            if (ScoreModel.ServerGameMode == "征服")
            {
                // 最大比分
                _serverData.MaxScore = Server.GetServerMaxScore();
                // 队伍1、队伍2分数
                _serverData.Team1Score = Server.GetTeam1Score();
                _serverData.Team2Score = Server.GetTeam2Score();
                // 队伍1、队伍2从击杀获取得分
                _serverData.Team1Kill = Server.GetTeam1KillScore();
                _serverData.Team2Kill = Server.GetTeam2KillScore();
                // 队伍1、队伍2从旗帜获取得分
                _serverData.Team1Flag = Server.GetTeam1FlagScore();
                _serverData.Team2Flag = Server.GetTeam2FlagScore();

                ///////////////////////////// 修正服务器得分数据 /////////////////////////////

                _serverData.Team1Score = PlayerUtil.FixedServerScore(_serverData.Team1Score);
                _serverData.Team2Score = PlayerUtil.FixedServerScore(_serverData.Team2Score);

                if (_serverData.MaxScore != 0)
                {
                    var scale = _serverData.MaxScore / 1000.0f;
                    ScoreModel.Team1ScoreWidth = PlayerUtil.FixedServerScore(_serverData.Team1Score / (8 * scale));
                    ScoreModel.Team2ScoreWidth = PlayerUtil.FixedServerScore(_serverData.Team2Score / (8 * scale));
                }
                else
                {
                    ScoreModel.Team1ScoreWidth = 0;
                    ScoreModel.Team2ScoreWidth = 0;
                }

                ScoreModel.Team1Score = _serverData.Team1Score;
                ScoreModel.Team2Score = _serverData.Team2Score;

                ScoreModel.Team1Flag = PlayerUtil.FixedServerScore(_serverData.Team1Flag);
                ScoreModel.Team1Kill = PlayerUtil.FixedServerScore(_serverData.Team1Kill);

                ScoreModel.Team2Flag = PlayerUtil.FixedServerScore(_serverData.Team2Flag);
                ScoreModel.Team2Kill = PlayerUtil.FixedServerScore(_serverData.Team2Kill);
            }

            /////////////////////////////////////////////////////////////////////////

            // 服务器数字Id
            _serverData.GameId = Server.GetGameId();
            ScoreModel.ServerGameId = _serverData.GameId;

            // 如果玩家没有进入服务器，要进行一些数据清理
            if (ScoreModel.ServerMapName == "大厅菜单")
            {
                Globals.GameId = 0;

                Globals.ServerAdmins_PID.Clear();
                Globals.ServerVIPs_PID.Clear();

                Globals.AutoKickBreakRulePlayer = false;
            }
            else
            {
                Globals.GameId = _serverData.GameId;
            }

            /////////////////////////////////////////////////////////////////////////

            Thread.Sleep(1000);
        }
    }

    /// <summary>
    /// 更新服务器玩家列表线程
    /// </summary>
    private void UpdatePlayerListThread()
    {
        while (MainWindow.IsAppRunning)
        {
            //////////////////////////////// 数据初始化 ////////////////////////////////

            PlayerList_Team01.Clear();
            PlayerList_Team02.Clear();
            PlayerList_Team1.Clear();
            PlayerList_Team2.Clear();

            _serverData.Team1MaxPlayerCount = 0;
            _serverData.Team1PlayerCount = 0;
            _serverData.Team1Rank150PlayerCount = 0;
            _serverData.Team1AllKillCount = 0;
            _serverData.Team1AllDeadCount = 0;

            _serverData.Team2MaxPlayerCount = 0;
            _serverData.Team2PlayerCount = 0;
            _serverData.Team2Rank150PlayerCount = 0;
            _serverData.Team2AllKillCount = 0;
            _serverData.Team2AllDeadCount = 0;

            //////////////////////////////// 玩家数据 ////////////////////////////////

            var time = PlayerUtil.SecondsToMinute(Server.GetServerTime());

            foreach (var item in Player.GetPlayerList())
            {
                if (item.SquadId == 0)
                    item.SquadId = 99;
                item.SquadId2 = ClientHelper.GetSquadChsName(item.SquadId);

                item.Kd = PlayerUtil.GetPlayerKD(item.Kill, item.Dead);
                item.Kpm = PlayerUtil.GetPlayerKPM(item.Kill, time);

                item.LifeKd = PlayerUtil.GetLifeKD(item.PersonaId);
                item.LifeKpm = PlayerUtil.GetLifeKPM(item.PersonaId);
                item.LifeTime = PlayerUtil.GetLifeTime(item.PersonaId);

                item.Admin = PlayerUtil.IsAdminVIP(item.PersonaId, Globals.ServerAdmins_PID);
                item.Vip = PlayerUtil.IsAdminVIP(item.PersonaId, Globals.ServerVIPs_PID);
                item.White = PlayerUtil.IsWhite(item.Name, Globals.CustomWhites_Name);

                item.Kit2 = ClientHelper.GetPlayerKitImage(item.Kit);
                item.Kit3 = ClientHelper.GetPlayerKitName(item.Kit);

                switch (item.TeamId)
                {
                    case 0:
                        if (item.Spectator == 0x01)
                            PlayerList_Team01.Add(item);
                        else if (Globals.GameId != 0)
                            PlayerList_Team02.Add(item);
                        break;
                    case 1:
                        PlayerList_Team1.Add(item);
                        break;
                    case 2:
                        PlayerList_Team2.Add(item);
                        break;
                }
            }

            //////////////////////////////// 队伍数据整理 ////////////////////////////////

            // 队伍1数据统计
            foreach (var item in PlayerList_Team1)
            {
                // 统计当前服务器玩家数量
                if (item.PersonaId != 0)
                    _serverData.Team1MaxPlayerCount++;

                // 统计当前服务器存活玩家数量
                if (item.WeaponS0 != "" || item.WeaponS7 != "")
                    _serverData.Team1PlayerCount++;

                // 统计当前服务器150级玩家数量
                if (item.Rank == 150)
                    _serverData.Team1Rank150PlayerCount++;

                // 总击杀数统计
                _serverData.Team1AllKillCount += item.Kill;
                // 总死亡数统计
                _serverData.Team1AllDeadCount += item.Dead;
            }

            // 队伍2数据统计
            foreach (var item in PlayerList_Team2)
            {
                // 统计当前服务器玩家数量
                if (item.PersonaId != 0)
                    _serverData.Team2MaxPlayerCount++;

                // 统计当前服务器存活玩家数量
                if (item.WeaponS0 != "" || item.WeaponS7 != "")
                    _serverData.Team2PlayerCount++;

                // 统计当前服务器150级玩家数量
                if (item.Rank == 150)
                    _serverData.Team2Rank150PlayerCount++;

                // 总击杀数统计
                _serverData.Team2AllKillCount += item.Kill;
                // 总死亡数统计
                _serverData.Team2AllDeadCount += item.Dead;
            }

            // 显示队伍1中文武器名称
            for (int i = 0; i < PlayerList_Team1.Count; i++)
            {
                PlayerList_Team1[i].WeaponS0 = ClientHelper.GetWeaponChsName(PlayerList_Team1[i].WeaponS0);
                PlayerList_Team1[i].WeaponS1 = ClientHelper.GetWeaponChsName(PlayerList_Team1[i].WeaponS1);
                PlayerList_Team1[i].WeaponS2 = ClientHelper.GetWeaponChsName(PlayerList_Team1[i].WeaponS2);
                PlayerList_Team1[i].WeaponS3 = ClientHelper.GetWeaponChsName(PlayerList_Team1[i].WeaponS3);
                PlayerList_Team1[i].WeaponS4 = ClientHelper.GetWeaponChsName(PlayerList_Team1[i].WeaponS4);
                PlayerList_Team1[i].WeaponS5 = ClientHelper.GetWeaponChsName(PlayerList_Team1[i].WeaponS5);
                PlayerList_Team1[i].WeaponS6 = ClientHelper.GetWeaponChsName(PlayerList_Team1[i].WeaponS6);
                PlayerList_Team1[i].WeaponS7 = ClientHelper.GetWeaponChsName(PlayerList_Team1[i].WeaponS7);
            }

            // 显示队伍2中文武器名称
            for (int i = 0; i < PlayerList_Team2.Count; i++)
            {
                PlayerList_Team2[i].WeaponS0 = ClientHelper.GetWeaponChsName(PlayerList_Team2[i].WeaponS0);
                PlayerList_Team2[i].WeaponS1 = ClientHelper.GetWeaponChsName(PlayerList_Team2[i].WeaponS1);
                PlayerList_Team2[i].WeaponS2 = ClientHelper.GetWeaponChsName(PlayerList_Team2[i].WeaponS2);
                PlayerList_Team2[i].WeaponS3 = ClientHelper.GetWeaponChsName(PlayerList_Team2[i].WeaponS3);
                PlayerList_Team2[i].WeaponS4 = ClientHelper.GetWeaponChsName(PlayerList_Team2[i].WeaponS4);
                PlayerList_Team2[i].WeaponS5 = ClientHelper.GetWeaponChsName(PlayerList_Team2[i].WeaponS5);
                PlayerList_Team2[i].WeaponS6 = ClientHelper.GetWeaponChsName(PlayerList_Team2[i].WeaponS6);
                PlayerList_Team2[i].WeaponS7 = ClientHelper.GetWeaponChsName(PlayerList_Team2[i].WeaponS7);
            }

            //////////////////////////////// 统计信息数据 ////////////////////////////////

            ScoreModel.Team1PlayerCount = _serverData.Team1PlayerCount;
            ScoreModel.Team1MaxPlayerCount = _serverData.Team1MaxPlayerCount;
            ScoreModel.Team1Rank150PlayerCount = _serverData.Team1Rank150PlayerCount;
            ScoreModel.Team1AllKillCount = _serverData.Team1AllKillCount;
            ScoreModel.Team1AllDeadCount = _serverData.Team1AllDeadCount;

            ScoreModel.Team2PlayerCount = _serverData.Team2PlayerCount;
            ScoreModel.Team2MaxPlayerCount = _serverData.Team2MaxPlayerCount;
            ScoreModel.Team2Rank150PlayerCount = _serverData.Team2Rank150PlayerCount;
            ScoreModel.Team2AllKillCount = _serverData.Team2AllKillCount;
            ScoreModel.Team2AllDeadCount = _serverData.Team2AllDeadCount;

            ScoreModel.AllPlayerCount = _serverData.Team1MaxPlayerCount + _serverData.Team2MaxPlayerCount;

            ////////////////////////////////////////////////////////////////////////////////

            this.Dispatcher.BeginInvoke(() =>
            {
                UpdateListViewTeam1();
            });

            this.Dispatcher.BeginInvoke(() =>
            {
                UpdateListViewTeam2();
            });

            this.Dispatcher.BeginInvoke(() =>
            {
                UpdateListBoxTeam01();
            });

            this.Dispatcher.BeginInvoke(() =>
            {
                UpdateListBoxTeam02();
            });

            /////////////////////////////////////////////////////////////////////////

            Thread.Sleep(1000);
        }
    }

    /// <summary>
    /// 动态更新 ListView 队伍1
    /// </summary>
    private void UpdateListViewTeam1()
    {
        if (PlayerList_Team1.Count == 0 && ListView_PlayerList_Team1.Count != 0)
            ListView_PlayerList_Team1.Clear();

        if (PlayerList_Team1.Count != 0)
        {
            // 更新ListView中现有的玩家数据，并把ListView中已经不在服务器的玩家清除
            for (int i = 0; i < ListView_PlayerList_Team1.Count; i++)
            {
                int index = PlayerList_Team1.FindIndex(val => val.PersonaId == ListView_PlayerList_Team1[i].PersonaId);
                if (index != -1)
                {
                    ListView_PlayerList_Team1[i].Rank = PlayerList_Team1[index].Rank;
                    ListView_PlayerList_Team1[i].Clan = PlayerList_Team1[index].Clan;
                    ListView_PlayerList_Team1[i].Admin = PlayerList_Team1[index].Admin;
                    ListView_PlayerList_Team1[i].Vip = PlayerList_Team1[index].Vip;
                    ListView_PlayerList_Team1[i].White = PlayerList_Team1[index].White;
                    ListView_PlayerList_Team1[i].SquadId = PlayerList_Team1[index].SquadId;
                    ListView_PlayerList_Team1[i].SquadId2 = PlayerList_Team1[index].SquadId2;
                    ListView_PlayerList_Team1[i].Kill = PlayerList_Team1[index].Kill;
                    ListView_PlayerList_Team1[i].Dead = PlayerList_Team1[index].Dead;
                    ListView_PlayerList_Team1[i].Kd = PlayerList_Team1[index].Kd;
                    ListView_PlayerList_Team1[i].Kpm = PlayerList_Team1[index].Kpm;
                    ListView_PlayerList_Team1[i].LifeKd = PlayerList_Team1[index].LifeKd;
                    ListView_PlayerList_Team1[i].LifeKpm = PlayerList_Team1[index].LifeKpm;
                    ListView_PlayerList_Team1[i].LifeTime = PlayerList_Team1[index].LifeTime;
                    ListView_PlayerList_Team1[i].Score = PlayerList_Team1[index].Score;
                    ListView_PlayerList_Team1[i].Kit = PlayerList_Team1[index].Kit;
                    ListView_PlayerList_Team1[i].Kit2 = PlayerList_Team1[index].Kit2;
                    ListView_PlayerList_Team1[i].Kit3 = PlayerList_Team1[index].Kit3;
                    ListView_PlayerList_Team1[i].WeaponS0 = PlayerList_Team1[index].WeaponS0;
                    ListView_PlayerList_Team1[i].WeaponS1 = PlayerList_Team1[index].WeaponS1;
                    ListView_PlayerList_Team1[i].WeaponS2 = PlayerList_Team1[index].WeaponS2;
                    ListView_PlayerList_Team1[i].WeaponS3 = PlayerList_Team1[index].WeaponS3;
                    ListView_PlayerList_Team1[i].WeaponS4 = PlayerList_Team1[index].WeaponS4;
                    ListView_PlayerList_Team1[i].WeaponS5 = PlayerList_Team1[index].WeaponS5;
                    ListView_PlayerList_Team1[i].WeaponS6 = PlayerList_Team1[index].WeaponS6;
                    ListView_PlayerList_Team1[i].WeaponS7 = PlayerList_Team1[index].WeaponS7;
                }
                else
                {
                    ListView_PlayerList_Team1.RemoveAt(i);
                }
            }

            // 增加ListView没有的玩家数据
            for (int i = 0; i < PlayerList_Team1.Count; i++)
            {
                int index = ListView_PlayerList_Team1.ToList().FindIndex(val => val.PersonaId == PlayerList_Team1[i].PersonaId);
                if (index == -1)
                {
                    ListView_PlayerList_Team1.Add(new()
                    {
                        Rank = PlayerList_Team1[i].Rank,
                        Clan = PlayerList_Team1[i].Clan,
                        Name = PlayerList_Team1[i].Name,
                        PersonaId = PlayerList_Team1[i].PersonaId,
                        Admin = PlayerList_Team1[i].Admin,
                        Vip = PlayerList_Team1[i].Vip,
                        White = PlayerList_Team1[i].White,
                        SquadId = PlayerList_Team1[i].SquadId,
                        SquadId2 = PlayerList_Team1[i].SquadId2,
                        Kill = PlayerList_Team1[i].Kill,
                        Dead = PlayerList_Team1[i].Dead,
                        Kd = PlayerList_Team1[i].Kd,
                        Kpm = PlayerList_Team1[i].Kpm,
                        LifeKd = PlayerList_Team1[i].LifeKd,
                        LifeKpm = PlayerList_Team1[i].LifeKpm,
                        LifeTime = PlayerList_Team1[i].LifeTime,
                        Score = PlayerList_Team1[i].Score,
                        Kit = PlayerList_Team1[i].Kit,
                        Kit2 = PlayerList_Team1[i].Kit2,
                        Kit3 = PlayerList_Team1[i].Kit3,
                        WeaponS0 = PlayerList_Team1[i].WeaponS0,
                        WeaponS1 = PlayerList_Team1[i].WeaponS1,
                        WeaponS2 = PlayerList_Team1[i].WeaponS2,
                        WeaponS3 = PlayerList_Team1[i].WeaponS3,
                        WeaponS4 = PlayerList_Team1[i].WeaponS4,
                        WeaponS5 = PlayerList_Team1[i].WeaponS5,
                        WeaponS6 = PlayerList_Team1[i].WeaponS6,
                        WeaponS7 = PlayerList_Team1[i].WeaponS7
                    });

                }
            }

            // 排序
            ListView_PlayerList_Team1.Sort();

            // 修正序号
            for (int i = 0; i < ListView_PlayerList_Team1.Count; i++)
                ListView_PlayerList_Team1[i].Index = i + 1;
        }
    }

    /// <summary>
    /// 动态更新 ListView 队伍2
    /// </summary>
    private void UpdateListViewTeam2()
    {
        if (PlayerList_Team2.Count == 0 && ListView_PlayerList_Team2.Count != 0)
            ListView_PlayerList_Team2.Clear();

        if (PlayerList_Team2.Count != 0)
        {
            // 更新ListView中现有的玩家数据，并把ListView中已经不在服务器的玩家清除
            for (int i = 0; i < ListView_PlayerList_Team2.Count; i++)
            {
                int index = PlayerList_Team2.FindIndex(val => val.PersonaId == ListView_PlayerList_Team2[i].PersonaId);
                if (index != -1)
                {
                    ListView_PlayerList_Team2[i].Rank = PlayerList_Team2[index].Rank;
                    ListView_PlayerList_Team2[i].Clan = PlayerList_Team2[index].Clan;
                    ListView_PlayerList_Team2[i].Admin = PlayerList_Team2[index].Admin;
                    ListView_PlayerList_Team2[i].Vip = PlayerList_Team2[index].Vip;
                    ListView_PlayerList_Team2[i].White = PlayerList_Team2[index].White;
                    ListView_PlayerList_Team2[i].SquadId = PlayerList_Team2[index].SquadId;
                    ListView_PlayerList_Team2[i].SquadId2 = PlayerList_Team2[index].SquadId2;
                    ListView_PlayerList_Team2[i].Kill = PlayerList_Team2[index].Kill;
                    ListView_PlayerList_Team2[i].Dead = PlayerList_Team2[index].Dead;
                    ListView_PlayerList_Team2[i].Kd = PlayerList_Team2[index].Kd;
                    ListView_PlayerList_Team2[i].Kpm = PlayerList_Team2[index].Kpm;
                    ListView_PlayerList_Team2[i].LifeKd = PlayerList_Team2[index].LifeKd;
                    ListView_PlayerList_Team2[i].LifeKpm = PlayerList_Team2[index].LifeKpm;
                    ListView_PlayerList_Team2[i].LifeTime = PlayerList_Team2[index].LifeTime;
                    ListView_PlayerList_Team2[i].Score = PlayerList_Team2[index].Score;
                    ListView_PlayerList_Team2[i].Kit = PlayerList_Team2[index].Kit;
                    ListView_PlayerList_Team2[i].Kit2 = PlayerList_Team2[index].Kit2;
                    ListView_PlayerList_Team2[i].Kit3 = PlayerList_Team2[index].Kit3;
                    ListView_PlayerList_Team2[i].WeaponS0 = PlayerList_Team2[index].WeaponS0;
                    ListView_PlayerList_Team2[i].WeaponS1 = PlayerList_Team2[index].WeaponS1;
                    ListView_PlayerList_Team2[i].WeaponS2 = PlayerList_Team2[index].WeaponS2;
                    ListView_PlayerList_Team2[i].WeaponS3 = PlayerList_Team2[index].WeaponS3;
                    ListView_PlayerList_Team2[i].WeaponS4 = PlayerList_Team2[index].WeaponS4;
                    ListView_PlayerList_Team2[i].WeaponS5 = PlayerList_Team2[index].WeaponS5;
                    ListView_PlayerList_Team2[i].WeaponS6 = PlayerList_Team2[index].WeaponS6;
                    ListView_PlayerList_Team2[i].WeaponS7 = PlayerList_Team2[index].WeaponS7;
                }
                else
                {
                    ListView_PlayerList_Team2.RemoveAt(i);
                }
            }

            // 增加ListView没有的玩家数据
            for (int i = 0; i < PlayerList_Team2.Count; i++)
            {
                int index = ListView_PlayerList_Team2.ToList().FindIndex(val => val.PersonaId == PlayerList_Team2[i].PersonaId);
                if (index == -1)
                {
                    ListView_PlayerList_Team2.Add(new()
                    {
                        Rank = PlayerList_Team2[i].Rank,
                        Clan = PlayerList_Team2[i].Clan,
                        Name = PlayerList_Team2[i].Name,
                        PersonaId = PlayerList_Team2[i].PersonaId,
                        Admin = PlayerList_Team2[i].Admin,
                        Vip = PlayerList_Team2[i].Vip,
                        White = PlayerList_Team2[i].White,
                        SquadId = PlayerList_Team2[i].SquadId,
                        SquadId2 = PlayerList_Team2[i].SquadId2,
                        Kill = PlayerList_Team2[i].Kill,
                        Dead = PlayerList_Team2[i].Dead,
                        Kd = PlayerList_Team2[i].Kd,
                        Kpm = PlayerList_Team2[i].Kpm,
                        LifeKd = PlayerList_Team2[i].LifeKd,
                        LifeKpm = PlayerList_Team2[i].LifeKpm,
                        LifeTime = PlayerList_Team2[i].LifeTime,
                        Score = PlayerList_Team2[i].Score,
                        Kit = PlayerList_Team2[i].Kit,
                        Kit2 = PlayerList_Team2[i].Kit2,
                        Kit3 = PlayerList_Team2[i].Kit3,
                        WeaponS0 = PlayerList_Team2[i].WeaponS0,
                        WeaponS1 = PlayerList_Team2[i].WeaponS1,
                        WeaponS2 = PlayerList_Team2[i].WeaponS2,
                        WeaponS3 = PlayerList_Team2[i].WeaponS3,
                        WeaponS4 = PlayerList_Team2[i].WeaponS4,
                        WeaponS5 = PlayerList_Team2[i].WeaponS5,
                        WeaponS6 = PlayerList_Team2[i].WeaponS6,
                        WeaponS7 = PlayerList_Team2[i].WeaponS7
                    });
                }
            }

            // 排序
            ListView_PlayerList_Team2.Sort();

            // 修正序号
            for (int i = 0; i < ListView_PlayerList_Team2.Count; i++)
                ListView_PlayerList_Team2[i].Index = i + 1;
        }
    }

    /// <summary>
    /// 动态更新 ListBox 队伍01
    /// </summary>
    private void UpdateListBoxTeam01()
    {
        if (PlayerList_Team01.Count == 0 && ListBox_PlayerList_Team01.Count != 0)
            ListBox_PlayerList_Team01.Clear();

        if (PlayerList_Team01.Count != 0)
        {
            for (int i = 0; i < ListBox_PlayerList_Team01.Count; i++)
            {
                int index = PlayerList_Team01.FindIndex(val => val.PersonaId == ListBox_PlayerList_Team01[i].PersonaId);
                if (index == -1)
                    ListBox_PlayerList_Team01.RemoveAt(i);
            }

            for (int i = 0; i < PlayerList_Team01.Count; i++)
            {
                int index = ListBox_PlayerList_Team01.ToList().FindIndex(val => val.PersonaId == PlayerList_Team01[i].PersonaId);
                if (index == -1)
                {
                    ListBox_PlayerList_Team01.Add(new()
                    {
                        Name = PlayerList_Team01[i].Name,
                        PersonaId = PlayerList_Team01[i].PersonaId
                    });
                }
            }
        }
    }

    /// <summary>
    /// 动态更新 ListBox 队伍02
    /// </summary>
    private void UpdateListBoxTeam02()
    {
        if (PlayerList_Team02.Count == 0 && ListBox_PlayerList_Team02.Count != 0)
            ListBox_PlayerList_Team02.Clear();

        if (PlayerList_Team02.Count != 0)
        {
            for (int i = 0; i < ListBox_PlayerList_Team02.Count; i++)
            {
                int index = PlayerList_Team02.FindIndex(val => val.PersonaId == ListBox_PlayerList_Team02[i].PersonaId);
                if (index == -1)
                    ListBox_PlayerList_Team02.RemoveAt(i);
            }

            for (int i = 0; i < PlayerList_Team02.Count; i++)
            {
                int index = ListBox_PlayerList_Team02.ToList().FindIndex(val => val.PersonaId == PlayerList_Team02[i].PersonaId);
                if (index == -1)
                {
                    ListBox_PlayerList_Team02.Add(new()
                    {
                        Name = PlayerList_Team02[i].Name,
                        PersonaId = PlayerList_Team02[i].PersonaId
                    });
                }
            }
        }
    }

    /// <summary>
    /// 得分板排序规则选中变更事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ComboBox_ScoreSortRule_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Globals.OrderBy = (OrderBy)ComboBox_ScoreSortRule.SelectedIndex;
    }

    /// <summary>
    /// 队伍1 ListView选中变更事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListView_Team1_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListView_Team1.SelectedItem is PlayerDataModel item)
            MenuItem_Team1.Header = $"T1 [{item.Rank}] {item.Name}    {item.Kill}/{item.Dead}/{item.Score}";
        else
            MenuItem_Team1.Header = "T1 当前未选中";
    }

    /// <summary>
    /// 队伍2 ListView选中变更事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListView_Team2_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListView_Team2.SelectedItem is PlayerDataModel item)
            MenuItem_Team2.Header = $"T2 [{item.Rank}] {item.Name}    {item.Kill}/{item.Dead}/{item.Score}";
        else
            MenuItem_Team2.Header = "T2 当前未选中";
    }

    /// <summary>
    /// 队伍01 ListBox选中变更事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListBox_Team01_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListBox_Team01.SelectedItem is SpectatorInfo item)
            MenuItem_Team01.Header = $"T01 {item.Name} {item.PersonaId}";
        else
            MenuItem_Team01.Header = "T01 当前未选中";
    }

    /// <summary>
    /// 队伍02 ListBox选中变更事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListBox_Team02_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ListBox_Team02.SelectedItem is SpectatorInfo item)
            MenuItem_Team02.Header = $"T02 {item.Name} {item.PersonaId}";
        else
            MenuItem_Team02.Header = "T02 当前未选中";
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

    #region 队伍1 右键菜单事件
    private void MenuItem_Team1_KickPlayerCustom_Click(object sender, RoutedEventArgs e)
    {
        if (!PlayerUtil.CheckAuth())
            return;

        if (ListView_Team1.SelectedItem is PlayerDataModel item)
        {
            var customKickWindow = new CustomKickWindow(item.Rank, item.Name, item.PersonaId)
            {
                Owner = MainWindow.MainWindowInstance
            };
            customKickWindow.ShowDialog();
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "T1 当前未选中任何玩家，操作取消");
        }
    }

    private void MenuItem_Team1_KickPlayerOffensiveBehavior_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team1.SelectedItem is PlayerDataModel item)
            KickPlayer(item.Rank, item.Name, item.PersonaId, "OFFENSIVEBEHAVIOR");
        else
            NotifierHelper.Show(NotifierType.Warning, "T1 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team1_KickPlayerLatency_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team1.SelectedItem is PlayerDataModel item)
            KickPlayer(item.Rank, item.Name, item.PersonaId, "LATENCY");
        else
            NotifierHelper.Show(NotifierType.Warning, "T1 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team1_KickPlayerRuleViolation_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team1.SelectedItem is PlayerDataModel item)
            KickPlayer(item.Rank, item.Name, item.PersonaId, "RULEVIOLATION");
        else
            NotifierHelper.Show(NotifierType.Warning, "T1 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team1_KickPlayerGeneral_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team1.SelectedItem is PlayerDataModel item)
            KickPlayer(item.Rank, item.Name, item.PersonaId, "GENERAL");
        else
            NotifierHelper.Show(NotifierType.Warning, "T1 当前未选中任何玩家，操作取消");
    }

    private async void MenuItem_Team1_ChangePlayerTeam_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team1.SelectedItem is PlayerDataModel item)
        {
            if (!PlayerUtil.CheckAuth())
                return;

            NotifierHelper.Show(NotifierType.Information, $"正在更换玩家 {item.Name} 队伍中...");

            var result = await BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, item.PersonaId, 1);
            if (result.IsSuccess)
                NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  更换玩家 {item.Name} 队伍成功");
            else
                NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  更换玩家 {item.Name} 队伍失败\n{result.Content}");
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "T1 当前未选中任何玩家，操作取消");
        }
    }

    private void MenuItem_Team1_CopyPlayerName_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team1.SelectedItem is PlayerDataModel item)
            Copy2Clipboard(item.Name);
        else
            NotifierHelper.Show(NotifierType.Warning, "T1 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team1_CopyPlayerPersonaId_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team1.SelectedItem is PlayerDataModel item)
            Copy2Clipboard(item.PersonaId);
        else
            NotifierHelper.Show(NotifierType.Warning, "T1 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team1_CopyPlayerAllData_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team1.SelectedItem is PlayerDataModel item)
        {
            var builder = new StringBuilder();

            builder.Append($"序号：{item.Index}，");
            builder.Append($"等级：{item.Rank}，");
            builder.Append($"战队：{item.Clan}，");
            builder.Append($"玩家ID：{item.Name}，");
            builder.Append($"数字ID：{item.PersonaId}，");
            builder.Append($"小队：{item.SquadId2}，");
            builder.Append($"击杀：{item.Kill}，");
            builder.Append($"死亡：{item.Dead}，");
            builder.Append($"KD：{item.Kd}，");
            builder.Append($"KPM：{item.Kpm}，");
            builder.Append($"得分：{item.Score}，");
            builder.Append($"生涯KD：{item.LifeKd}，");
            builder.Append($"生涯KPM：{item.LifeKpm}，");
            builder.Append($"生涯时长：{item.LifeTime}，");
            builder.Append($"兵种：{item.Kit3}，");
            builder.Append($"主武器：{item.WeaponS0}，");
            builder.Append($"配枪：{item.WeaponS1}，");
            builder.Append($"配备一：{item.WeaponS2}，");
            builder.Append($"配备二：{item.WeaponS5}，");
            builder.Append($"特殊：{item.WeaponS3}，");
            builder.Append($"手榴弹：{item.WeaponS6}，");
            builder.Append($"近战：{item.WeaponS7}。");

            Copy2Clipboard(builder.ToString());
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "T1 当前未选中任何玩家，操作取消");
        }
    }

    private void MenuItem_Team1_QueryPlayerData_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team1.SelectedItem is PlayerDataModel item)
        {
            if (!PlayerUtil.CheckSId())
                return;

            var queryRecordWindow = new QueryRecordWindow(item.Name, item.PersonaId, item.Rank);
            queryRecordWindow.Show();
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "T1 当前未选中任何玩家，操作取消");
        }
    }
    #endregion

    #region 队伍2 右键菜单事件
    private void MenuItem_Team2_KickPlayerCustom_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team2.SelectedItem is PlayerDataModel item)
        {
            if (!PlayerUtil.CheckAuth())
                return;

            var customKickWindow = new CustomKickWindow(item.Rank, item.Name, item.PersonaId)
            {
                Owner = MainWindow.MainWindowInstance
            };
            customKickWindow.ShowDialog();
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "T2 当前未选中任何玩家，操作取消");
        }
    }

    private void MenuItem_Team2_KickPlayerOffensiveBehavior_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team2.SelectedItem is PlayerDataModel item)
            KickPlayer(item.Rank, item.Name, item.PersonaId, "OFFENSIVEBEHAVIOR");
        else
            NotifierHelper.Show(NotifierType.Warning, "T2 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team2_KickPlayerLatency_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team2.SelectedItem is PlayerDataModel item)
            KickPlayer(item.Rank, item.Name, item.PersonaId, "LATENCY");
        else
            NotifierHelper.Show(NotifierType.Warning, "T2 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team2_KickPlayerRuleViolation_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team2.SelectedItem is PlayerDataModel item)
            KickPlayer(item.Rank, item.Name, item.PersonaId, "RULEVIOLATION");
        else
            NotifierHelper.Show(NotifierType.Warning, "T2 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team2_KickPlayerGeneral_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team2.SelectedItem is PlayerDataModel item)
            KickPlayer(item.Rank, item.Name, item.PersonaId, "GENERAL");
        else
            NotifierHelper.Show(NotifierType.Warning, "T2 当前未选中任何玩家，操作取消");
    }

    private async void MenuItem_Team2_ChangePlayerTeam_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team2.SelectedItem is PlayerDataModel item)
        {
            if (!PlayerUtil.CheckAuth())
                return;

            NotifierHelper.Show(NotifierType.Information, $"正在更换玩家 {item.Name} 队伍中...");

            var result = await BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, item.PersonaId, 2);
            if (result.IsSuccess)
                NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  更换玩家 {item.Name} 队伍成功");
            else
                NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  更换玩家 {item.Name} 队伍失败\n{result.Content}");
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "T2 当前未选中任何玩家，操作取消");
        }
    }

    private void MenuItem_Team2_CopyPlayerName_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team2.SelectedItem is PlayerDataModel item)
            Copy2Clipboard(item.Name);
        else
            NotifierHelper.Show(NotifierType.Warning, "T2 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team2_CopyPlayerPersonaId_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team2.SelectedItem is PlayerDataModel item)
            Copy2Clipboard(item.PersonaId);
        else
            NotifierHelper.Show(NotifierType.Warning, "T2 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team2_CopyPlayerAllData_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team2.SelectedItem is PlayerDataModel item)
        {
            var builder = new StringBuilder();

            builder.Append($"序号：{item.Index}，");
            builder.Append($"等级：{item.Rank}，");
            builder.Append($"战队：{item.Clan}，");
            builder.Append($"玩家ID：{item.Name}，");
            builder.Append($"数字ID：{item.PersonaId}，");
            builder.Append($"小队：{item.SquadId2}，");
            builder.Append($"击杀：{item.Kill}，");
            builder.Append($"死亡：{item.Dead}，");
            builder.Append($"得分：{item.Score}，");
            builder.Append($"KD：{item.Kd}，");
            builder.Append($"KPM：{item.Kpm}，");
            builder.Append($"生涯KD：{item.LifeKd}，");
            builder.Append($"生涯KPM：{item.LifeKpm}，");
            builder.Append($"生涯时长：{item.LifeTime}，");
            builder.Append($"兵种：{item.Kit3}，");
            builder.Append($"主武器：{item.WeaponS0}，");
            builder.Append($"配枪：{item.WeaponS1}，");
            builder.Append($"配备一：{item.WeaponS2}，");
            builder.Append($"配备二：{item.WeaponS5}，");
            builder.Append($"特殊：{item.WeaponS3}，");
            builder.Append($"手榴弹：{item.WeaponS6}，");
            builder.Append($"近战：{item.WeaponS7}。");

            Copy2Clipboard(builder.ToString());
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "T2 当前未选中任何玩家，操作取消");
        }
    }

    private void MenuItem_Team2_QueryPlayerData_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team2.SelectedItem is PlayerDataModel item)
        {
            if (string.IsNullOrEmpty(Globals.SessionId))
            {
                NotifierHelper.Show(NotifierType.Warning, "请先获取玩家SessionId后，再执行本操作");
                return;
            }

            var queryRecordWindow = new QueryRecordWindow(item.Name, item.PersonaId, item.Rank);
            queryRecordWindow.Show();
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "T2 当前未选中任何玩家，操作取消");
        }
    }
    #endregion

    #region 队伍01 右键菜单事件
    private void MenuItem_Team01_KickPlayerCustom_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team01.SelectedItem is SpectatorInfo item)
        {
            if (!PlayerUtil.CheckAuth())
                return;

            var customKickWindow = new CustomKickWindow(-1, item.Name, item.PersonaId)
            {
                Owner = MainWindow.MainWindowInstance
            };
            customKickWindow.ShowDialog();
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "T01 当前未选中任何玩家，操作取消");
        }
    }

    private void MenuItem_Team01_KickPlayerOffensiveBehavior_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team01.SelectedItem is SpectatorInfo item)
            KickPlayer(-1, item.Name, item.PersonaId, "OFFENSIVEBEHAVIOR");
        else
            NotifierHelper.Show(NotifierType.Warning, "T01 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team01_KickPlayerLatency_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team01.SelectedItem is SpectatorInfo item)
            KickPlayer(-1, item.Name, item.PersonaId, "LATENCY");
        else
            NotifierHelper.Show(NotifierType.Warning, "T01 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team01_KickPlayerRuleViolation_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team01.SelectedItem is SpectatorInfo item)
            KickPlayer(-1, item.Name, item.PersonaId, "RULEVIOLATION");
        else
            NotifierHelper.Show(NotifierType.Warning, "T01 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team01_KickPlayerGeneral_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team01.SelectedItem is SpectatorInfo item)
            KickPlayer(-1, item.Name, item.PersonaId, "GENERAL");
        else
            NotifierHelper.Show(NotifierType.Warning, "T01 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team01_CopyPlayerName_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team01.SelectedItem is SpectatorInfo item)
            Copy2Clipboard(item.Name);
        else
            NotifierHelper.Show(NotifierType.Warning, "T01 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team01_CopyPlayerPersonaId_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team01.SelectedItem is SpectatorInfo item)
            Copy2Clipboard(item.PersonaId);
        else
            NotifierHelper.Show(NotifierType.Warning, "T01 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team01_QueryPlayerData_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team01.SelectedItem is SpectatorInfo item)
        {
            if (!PlayerUtil.CheckSId())
                return;

            var queryRecordWindow = new QueryRecordWindow(item.Name, item.PersonaId, -1);
            queryRecordWindow.Show();
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "T01 当前未选中任何玩家，操作取消");
        }
    }
    #endregion

    #region 队伍02 右键菜单事件
    private void MenuItem_Team02_KickPlayerCustom_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team02.SelectedItem is SpectatorInfo item)
        {
            if (!PlayerUtil.CheckAuth())
                return;

            var customKickWindow = new CustomKickWindow(-2, item.Name, item.PersonaId)
            {
                Owner = MainWindow.MainWindowInstance
            };
            customKickWindow.ShowDialog();
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "T02 当前未选中任何玩家，操作取消");
        }
    }

    private void MenuItem_Team02_KickPlayerOffensiveBehavior_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team02.SelectedItem is SpectatorInfo item)
            KickPlayer(-2, item.Name, item.PersonaId, "OFFENSIVEBEHAVIOR");
        else
            NotifierHelper.Show(NotifierType.Warning, "T02 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team02_KickPlayerLatency_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team02.SelectedItem is SpectatorInfo item)
            KickPlayer(-2, item.Name, item.PersonaId, "LATENCY");
        else
            NotifierHelper.Show(NotifierType.Warning, "T02 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team02_KickPlayerRuleViolation_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team02.SelectedItem is SpectatorInfo item)
            KickPlayer(-2, item.Name, item.PersonaId, "RULEVIOLATION");
        else
            NotifierHelper.Show(NotifierType.Warning, "T02 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team02_KickPlayerGeneral_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team02.SelectedItem is SpectatorInfo item)
            KickPlayer(-2, item.Name, item.PersonaId, "GENERAL");
        else
            NotifierHelper.Show(NotifierType.Warning, "T02 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team02_CopyPlayerName_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team02.SelectedItem is SpectatorInfo item)
            Copy2Clipboard(item.Name);
        else
            NotifierHelper.Show(NotifierType.Warning, "T02 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team02_CopyPlayerPersonaId_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team02.SelectedItem is SpectatorInfo item)
            Copy2Clipboard(item.PersonaId);
        else
            NotifierHelper.Show(NotifierType.Warning, "T02 当前未选中任何玩家，操作取消");
    }

    private void MenuItem_Team02_QueryPlayerData_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_Team02.SelectedItem is SpectatorInfo item)
        {
            if (!PlayerUtil.CheckSId())
                return;

            var queryRecordWindow = new QueryRecordWindow(item.Name, item.PersonaId, -2);
            queryRecordWindow.Show();
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "T02 当前未选中任何玩家，操作取消");
        }
    }
    #endregion

    /// <summary>
    /// 自动调整列宽
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_AutoColumWidth_Click(object sender, RoutedEventArgs e)
    {
        lock (this)
        {
            if (ListView_Team1.View is GridView gv1)
            {
                foreach (GridViewColumn gvc in gv1.Columns)
                {
                    gvc.Width = 100;
                    gvc.Width = double.NaN;
                }
            }

            if (ListView_Team2.View is GridView gv2)
            {
                foreach (GridViewColumn gvc in gv2.Columns)
                {
                    gvc.Width = 100;
                    gvc.Width = double.NaN;
                }
            }
        }
    }
}
