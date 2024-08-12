using BF1ServerTools.QQ;
using BF1ServerTools.QQ.RespJson;
using BF1ServerTools.API;
using BF1ServerTools.SDK;
using BF1ServerTools.SDK.Core;
using BF1ServerTools.Data;
using BF1ServerTools.Utils;
using BF1ServerTools.Helper;
using BF1ServerTools.Configs;

using Websocket.Client;

using System.Drawing;
using System.Drawing.Imaging;
using System.Reactive.Linq;

using Size = System.Drawing.Size;
using Point = System.Drawing.Point;
using BF1ServerTools.SDK.Data;
using System;
using System.Runtime.InteropServices;
using static BF1ServerTools.RES.Data.MapData;
using BF1ServerTools.Windows;
using System.Collections.Generic;
using BF1ServerTools.API.RespJson;
using BF1ServerTools.RES;
using System.Collections;
using System.ComponentModel;
using BF1ServerTools.RES.Img;
using System.Net.Sockets;
using System.Net.Http;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Numerics;
using System.Collections.Concurrent;
using System.Windows.Shapes;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace BF1ServerTools.Views;

/// <summary>
/// RobotView.xaml 的交互逻辑
/// </summary>
public partial class RobotView : UserControl
{  /// <summary>
///   换图播报控制
/// </summary>
    public static bool showflag { get; set; }
    public static bool liveflag { get; set; } = true;//监控存活判断

    public static int jiankongflag = 0; // 1表示启动切换地图
    public static bool attackwinflag { get; set; } = false;//进攻获胜判断

    private Queue<PlayerData> excludeList = new Queue<PlayerData>(); // 使用队列来管理排除名单 
    static Queue<int> team1Scores = new Queue<int>();//旗帜得分
    static Queue<int> team2Scores = new Queue<int>();


    ////////////////////////////////////////////////////////




    public RobotView()
    {

        InitializeComponent();
        InitializeVoteTimer();
        _logUpdateTimer = new Timer(200); // 每200毫秒更新一次UI
        _logUpdateTimer.Elapsed += ProcessLogQueue;
        _logUpdateTimer.Start();

    }
    private void Reportmapinfo_Click(object sender, RoutedEventArgs e)
    {
        // 调用播报信息方法
        showflag = true;
        _ = Task.Run(async () =>
        {
            
                //await Xyz();
            
        });
    }
    public async Task Xyz()
    {
        List<PointXZ> polygon = new List<PointXZ>
        {
      
        };
        while (true)
        {
            try
            {
                var apiResponse = await GetPlayerListAsync();
                AppendLog("Code: " + apiResponse.Code+"\n");

                foreach (var player in apiResponse.Data)
                {
                   
                    AppendLog("Name: " + player.Name);
                        PointXZ point = new PointXZ(player.X, player.Z);
                        bool isInside = Polygon.IsPointInPolygon(point, polygon);
                        AppendLog($"x:{player.X} y:{player.Y} z:{player.Z}{isInside}\n");
                        
                        Application.Current.Dispatcher.Invoke(() =>
                    {
                        NotifierHelper.Show(NotifierType.Success, $"x:{player.X} y:{player.Y} z:{player.Z}{isInside}");
                    });
                    
                }
            }
            catch (Exception e)
            {
                AppendLog("Request failed: " + e.Message+"\n");
            }
            await Task.Delay(1000);
        }
    }
    /// <summary>
    /// 启动定时平衡
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private bool isTimerRunning = false;
   
    
    private bool autoallchange = false;
    private void Button_RunAutoAllchange_Click(object sender, RoutedEventArgs e)
    {
        if (!autoallchange)
        {
            AutoAllchange();
            autoallchange = true;
        }
        else
        {
            NotifierHelper.Show(NotifierType.Information, "压家检测已经启动了");
        }
    }
 
    // 滑块值改变时的事件处理器
   
    private void Slider_ValueChangedAllchange(object sender, RoutedPropertyChangedEventArgs<double> e)
    {

        if (labelAllchange != null) // 检查控件是否为null
        {
            double newValue = e.NewValue; // 获取滑块的当前值
            labelAllchange.Text = $"启动压家换边的分数上限[{newValue}]（仅征服）（高于该分数不启动全部换边）"; // 更新标签内容
        }
    }
    private void Slider_Valuedelayallchange(object sender, RoutedPropertyChangedEventArgs<double> e)
    {

        if (labeldelayallchange != null) // 检查控件是否为null
        {
            double newValue = e.NewValue; // 获取滑块的当前值
            labeldelayallchange.Text = $"延迟（{newValue}）秒"; // 更新标签内容
        }
    }
    private DispatcherTimer voteTimer;
    private void InitializeVoteTimer()
    {
        voteTimer = new DispatcherTimer();
        voteTimer.Tick += VoteTimer_Tick;
        // 默认不启动定时器
        voteTimer.IsEnabled = false;
    }

    private void Slider_ValueChangedvote(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var interval = (int)slidervote.Value;
        labelCurrentdvote.Text = $"播报间隔为 {interval} 分钟 0=永不自动播报";

        if (interval > 0)
        {
            voteTimer.Interval = TimeSpan.FromMinutes(interval);
            voteTimer.Start();
        }
        else
        {
            voteTimer.Stop();
        }
    }

    private void VoteTimer_Tick(object sender, EventArgs e)
    {
        // 滑块的值不是0，则设置 showflag 为1
        if (slidervote.Value > 0)
        {
            showflag = true;
        }
    }



    // 地图投票系统
    private async void Autochangemap()
    {

        List<PlayerData> playerListbegin = Player.GetPlayerList(); // 获取当前所有玩家的列表
        
        bool isPlayerInTeam3 = playerListbegin.Any(player => player.PersonaId == Globals.PersonaId && player.TeamId == 0);
        int flag = 0;
        int flagthen = 0;
        if (isPlayerInTeam3 && false)
        {
            // 在UI线程中显示通知
            Application.Current.Dispatcher.Invoke(() =>
            {
                NotifierHelper.Show(NotifierType.Success, "以观众模式运行");
            });

            // 在后台线程中异步启动监控，不等待其完成
            _ = Task.Run(async () =>
            {
                try
                {
                    await ContinuousMonitoring();
                }
                catch (OperationCanceledException)
                {
                    // 监控被取消了

                }
                catch (Exception ex)
                {
                    // 处理其他类型的异常
                    MessageBox.Show($"监控过程中出错: {ex.Message}");
                }
            });
        }
        else
        {
            //NotifierHelper.Show(NotifierType.Success, "以玩家模式运行");
            //flag = await QueryRecordWindow.GetGameCount(Globals.PersonaId);
           // await Task.Delay(500);
            //flagthen = flag;
        }

        ChatInputWindow.SendChsToBF1Chat("使用 vote 地图名称（拼音）来投票\n投票示例 vote yamian");

        var mapDetailsResult = await BF1API.GetFullServerDetails(Globals.SessionId, Globals.GameId);
        if (mapDetailsResult.IsSuccess)
        {
            var fullServerDetailslocal = JsonHelper.JsonDese<BF1ServerTools.API.RespJson.FullServerDetails>(mapDetailsResult.Content);
            var mapList = new StringBuilder();

            foreach (var item in fullServerDetailslocal.result.serverInfo.rotation)
            {
                string mapName = ChsUtil.ToSimplified(item.mapPrettyName);
                string mapMode = ChsUtil.ToSimplified(item.modePrettyName);

                mapList.AppendLine($"{mapName} - {mapMode}");
            }


        }
        if (!mapDetailsResult.IsSuccess)
        {
            MessageBox.Show("获取服务器详情失败。请稍后重试");
            autochange = false;
            return;
        }

        var fullServerDetails = JsonHelper.JsonDese<FullServerDetails>(mapDetailsResult.Content);

         Autochangegamemap();//不等待
        // 中文到拼音映射
        var chineseToPinyin = new Dictionary<string, List<string>>
{
    {"索姆河", new List<string> {"suomuhe"}},
    {"決裂", new List<string> {"jueli"}},
    {"勃魯西洛夫關口", new List<string> {"boluxiluofuguankou", "guankou"}},
    {"加利西亞", new List<string> {"jialixiya"}},
    {"龐然闇影", new List<string> {"pangrananying"}},
    {"法烏克斯要塞", new List<string> {"fawukesiyaosai", "yaosai"}},
    {"亞眠", new List<string> {"yamian"}},
    {"阿奇巴巴", new List<string> {"aqibaba", "2788"}},
    {"凡爾登高地", new List<string> {"fanerdeng", "deng", "den", "fanerden"}},
    {"海麗絲岬", new List<string> {"hailisijia"}},
    {"察里津", new List<string> {"chalijin"}},
    {"蘇伊士", new List<string> {"suyishi"}},
    {"阿爾貢森林", new List<string> {"aergong", "senlin"}},
    {"澤布呂赫", new List<string> {"zebulvhe", "zebuyuhe"}},
    {"武普庫夫山口", new List<string> {"wupukufushankou", "shankou"}},
    {"蘇瓦松", new List<string> {"suwasong"}},
    {"窩瓦河", new List<string> {"wowahe"}},
    {"西奈沙漠", new List<string> {"xinaishamo", "xinai"}},
    {"流血宴廳", new List<string> {"liuxueyantin", "yantin","liuxueyanting", "yanting"}},
    {"聖康坦的傷痕", new List<string> {"shengkangtandeshanghen", "shengkangtan"}},
    {"法歐堡", new List<string> {"faoubao"}},
    {"帝國邊境", new List<string> {"diguobianjing", "bianjing"}},
    {"攻佔托爾", new List<string> {"gongzhantuoer", "tuoer"}},
    {"格拉巴山", new List<string> {"gelabashan", "lababashan"}},
    {"尼維爾之夜", new List<string> {"niweierzhiye"}},
    {"阿爾比恩", new List<string> {"aerbien"}},
    { "黑爾戈蘭灣", new List<string> {"heiergelanwan"}},
    {"卡波雷托", new List<string> {"kaboleituo"}},
    {"帕斯尚爾", new List<string> {"pasishanger"}},
    {"剃刀邊緣", new List<string> {"tidaobianyuan"}},
    {"倫敦的呼喚：災禍", new List<string> {"zaihuo"}},
    {"倫敦的呼喚：夜襲", new List<string> {"yexi"}}

};



        var mapNamesToId = new Dictionary<string, int>();
        var mapIdToName = new Dictionary<int, string>();

        int mapId = 0;
        foreach (var item in fullServerDetails.result.serverInfo.rotation)
        {
            string mapNameChinese = item.mapPrettyName;
            if (chineseToPinyin.TryGetValue(mapNameChinese, out var pinyinList))
            {
                foreach (var pinyin in pinyinList)
                {

                    mapNamesToId[pinyin] = mapId;
                }
                mapIdToName[mapId] = mapNameChinese; // 使用中文显示
            }

            mapId++;
        }

        Dictionary<int, int> votes = new Dictionary<int, int>();
        Dictionary<string, bool> userHasVoted = new Dictionary<string, bool>();

        while (true)
        {
            if (!isPlayerInTeam3&&false)
            {
                flagthen = await QueryRecordWindow.GetGameCount(Globals.PersonaId);
            }
            else
            {
                //flagthen = flag + jiankongflag;
                //NotifierHelper.Show(NotifierType.Success, "运行");
            }
            string lastSender = Chat.GetLastChatSender(out _);
            string lastContent = Chat.GetLastChatContent(out _).ToLower();

           
            if (!string.IsNullOrEmpty(lastContent) && lastContent.StartsWith("vote "))
            {
                // 去除开头的 "vote " 并获取投票名称部分
                string voteContent = lastContent.Substring(5).Trim().ToLower();

                // 去除所有空格
                string voteName = voteContent.Replace(" ", "");

                // 遍历mapNamesToId，忽略投票名称中的空格进行匹配
                if (!string.IsNullOrEmpty(voteName))
                {
                    foreach (var entry in mapNamesToId)
                    {
                        string keyWithoutSpaces = entry.Key.Replace(" ", "").ToLower();
                        if (keyWithoutSpaces == voteName)
                        {
                            var localMapId = entry.Value;

                            if (!userHasVoted.ContainsKey(lastSender))
                            {
                                votes[localMapId] = votes.TryGetValue(localMapId, out var currentCount) ? currentCount + 1 : 1;
                                userHasVoted[lastSender] = true;
                            }
                            break;
                        }
                    }
                }
            }

            

            if (votes.Count > 0 && showflag)
            {
                
                    // 获取最高票数
                    var maxVote = votes.Max(v => v.Value);

                    // 获取所有得票最高的地图
                    var highestVotedMaps = votes.Where(v => v.Value == maxVote).ToList();

                    // 创建包含所有地图名称和得票数的消息
                    var allMapsVotes = votes.Select(v => $"{mapIdToName[v.Key]}: {v.Value}票").ToList();
                    string allMapsVotesMessage = string.Join(", ", allMapsVotes);

                    // 创建得票最多的地图的消息
                    var mapNames = highestVotedMaps.Select(v => mapIdToName[v.Key]).ToList();
                    string highestMaps = string.Join(", ", mapNames);
                    string highestVoteMessage = highestVotedMaps.Count == 1
                        ? $"得票最多的地图是: {highestMaps}，共{maxVote}票。"
                        : $"得票最多的地图有: {highestMaps}，每张地图都得到了{maxVote}票。";

                    // 合并两条消息
                    string combinedMessage = highestVoteMessage + "\n所有地图的得票数: " + allMapsVotesMessage + "\n使用 vote 地图名称（拼音）来投票\n投票示例 vote yamian";

                    // 发送消息
                    ChatInputWindow.SendChsToBF1Chat(combinedMessage);
                    NotifierHelper.Show(NotifierType.Success, combinedMessage);

               

            }
            if(showflag && votes.Count == 0) 
            {
                ChatInputWindow.SendChsToBF1Chat("使用 vote 地图名称（拼音）来投票\n投票示例 vote yamian\n地图名称中不要有空格");
            }

            if (showflag)
            { showflag = false; }

            if (votes.Count > 0)
            {
                var highestVote = votes.Aggregate((l, r) => l.Value > r.Value ? l : r);
                int mapIdlocal = highestVote.Key; // 直接使用得票最多的mapId              

                votechangemap = mapIdlocal;
                string mapName = mapIdToName[mapIdlocal];
               
            }
            if(clearvote)
            {

                votes.Clear();
                userHasVoted.Clear();// 清空投票，开始新一轮
                clearvote = false;
            }

            await Task.Delay(200); // 每200毫秒检查一次聊天
        }
    }
    static int EstimateNumberOfPoints(Queue<int> scores)
    {
        // 确保队列不为空
        if (scores.Count == 0)
            return 0;

        // 转换为数组以访问元素
        int[] scoresArray = scores.ToArray();

        // 计算最后一个元素和第一个元素的差值
        int scoreIncrease = scoresArray[scoresArray.Length - 1] - scoresArray[0];
        return scoreIncrease / 3;  // 每5秒1分，15秒内每个点最多计3次
    }
    static int EstimateNumberOfPointsquick(Queue<int> scores)
    {
        // 确保队列不为空
        if (scores.Count == 0)
            return 0;

        // 转换为数组以访问元素
        int[] scoresArray = scores.ToArray();

        // 计算最后一个元素和第一个元素的差值
        int scoreIncrease = scoresArray[scoresArray.Length - 1] - scoresArray[scoresArray.Length - 11];
        return scoreIncrease ;  
    }
    //压家
    private async void AutoAllchange()
    {
        NotifierHelper.Show(NotifierType.Information, "压家检测已启动");
        if (monitoringcount < 2)
        {
            Task.Run(() => MonitorFlags());
            await Task.Delay(6000);
            Task.Run(() => MonitorFlags());
        }

        bool delayallchangeflag = delayallchange.IsChecked ?? false;
        double delaytimeflag = delayallchangetime != null ? delayallchangetime.Value : 0;
        double scoreflag = sliderAllchange != null ? sliderAllchange.Value : 700;
        var maptopoint = new Dictionary<string, int>
{

    {"决裂", 5},
    {"勃鲁西洛夫关口", 4},
    {"加利西亚", 5},
    {"庞然暗影", 6},
    {"法乌克斯要塞", 5},
    {"亚眠", 6},
    {"阿奇巴巴", 5},
    {"凡尔登高地", 5},

    {"察里津", 3},
    {"苏伊士", 5},
    {"阿尔贡森林", 5},

    {"武普库夫山口", 7},
    {"苏瓦松", 5},
    {"窝瓦河", 7},
    {"西奈沙漠", 7},
    {"流血宴厅", 5},
    {"圣康坦的伤痕", 6},
    {"法欧堡", 7},
    {"帝国边境", 7},
    {"攻占托尔", 5},
    {"格拉巴山", 5},
    {"尼维尔之夜", 6},
    {"阿尔比恩", 7},
     

    {"帕斯尚尔", 5},

};
        //获取地图列表，便于重开
        //NotifierHelper.Show(NotifierType.Information, "正在尝试获取地图列表");
        //var mapItems = await GetServerMapList();
        int team1Score = 0;
        int team2Score = 0;
        int time = 495;
        int countflag = 0;
    BEGIN:     do
        { // 开始计时
          // 创建一个Stopwatch实例
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int point = 0;
           
            do
            {
                if (maptopoint.TryGetValue(ScoreView.mapname, out point))
                {
                    break;
                }
                else
                {
                    originpoint = 0;
                    await Task.Delay(10000);

                }
            } while (true);
            originpoint = point;
            if ((onepointmode.IsChecked ?? false) && ScoreView.mapname != "察里津")
            {
                point--;
               
            }
          
           
         
           
           
                delayallchangeflag = delayallchange.IsChecked ?? false;
                if (Server.GetTeam1Score() > scoreflag || Server.GetTeam2Score() > scoreflag)
                { 
                    goto END; 
                }
              


               
                    

               
                if ((point == team1Flags || point == team2Flags) && autoallchange && Server.GetTeam1FlagScore() != 0 && Server.GetTeam2FlagScore() != 0)
                {
                    if (delayallchangeflag)
                    {
                        await Task.Delay((int)((delaytimeflag - 14) * 1000));
                        delayallchangeflag = false;
                        goto BEGIN;
                    }
                    delayallchangeflag = delayallchange.IsChecked ?? false;
                    break;
                }
                delayallchangeflag = delayallchange.IsChecked ?? false;
            
            await Task.Delay(time);
            // 停止计时
            stopwatch.Stop();
            //NotifierHelper.Show(NotifierType.Information, $"{stopwatch.ElapsedMilliseconds}");
            if (time >= 510)
            {
                time = time - 3;
            }
          // NotifierHelper.Show(NotifierType.Information, $"{team1Flags}||{team2Flags}");
        } while (true);
        List<PlayerData> playerList = Player.GetPlayerList(); // 获取当前所有玩家的列表
        int count = playerList.Count(p => p.PersonaId != 0);
        if (count < 60 && count > 0 && !(autooconquerplayerchange.IsChecked ?? false))
        { await ChangeAllPlayers(); }
        else
        {
            NotifierHelper.Show(NotifierType.Information, "人数过多，正在重开");
            int? mapid = await FindMapId(ScoreView.mapname, ScoreView.mapmode);
            if (mapid.HasValue)
            {

                var result = await BF1API.RSPChooseLevel(Globals.SessionId, Globals.PersistedGameId, (int)mapid);
                if (result.IsSuccess)
                {
                    NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换服务器 {Globals.GameId} 地图 成功");

                }
                else
                {
                    NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换服务器 {Globals.GameId} 地图 失败");
                    MessageBox.Show("重开失败");
                }
            }
            else
            {
                MessageBox.Show("程序出错");
            }
        }

    END: 
        team1Score = Server.GetTeam1FlagScore();
        team2Score = Server.GetTeam2FlagScore();
        team1Scores.Enqueue(team1Score);
        team2Scores.Enqueue(team2Score);
        await Task.Delay(13000);
        team1Flags = 0;
        team2Flags = 0;
        while (true)
        {   await Task.Delay(6000);
            team1Score = Server.GetTeam1FlagScore();
            team2Score = Server.GetTeam2FlagScore();
            if(team1Score < team1Scores.Peek() || team2Score< team2Scores.Peek())
            {
                await Task.Delay(60000);
                goto BEGIN;
            }
        }
    }
    public static int team1Flags = 0;
    public static int team2Flags = 0;
    static bool monitoring = true;
    static int originpoint = 0;
    public static int monitoringcount = 0;

    public static async Task MonitorFlags()
    {   
        double intervalSeconds = 0.1;
        int intervalMilliseconds = (int)(intervalSeconds * 1000);
        double monitoringPeriodSeconds = 12.0; // 总监测时间

        monitoringcount++;



        List<ScoreEntry> scoreTimelineTeam1 = new List<ScoreEntry>();
        List<ScoreEntry> scoreTimelineTeam2 = new List<ScoreEntry>();

        while (monitoring)
        {
            var maptopoint = new Dictionary<string, int>
{

    {"决裂", 5},
    {"勃鲁西洛夫关口", 4},
    {"加利西亚", 5},
    {"庞然暗影", 6},
    {"法乌克斯要塞", 5},
    {"亚眠", 6},
    {"阿奇巴巴", 5},
    {"凡尔登高地", 5},

    {"察里津", 3},
    {"苏伊士", 5},
    {"阿尔贡森林", 5},

    {"武普库夫山口", 7},
    {"苏瓦松", 5},
    {"窝瓦河", 7},
    {"西奈沙漠", 7},
    {"流血宴厅", 5},
    {"圣康坦的伤痕", 6},
    {"法欧堡", 7},
    {"帝国边境", 7},
    {"攻占托尔", 5},
    {"格拉巴山", 5},
    {"尼维尔之夜", 6},
    {"阿尔比恩", 7},


    {"帕斯尚尔", 5},

};
            int point = 0;

            do
            {
                if (maptopoint.TryGetValue(ScoreView.mapname, out point))
                {
                    break;
                }
                else
                {
                    originpoint = 0;
                    await Task.Delay(10000);

                }
            } while (true);
            originpoint = point;
            Dictionary<double, int> scoring = new Dictionary<double, int>();
            switch (originpoint) {
            case 3: scoring.Add(5, 1);
                    scoring.Add(2.75, 2);
                    scoring.Add(0.5, 3);
                    break;
            case 4:scoring.Add(5, 1);
                   scoring.Add(3.5, 2);
                    scoring.Add(2, 3);
                    scoring.Add(0.5, 4);
                    break;
                case 5: scoring.Add(5, 1);
                    scoring.Add(3.875, 2);
                    scoring.Add(2.75,3);
                    scoring.Add(1.625, 4);
                    scoring.Add(0.5, 5);
                    break;
                case 6:scoring.Add(5, 1);
                       scoring.Add(4.1,2);
                    scoring.Add(3.2, 3);
                    scoring.Add(2.3,4);
                    scoring.Add(1.4,5);
                    scoring.Add(0.5, 6);
                    break;
                case 7:scoring.Add(5, 1);
                    scoring.Add(4.25, 2);
                    scoring.Add(3.5, 3);
                    scoring.Add(2.75, 4);
                    scoring.Add(2, 5);
                    scoring.Add(1.25, 6);
                    scoring.Add(0.5, 7);
                    break;
                default:
                    await Task.Delay(3000);
                    continue;             
                };
            // 清空历史记录
            scoreTimelineTeam1.Clear();
            scoreTimelineTeam2.Clear();

            // 记录初始得分
            int initialScoreTeam1 = Server.GetTeam1FlagScore();
            int initialScoreTeam2 = Server.GetTeam2FlagScore();
            scoreTimelineTeam1.Add(new ScoreEntry { Score = 0, Time = DateTime.Now });
            scoreTimelineTeam2.Add(new ScoreEntry { Score = 0, Time = DateTime.Now });

            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalSeconds < monitoringPeriodSeconds)
            {
                await Task.Delay(intervalMilliseconds);

                int currentScoreTeam1 = Server.GetTeam1FlagScore();
                int currentScoreTeam2 = Server.GetTeam2FlagScore();
                scoreTimelineTeam1.Add(new ScoreEntry { Score = currentScoreTeam1 - initialScoreTeam1, Time = DateTime.Now });
                scoreTimelineTeam2.Add(new ScoreEntry { Score = currentScoreTeam2 - initialScoreTeam2, Time = DateTime.Now });
                initialScoreTeam1 = currentScoreTeam1;
                initialScoreTeam2 = currentScoreTeam2;
            }
            int flagsCapturedTeam1 = -1;
            int flagsCapturedTeam2 = -1;
            foreach (KeyValuePair<double, int> kvp in scoring)
            {
                var newline = DeepCopyList(scoreTimelineTeam1);
                int a = CalculateFlags(newline, kvp.Key);
                if (a == 1)
                {
                    var newline2 = DeepCopyList(scoreTimelineTeam1);
                    int b = 0;
                    for (int index = 0; index < newline2.Count - 1; index++)
                    {
                        if (newline2[index].Score > 0)
                        {
                            b++;
                        }
                    }
                    if (b >= (int)(12 / kvp.Key))
                    {
                        flagsCapturedTeam1 = kvp.Value;
                    }
                    
                }
                newline = DeepCopyList(scoreTimelineTeam2);
                a = CalculateFlags(newline, kvp.Key);
                if (a == 1 )
                {
                    var newline2 = DeepCopyList(scoreTimelineTeam2);
                    int b = 0;
                    for (int index = 0; index < newline2.Count - 1; index++)
                    {
                        if (newline2[index].Score > 0)
                        {
                            b++;
                        }
                    }
                    if (b >= (int)(12 / kvp.Key))
                    {
                        flagsCapturedTeam2 = kvp.Value;
                    }
                }
               

            }
            
               

                // 更新全局变量
                if (flagsCapturedTeam1 != -1)
                {
                    Interlocked.Exchange(ref team1Flags, flagsCapturedTeam1);
                }
                if (flagsCapturedTeam2 != -1)
                {
                    Interlocked.Exchange(ref team2Flags, flagsCapturedTeam2);
                }
            
        }
    }

    public class ScoreEntry
    {
        public int Score { get; set; }
        public DateTime Time { get; set; }
        public ScoreEntry DeepCopy()
        {
            return new ScoreEntry { Score = this.Score, Time = this.Time };
        }
    }
    public static List<ScoreEntry> DeepCopyList(List<ScoreEntry> originalList)
    {
        List<ScoreEntry> newList = new List<ScoreEntry>();
        foreach (var item in originalList)
        {
            newList.Add(item.DeepCopy());
        }
        return newList;
    }

    public static int CalculateFlags(List<ScoreEntry> scoreTimeline,double time)
    {
        int flags = 0;

        for (int index = 0; index < scoreTimeline.Count - 1; index++)
        {
            while (scoreTimeline[index].Score > 0)
            {
                // 获取分钟、秒和毫秒
                int minutes = scoreTimeline[index].Time.Minute;
                int seconds = scoreTimeline[index].Time.Second;
                int milliseconds = scoreTimeline[index].Time.Millisecond;

                // 格式化并显示分钟、秒和毫秒
                string formattedTime = string.Format("{0:D2}:{1:D2}.{2:D3}", minutes, seconds, milliseconds);
                //AppendLog($"最初{index}{formattedTime}当前使用间隔{time}");
                flags++;
                scoreTimeline[index].Score--;
                scoreTimeline = ClearScore(scoreTimeline, index,time);
                //AppendLog("\n");
            }
        }

        return flags;
    }

    public static List<ScoreEntry> ClearScore(List<ScoreEntry> scoreTimeline, int index,double time)
    {
        if (index + 1 >= scoreTimeline.Count)
        {
            return scoreTimeline;
        }

        var startTime = scoreTimeline[index].Time;
        double timeDiff = 1000;
        int nextIndex = -1;

        for (int i = index + 1; i < scoreTimeline.Count; i++)
        {
            double timeDiff2 = Math.Abs((scoreTimeline[i].Time - startTime).TotalSeconds);
            if (scoreTimeline[i].Score > 0 && timeDiff2 >= time - 0.3 && timeDiff2 <= time + 0.3 && timeDiff2 < timeDiff)
            {
                timeDiff = timeDiff2;
                nextIndex = i;
            }
        }

        if (nextIndex != -1)
        {
            // 获取分钟、秒和毫秒
            int minutes = scoreTimeline[nextIndex].Time.Minute;
            int seconds = scoreTimeline[nextIndex].Time.Second;
            int milliseconds = scoreTimeline[nextIndex].Time.Millisecond;

            // 格式化并显示分钟、秒和毫秒
            string formattedTime = string.Format("{0:D2}:{1:D2}.{2:D3}", minutes, seconds, milliseconds);
            //AppendLog($"递归{nextIndex} {formattedTime}当前使用间隔{time}");
            scoreTimeline[nextIndex].Score--;
            scoreTimeline = ClearScore(scoreTimeline, nextIndex, time);
        }

        return scoreTimeline;
    }
    // 查找地图 ID 的方法
    public async Task<int?> FindMapId(string mapName, string mapMode)
    {
        var mapItems = await GetServerMapList(); // 获取地图列表

        var mapItem = mapItems.FirstOrDefault(item =>
            item.MapName.Equals(mapName, StringComparison.OrdinalIgnoreCase) &&
            item.MapMode.Equals(mapMode, StringComparison.OrdinalIgnoreCase));

        return mapItem?.MapId;
    }
    
    

    private DispatcherTimer timer;
    /// <summary>
    /// 启动投票换图服务
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private Thread autoChangeMapThread; // 定义一个线程变量来控制线程的创建
    private bool autochange;
    private void Button_RunWebsocketServer_Click(object sender, RoutedEventArgs e)
    {


        // 检查线程是否已经存在并且正在运行，如果不是，则创建并启动线程
        if (!autochange)
        {
            NotifierHelper.Show(NotifierType.Success, "投票换图已启动");
            autoChangeMapThread = new Thread(Autochangemap)
            {
                Name = "auto changemap",
                IsBackground = true
            };
            autoChangeMapThread.Start();
            autochange = true;

        }
        else { NotifierHelper.Show(NotifierType.Information, "投票换图已经运行了"); }
    }
    /// <summary>
    /// 停止自动平衡
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_StopWebsocketServer_Click(object sender, RoutedEventArgs e)
    {
        if (timer != null)
        {
            timer.Stop();
            NotifierHelper.Show(NotifierType.Information, "已停止自动平衡");
            isTimerRunning = false;
        }
      
        //测试用

        //ChatInputWindow.SendChsToBF1Chat("你好");
        // 执行移动

        //ShowServerMapList();
    }
    private async void Button_Allchange_Click(object sender, RoutedEventArgs e)
    {
        await ChangeAllPlayers();
    }
    //获取地图列表
    private async void shuafenfu(object sender, RoutedEventArgs e)
    {
        NotifierHelper.Show(NotifierType.Information, "正在尝试获取地图列表");
        await ShowServerMapList();

    }
    bool Changerun = false;
    private async void changemap(object sender, RoutedEventArgs e)
    {
        if (Changerun)
        {
            NotifierHelper.Show(NotifierType.Information, "换图已经启动了");
        }
        if (!Changerun)
        {
            Changerun = true;
            NotifierHelper.Show(NotifierType.Information, "已启动换图");
             XPFARM();
        }
       
    }
    private object _draggedItem;

    private void MapListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var listView = sender as ListView;
        var position = e.GetPosition(listView);
        var result = VisualTreeHelper.HitTest(listView, position);

        if (result != null && result.VisualHit.FindAncestorOrSelf<CheckBox>() == null)
        {
            _draggedItem = GetItemAt(position);
            if (_draggedItem != null)
            {
                DragDrop.DoDragDrop(listView, _draggedItem, DragDropEffects.Move);
            }
        }
    }

    private object GetItemAt(System.Windows.Point position)
    {
        var hitTestResult = VisualTreeHelper.HitTest(MapListView, position);
        if (hitTestResult != null)
        {
            var target = hitTestResult.VisualHit;
            while (target != null && !(target is ListViewItem))
            {
                target = VisualTreeHelper.GetParent(target);
            }
            return target != null ? ((ListViewItem)target).DataContext : null;
        }
        return null;
    }

    private void MapListView_Drop(object sender, DragEventArgs e)
    {
        var listView = sender as ListView;
        var targetItem = GetItemAt(e.GetPosition(listView));

        if (_draggedItem != null && targetItem != null && _draggedItem != targetItem)
        {
            var items = listView.ItemsSource as IList;
            if (items != null)
            {
                int oldIndex = items.IndexOf(_draggedItem);
                int newIndex = items.IndexOf(targetItem);

                items.RemoveAt(oldIndex);
                items.Insert(newIndex, _draggedItem);

                listView.Items.Refresh();
            }
        }
    }
    public class MapItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string MapName { get; set; }
        public string MapMode { get; set; }
        public int MapId { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string DisplayText => $"{MapName} - {MapMode}";
    }
    //获取地图列表
    public async Task<List<MapItem>> GetServerMapList()
    {

        if (string.IsNullOrEmpty(Globals.SessionId))
        {
            MessageBox.Show("会话 ID 为空，无法获取地图列表。");
            return null;
        }
        var mapNamesToId = await CreateMapNamesToIdMapAsync();
        var result = await BF1API.GetFullServerDetails(Globals.SessionId, Globals.GameId);
        if (result.IsSuccess)
        {
            var fullServerDetails = JsonHelper.JsonDese<BF1ServerTools.API.RespJson.FullServerDetails>(result.Content);
            var mapItems = new List<MapItem>();
            int mapId = 0;
            foreach (var item in fullServerDetails.result.serverInfo.rotation)
            {
                mapItems.Add(new MapItem
                {
                    MapName = ChsUtil.ToSimplified(item.mapPrettyName),
                    MapMode = ChsUtil.ToSimplified(item.modePrettyName),
                    MapId = mapId++  // 分配并自增 mapId
                });
            }

            return mapItems;
        }
        else
        {
            MessageBox.Show("获取服务器详情失败。");
            return null;
        }
    }
    public async Task ShowServerMapList()
    {

        if (string.IsNullOrEmpty(Globals.SessionId))
        {
            MessageBox.Show("会话 ID 为空，无法获取地图列表。");
            return;
        }
        var mapNamesToId = await CreateMapNamesToIdMapAsync();
        var result = await BF1API.GetFullServerDetails(Globals.SessionId, Globals.GameId);
        if (result.IsSuccess)
        {
            var fullServerDetails = JsonHelper.JsonDese<BF1ServerTools.API.RespJson.FullServerDetails>(result.Content);
            var mapItems = new List<MapItem>();
            int mapId = 0;
            foreach (var item in fullServerDetails.result.serverInfo.rotation)
            {
                mapItems.Add(new MapItem
                {
                    MapName = ChsUtil.ToSimplified(item.mapPrettyName),
                    MapMode = ChsUtil.ToSimplified(item.modePrettyName),
                    MapId = mapId++  // 分配并自增 mapId
                });
            }

            // 确保在UI线程更新ListView
            Dispatcher.Invoke(() =>
            {
                MapListView.ItemsSource = mapItems;
            });
        }
        else
        {
            MessageBox.Show("获取服务器详情失败。");
        }
    }

    private static int count = 100;
    static int GetRandomResult()
    {
        Random random = new Random();
        return random.Next(2) == 0 ? 0 :count;  // 50%的机会返回0，50%的机会返回100
    }
    private CancellationTokenSource monitoringCts = new CancellationTokenSource();//监控控制
                                                                                  // 监控玩家场数变化
    public async Task StartMonitoringTopPlayersGameCount(CancellationToken cancellationToken)
    {
        const int checkInterval = 200;
        // 使用 Dispatcher 来确保在 UI 线程上访问 UI 元件
        bool onlyscoreflag = false;
        Application.Current.Dispatcher.Invoke(() =>
        {
            onlyscoreflag = onlyscore.IsChecked ?? false;
        });
        if (onlyscoreflag) 
        {
            while (!cancellationToken.IsCancellationRequested || liveflag)
            {
                if (ScoreView.mapmode == "征服" && (Server.GetTeam1Score() >= 993 || Server.GetTeam2Score() >= 993) && Server.GetTeam1Score()<2001 && Server.GetTeam2Score() <2001)
                {
                    //MessageBox.Show($"{Server.GetTeam1Score}+++{Server.GetTeam2Score()}");
                    if (cancellationToken.IsCancellationRequested)
                    { return; }
                    if (!liveflag)
                    {
                        return;
                    }
                    jiankongflag = 1;
                    return;
                    
                }
                await Task.Delay(checkInterval, cancellationToken);
            }
            
        }
        var playerList = Player.GetPlayerList()
                                .Where(player => (player.TeamId == 1 || player.TeamId == 2) && player.PersonaId != 0)
                                .ToList();
        bool isPlayerInTeam0 = playerList.Any(player => player.PersonaId == Globals.PersonaId && player.TeamId == 0);
        // 确保有玩家参与
        if (playerList.Count == 0)
        {
            return; // 直接返回，因为没有玩家
        }
        
        // 获取前三名玩家进行监控
        var topPlayers = playerList.GroupBy(p => p.TeamId)
                                   .SelectMany(g => g.OrderByDescending(p => p.Rank).Take(3))
                                   .ToList();
        AppendLog("参照玩家名单为");
        foreach (var player in topPlayers)
        {
            AppendLog($"{player.Name}" );
        }
        AppendLog("\n");
        var initialGameCounts = new Dictionary<long, int>();
        var initialWinGameCounts = new Dictionary<long, int>();
        var initialPlayKill = new Dictionary<long, int>();
        var initialPlayDeaths = new Dictionary<long, int>();
        foreach (var player in topPlayers)
        {
            for (int i = 0; i < 5; i++)
            {
                int[] array = await DetailedStats(player.PersonaId);
                if (cancellationToken.IsCancellationRequested)
                { return; }
                if (!liveflag)
                {
                    return;
                }
                initialGameCounts[player.PersonaId] = array[0];
                initialWinGameCounts[player.PersonaId] = array[1];
                initialPlayKill[player.PersonaId] = array[2];
                initialPlayDeaths[player.PersonaId] = array[3];
                //AddChangeMapLog(player, array);
                if (array[0] != 0 && array[1] != 0 && array[2] !=0 && array[3] != 0)
                { break; }
                if (i == 4)
                {
                    NotifierHelper.Show(NotifierType.Warning, "网络错误");
                    //MessageBox.Show("网络错误");
                    return;
                }
            }
        }
       
        // 监控循环
        while (!cancellationToken.IsCancellationRequested || liveflag)
        {
            int increasedCount = 0;
            int increasedWinCount = 0;
            foreach (var player in topPlayers)
            {
                int currentGameCount = 0;
                int currentWinGameCount = 0;
                int currentKill = 0;
                int currentDeaths = 0;
                for (int i = 0; i < 3; i++)
                {
                    int[] array = await DetailedStats(player.PersonaId);
                    if (cancellationToken.IsCancellationRequested)
                    { return; }
                    if (!liveflag)
                    {
                        return;
                    }
                    currentGameCount = array[0];
                    currentWinGameCount = array[1];
                    currentKill = array[2];
                    currentDeaths = array[3];
                   // AddChangeMapLog(player, array);
                    if (array[0] != 0 && array[1] != 0)
                    {
                        //NotifierHelper.Show(NotifierType.Success, $" {currentPlayTime}");
                        break;
                    }
                    

                }
                if (initialGameCounts[player.PersonaId] < currentGameCount && (initialPlayKill[player.PersonaId] + initialPlayDeaths[player.PersonaId]) < (currentKill + currentDeaths))
                {
                    increasedCount++;

                    if (player.TeamId == 1 && initialWinGameCounts[player.PersonaId] < currentWinGameCount)
                    {
                        increasedWinCount++;
                        AppendLog("进攻胜利+1\n");

                    }
                    if (player.TeamId == 2 && initialWinGameCounts[player.PersonaId] < currentWinGameCount)
                    {
                        increasedWinCount--;
                        AppendLog("防守胜利+1\n");
                    }
                }
            }

            int playerCountThreshold = topPlayers.Count <= 3 ? 1 : topPlayers.Count - 3;

           
            if (increasedCount >= playerCountThreshold)
            {
                if (cancellationToken.IsCancellationRequested)
                { return; }
                if (!liveflag)
                {
                    return;
                }
                if (increasedWinCount > 0)
                {
                    attackwinflag = true;
                    AppendLog("队伍一胜利\n");
                }
                else
                {
                    AppendLog("队伍二胜利\n");
                }
                jiankongflag = 1;
                
                return; // 至少比玩家数少3的玩家游戏场数加1，或在少于4人时一个人场数加1
            }

            await Task.Delay(checkInterval, cancellationToken);
        }
    }


    public async Task ContinuousMonitoring()
    {
        while (!monitoringCts.Token.IsCancellationRequested && liveflag)
        {
            // 为当前的监控任务创建新的 CancellationTokenSource，超时设置为30秒
            using (var taskCts = CancellationTokenSource.CreateLinkedTokenSource(monitoringCts.Token))
            {
                taskCts.CancelAfter(TimeSpan.FromSeconds(30));

                // 启动监控任务，传递新的 CancellationToken
                _ = StartMonitoringTopPlayersGameCount(taskCts.Token);

                // 等待15秒后再启动下一个监控任务
                // 如果在此期间收到了取消请求，则退出循环
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(15), monitoringCts.Token);
                }
                catch (TaskCanceledException)
                {
                    // 外部 CancellationTokenSource 被取消，退出循环
                    break;
                }
            }
            if(!liveflag)
            { break;}
            // 如果 monitoringCts 请求了取消操作，那么应该退出循环
            if (monitoringCts.Token.IsCancellationRequested)
            {
                break;
            }
        }
    }
    //创建地图名称到mapid的映射
    async Task<Dictionary<string, int>> CreateMapNamesToIdMapAsync()
    {
        var mapNamesToId = new Dictionary<string, int>();
        var result = await BF1API.GetFullServerDetails(Globals.SessionId, Globals.GameId);

        if (result.IsSuccess)
        {
            var serverDetails = JsonHelper.JsonDese<BF1ServerTools.API.RespJson.FullServerDetails>(result.Content);

            int mapId = 0; // 每个地图有一个唯一的ID
            foreach (var map in serverDetails.result.serverInfo.rotation)
            {
                string mapNameBegin = map.mapPrettyName; // 获取地图的原始名称
                string mapNameChinese = ChsUtil.ToSimplified(mapNameBegin); // 转换为简体中文名称

                if (!mapNamesToId.ContainsKey(mapNameChinese))
                {
                    mapNamesToId.Add(mapNameChinese, mapId++);
                }
            }
        }
        else
        {
            //Console.WriteLine("获取服务器详情失败。");
        }

        return mapNamesToId;
    }

    public async Task ChangeAllPlayers()
    {
        List<PlayerData> playerList = Player.GetPlayerList(); // 获取当前所有玩家的列表
        int count = playerList.Count(p => p.PersonaId != 0);
        if (count < 60 && count > 0)
        {
            var team1Players = playerList.Where(p => p.TeamId == 1).ToList();
            var team2Players = playerList.Where(p => p.TeamId == 2).ToList();
            for (int z = 0; z < 22; z++)
            {
                // 移动队伍1的玩家到队伍2
                var tasksTeam1ToTeam2 = team1Players.Select(player => new
                {
                    Task = BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, player.PersonaId, 1),
                    Player = player
                }).ToList();

                var resultsTeam1ToTeam2 = await Task.WhenAll(tasksTeam1ToTeam2.Select(x => x.Task));

                for (int i = 0; i < resultsTeam1ToTeam2.Length; i++)
                {
                    var result = resultsTeam1ToTeam2[i];
                    var player = tasksTeam1ToTeam2[i].Player;
                    if (result.IsSuccess)
                    {
                        NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换玩家 {player.Name} 到队伍2成功");
                    }
                    else
                    {
                        NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换玩家 {player.Name} 到队伍2失败\n{result.Content}");
                    }
                }

                // 移动队伍2的玩家到队伍1
                var tasksTeam2ToTeam1 = team2Players.Select(player => new
                {
                    Task = BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, player.PersonaId, 2),
                    Player = player
                }).ToList();

                var resultsTeam2ToTeam1 = await Task.WhenAll(tasksTeam2ToTeam1.Select(x => x.Task));

                for (int i = 0; i < resultsTeam2ToTeam1.Length; i++)
                {
                    var result = resultsTeam2ToTeam1[i];
                    var player = tasksTeam2ToTeam1[i].Player;
                    if (result.IsSuccess)
                    {
                        NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换玩家 {player.Name} 到队伍1成功");
                    }
                    else
                    {
                        NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换玩家 {player.Name} 到队伍1失败\n{result.Content}");
                    }
                }
                await Task.Delay(100);
            }
        }
    }
    private static async Task<int[]> DetailedStats(long personaId)
    {
        int[] stats = new int[4];
        var result = await BF1API.DetailedStatsByPersonaId(Globals.SessionId, personaId);
        if (result.IsSuccess)
        {
            var detailed = JsonHelper.JsonDese<DetailedStats>(result.Content);

            var basic = detailed.result.basicStats;

           

            stats[0] = detailed.result.roundsPlayed;

            stats[1] = basic.wins;

            stats[2] = basic.kills;

            stats[3] = basic.deaths;
        }
        return stats;
    }
    public static List<MapItem> mapchoose;
    public static bool Autochangegamemapflag = false;
    public static int votechangemap = -1;
    public static int currentIndex = -1;
    public static bool clearvote = false;
    public async Task Autochangegamemap()

    {   if (Autochangegamemapflag)
        { return; }
        Autochangegamemapflag=true;
        // 在后台线程中异步启动监控，不等待其完成
        _ = Task.Run(async () =>
        {
            try
            {
                await ContinuousMonitoring();
            }
            catch (OperationCanceledException)
            {
                // 监控被取消了
            }
            catch (Exception ex)
            {
                // 处理其他类型的异常
                MessageBox.Show($"监控过程中出错: {ex.Message}");
            }
        });
        while (true)
        {
            if (jiankongflag == 1)
            {
                if (ScoreView.mapmode == "行动模式" && attackwinflag)
                {
                    List<string> Mapname = new List<string> {
    "加利西亚",
    "凡尔登高地",
    "海丽斯岬",
    "苏伊士",
    "苏瓦松",
    "窝瓦河",
    "流血宴厅",
    "圣康坦的伤痕",
    "法欧堡",
    "格拉巴山",
};
                    foreach (var str in Mapname)
                    {
                        if (ScoreView.mapname.Equals(str, StringComparison.OrdinalIgnoreCase))
                        {
                            NotifierHelper.Show(NotifierType.Information, "行动进攻胜利不换图");
                            goto NEXT;

                        }
                    }
                }
                if (votechangemap >= 0)
                {
                    var result = await BF1API.RSPChooseLevel(Globals.SessionId, Globals.PersistedGameId, votechangemap);
                    if (result.IsSuccess)
                    {
                        NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换服务器 {Globals.GameId} 地图为 第{votechangemap + 1}图 成功");
                    }
                    else
                    {
                        NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换服务器 {Globals.GameId} 地图为 第{votechangemap + 1}图 失败");
                    }
                    if (currentIndex >= 0)//防止换图机下一张图为相同地图
                    {
                        var nextMap = mapchoose[(currentIndex + 1) % mapchoose.Count];
                        if (votechangemap == nextMap.MapId)
                        {
                            currentIndex = (currentIndex + 1) % mapchoose.Count;
                        }
                    }
                }
                else if (currentIndex >= 0)//无人投票或投票换图未启动
                {
                    currentIndex = (currentIndex + 1) % mapchoose.Count;
                    var nextMap = mapchoose[currentIndex];
                    // 使用 mapItems 里的 MapId 进行地图更换
                    int mapIdNext = nextMap.MapId;
                    var result = await BF1API.RSPChooseLevel(Globals.SessionId, Globals.PersistedGameId, mapIdNext);
                    if (result.IsSuccess)
                    {
                        NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换服务器 {Globals.GameId} 地图为 {nextMap.MapName} 成功");
                    }
                    else
                    {
                        NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换服务器 {Globals.GameId} 地图为 {nextMap.MapName} 失败");
                    }
                }
                await Task.Delay(100);
                // 使用 Dispatcher 来确保在 UI 线程上访问 UI 元件
                bool chooseflag = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    chooseflag = autooperationplayerchange.IsChecked ?? false;
                });
                if (ScoreView.mapmode == "行动模式" && chooseflag)//测试时为true(autooperationplayerchange.IsChecked ?? false)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        List<PlayerData> playerList = Player.GetPlayerList();
                        int count = playerList.Count(p => p.PersonaId != 0);
                        await Task.Delay(100);
                        if (count != 0)
                        { break; }
                    }
                    monitoringCts.Cancel();
                    liveflag = false;
                    await ChangeAllPlayers();
                }
                clearvote = true;
            NEXT:;
                liveflag = false;
                while (liveflag)
                {
                    monitoringCts.Cancel();
                    liveflag = false;
                    await Task.Delay(1000);  // 等待一段时间确保监控任务已经完全停止
                }
                jiankongflag = 0;
                jiankongflag = 0;
              
                await Task.Delay(300000);  // 等待一段时间确保监控任务已经完全停止
                while (liveflag)
                {
                    monitoringCts.Cancel();
                    liveflag = false;
                    await Task.Delay(1000);  // 等待一段时间确保监控任务已经完全停止

                }

                jiankongflag = 0;
                jiankongflag = 0;
                attackwinflag = false;
                liveflag = true;
                jiankongflag = 0;//求求别出错了
                monitoringCts = new CancellationTokenSource();  // 创建新的 CancellationTokenSource 以便于重新启动监控
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ContinuousMonitoring();
                    }
                    catch (OperationCanceledException)
                    {
                        // 监控被取消了
                    }
                    catch (Exception ex)
                    {
                        // 处理其他类型的异常
                        MessageBox.Show($"监控过程中出错: {ex.Message}");
                    }
                });
                
            }
        

           

        
            await Task.Delay(100);
        }
    }

    public async Task XPFARM()
    {
         mapchoose = MapListView.Items.Cast<MapItem>().Where(item => item.IsSelected).ToList();
        if (!mapchoose.Any())
        {
            MessageBox.Show("没有选中任何地图。");
            return;
        }

        
        

        // 找到当前地图在列表中的位置
        var currentMap = mapchoose.FirstOrDefault(item => item.MapName == ScoreView.mapname && item.MapMode == ScoreView.mapmode);

        if (currentMap != null)
        {
            currentIndex = mapchoose.IndexOf(currentMap);
        }
         Autochangegamemap();


    }
    /// <summary>
    /// 追加日志
    /// </summary>
    /// <param name="info"></param>
    private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
    private const int MaxLogLines = 1000;
    private const int MaxQueueSize = 5000; // 最大队列大小
    private static Timer _logUpdateTimer;

    public void YourClassConstructor()
    {
        // 初始化定时器
        _logUpdateTimer = new Timer(200); // 每200毫秒更新一次UI
        _logUpdateTimer.Elapsed += ProcessLogQueue;
        _logUpdateTimer.Start();
    }

    private void AddChangeMapLog(PlayerData player, int[] array)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"玩家:{player.Name}");
        if (array[0] != 0 && array[1] != 0 && array[2] != 0 && array[3] != 0)
        {
            sb.AppendLine($"游戏总场数{array[0]}");
            sb.AppendLine($"胜利场数{array[1]}");
            sb.AppendLine($"总击杀{array[2]}");
            sb.AppendLine($"总死亡{array[3]}");
        }
        else
        {
            sb.AppendLine("网络错误");
        }

        AppendLog(sb.ToString());
    }

    private  void AppendLog(string log)
    {
        if (_logQueue.Count < MaxQueueSize)
        {
            _logQueue.Enqueue(log);
        }
        else
        {
            // 如果队列已满，可以选择丢弃新日志或移除旧日志
            _logQueue.TryDequeue(out _);
            _logQueue.Enqueue(log);
        }
    }

    private void ProcessLogQueue(object sender, ElapsedEventArgs e)
    {
        // 只允许一个线程更新UI
        if (Dispatcher.CheckAccess())
        {
            UpdateLogUI();
        }
        else
        {
            Dispatcher.Invoke(UpdateLogUI);
        }
    }

    private void UpdateLogUI()
    {
        if (TextBox_ChangeMapLog.LineCount >= MaxLogLines)
            TextBox_ChangeMapLog.Clear();

        while (_logQueue.TryDequeue(out string queuedLog))
        {
            TextBox_ChangeMapLog.AppendText(queuedLog);
        }
    }
    private void MenuItem_ChangeMap_Click(object sender, RoutedEventArgs e)
    {
        TextBox_ChangeMapLog.Clear();
       // NotifierHelper.Show(NotifierType.Success, "清空换图日志成功");
    }
    private static readonly HttpClient client = new HttpClient();

    public static async Task<ApiResponse> GetPlayerListAsync()
    {
        var url = "http://127.0.0.1:10086/Player/GetAllPlayerList";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseBody, options);

        return apiResponse;
    }
    public static async Task<List<PlayerData>> GetPlayerListXYZ(List<PlayerData> oldplayerlist)
    {
        if (ScoreView.zhangapi)
        {
            try
            {
                var apiResponse = await GetPlayerListAsync();


                foreach (var newPlayer in apiResponse.Data)
                {
                    var oldPlayer = oldplayerlist.FirstOrDefault(p => p.PersonaId == newPlayer.PersonaId);
                    if (oldPlayer != null)
                    {
                        oldPlayer.X = newPlayer.X;
                        oldPlayer.Y = newPlayer.Y;
                        oldPlayer.Z = newPlayer.Z;
                    }
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show("Request failed: " + e.Message + "\n");
            }
            return oldplayerlist;
        }
        else
        {
           
            return oldplayerlist;

        }

    }


}

public static class VisualTreeExtensions
    {
        public static T FindAncestorOrSelf<T>(this DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                if (obj is T)
                    return (T)obj;

                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }
    }
public class ApiPlayer
{
    public int Index { get; set; }
    public int Mark { get; set; }
    public int TeamId { get; set; }
    public bool IsSpectator { get; set; }
    public string Clan { get; set; }
    public string Name { get; set; }
    public long PersonaId { get; set; }
    public int SquadId { get; set; }
    public string SquadName { get; set; }
    public int Rank { get; set; }
    public int Kill { get; set; }
    public int Dead { get; set; }
    public int Score { get; set; }
    public string Kit { get; set; }
    public string KitName { get; set; }
    public double AuthorativeYaw { get; set; }
    public int PoseType { get; set; }
    public string PoseName { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public bool IsAlive { get; set; }
    public bool IsInVehicle { get; set; }
    public ApiWeapon WeaponS0 { get; set; }
    public ApiWeapon WeaponS1 { get; set; }
    public ApiWeapon WeaponS2 { get; set; }
    public ApiWeapon WeaponS3 { get; set; }
    public ApiWeapon WeaponS4 { get; set; }
    public ApiWeapon WeaponS5 { get; set; }
    public ApiWeapon WeaponS6 { get; set; }
    public ApiWeapon WeaponS7 { get; set; }
}

public class ApiWeapon
{
    public string Kind { get; set; }
    public string Guid { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
}

public class ApiResponse
{
    public int Code { get; set; }
    public string Message { get; set; }
    public List<ApiPlayer> Data { get; set; }
    public long Timestamp { get; set; }
}
public class PointXZ
{
    public double X { get; set; }
    public double Z { get; set; }

    public PointXZ(double x, double z)
    {
        X = x;
        Z = z;
    }
}
public class Polygon
{
    public static bool IsPointInPolygon(PointXZ point, List<PointXZ> polygon)
    {
        int polygonLength = polygon.Count, i = 0;
        bool inside = false;
        double pointX = point.X, pointZ = point.Z;
        double startX, startZ, endX, endZ;
        PointXZ endPoint = polygon[polygonLength - 1];
        endX = endPoint.X;
        endZ = endPoint.Z;

        while (i < polygonLength)
        {
            startX = endX; startZ = endZ;
            endPoint = polygon[i++];
            endX = endPoint.X; endZ = endPoint.Z;

            inside ^= (endZ > pointZ ^ startZ > pointZ) &&
                      (pointX - endX < (pointZ - endZ) * (startX - endX) / (startZ - endZ));
        }

        return inside;
    }
}



