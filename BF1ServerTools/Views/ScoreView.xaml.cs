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
using Newtonsoft.Json;
using System.Net.Http;

using System.Threading.Tasks;
using static BF1ServerTools.Views.ScoreView;
using System.IO.Pipes;
using System.Windows.Controls;
using BF1ServerTools.RES.Data;

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

    public static string mapname = "";//用于判断当前地图
    public static string mapmode = "";
    public static int Team1FlagScore = 0;
    public static int Team2FlagScore = 0;
    public static bool zhangapi = false;

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
        InitializeGridViewColumnWidthTracking();
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
  
    //toolapi解析
    public class ServerInfoRoot
    {
        [JsonProperty("serverinfo")]
        public ServerInfo ServerInfo { get; set; }
        public int gameId  { get; set; }
        [JsonProperty("teams")]
        public List<TeamInfo> Teams { get; set; }
    }

    public class ServerInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("maps")]
        public List<string> Maps { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("settings")]
        public List<string> Settings { get; set; }

        [JsonProperty("servertype")]
        public string ServerType { get; set; }
    }

    public class TeamInfo
    {
        [JsonProperty("teamid")]
        public string TeamId { get; set; }

        [JsonProperty("players")]
        public List<PlayerInfo> Players { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("faction")]
        public string Faction { get; set; }
    }

    public class PlayerInfo
    {
        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("latency")]
        public int Latency { get; set; }

        [JsonProperty("slot")]
        public int Slot { get; set; }

        [JsonProperty("join_time")]
        public long JoinTime { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("player_id")]
        public long PlayerId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("platoon")]
        public string Platoon { get; set; }
    }
    public class Config
    {
        public string ServerUrl { get; set; }
    }
    private readonly string F_Auth_Path2 = BF1ServerTools.Utils.FileUtil.D_Config_Path + @"\AuthConfig2.json";
    private Config ReadConfig()
    {
        // 从文件中读取JSON字符串
        string json = File.ReadAllText(F_Auth_Path2);

        // 反序列化JSON字符串为Config对象
        Config config = JsonConvert.DeserializeObject<Config>(json);

        return config;
    }
    public class ServerResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public ServerData2 Data { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }
    }
    public class ServerData2
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("gameId")]
        public long GameId { get; set; }

        [JsonProperty("mapName")]
        public string MapName { get; set; }

        [JsonProperty("mapName2")]
        public string MapName2 { get; set; }

        [JsonProperty("gameMode")]
        public string GameMode { get; set; }

        [JsonProperty("gameMode2")]
        public string GameMode2 { get; set; }
    }
    /// <summary>
    /// 更新服务器信息线程
    /// </summary>
    private async void UpdateServerInfoThread()
    {
            int apicount = 0;
        // 初始化 HttpClient 实例
        using (var httpClient = new HttpClient())
            while (MainWindow.IsAppRunning)
            {
            // 创建 HttpClient 实例
            using HttpClient client = new HttpClient();
                /*
            if (apicount >= 3)
            {
              

                try
                {   
                    // 发送 GET 请求
                    HttpResponseMessage response = await client.GetAsync("http://127.0.0.1:10086/Game/GetChatStatus");

                    // 检查状态码是否为 200 OK
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        zhangapi = true;
                    }
                    else
                    {
                        zhangapi = false;
                    }
                }
                catch (HttpRequestException e1)
                {
                    zhangapi = false;
                }
                apicount = 0;
            }
            else
            {
                apicount++;
            }*/
                Player.IsUseMode1 = Globals.IsUseMode1;
                if (!Globals.IsUseMode1)
                {
                    try
                    {
                        string zhangurl = "http://127.0.0.1:10086/Server/GetServerData";

                        string serverUrl = "";
                        ServerResponse serverInfo2;
                        try
                        {
                            string response2 = await httpClient.GetStringAsync(zhangurl);
                            serverInfo2 = JsonConvert.DeserializeObject<ServerResponse>(response2);
                        }
                        catch (HttpRequestException ex)  // 处理 HTTP 请求异常
                        {
                            
                            serverInfo2 = new ServerResponse { Code = -1, Message = "网络请求失败" };
                        }
                        catch (Newtonsoft.Json.JsonException ex)  // 处理 JSON 解析异常
                        {
                         
                            serverInfo2 = new ServerResponse { Code = -2, Message = "JSON 解析失败" };
                        }
                        catch (Exception ex)  // 其他未知异常
                        {
                            
                            serverInfo2 = new ServerResponse { Code = -3, Message = "未知错误" };
                        }
                        if (serverInfo2.Code == 200)
                        {
                            _serverData.Name = serverInfo2.Data.Name;
                            _serverData.Name = string.IsNullOrEmpty(_serverData.Name) ? "未知" : _serverData.Name;
                            ScoreModel.ServerName = _serverData.Name;
                            _serverData.GameMode = serverInfo2.Data.GameMode;
                            ScoreModel.ServerGameMode = serverInfo2.Data.GameMode2;
                            _serverData.MapName = serverInfo2.Data.MapName.Split('/').Last(); //原始名称
                            ScoreModel.ServerMapName = serverInfo2.Data.MapName2;
                            _serverData.GameId = serverInfo2.Data.GameId;
                            if (_serverData.GameId == 0)
                            {
                                _serverData.GameId = BF1ServerTools.Views.AuthView.gameid233;//服务器id获取

                            }
                            ScoreModel.ServerGameId = _serverData.GameId;
                            ScoreModel.Team1Img = ClientHelper.GetTeam1Image2(_serverData.MapName);
                            ScoreModel.Team2Img = ClientHelper.GetTeam2Image2(_serverData.MapName);

                            ScoreModel.Team1Name = ClientHelper.GetTeamChsName2(_serverData.MapName, 1);
                            ScoreModel.Team2Name = ClientHelper.GetTeamChsName2(_serverData.MapName, 2);
                            
                        }
                        else
                        {
                            if (AuthView.URL != null)
                            {
                                serverUrl = AuthView.URL;
                                string globalString = serverUrl;
                                Player.retrievedString = globalString;
                                Player.gameId = Globals.GameId;
                            }
                            else
                            {
                                serverUrl = "1";
                            }

                            var response = await httpClient.GetStringAsync(serverUrl);
                            var serverInfo = JsonConvert.DeserializeObject<ServerInfoRoot>(response);

                            // 使用serverInfo中的数据

                            _serverData.Name = serverInfo.ServerInfo.Name;
                            _serverData.Name = string.IsNullOrEmpty(_serverData.Name) ? "未知" : _serverData.Name;

                            _serverData.MapName = serverInfo.ServerInfo.Level;
                            _serverData.MapName = string.IsNullOrEmpty(_serverData.MapName) ? "未知" : _serverData.MapName;

                            _serverData.GameMode = serverInfo.ServerInfo.Mode;


                            // 服务器时间
                            //_serverData.Time = Server.GetServerTime();

                            //////////////////////////////// 服务器数据整理 ////////////////////////////////

                            ScoreModel.ServerName = _serverData.Name;
                            //ScoreModel.ServerTime = PlayerUtil.SecondsToMMSS(_serverData.Time);

                            ScoreModel.ServerMapName = ClientHelper.GetMapChsName2(_serverData.MapName);
                            ScoreModel.ServerMapImg = ClientHelper.GetMapPrevImage2(_serverData.MapName);
                            //test

                            // 最大比分
                            _serverData.MaxScore = Server.GetServerMaxScore();
                            // 队伍1、队伍2分数
                            _serverData.Team1Score = Server.GetTeam1Score();
                            _serverData.Team2Score = Server.GetTeam2Score();

                            //test
                            ScoreModel.ServerGameMode = ClientHelper.GetGameMode2(_serverData.GameMode);

                            ScoreModel.Team1Img = ClientHelper.GetTeam1Image2(_serverData.MapName);
                            ScoreModel.Team2Img = ClientHelper.GetTeam2Image2(_serverData.MapName);

                            ScoreModel.Team1Name = ClientHelper.GetTeamChsName2(_serverData.MapName, 1);
                            ScoreModel.Team2Name = ClientHelper.GetTeamChsName2(_serverData.MapName, 2);



                            /////////////////////////////////////////////////////////////////////////

                            // 服务器数字Id
                            _serverData.GameId = BF1ServerTools.Views.AuthView.gameid233;//服务器id获取

                            ScoreModel.ServerGameId = _serverData.GameId;
                        }
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

                        // ... 其他数据更新
                    }
                    catch (HttpRequestException httpEx)
                    {
                        //MessageBox.Show("http");
                    }
                    catch (Exception ex)
                    {
                        // TODO: 处理其他异常
                    }
                }
                else
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
                    mapname = ScoreModel.ServerMapName;
                    ScoreModel.ServerMapImg = ClientHelper.GetMapPrevImage(_serverData.MapName);

                    if (_serverData.MapName == "未知" || ScoreModel.ServerMapName == "大厅菜单")
                        ScoreModel.ServerGameMode = "未知";
                    else { 
                    ScoreModel.ServerGameMode = ClientHelper.GetGameMode(_serverData.GameMode);
                        mapmode = ScoreModel.ServerGameMode;
                    }

                ScoreModel.Team1Img = ClientHelper.GetTeam1Image(_serverData.MapName);
                ScoreModel.Team2Img = ClientHelper.GetTeam2Image(_serverData.MapName);

                ScoreModel.Team1Name = ClientHelper.GetTeamChsName(_serverData.MapName, 1);
                ScoreModel.Team2Name = ClientHelper.GetTeamChsName(_serverData.MapName, 2);

                // 当服务器模式为征服时，下列数据才有效
                if (ScoreModel.ServerGameMode == "征服" || true)
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
                    if (_serverData.GameId == 0) {
                        _serverData.GameId = BF1ServerTools.Views.AuthView.gameid233;//服务器id获取
                    }
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
        using (var httpClient = new HttpClient())
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

                    if (item.LifeKd == 0 || item.LifeKpm == 0)
                    {
                        item.LifeKd = PlayerUtil.GetLifeKD(item.PersonaId);
                        item.LifeKpm = PlayerUtil.GetLifeKPM(item.PersonaId);
                    }
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
  private void CopyPlayerFields(PlayerData updated, PlayerDataModel item)
{
    item.Rank = updated.Rank;
    item.Clan = updated.Clan;
    item.Admin = updated.Admin;
    item.Vip = updated.Vip;
    item.White = updated.White;
    item.SquadId = updated.SquadId;
    item.SquadId2 = updated.SquadId2;
    item.Kill = updated.Kill;
    item.Dead = updated.Dead;
    item.Kd = updated.Kd;
    item.Kpm = updated.Kpm;
    item.LifeKd = updated.LifeKd;
    item.LifeKpm = updated.LifeKpm;
    item.LifeTime = updated.LifeTime;
    item.Score = updated.Score;
    item.Kit = updated.Kit;
    item.Kit2 = updated.Kit2;
    item.Kit3 = updated.Kit3;
    item.WeaponS0 = updated.WeaponS0;
    item.WeaponS1 = updated.WeaponS1;
    item.WeaponS2 = updated.WeaponS2;
    item.WeaponS3 = updated.WeaponS3;
    item.WeaponS4 = updated.WeaponS4;
    item.WeaponS5 = updated.WeaponS5;
    item.WeaponS6 = updated.WeaponS6;
    item.WeaponS7 = updated.WeaponS7;
}


    /// <summary>
    /// 动态更新 ListView 队伍1
    /// </summary>
    private void UpdateListViewTeam1()
    {
        // 没有玩家，清空列表
        if (PlayerList_Team1.Count == 0)
        {
            if (ListView_PlayerList_Team1.Count != 0)
                ListView_PlayerList_Team1.Clear();

            return;
        }

        // 构建 PersonaId 索引，提升查找性能
        var serverPlayerMap = PlayerList_Team1.ToDictionary(p => p.PersonaId);

        // 构建新列表用于更新 UI 列表
        var updatedList = new List<PlayerDataModel>();

        // 保留并更新仍在线的玩家
        foreach (var item in ListView_PlayerList_Team1)
        {
            if (serverPlayerMap.TryGetValue(item.PersonaId, out var updated))
            {
                CopyPlayerFields(updated, item); // 更新字段
                updatedList.Add(item);           // 保留该项
                serverPlayerMap.Remove(item.PersonaId); // 标记为已处理
            }
        }

        // 添加新玩家
        foreach (var kvp in serverPlayerMap)
        {
            var p = kvp.Value;
            updatedList.Add(new PlayerDataModel
            {
                Rank = p.Rank,
                Clan = p.Clan,
                Name = p.Name,
                PersonaId = p.PersonaId,
                Admin = p.Admin,
                Vip = p.Vip,
                White = p.White,
                SquadId = p.SquadId,
                SquadId2 = p.SquadId2,
                Kill = p.Kill,
                Dead = p.Dead,
                Kd = p.Kd,
                Kpm = p.Kpm,
                LifeKd = p.LifeKd,
                LifeKpm = p.LifeKpm,
                LifeTime = p.LifeTime,
                Score = p.Score,
                Kit = p.Kit,
                Kit2 = p.Kit2,
                Kit3 = p.Kit3,
                WeaponS0 = p.WeaponS0,
                WeaponS1 = p.WeaponS1,
                WeaponS2 = p.WeaponS2,
                WeaponS3 = p.WeaponS3,
                WeaponS4 = p.WeaponS4,
                WeaponS5 = p.WeaponS5,
                WeaponS6 = p.WeaponS6,
                WeaponS7 = p.WeaponS7
            });
        }

        // 替换旧列表内容
        ListView_PlayerList_Team1.Clear();
        foreach (var item in updatedList)
        {
            ListView_PlayerList_Team1.Add(item);
        }

        // 排序
        ListView_PlayerList_Team1.Sort();

        // 设置序号
        for (int i = 0; i < ListView_PlayerList_Team1.Count; i++)
            ListView_PlayerList_Team1[i].Index = i + 1;
    }




    /// <summary>
    /// 动态更新 ListView 队伍2
    /// </summary>
    private void UpdateListViewTeam2()
    {
        if (PlayerList_Team2.Count == 0)
        {
            if (ListView_PlayerList_Team2.Count != 0)
                ListView_PlayerList_Team2.Clear();
            return;
        }

        // 构建快速索引
        var serverPlayerMap = PlayerList_Team2.ToDictionary(p => p.PersonaId);

        // 用于更新的新列表
        var updatedList = new List<PlayerDataModel>();

        // 更新现有项
        foreach (var item in ListView_PlayerList_Team2)
        {
            if (serverPlayerMap.TryGetValue(item.PersonaId, out var updated))
            {
                CopyPlayerFields(updated, item);
                updatedList.Add(item);
                serverPlayerMap.Remove(item.PersonaId);
            }
        }

        // 添加新项
        foreach (var kvp in serverPlayerMap)
        {
            var p = kvp.Value;
            updatedList.Add(new PlayerDataModel
            {
                Rank = p.Rank,
                Clan = p.Clan,
                Name = p.Name,
                PersonaId = p.PersonaId,
                Admin = p.Admin,
                Vip = p.Vip,
                White = p.White,
                SquadId = p.SquadId,
                SquadId2 = p.SquadId2,
                Kill = p.Kill,
                Dead = p.Dead,
                Kd = p.Kd,
                Kpm = p.Kpm,
                LifeKd = p.LifeKd,
                LifeKpm = p.LifeKpm,
                LifeTime = p.LifeTime,
                Score = p.Score,
                Kit = p.Kit,
                Kit2 = p.Kit2,
                Kit3 = p.Kit3,
                WeaponS0 = p.WeaponS0,
                WeaponS1 = p.WeaponS1,
                WeaponS2 = p.WeaponS2,
                WeaponS3 = p.WeaponS3,
                WeaponS4 = p.WeaponS4,
                WeaponS5 = p.WeaponS5,
                WeaponS6 = p.WeaponS6,
                WeaponS7 = p.WeaponS7
            });
        }

        // 替换旧列表
        ListView_PlayerList_Team2.Clear();
        foreach (var item in updatedList)
        {
            ListView_PlayerList_Team2.Add(item);
        }

        // 排序
        ListView_PlayerList_Team2.Sort();

        // 设置 Index
        for (int i = 0; i < ListView_PlayerList_Team2.Count; i++)
        {
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
        // 在清空ListBox之前保存数据
        if (ListBox_PlayerList_Team02.Count != 0)
        {
            // 读取现有数据或初始化新的列表
            List<dynamic> records;
            string filePath = AuthView.F_Auth_Path3;

            if (File.Exists(filePath))
            {
                string existingData = File.ReadAllText(filePath);
                records = JsonConvert.DeserializeObject<List<dynamic>>(existingData) ?? new List<dynamic>();
            }
            else
            {
                records = new List<dynamic>();
            }

            // 创建新记录，包含时间戳
            var newRecord = new
            {
                Timestamp = DateTime.Now,
                Players = ListBox_PlayerList_Team02.Select(player => new { player.Name, player.PersonaId }).ToList()
            };

            // 确保列表不超过20个元素
            if (records.Count >= 20)
            {
                // 移除最老的记录并添加新记录
                records.RemoveAt(0);
            }

            // 添加新记录
            records.Add(newRecord);

            // 序列化整个记录列表并保存
            string json = JsonConvert.SerializeObject(records, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
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
            if (item == null || string.IsNullOrEmpty(item.Name) || item.PersonaId <= 0)
            {
                NotifierHelper.Show(NotifierType.Warning, "选中的玩家信息无效");
                return;
            }
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
            if (item == null || string.IsNullOrEmpty(item.Name) || item.PersonaId <= 0)
            {
                NotifierHelper.Show(NotifierType.Warning, "选中的玩家信息无效");
                return;
            }
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
    private async void MenuItem_Team1_AutoWatch_Click(object sender, RoutedEventArgs e)
    {
         if (ListView_Team1.SelectedItem is PlayerDataModel item)
        {
          await  Autobalance.Autowatch(item.Name);    
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
            if (item == null || string.IsNullOrEmpty(item.Name) || item.PersonaId <= 0)
            {
                NotifierHelper.Show(NotifierType.Warning, "选中的玩家信息无效");
                return;
            }
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
            if (item == null || string.IsNullOrEmpty(item.Name) || item.PersonaId <= 0)
            {
                NotifierHelper.Show(NotifierType.Warning, "选中的玩家信息无效");
                return;
            }
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
    private async void MenuItem_Team2_AutoWatch_Click(object sender, RoutedEventArgs e)
    {
        if (ListView_Team2.SelectedItem is PlayerDataModel item)
        {
          await  Autobalance.Autowatch(item.Name);    
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

    public void InitializeGridViewColumnWidthTracking()
    {
        foreach (GridViewColumn column in GridTeam1.Columns)
        {
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(GridViewColumn.WidthProperty, typeof(GridViewColumn));
            dpd.AddValueChanged(column, new EventHandler(ColumnWidthChanged));
        }

        // 加载现有宽度设置
        LoadColumnWidths(GridTeam1, "Team1");
        foreach (GridViewColumn column in GridTeam2.Columns)
        {
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(GridViewColumn.WidthProperty, typeof(GridViewColumn));
            dpd.AddValueChanged(column, new EventHandler(ColumnWidthChanged));
        }
        // 加载现有宽度设置
        LoadColumnWidths(GridTeam2, "Team2");
    }

        private void ColumnWidthChanged(object sender, EventArgs e)
    {
        GridViewColumn column = sender as GridViewColumn;
        if (column != null)
        {
            // 保存所有列宽
            SaveColumnWidths();
        }
    }

    private void SaveColumnWidths()
    {
        var columnWidths = new Dictionary<string, double>();
        foreach (GridViewColumn column in GridTeam1.Columns)
        {
            if (column.Header != null)
            {

                string key = "Team1_" + column.Header.ToString();
                columnWidths[key] = double.IsNaN(column.Width) ? -1 : column.Width; // 如果NaN则设置为-1
            }
        }
        foreach (GridViewColumn column in GridTeam2.Columns)
        {
            if (column.Header != null)
            {
                string key = "Team2_" + column.Header.ToString();
                columnWidths[key] = double.IsNaN(column.Width) ? -1 : column.Width; // 如果NaN则设置为-1
            }
        }
        File.WriteAllText(AuthView.F_Auth_Path2, JsonConvert.SerializeObject(columnWidths));
    }

    private void LoadColumnWidths(GridView gridView, string teamIdentifier)
    {
        string filePath = AuthView.F_Auth_Path2; // 配置文件路径
        if (File.Exists(filePath))
        {
            var columnWidths = JsonConvert.DeserializeObject<Dictionary<string, double>>(File.ReadAllText(filePath));
            foreach (GridViewColumn column in gridView.Columns)
            {
                string key = teamIdentifier + "_" + column.Header.ToString();
                if (column.Header != null && columnWidths.ContainsKey(key))
                {
                    double width = columnWidths[key];
                    if (width != -1) // 检查是否设置为默认值标识符
                    {
                        column.Width = width;
                    }
                    // 如果是-1，则不设置宽度，保持默认值
                }
            }
        }
    }
}
