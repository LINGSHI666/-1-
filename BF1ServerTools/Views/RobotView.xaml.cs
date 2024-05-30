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

    public Queue<PlayerData> excludeList = new Queue<PlayerData>(); // 使用队列来管理排除名单 
    static Queue<int> team1Scores = new Queue<int>();//旗帜得分
    static Queue<int> team2Scores = new Queue<int>();


    ////////////////////////////////////////////////////////




    public RobotView()
    {

        InitializeComponent();
        InitializeVoteTimer();

    }
    private void Reportmapinfo_Click(object sender, RoutedEventArgs e)
    {
        // 调用播报信息方法
        showflag = true;
    }
    /// <summary>
    /// 启动定时平衡
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private bool isTimerRunning = false;
    private void Button_RunGoCqHttpServer_Click(object sender, RoutedEventArgs e)
    {
        // 检查定时器是否已经在运行，如果是则返回，防止重复执行
        if (isTimerRunning)
        {
            MessageBox.Show("自动平衡已经运行了");
            return;
        }

        // 创建定时器实例
        timer = new DispatcherTimer();

        // 获取滑块当前的值，如果slider为null，则使用默认的10分钟
        double minutes = slider != null ? slider.Value : 10;

        // 设置定时器的时间间隔为滑块的值
        timer.Interval = TimeSpan.FromMinutes(minutes);

        // 设置定时器触发的事件
        timer.Tick += Timer_Tick;

        // 启动定时器
        timer.Start();

        // 立即执行任务
        RunPeriodicTasks();
        isTimerRunning = true;
        MessageBox.Show($"当前自动平衡间隔为{minutes}分钟");



    }
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
    private void Button_Balance_Click(object sender, RoutedEventArgs e)
    {
        RunPeriodicTasks();
    }
    // 滑块值改变时的事件处理器
    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {

        if (labelCurrentMinutes != null) // 检查控件是否为null
        {
            double newValue = e.NewValue; // 获取滑块的当前值
            labelCurrentMinutes.Content = $"当前为{newValue}分钟"; // 更新标签内容
        }
    }
    private void Slider_ValueChangedkdkpm(object sender, RoutedPropertyChangedEventArgs<double> e)
    {

        if (labelCurrentkdkpm != null) // 检查控件是否为null
        {
            double newValue = e.NewValue; // 获取滑块的当前值
            labelCurrentkdkpm.Text = $"平衡目标为进攻(队伍1)lifekd、lifekp高于防守(队伍2){newValue:F2}(+-0.05)"; // 更新标签内容
        }
    }
    private void Slider_ValueChangedskill(object sender, RoutedPropertyChangedEventArgs<double> e)
    {

        if (labelCurrentdskill != null) // 检查控件是否为null
        {
            double newValue = e.NewValue; // 获取滑块的当前值
            labelCurrentdskill.Text = $"平衡目标为进攻(队伍1)技巧值高于防守(队伍2){newValue}(+-30)"; // 更新标签内容
        }
    }
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

    private void Timer_Tick(object sender, EventArgs e)
    {
        // 定时器触发时执行的任务
        RunPeriodicTasks();
    }

    // 地图投票系统
    public async void Autochangemap()
    {

        List<PlayerData> playerListbegin = Player.GetPlayerList(); // 获取当前所有玩家的列表
        long personaIdToCheck = Globals.PersonaId;
        bool isPlayerInTeam3 = playerListbegin.Any(player => player.PersonaId == personaIdToCheck && player.TeamId == 0);
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
                string voteName = lastContent.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1)?.ToLower();
                if (voteName != null && mapNamesToId.TryGetValue(voteName, out var localMapId))
                {
                    if (!userHasVoted.ContainsKey(lastSender))
                    {
                        votes[localMapId] = votes.TryGetValue(localMapId, out var currentCount) ? currentCount + 1 : 1;
                        userHasVoted[lastSender] = true;
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
        return scoreIncrease / 1;  
    }
    //压家
    private async void AutoAllchange()
    {
        NotifierHelper.Show(NotifierType.Information, "压家检测已启动");
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
        
   BEGIN:     do
        {
            int point = 0;
            team1Score = Server.GetTeam1FlagScore();
            team2Score = Server.GetTeam2FlagScore();
            do
            {
                if (maptopoint.TryGetValue(ScoreView.mapname, out point))
                {
                    break;
                }
                else
                {
                    await Task.Delay(10000);

                }
            } while (true);
            if ((onepointmode.IsChecked ?? false) && ScoreView.mapname != "察里津")
            {
                point--;
            }
            // 维护队列长度为30
            if (team1Scores.Count >= 30)
                team1Scores.Dequeue();
            if (team2Scores.Count >= 30)
                team2Scores.Dequeue();

            team1Scores.Enqueue(team1Score);
            team2Scores.Enqueue(team2Score);
            // 只有当队列满时才进行估算
            if (team1Scores.Count == 30 && team2Scores.Count == 30)
            {   if(delayallchangeflag)
                {
                    await Task.Delay((int)((delaytimeflag-14) * 1000));
                    delayallchangeflag = false;
                    team1Scores.Clear();
                    team2Scores.Clear();
                    goto BEGIN;
                }
                delayallchangeflag = delayallchange.IsChecked ?? false;
                if (team1Scores.Peek() > scoreflag || team2Scores.Peek() > scoreflag)
                { goto END; }
                int team1Points = EstimateNumberOfPoints(team1Scores);
                int team2Points = EstimateNumberOfPoints(team2Scores);
                int team1Points2 = EstimateNumberOfPointsquick(team1Scores);
                int team2Points2 = EstimateNumberOfPointsquick(team2Scores);
                if (point == team1Points || point == team2Points || point == team1Points2 || point == team1Points2)
                { break; }
            }
            await Task.Delay(490);
        }while (true);
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
                    NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换服务器 {Globals.GameId} 地图 成功");
                }
            }
            else
            {
                MessageBox.Show("程序出错");
            }
        }
    END: while (true)
        {   await Task.Delay(1000);
            team1Score = Server.GetTeam1FlagScore();
            team2Score = Server.GetTeam2FlagScore();
            if(team1Score < team1Scores.Peek() || team2Score< team2Scores.Peek())
            {
                goto BEGIN;
            }
        }
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
    //自动平衡
    private async void RunPeriodicTasks()
    {
        try
        {
            bool balanceAchieved = false;
            int movecount = 0;
            for (int i = 0; i < 99 && !balanceAchieved; i++)
            {
                
                List<PlayerData> playerListbegin = Player.GetPlayerList(); // 获取当前所有玩家的列表
                List<PlayerData> playerList = playerListbegin.Where(p => p.Kill >= 1 || p.Dead >= 2).ToList(); //排除机器人
                                                                                                           

                if (playerList == null || playerList.Count == 0)
                {
                    NotifierHelper.Show(NotifierType.Error, "没有足够的玩家数据进行操作");
                    await Task.Delay(1000); // 暂停一秒再继续，避免频繁操作
                    break;
                }
                int count = playerList.Count(p => p.PersonaId != 0);

                double kdkpmflag = sliderkdkpm != null ? sliderkdkpm.Value : 0;
                double skillflag = sliderskill != null ? sliderskill.Value : 0;
                var team1Players = playerList.Where(p => p.TeamId == 1).ToList();
                var team2Players = playerList.Where(p => p.TeamId == 2).ToList();
                if (count < 15 || team1Players.Count == 0 || team2Players.Count == 0)
                {
                    NotifierHelper.Show(NotifierType.Error, "人数不足,或游戏刚开始");
                    break;
                }

                // 更新玩家的生涯KD和KPM及技巧值
                foreach (var item in playerList)
                {
                    try
                    {
                        // 获取生涯KD
                        float kd = PlayerUtil.GetLifeKD(item.PersonaId);
                        item.LifeKd = (float)Math.Min(kd, 4.0); // 限制最大值为4

                        // 获取生涯KPM
                        float kpm = PlayerUtil.GetLifeKPM(item.PersonaId);
                        item.LifeKpm = (float)Math.Min(kpm, 4.0); // 限制最大值为4

                        // 获取技巧值
                        float skill = PlayerUtil.GetSkill(item.PersonaId);
                        item.Skill = Math.Min(skill, 900); // 限制最大值为900
                    }
                    catch (Exception ex)
                    {
                        NotifierHelper.Show(NotifierType.Error, $"Error updating player stats: {ex.Message}");
                        continue;
                    }
                }


               
                double avgLifeKdTeam1 = team1Players.Any() ? team1Players.Average(p => p.LifeKd) : 0;
                double avgLifeKpmTeam1 = team1Players.Any() ? team1Players.Average(p => p.LifeKpm) : 0;
                double avgSkillTeam1 = team1Players.Any() ? team1Players.Average(p => p.Skill) : 0;

                double avgLifeKdTeam2 = team2Players.Any() ? team2Players.Average(p => p.LifeKd) : 0;
                double avgLifeKpmTeam2 = team2Players.Any() ? team2Players.Average(p => p.LifeKpm) : 0;
                double avgSkillTeam2 = team2Players.Any() ? team2Players.Average(p => p.Skill) : 0;
                List<PlayerData> excludeplayer = new List<PlayerData>();

                if (Excludesuperman.IsChecked ?? false)
                {
                    excludeplayer.AddRange(playerList.Where(item =>
                    {
                        string kitName = ClientHelper.GetPlayerKitName(item.Kit);
                        return (kitName == "12 坦克" ||
                                kitName == "11 飞机" ||
                                kitName == "10 骑兵" ||
                                kitName == "09 哨兵" ||
                                kitName == "08 喷火兵" ||
                                kitName == "07 入侵者" ||
                                kitName == "06 战壕奇兵" ||
                                kitName == "05 坦克猎手");
                    }).ToList());
                }

                if (ExcludeAdminsAndVIPsCheckBox.IsChecked ?? false)
                {
                    List<long> adminAndVipIds = Globals.ServerAdmins_PID.Concat(Globals.ServerVIPs_PID).ToList();
                    excludeplayer.AddRange(playerList.Where(player => PlayerUtil.IsAdminVIP(player.PersonaId, adminAndVipIds)).ToList());
                }

                excludeplayer.AddRange(playerList.Where(player => excludeList.Select(p => p.PersonaId).Contains(player.PersonaId)).ToList());//移除已经换过边的玩家

                if (!playerList.Any())
                {
                    NotifierHelper.Show(NotifierType.Information, "所有玩家均在排除名单中。");
                    break;
                }
                if (movecount == 5)
                {
                   MessageBox.Show($"平衡已移动5次仍未平衡\n当前team1kd [{avgLifeKdTeam1:0.00}]kpm [{avgLifeKpmTeam1:0.00}] || team2kd [{avgLifeKdTeam2:0.00}]kpm [{avgLifeKpmTeam2:0.00}]");
                    return;
                }
              
                if (skillbalance.IsChecked ?? false)
                {
                    // 获取队伍1技巧值最高的玩家
                    var highestSkillPlayerTeam1 = team1Players.OrderByDescending(p => p.Skill).FirstOrDefault();
                    var highestSkillTeam1 = highestSkillPlayerTeam1?.Skill ?? 0;

                    // 获取队伍1技巧值最低的玩家
                    var lowestSkillPlayerTeam1 = team1Players.OrderBy(p => p.Skill).FirstOrDefault();
                    var lowestSkillTeam1 = lowestSkillPlayerTeam1?.Skill ?? 0;

                    // 获取队伍2技巧值最高的玩家
                    var highestSkillPlayerTeam2 = team2Players.OrderByDescending(p => p.Skill).FirstOrDefault();
                    var highestSkillTeam2 = highestSkillPlayerTeam2?.Skill ?? 0;

                    // 获取队伍2技巧值最低的玩家
                    var lowestSkillPlayerTeam2 = team2Players.OrderBy(p => p.Skill).FirstOrDefault();
                    var lowestSkillTeam2 = lowestSkillPlayerTeam2?.Skill ?? 0;
                    //平衡
                    if ((avgSkillTeam1 < avgSkillTeam2 + skillflag - 30) || (avgSkillTeam1 > avgSkillTeam2 + skillflag + 30))
                    {


                        // 执行行动，获取应该移动的玩家
                        var playerToMove = BalanceTeam(team1Players, team2Players, avgSkillTeam1, avgSkillTeam2, skillflag, excludeplayer);

                        // 根据playerToMove所在队伍决定移动方向
                        int OriginTeam = playerToMove.TeamId == 1 ? 1 : 2;

                        // 执行移动
                        var result = await BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, playerToMove.PersonaId, OriginTeam);
                        // 重新获取玩家列表以验证换边是否成功
                        await Task.Delay(1000);
                        var updatedPlayerList = Player.GetPlayerList();
                        var movedPlayer = updatedPlayerList.FirstOrDefault(p => p.PersonaId == playerToMove.PersonaId);
                        OriginTeam = playerToMove.TeamId == 1 ? 2 : 1;
                        if (movedPlayer != null && movedPlayer.TeamId == OriginTeam)
                        {
                            // 如果排除名单已有三人，则移除最早添加的玩家
                            if (excludeList.Count >= 5)
                            {
                               
                                excludeList.Dequeue(); // 移除队列前端的元素
                            }
                            LogView.ActionAddChangeTeamInfoLog(new ChangeTeamInfo()
                            {
                                Rank = playerToMove.Rank,
                                Name = playerToMove.Name,
                                PersonaId = playerToMove.PersonaId,
                                GameMode = ScoreView.mapmode,
                                MapName = ScoreView.mapname,
                                Team1Name = "队伍一",
                                Team2Name = "队伍二",
                                State = $" >>> 队伍{OriginTeam}",
                                Time = DateTime.Now
                            });
                            movecount++;
                            NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{OriginTeam}成功");
                            // 将新的玩家添加到排除名单的队尾
                            excludeList.Enqueue(movedPlayer);
                        }
                        else
                        {
                            //NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{targetTeam}失败");
                        }
                    }
                    else if (Math.Abs(team1Players.Count - team2Players.Count) > 3)
                    {

                        // 执行行动，获取应该移动的玩家
                        var playerToMove = FindBestPlayerToMoveForSkill(team1Players, team2Players, avgSkillTeam1, avgSkillTeam2, skillflag,excludeplayer);

                        // 根据playerToMove所在队伍决定移动方向
                        int OriginTeam = playerToMove.TeamId == 1 ? 1 : 2;

                        // 执行移动
                        var result = await BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, playerToMove.PersonaId, OriginTeam);
                        // 重新获取玩家列表以验证换边是否成功
                        await Task.Delay(1000);
                        var updatedPlayerList = Player.GetPlayerList();
                        var movedPlayer = updatedPlayerList.FirstOrDefault(p => p.PersonaId == playerToMove.PersonaId);
                        OriginTeam = playerToMove.TeamId == 1 ? 2 : 1;
                        if (movedPlayer != null && movedPlayer.TeamId == OriginTeam)
                        {
                            // 如果排除名单已有三人，则移除最早添加的玩家
                            if (excludeList.Count >= 5)
                            {
                                
                                excludeList.Dequeue(); // 移除队列前端的元素
                            }
                            LogView.ActionAddChangeTeamInfoLog(new ChangeTeamInfo()
                            {
                                Rank = playerToMove.Rank,
                                Name = playerToMove.Name,
                                PersonaId = playerToMove.PersonaId,
                                GameMode = ScoreView.mapmode,
                                MapName = ScoreView.mapname,
                                Team1Name = "队伍一",
                                Team2Name = "队伍二",
                                State = $" >>> 队伍{OriginTeam}",
                                Time = DateTime.Now
                            });
                            movecount++;
                            NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{OriginTeam}成功");
                            // 将新的玩家添加到排除名单的队尾
                            excludeList.Enqueue(movedPlayer);
                        }
                        else
                        {
                            //NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{targetTeam}失败");
                        }
                    }

                    else
                    {
                        balanceAchieved = true;
                        NotifierHelper.Show(NotifierType.Information, $"队伍已平衡，无需进一步操作\navgSkillTeam1 [{avgSkillTeam1:0.00}] || avgSkillTeam2 [{avgSkillTeam2:0.00}]");
                    }

                }

                else
                {

                    // 判断是否需要调整玩家队伍

                    if ((avgLifeKdTeam1 < avgLifeKdTeam2 + kdkpmflag - 0.05 && avgLifeKpmTeam1 < avgLifeKpmTeam2 + kdkpmflag - 0.05) || (avgLifeKdTeam1 > avgLifeKdTeam2 + kdkpmflag + 0.05 && avgLifeKpmTeam1 > avgLifeKpmTeam2 + kdkpmflag + 0.05))
                    {

                        // 计算目标值
                        double targetKdDifference = avgLifeKdTeam2 + kdkpmflag;
                        double targetKpmDifference = avgLifeKpmTeam2 + kdkpmflag;

                        // 查找最佳移动玩家
                        var playerToMove = FindBestPlayerToMove(team1Players, team2Players, avgLifeKdTeam1, avgLifeKdTeam2, avgLifeKpmTeam1, avgLifeKpmTeam2, targetKdDifference, targetKpmDifference, excludeplayer);

                        if (playerToMove != null)
                        {
                            // 确定移动的目标队伍
                            int OriginTeam = playerToMove.TeamId == 1 ? 1 : 2;

                            // 执行移动
                            var result = await BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, playerToMove.PersonaId, OriginTeam);







                            await Task.Delay(1200);
                            var updatedPlayerList = Player.GetPlayerList();
                            var movedPlayer = updatedPlayerList.FirstOrDefault(p => p.PersonaId == playerToMove.PersonaId);
                            OriginTeam = playerToMove.TeamId == 1 ? 2 : 1;
                            if (movedPlayer != null && movedPlayer.TeamId == OriginTeam)
                            {
                                // 如果排除名单已有三人，则移除最早添加的玩家
                                if (excludeList.Count >= 5)
                                {
                                    
                                    excludeList.Dequeue(); // 移除队列前端的元素
                                }
                                LogView.ActionAddChangeTeamInfoLog(new ChangeTeamInfo()
                                {
                                    Rank = playerToMove.Rank,
                                    Name = playerToMove.Name,
                                    PersonaId = playerToMove.PersonaId,
                                    GameMode = ScoreView.mapmode,
                                    MapName = ScoreView.mapname,
                                    Team1Name = "队伍一",
                                    Team2Name = "队伍二",
                                    State = $" >>> 队伍{OriginTeam}",
                                    Time = DateTime.Now
                                });
                                movecount++;
                                NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{OriginTeam}成功");
                                // 将新的玩家添加到排除名单的队尾
                                excludeList.Enqueue(movedPlayer);
                            }
                            else
                            {
                                //NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{targetTeam}失败");
                            }
                        }
                    }
                    else if (Math.Abs(team1Players.Count - team2Players.Count) > 3)
                    {

                        // 计算目标值
                        double targetKdDifference = avgLifeKdTeam2 + kdkpmflag;
                        double targetKpmDifference = avgLifeKpmTeam2 + kdkpmflag;

                        // 查找最佳移动玩家
                        var playerToMove = FindBestPlayerToMoveOverPlayer(team1Players, team2Players, avgLifeKdTeam1, avgLifeKdTeam2, avgLifeKpmTeam1, avgLifeKpmTeam2, targetKdDifference, targetKpmDifference, excludeplayer);

                        if (playerToMove != null)
                        {
                            // 确定移动的目标队伍
                            int OriginTeam = playerToMove.TeamId == 1 ? 1 : 2;

                            // 执行移动
                            var result = await BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, playerToMove.PersonaId, OriginTeam);







                            await Task.Delay(1000);
                            var updatedPlayerList = Player.GetPlayerList();
                            var movedPlayer = updatedPlayerList.FirstOrDefault(p => p.PersonaId == playerToMove.PersonaId);
                            OriginTeam = playerToMove.TeamId == 1 ? 2 : 1;
                            if (movedPlayer != null && movedPlayer.TeamId == OriginTeam)
                            {
                                // 如果排除名单已有三人，则移除最早添加的玩家
                                if (excludeList.Count >= 5)
                                {
                                   
                                    excludeList.Dequeue(); // 移除队列前端的元素
                                }
                                LogView.ActionAddChangeTeamInfoLog(new ChangeTeamInfo()
                                {
                                    Rank = playerToMove.Rank,
                                    Name = playerToMove.Name,
                                    PersonaId = playerToMove.PersonaId,
                                    GameMode = ScoreView.mapmode,
                                    MapName = ScoreView.mapname,
                                    Team1Name = "队伍一",
                                    Team2Name = "队伍二",
                                    State = $" >>> 队伍{OriginTeam}",
                                    Time = DateTime.Now
                                });
                               movecount++;
                                NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{OriginTeam}成功");
                                // 将新的玩家添加到排除名单的队尾
                                excludeList.Enqueue(movedPlayer);
                            }
                            else
                            {
                                //NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{targetTeam}失败");
                            }
                        }
                    }
                   
                    else
                    {
                        balanceAchieved = true;
                        NotifierHelper.Show(NotifierType.Information, $"队伍已平衡，无需进一步操作\nteam1kd [{avgLifeKdTeam1:0.00}]kpm [{avgLifeKpmTeam1:0.00}] || team2kd [{avgLifeKdTeam2:0.00}]kpm [{avgLifeKpmTeam2:0.00}]");
                    }

                    
                    if (!balanceAchieved)
                    {
                        await Task.Delay(1000);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // 处理异常

            NotifierHelper.Show(NotifierType.Error, "An unexpected error occurred: " + ex.Message);
        }
    }
    /// <summary>
    /// 找出最适合移动的玩家
    /// </summary>
    /// <param name="team1Players"></param>
    /// <param name="team2Players"></param>
    /// <param name="avgKdTeam1"></param>
    /// <param name="avgKdTeam2"></param>
    /// <param name="avgKpmTeam1"></param>
    /// <param name="avgKpmTeam2"></param>
    /// <param name="targetKdDiff"></param>
    /// <param name="targetKpmDiff"></param>
    /// <returns></returns>
    private PlayerData FindBestPlayerToMove(List<PlayerData> team1Players, List<PlayerData> team2Players, double avgKdTeam1, double avgKdTeam2, double avgKpmTeam1, double avgKpmTeam2, double targetKdDiff, double targetKpmDiff, List<PlayerData> excludeplayer)
    {
        PlayerData bestPlayerToMove = null;
        double smallestImpactScore = double.MaxValue;

        bool moveFromTeam1ToTeam2 = false;
        bool moveFromTeam2ToTeam1 = false;

        // 确定移动方向
        int countDiff = team1Players.Count - team2Players.Count;
        if (countDiff > 2)
        {
            moveFromTeam1ToTeam2 = true;
        }
        else if (countDiff < -2)
        {
            moveFromTeam2ToTeam1 = true;
        }
        else
        {
            // 如果人数差距在2人及以内，则不考虑人数，只考虑KD和KPM
            moveFromTeam1ToTeam2 = team1Players.Count > team2Players.Count;
            moveFromTeam2ToTeam1 = team2Players.Count > team1Players.Count;
        }

        foreach (var player in team1Players.Concat(team2Players))

        {
            if (excludeplayer.Any(player1 => player1.PersonaId == player.PersonaId))
            { continue; }
            // 忽略将玩家移动到已满的队伍
            if ((player.TeamId == 1 && team2Players.Count >= 32) || (player.TeamId == 2 && team1Players.Count >= 32))
            {
                continue;
            }

            // 只考虑从人数多的队伍向人数少的队伍移动
            if ((moveFromTeam1ToTeam2 && player.TeamId != 1) || (moveFromTeam2ToTeam1 && player.TeamId != 2))
            {
                continue;
            }

            double newAvgKdTeam1, newAvgKdTeam2, newAvgKpmTeam1, newAvgKpmTeam2;
            int newTeam1Count, newTeam2Count;

            if (player.TeamId == 1)
            {
                newTeam1Count = team1Players.Count - 1;
                newTeam2Count = team2Players.Count + 1;

                newAvgKdTeam1 = newTeam1Count > 0 ? (team1Players.Sum(p => p.LifeKd) - player.LifeKd) / newTeam1Count : 0;
                newAvgKdTeam2 = newTeam2Count > 0 ? (team2Players.Sum(p => p.LifeKd) + player.LifeKd) / newTeam2Count : 0;

                newAvgKpmTeam1 = newTeam1Count > 0 ? (team1Players.Sum(p => p.LifeKpm) - player.LifeKpm) / newTeam1Count : 0;
                newAvgKpmTeam2 = newTeam2Count > 0 ? (team2Players.Sum(p => p.LifeKpm) + player.LifeKpm) / newTeam2Count : 0;
            }
            else
            {
                newTeam1Count = team1Players.Count + 1;
                newTeam2Count = team2Players.Count - 1;

                newAvgKdTeam1 = newTeam1Count > 0 ? (team1Players.Sum(p => p.LifeKd) + player.LifeKd) / newTeam1Count : 0;
                newAvgKdTeam2 = newTeam2Count > 0 ? (team2Players.Sum(p => p.LifeKd) - player.LifeKd) / newTeam2Count : 0;

                newAvgKpmTeam1 = newTeam1Count > 0 ? (team1Players.Sum(p => p.LifeKpm) + player.LifeKpm) / newTeam1Count : 0;
                newAvgKpmTeam2 = newTeam2Count > 0 ? (team2Players.Sum(p => p.LifeKpm) - player.LifeKpm) / newTeam2Count : 0;
            }

            // 计算新的差值
            double newKdDiff = Math.Abs(newAvgKdTeam1 - newAvgKdTeam2);
            double newKpmDiff = Math.Abs(newAvgKpmTeam1 - newAvgKpmTeam2);

            // 计算新的人数差距
            double newCountDiff = Math.Abs(newTeam1Count - newTeam2Count);

            // 计算综合影响分数，优先考虑人数差距，其次是KD和KPM差值
            double impactScore = newCountDiff + (newKdDiff - targetKdDiff) + (newKpmDiff - targetKpmDiff);

            // 找到影响最小的玩家
            if (impactScore < smallestImpactScore)
            {
                smallestImpactScore = impactScore;
                bestPlayerToMove = player;
            }
        }

        return bestPlayerToMove;
    }
    private PlayerData FindBestPlayerToMoveOverPlayer(List<PlayerData> team1Players, List<PlayerData> team2Players, double avgKdTeam1, double avgKdTeam2, double avgKpmTeam1, double avgKpmTeam2, double targetKdDiff, double targetKpmDiff, List<PlayerData> excludeplayer)
    {
        PlayerData bestPlayerToMove = null;
        double smallestImpactScore = double.MaxValue;

        bool moveFromTeam1ToTeam2 = false;
        bool moveFromTeam2ToTeam1 = false;

        // 确定移动方向
        int countDiff = team1Players.Count - team2Players.Count;
        if (countDiff > 0)
        {
            moveFromTeam1ToTeam2 = true;
        }
        else if (countDiff < 0)
        {
            moveFromTeam2ToTeam1 = true;
        }

        foreach (var player in team1Players.Concat(team2Players))
        {
            if (excludeplayer.Any(player1 => player1.PersonaId == player.PersonaId))
            { continue; }
            // 忽略将玩家移动到已满的队伍
            if ((player.TeamId == 1 && team2Players.Count >= 32) || (player.TeamId == 2 && team1Players.Count >= 32))
            {
                continue;
            }

            // 只考虑从人数多的队伍向人数少的队伍移动
            if ((moveFromTeam1ToTeam2 && player.TeamId != 1) || (moveFromTeam2ToTeam1 && player.TeamId != 2))
            {
                continue;
            }

            double newAvgKdTeam1, newAvgKdTeam2, newAvgKpmTeam1, newAvgKpmTeam2;
            int newTeam1Count, newTeam2Count;

            if (player.TeamId == 1)
            {
                newTeam1Count = team1Players.Count - 1;
                newTeam2Count = team2Players.Count + 1;

                newAvgKdTeam1 = newTeam1Count > 0 ? (team1Players.Sum(p => p.LifeKd) - player.LifeKd) / newTeam1Count : 0;
                newAvgKdTeam2 = newTeam2Count > 0 ? (team2Players.Sum(p => p.LifeKd) + player.LifeKd) / newTeam2Count : 0;

                newAvgKpmTeam1 = newTeam1Count > 0 ? (team1Players.Sum(p => p.LifeKpm) - player.LifeKpm) / newTeam1Count : 0;
                newAvgKpmTeam2 = newTeam2Count > 0 ? (team2Players.Sum(p => p.LifeKpm) + player.LifeKpm) / newTeam2Count : 0;
            }
            else
            {
                newTeam1Count = team1Players.Count + 1;
                newTeam2Count = team2Players.Count - 1;

                newAvgKdTeam1 = newTeam1Count > 0 ? (team1Players.Sum(p => p.LifeKd) + player.LifeKd) / newTeam1Count : 0;
                newAvgKdTeam2 = newTeam2Count > 0 ? (team2Players.Sum(p => p.LifeKd) - player.LifeKd) / newTeam2Count : 0;

                newAvgKpmTeam1 = newTeam1Count > 0 ? (team1Players.Sum(p => p.LifeKpm) + player.LifeKpm) / newTeam1Count : 0;
                newAvgKpmTeam2 = newTeam2Count > 0 ? (team2Players.Sum(p => p.LifeKpm) - player.LifeKpm) / newTeam2Count : 0;
            }

            // 计算新的差值
            double newKdDiff = Math.Abs(newAvgKdTeam1 - newAvgKdTeam2 - targetKdDiff);
            double newKpmDiff = Math.Abs(newAvgKpmTeam1 - newAvgKpmTeam2 - targetKpmDiff);

            // 计算综合影响分数，优先考虑KD和KPM差值
            double impactScore = newKdDiff + newKpmDiff;

            // 找到影响最小的玩家
            if (impactScore < smallestImpactScore)
            {
                smallestImpactScore = impactScore;
                bestPlayerToMove = player;
            }
        }

        return bestPlayerToMove;
    }
    /// <summary>
    /// 找出最适合移动的玩家
    /// </summary>
    /// <param name="team1Players"></param>
    /// <param name="team2Players"></param>
    /// <param name="avgSkillTeam1"></param>
    /// <param name="avgSkillTeam2"></param>
    /// <param name="isTeam1Weaker"></param>
    /// <returns></returns>
    private PlayerData BalanceTeam(List<PlayerData> team1Players, List<PlayerData> team2Players, double avgSkillTeam1, double avgSkillTeam2, double skillflag, List<PlayerData> excludeplayer)
    {
        PlayerData bestPlayerToMove = null;
        double smallestImpactScore = double.MaxValue;

        bool moveFromTeam1ToTeam2 = false;
        bool moveFromTeam2ToTeam1 = false;

        // 确定移动方向
        int countDiff = team1Players.Count - team2Players.Count;
        if (countDiff > 2)
        {
            moveFromTeam1ToTeam2 = true;
        }
        else if (countDiff < -2)
        {
            moveFromTeam2ToTeam1 = true;
        }
        else
        {
            // 如果人数差距在2人及以内，则不考虑人数，只考虑Skill
            moveFromTeam1ToTeam2 = team1Players.Count > team2Players.Count;
            moveFromTeam2ToTeam1 = team2Players.Count > team1Players.Count;
        }

        foreach (var player in team1Players.Concat(team2Players))
        {
            if (excludeplayer.Any(player1 => player1.PersonaId == player.PersonaId))
            { continue; }
            // 忽略将玩家移动到已满的队伍
            if ((player.TeamId == 1 && team2Players.Count >= 32) || (player.TeamId == 2 && team1Players.Count >= 32))
            {
                continue;
            }

            // 只考虑从人数多的队伍向人数少的队伍移动
            if ((moveFromTeam1ToTeam2 && player.TeamId != 1) || (moveFromTeam2ToTeam1 && player.TeamId != 2))
            {
                continue;
            }

            double newAvgSkillTeam1, newAvgSkillTeam2;
            int newTeam1Count, newTeam2Count;

            if (player.TeamId == 1)
            {
                newTeam1Count = team1Players.Count - 1;
                newTeam2Count = team2Players.Count + 1;

                newAvgSkillTeam1 = newTeam1Count > 0 ? (team1Players.Sum(p => p.Skill) - player.Skill) / newTeam1Count : 0;
                newAvgSkillTeam2 = newTeam2Count > 0 ? (team2Players.Sum(p => p.Skill) + player.Skill) / newTeam2Count : 0;
            }
            else
            {
                newTeam1Count = team1Players.Count + 1;
                newTeam2Count = team2Players.Count - 1;

                newAvgSkillTeam1 = newTeam1Count > 0 ? (team1Players.Sum(p => p.Skill) + player.Skill) / newTeam1Count : 0;
                newAvgSkillTeam2 = newTeam2Count > 0 ? (team2Players.Sum(p => p.Skill) - player.Skill) / newTeam2Count : 0;
            }

            // 计算新的差值
            double newDiff = Math.Abs(newAvgSkillTeam1 - newAvgSkillTeam2 - skillflag);

            // 计算新的人数差距
            double newCountDiff = Math.Abs(newTeam1Count - newTeam2Count);

            // 计算综合影响分数，优先考虑人数差距，其次是Skill差值
            double impactScore = newCountDiff + newDiff;

            // 找到影响最小的玩家
            if (impactScore < smallestImpactScore)
            {
                smallestImpactScore = impactScore;
                bestPlayerToMove = player;
            }
        }

        return bestPlayerToMove;
    }
    private PlayerData FindBestPlayerToMoveForSkill(List<PlayerData> team1Players, List<PlayerData> team2Players, double avgSkillTeam1, double avgSkillTeam2, double targetSkillDiff, List<PlayerData> excludeplayer)
    {
        PlayerData bestPlayerToMove = null;
        double smallestImpactScore = double.MaxValue;

        bool moveFromTeam1ToTeam2 = false;
        bool moveFromTeam2ToTeam1 = false;

        // 确定移动方向
        int countDiff = team1Players.Count - team2Players.Count;
        if (countDiff > 0)
        {
            moveFromTeam1ToTeam2 = true;
        }
        else if (countDiff < 0)
        {
            moveFromTeam2ToTeam1 = true;
        }

        foreach (var player in team1Players.Concat(team2Players))
        {   
            if (excludeplayer.Any(player1 => player1.PersonaId == player.PersonaId))
            { continue; }
            // 忽略将玩家移动到已满的队伍
            if ((player.TeamId == 1 && team2Players.Count >= 32) || (player.TeamId == 2 && team1Players.Count >= 32))
            {
                continue;
            }

            // 只考虑从人数多的队伍向人数少的队伍移动
            if ((moveFromTeam1ToTeam2 && player.TeamId != 1) || (moveFromTeam2ToTeam1 && player.TeamId != 2))
            {
                continue;
            }

            double newAvgSkillTeam1, newAvgSkillTeam2;
            int newTeam1Count, newTeam2Count;

            if (player.TeamId == 1)
            {
                newTeam1Count = team1Players.Count - 1;
                newTeam2Count = team2Players.Count + 1;

                newAvgSkillTeam1 = newTeam1Count > 0 ? (team1Players.Sum(p => p.Skill) - player.Skill) / newTeam1Count : 0;
                newAvgSkillTeam2 = newTeam2Count > 0 ? (team2Players.Sum(p => p.Skill) + player.Skill) / newTeam2Count : 0;
            }
            else
            {
                newTeam1Count = team1Players.Count + 1;
                newTeam2Count = team2Players.Count - 1;

                newAvgSkillTeam1 = newTeam1Count > 0 ? (team1Players.Sum(p => p.Skill) + player.Skill) / newTeam1Count : 0;
                newAvgSkillTeam2 = newTeam2Count > 0 ? (team2Players.Sum(p => p.Skill) - player.Skill) / newTeam2Count : 0;
            }

            // 计算新的差值
            double newSkillDiff = Math.Abs(newAvgSkillTeam1 - newAvgSkillTeam2 - targetSkillDiff);

            // 计算新的人数差距
            double newCountDiff = Math.Abs(newTeam1Count - newTeam2Count);

            // 计算综合影响分数，优先考虑Skill差值
            double impactScore = newSkillDiff + newCountDiff;

            // 找到影响最小的玩家
            if (impactScore < smallestImpactScore)
            {
                smallestImpactScore = impactScore;
                bestPlayerToMove = player;
            }
        }

        return bestPlayerToMove;
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

        var initialGameCounts = new Dictionary<long, int>();
        var initialWinGameCounts = new Dictionary<long, int>();
        foreach (var player in topPlayers)
        {
            for (int i = 0; i < 5; i++)
            {
                int[] array = await QueryRecordWindow.GetAllGameCount(player.PersonaId);
                initialGameCounts[player.PersonaId] = array[0];
                initialWinGameCounts[player.PersonaId] = array[1];
                if (array[0] != 0 && array[1] != 0)
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
                for (int i = 0; i < 3; i++)
                {
                    int[] array = await QueryRecordWindow.GetAllGameCount(player.PersonaId);
                    currentGameCount = array[0];
                    currentWinGameCount = array[1];
                    if (array[0] != 0 && array[1] != 0)
                    {
                        //NotifierHelper.Show(NotifierType.Success, $" {currentGameCount}");
                        break;
                    }

                }
                if (initialGameCounts[player.PersonaId] < currentGameCount)
                {
                    increasedCount++;
                }
                if (player.TeamId == 1 && initialWinGameCounts[player.PersonaId] < currentWinGameCount)
                {
                    increasedWinCount++;
                }
                if (player.TeamId == 2 && initialWinGameCounts[player.PersonaId] < currentWinGameCount)
                {
                    increasedWinCount--;
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
                jiankongflag = 1;
                if (increasedWinCount > 0)
                { attackwinflag = true; }
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



