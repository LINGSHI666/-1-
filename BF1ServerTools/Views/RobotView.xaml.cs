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

namespace BF1ServerTools.Views;

/// <summary>
/// RobotView.xaml 的交互逻辑
/// </summary>
public partial class RobotView : UserControl
{  /// <summary>
///   换图播报控制
/// </summary>
    public static bool showflag { get; set; }
    public static int mapflag = 1;  // 1表示启动切换地图
    public static int jiankongflag = 0;
    public Queue<PlayerData> excludeList = new Queue<PlayerData>(); // 使用队列来管理排除名单 
    /// <summary>
    /// Robot配置文件集，以json格式保存到本地
    /// </summary>
    private RobotConfig RobotConfig { get; set; } = new();

    /// <summary>
    /// Robot配置文件路径
    /// </summary>
    private readonly string F_Robot_Path = FileUtil.D_Config_Path + @"\RobotConfig.json";

    ////////////////////////////////////////////////////////




    public RobotView()
    {

        InitializeComponent();

    }

    /// <summary>
    /// 主窗口关闭事件
    /// </summary>
    private void MainWindow_WindowClosingEvent()
    {
        SaveConfig();
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    private void SaveConfig()
    {
    }

    /// <summary>
    /// 追加控制台日志
    /// </summary>
    /// <param name="txt"></param>
    private void AppendLogger(string txt)
    {
        this.Dispatcher.BeginInvoke(() =>
        {
            TextBox_ConsoleLogger.AppendText($"[{DateTime.Now:T}] {txt}\r\n");
        });
    }
    private void Reportmapinfo_Click(object sender, RoutedEventArgs e)
    {
        // 调用您的播报信息方法
        showflag = true;
    }
    /// <summary>
    /// 启动QQ机器人服务
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_RunGoCqHttpServer_Click(object sender, RoutedEventArgs e)
    {

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
        MessageBox.Show($"当前自动平衡间隔为{minutes}分钟");



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
            labelCurrentkdkpm.Text = $"平衡目标为进攻(队伍1)lifekd、lifekp高于防守(队伍2){newValue:F2}(+-0.5)"; // 更新标签内容
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
    private void Timer_Tick(object sender, EventArgs e)
    {
        // 定时器触发时执行的任务
        RunPeriodicTasks();
    }

    // 地图投票系统
    public static async void Autochangemap()
    {
        int i = 2400;

        int flag = await QueryRecordWindow.GetGameCount(Globals.PersonaId);
        await Task.Delay(500);
        int flagthen = flag;
        ChatInputWindow.SendChsToBF1Chat("使用 vote 地图名称（拼音）来投票");

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
            MessageBox.Show("获取服务器详情失败。");
            return;
        }

        var fullServerDetails = JsonHelper.JsonDese<FullServerDetails>(mapDetailsResult.Content);


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
    {"流血宴廳", new List<string> {"liuxueyantin", "yantin"}},
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
                mapIdToName[mapId] = mapNameChinese; // Keep the Chinese name for display purposes
            }

            mapId++;
        }

        Dictionary<int, int> votes = new Dictionary<int, int>();
        Dictionary<string, bool> userHasVoted = new Dictionary<string, bool>();

        while (true)
        {
            flagthen = await QueryRecordWindow.GetGameCount(Globals.PersonaId);
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

            if (votes.Count > 0)
            {
                if (i == 2400 || showflag)
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
                    string combinedMessage = highestVoteMessage + "\n所有地图的得票数: " + allMapsVotesMessage + "\n投票示例 vote yamian";

                    // 发送消息
                    ChatInputWindow.SendChsToBF1Chat(combinedMessage);
                    NotifierHelper.Show(NotifierType.Success, combinedMessage);

                }
                i--;
            }
            if (i == 1)
            { i = 2400; }
            if (showflag)
            { showflag = false; }

            if (flagthen - flag == 1 && votes.Count > 0)
            {
                var highestVote = votes.Aggregate((l, r) => l.Value > r.Value ? l : r);
                int mapIdlocal = highestVote.Key; // 直接使用得票最多的mapId              
                var result = await BF1API.RSPChooseLevel(Globals.SessionId, Globals.PersistedGameId, mapIdlocal);
                if (result.IsSuccess)
                {
                    string mapName = mapIdToName[mapIdlocal];
                    NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换服务器 {Globals.GameId} 地图为 {mapName} 成功");
                    votes.Clear();
                    userHasVoted.Clear();// 清空投票，开始新一轮
                    i = 2400;
                    flag = await QueryRecordWindow.GetGameCount(Globals.PersonaId);
                    flagthen = flag;
                    await Task.Delay(2000);
                }
                else
                {
                    string failedMapName = mapIdToName.ContainsKey(highestVote.Key) ? mapIdToName[highestVote.Key] : "未知地图";
                    NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换服务器 {Globals.GameId} 地图为 {failedMapName} 失败\n{result.Content}");
                }
            }

            await Task.Delay(200); // 每200毫秒检查一次聊天
        }
    }

    private async void RunPeriodicTasks()
    {

        bool balanceAchieved = false;
        for (int i = 0; i < 99 && !balanceAchieved; i++)
        {
            //OpenConsoleWindow();
            //Console.WriteLine("Hello, console!");
            // 保持控制台打开，直到用户按下任意键
            //Console.ReadKey();
            List<PlayerData> playerListbegin = Player.GetPlayerList(); // 获取当前所有玩家的列表
            List<PlayerData> playerList = playerListbegin.Where(p => p.Kill >= 1 || p.Dead >= 1).ToList();
            //排除机器人
            int count = playerList.Count(p => p.PersonaId != 0);

            double kdkpmflag = sliderkdkpm != null ? sliderkdkpm.Value : 10;
            double skillflag = sliderskill != null ? sliderskill.Value : 100;
            if (count < 30)
            {
                NotifierHelper.Show(NotifierType.Error, "人数不足,或游戏刚开始");
                break;
            }

            // 更新玩家的生涯KD和KPM及技巧值
            foreach (var item in playerList)
            {
                item.LifeKd = PlayerUtil.GetLifeKD(item.PersonaId);
                item.LifeKpm = PlayerUtil.GetLifeKPM(item.PersonaId);
                item.Skill = PlayerUtil.GetSkill(item.PersonaId);
            }

            var team1Players = playerList.Where(p => p.TeamId == 1).ToList();
            var team2Players = playerList.Where(p => p.TeamId == 2).ToList();

            var avgLifeKdTeam1 = team1Players.Average(p => p.LifeKd);
            var avgLifeKpmTeam1 = team1Players.Average(p => p.LifeKpm);
            var avgSkillTeam1 = team1Players.Average(p => p.Skill);

            var avgLifeKdTeam2 = team2Players.Average(p => p.LifeKd);
            var avgLifeKpmTeam2 = team2Players.Average(p => p.LifeKpm);
            var avgSkillTeam2 = team2Players.Average(p => p.Skill);
            if (ExcludeAdminsAndVIPsCheckBox.IsChecked ?? false)
            {
                // 如果复选框被选中，则执行排除逻辑

                // 获取VIP和管理员的personaId列表，合并这两个列表
                List<long> adminAndVipIds = Globals.ServerAdmins_PID.Concat(Globals.ServerVIPs_PID).ToList();

                // 移除VIP和管理员
                playerList.RemoveAll(player => PlayerUtil.IsAdminVIP(player.PersonaId, adminAndVipIds));
            }
            playerList.RemoveAll(player => excludeList.Select(p => p.PersonaId).Contains(player.PersonaId));
            //移除已经换过边的玩家
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
                //队伍一过弱
                if (avgSkillTeam1 < avgSkillTeam2 + skillflag - 30)
                {
                    // 计算差值
                    var team1SkillDiff = avgSkillTeam1 - lowestSkillTeam1;
                    var team2SkillDiff = highestSkillTeam2 - avgSkillTeam2;

                    // 创建差值和相应行动的映射
                    var actions = new List<(double diff, Func<PlayerData> action)>
                 {
                  (team1SkillDiff, () => lowestSkillPlayerTeam1),
                   (team2SkillDiff, () => highestSkillPlayerTeam2)
                   };

                    // 找出最大差值及对应的行动
                    var maxAction = actions.OrderByDescending(a => a.diff).First().action;

                    // 执行行动，获取应该移动的玩家
                    var playerToMove = maxAction();

                    // 根据playerToMove所在队伍决定移动方向
                    int targetTeam = playerToMove.TeamId == 1 ? 2 : 1;

                    // 执行移动
                    var result = await BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, playerToMove.PersonaId, targetTeam);
                    // 重新获取玩家列表以验证换边是否成功
                    var updatedPlayerList = Player.GetPlayerList();
                    var movedPlayer = updatedPlayerList.FirstOrDefault(p => p.PersonaId == playerToMove.PersonaId);

                    if (movedPlayer != null && movedPlayer.TeamId == targetTeam)
                    {
                        // 如果排除名单已有三人，则移除最早添加的玩家
                        if (excludeList.Count >= 3)
                        {
                            NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{targetTeam}成功");
                            excludeList.Dequeue(); // 移除队列前端的元素
                        }

                        // 将新的玩家添加到排除名单的队尾
                        excludeList.Enqueue(movedPlayer);
                    }
                    else
                    {
                        //NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{targetTeam}失败");
                    }
                }
                //队伍2过弱
                else if (avgSkillTeam1 > avgSkillTeam2 + skillflag + 30)
                {
                    // 计算差值
                    var team1SkillDiff = highestSkillTeam1 - avgSkillTeam1;
                    var team2SkillDiff = avgSkillTeam2 - lowestSkillTeam2;

                    // 创建差值和相应行动的映射
                    var actions = new List<(double diff, Func<PlayerData> action)>
                 {
                   (team1SkillDiff, () => highestSkillPlayerTeam1),
                   (team2SkillDiff, () => lowestSkillPlayerTeam2)
                  };

                    // 找出最大差值及对应的行动
                    var maxAction = actions.OrderByDescending(a => a.diff).First().action;

                    // 执行行动，获取应该移动的玩家
                    var playerToMove = maxAction();

                    // 根据playerToMove所在队伍决定移动方向
                    int targetTeam = playerToMove.TeamId == 1 ? 2 : 1;

                    // 执行移动
                    var result = await BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, playerToMove.PersonaId, targetTeam);
                    // 重新获取玩家列表以验证换边是否成功
                    var updatedPlayerList = Player.GetPlayerList();
                    var movedPlayer = updatedPlayerList.FirstOrDefault(p => p.PersonaId == playerToMove.PersonaId);

                    if (movedPlayer != null && movedPlayer.TeamId == targetTeam)
                    {
                        // 如果排除名单已有三人，则移除最早添加的玩家
                        if (excludeList.Count >= 3)
                        {
                            NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{targetTeam}成功");
                            excludeList.Dequeue(); // 移除队列前端的元素
                        }

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
                // 获取队伍1生涯KPM最高的玩家
                var highestLifeKpmPlayerTeam1 = team1Players.OrderByDescending(p => p.LifeKpm).FirstOrDefault();
                var highestLifeKpmTeam1 = highestLifeKpmPlayerTeam1?.LifeKpm ?? 0;

                // 获取队伍1生涯KPM最低的玩家
                var lowestLifeKpmPlayerTeam1 = team1Players.OrderBy(p => p.LifeKpm).FirstOrDefault();
                var lowestLifeKpmTeam1 = lowestLifeKpmPlayerTeam1?.LifeKpm ?? 0;

                // 获取队伍1生涯KD最高的玩家
                var highestLifeKdPlayerTeam1 = team1Players.OrderByDescending(p => p.LifeKd).FirstOrDefault();
                var highestLifeKdTeam1 = highestLifeKdPlayerTeam1?.LifeKd ?? 0;

                // 获取队伍1生涯KD最低的玩家
                var lowestLifeKdPlayerTeam1 = team1Players.OrderBy(p => p.LifeKd).FirstOrDefault();
                var lowestLifeKdTeam1 = lowestLifeKdPlayerTeam1?.LifeKd ?? 0;

                // 获取队伍2生涯KPM最高的玩家
                var highestLifeKpmPlayerTeam2 = team2Players.OrderByDescending(p => p.LifeKpm).FirstOrDefault();
                var highestLifeKpmTeam2 = highestLifeKpmPlayerTeam2?.LifeKpm ?? 0;

                // 获取队伍2生涯KPM最低的玩家
                var lowestLifeKpmPlayerTeam2 = team2Players.OrderBy(p => p.LifeKpm).FirstOrDefault();
                var lowestLifeKpmTeam2 = lowestLifeKpmPlayerTeam2?.LifeKpm ?? 0;

                // 获取队伍2生涯KD最高的玩家
                var highestLifeKdPlayerTeam2 = team2Players.OrderByDescending(p => p.LifeKd).FirstOrDefault();
                var highestLifeKdTeam2 = highestLifeKdPlayerTeam2?.LifeKd ?? 0;

                // 获取队伍2生涯KD最低的玩家
                var lowestLifeKdPlayerTeam2 = team2Players.OrderBy(p => p.LifeKd).FirstOrDefault();
                var lowestLifeKdTeam2 = lowestLifeKdPlayerTeam2?.LifeKd ?? 0;
                // 判断是否需要调整玩家队伍
                //队伍1过弱
                if (avgLifeKdTeam1 < avgLifeKdTeam2 + kdkpmflag - 0.05 && avgLifeKpmTeam1 < avgLifeKpmTeam2 + kdkpmflag - 0.05)
                {
                    // 计算差值
                    var team1KdDiff = avgLifeKdTeam1 - lowestLifeKdTeam1;
                    var team1KpmDiff = avgLifeKpmTeam1 - lowestLifeKpmTeam1;
                    var team2KdDiff = highestLifeKdTeam2 - avgLifeKdTeam2;
                    var team2KpmDiff = highestLifeKpmTeam2 - avgLifeKpmTeam2;

                    // 创建差值和相应行动的映射
                    var actions = new List<(double diff, Func<PlayerData> action)>
               {
                 (team1KdDiff, () => lowestLifeKdPlayerTeam1),
                 (team1KpmDiff, () => lowestLifeKpmPlayerTeam1),
                 (team2KdDiff, () => highestLifeKdPlayerTeam2),
                 (team2KpmDiff, () => highestLifeKpmPlayerTeam2)
                 };

                    // 找出最大差值及对应的行动
                    var maxAction = actions.OrderByDescending(a => a.diff).First().action;

                    // 执行行动，获取应该移动的玩家
                    var playerToMove = maxAction();

                    // 根据playerToMove所在队伍决定移动方向
                    int targetTeam = playerToMove.TeamId == 1 ? 2 : 1;

                    // 执行移动
                    var result = await BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, playerToMove.PersonaId, targetTeam);
                    // if (result.IsSuccess)由于管理反应展示过于频繁，不再提示
                    //{
                    // NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{targetTeam}成功");
                    // }
                    // else
                    // {
                    // NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{targetTeam}失败\n{result.Content}");
                    // }
                    // 重新获取玩家列表以验证换边是否成功
                    var updatedPlayerList = Player.GetPlayerList();
                    var movedPlayer = updatedPlayerList.FirstOrDefault(p => p.PersonaId == playerToMove.PersonaId);

                    if (movedPlayer != null && movedPlayer.TeamId == targetTeam)
                    {
                        // 如果排除名单已有三人，则移除最早添加的玩家
                        if (excludeList.Count >= 3)
                        {
                            NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{targetTeam}成功");
                            excludeList.Dequeue(); // 移除队列前端的元素
                        }

                        // 将新的玩家添加到排除名单的队尾
                        excludeList.Enqueue(movedPlayer);
                    }
                    else
                    {
                        //NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{targetTeam}失败");
                    }
                }
                //队伍2过弱
                else if (avgLifeKdTeam1 > avgLifeKdTeam2 + kdkpmflag + 0.05 && avgLifeKpmTeam1 > avgLifeKpmTeam2 + kdkpmflag + 0.05)
                {
                    // 计算差值
                    var team1KdDiff = highestLifeKdTeam1 - avgLifeKdTeam1;
                    var team1KpmDiff = highestLifeKpmTeam1 - avgLifeKpmTeam1;
                    var team2KdDiff = avgLifeKdTeam2 - highestLifeKdTeam2;
                    var team2KpmDiff = avgLifeKpmTeam2 - highestLifeKpmTeam2;
                    // 创建差值和相应行动的映射
                    var actions = new List<(double diff, Func<PlayerData> action)>
               {
                 (team1KdDiff, () => highestLifeKdPlayerTeam1),
                 (team1KpmDiff, () => highestLifeKpmPlayerTeam1),
                 (team2KdDiff, () => lowestLifeKdPlayerTeam2),
                 (team2KpmDiff, () => lowestLifeKpmPlayerTeam2)
                 };
                    // 找出最大差值及对应的行动
                    var maxAction = actions.OrderByDescending(a => a.diff).First().action;

                    // 执行行动，获取应该移动的玩家
                    var playerToMove = maxAction();

                    // 根据playerToMove所在队伍决定移动方向
                    int targetTeam = playerToMove.TeamId == 1 ? 2 : 1;

                    // 执行移动
                    var result = await BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, playerToMove.PersonaId, targetTeam);
                    //if (result.IsSuccess)
                    //{
                    //NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{targetTeam}成功");
                    //}
                    // else
                    // {
                    //   NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{targetTeam}失败\n{result.Content}");
                    // }
                    // 重新获取玩家列表以验证换边是否成功
                    var updatedPlayerList = Player.GetPlayerList();
                    var movedPlayer = updatedPlayerList.FirstOrDefault(p => p.PersonaId == playerToMove.PersonaId);

                    if (movedPlayer != null && movedPlayer.TeamId == targetTeam)
                    {
                        // 如果排除名单已有三人，则移除最早添加的玩家
                        if (excludeList.Count >= 3)
                        {
                            NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍{targetTeam}成功");
                            excludeList.Dequeue(); // 移除队列前端的元素
                        }

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
                    NotifierHelper.Show(NotifierType.Information, $"队伍已平衡，无需进一步操作\nteam1kd [{avgLifeKdTeam1:0.00}]kpm [{avgLifeKpmTeam1:0.00}] || team2kd [{avgLifeKdTeam2:0.00}]kpm [{avgLifeKpmTeam2:0.00}]");
                }

            }
            if (!balanceAchieved)
            {
                await Task.Delay(3000);
            }
        }
    }


    private DispatcherTimer timer;
    /// <summary>
    /// 启动投票换图服务
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private Thread autoChangeMapThread; // 定义一个线程变量来控制线程的创建
    private void Button_RunWebsocketServer_Click(object sender, RoutedEventArgs e)
    {
        NotifierHelper.Show(NotifierType.Success, "投票换图已启动");

        // 检查线程是否已经存在并且正在运行，如果不是，则创建并启动线程
        if (autoChangeMapThread == null || !autoChangeMapThread.IsAlive)
        {
            autoChangeMapThread = new Thread(Autochangemap)
            {
                Name = "auto changemap",
                IsBackground = true
            };
            autoChangeMapThread.Start();
        }
    }
    /// <summary>
    /// 停止自动平衡
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_StopWebsocketServer_Click(object sender, RoutedEventArgs e)
    {
        if (timer != null)
        {
            timer.Stop();
            NotifierHelper.Show(NotifierType.Information, "已停止自动平衡");
        }
        //ShowServerMapList();
    }



    private async Task ShowServerMapList()
    {
        if (string.IsNullOrEmpty(Globals.SessionId))
        {
            MessageBox.Show("会话 ID 为空，无法获取地图列表。");
            return;
        }

        var result = await BF1API.GetFullServerDetails(Globals.SessionId, Globals.GameId);
        if (result.IsSuccess)
        {
            var fullServerDetails = JsonHelper.JsonDese<BF1ServerTools.API.RespJson.FullServerDetails>(result.Content);
            var mapList = new StringBuilder();

            foreach (var item in fullServerDetails.result.serverInfo.rotation)
            {
                string mapName = ChsUtil.ToSimplified(item.mapPrettyName);
                string mapMode = ChsUtil.ToSimplified(item.modePrettyName);

                mapList.AppendLine($"{mapName} - {mapMode}");
            }

            MessageBox.Show(mapList.ToString(), "地图列表");
        }
        else
        {
            MessageBox.Show("获取服务器详情失败。");
        }
    }
    //换超过25000分的人
    public static async Task MoveHighScoringPlayers()
    {
        // 获取当前所有玩家的列表
        List<PlayerData> playerList = Player.GetPlayerList();
        int count = playerList.Count(p => p.PersonaId != 0);
        if (count > 32)
        {
            // 遍历玩家列表
            foreach (var player in playerList)
            {
                // 检查玩家是否在队伍1且分数高于25000
                if (player.TeamId == 1 && player.Score > 25000)
                {
                    // 将符合条件的玩家移动到队伍2
                    int targetTeam = 2; // 目标队伍ID
                    var result = await BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, player.PersonaId, targetTeam);

                    // 根据移动结果进行处理
                    if (result.IsSuccess)
                    {
                        Console.WriteLine($"玩家 {player.Name} 已成功移动到队伍 {targetTeam}。");
                    }
                    else
                    {
                        Console.WriteLine($"移动玩家 {player.Name} 到队伍 {targetTeam} 失败：{result.Content}。");
                    }
                }
            }
        }
    }
    //刷分换图
    public static async void Autochangemapshuafen()
    {
        

        var mapNamesToId = new Dictionary<string, int>();
        var mapIdToName = new Dictionary<int, string>();

        // 获取服务器地图详情并填充字典
        var mapDetailsResult = await BF1API.GetFullServerDetails(Globals.SessionId, Globals.GameId);
        if (mapDetailsResult.IsSuccess)
        {
            var fullServerDetails = JsonHelper.JsonDese<BF1ServerTools.API.RespJson.FullServerDetails>(mapDetailsResult.Content);
            int mapId = 0;

            foreach (var item in fullServerDetails.result.serverInfo.rotation)
            {
                string mapName = ChsUtil.ToSimplified(item.mapPrettyName);
                mapNamesToId[mapName] = mapId;
                mapIdToName[mapId] = mapName;
                mapId++;
            }
        }
        else
        {
            MessageBox.Show("获取服务器详情失败。");
            return;
        }

        // 使用实际的mapId
        int[] targetMapIds = new int[2];
        targetMapIds[0] = mapNamesToId.ContainsKey("阿奇巴巴") ? mapNamesToId["阿奇巴巴"] : -1;
        targetMapIds[1] = mapNamesToId.ContainsKey("西奈沙漠") ? mapNamesToId["西奈沙漠"] : -1;

        int currentMapIndex = 0;

        while (true)
        {
            if (mapflag == 1)
            {
                var currentMapId = targetMapIds[currentMapIndex];
                if (currentMapId != -1)
                {
                    var result = await BF1API.RSPChooseLevel(Globals.SessionId, Globals.PersistedGameId, currentMapId);
                    if (result.IsSuccess)
                    {
                        string mapName = mapIdToName[currentMapId];
                        NotifierHelper.Show(NotifierType.Success, $"更换服务器 {Globals.GameId} 地图为 {mapName} 成功");

                        // 重置mapflag并等待约20秒以处理服务器数据查询的延迟
                        jiankongflag = 0;
                        mapflag = 0;
                        await Task.Delay(20000);
                    }
                    else
                    {
                        NotifierHelper.Show(NotifierType.Error, $"更换服务器 {Globals.GameId} 地图失败\n{result.Content}");
                    }
                }

                // 切换到下一张地图
                currentMapIndex = (currentMapIndex + 1) % targetMapIds.Length;
            }

           
            
            await MoveHighScoringPlayers();//25000分换边
            

            await Task.Delay(200); // 每100ms检查一次
        }
    }
    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();//监控控制
    //检查玩家场数变化
    public void StartMonitoringTopPlayersGameCount(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            const int checkInterval = 200; // 检查间隔（毫秒）
            const int duration = 30000; // 监控持续时间（毫秒）
            int elapsedTime = 0;
            var initialGameCounts = new Dictionary<long, int>();

            // 首先获取所有玩家的初始游戏场数
            var playerList = Player.GetPlayerList();
            foreach (var player in playerList)
            {
                initialGameCounts[player.PersonaId] = await QueryRecordWindow.GetGameCount(player.PersonaId);
            }

            while (elapsedTime < duration && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (mapflag == 1)
                {
                    break;
                }

                var topPlayersByTeam = playerList
                    .GroupBy(p => p.TeamId)
                    .SelectMany(group => group.OrderByDescending(p => p.Score).Take(3))
                    .ToList();

                if (!topPlayersByTeam.Any())
                {
                    await Task.Delay(checkInterval);
                    elapsedTime += checkInterval;
                    continue;
                }

                int increasedCount = 0;
                int totalCount = topPlayersByTeam.Count;

                foreach (var player in topPlayersByTeam)
                {
                    if (!initialGameCounts.ContainsKey(player.PersonaId))
                    {
                        continue;  // 如果玩家不在初始列表中，跳过
                    }

                    int currentGameCount = await QueryRecordWindow.GetGameCount(player.PersonaId);
                    int initialGameCount = initialGameCounts[player.PersonaId];

                    if (currentGameCount > initialGameCount)
                    {
                        increasedCount++;
                    }
                }

                if (totalCount > 0 && increasedCount >= totalCount / 2)
                {
                    mapflag = 1;
                    cancellationTokenSource.Cancel();
                    break;
                }

                await Task.Delay(checkInterval);
                elapsedTime += checkInterval;
            }
        });
    }

    private void shuafen(object sender, RoutedEventArgs e)
    {
        jiankong();
        Autochangemapshuafen();//启动刷分换图
    }
    public  async void jiankong()
    {
        await Task.Delay(5000);
        while (true)
        {
             if(mapflag ==  1 || jiankongflag == 0)
            {
                cancellationTokenSource.Cancel();
                jiankongflag = 1;
                await Task.Delay(15000);//换图等待
            }
            StartMonitoringTopPlayersGameCount(cancellationTokenSource.Token);
            await Task.Delay(15000);
        }
    }
    
}