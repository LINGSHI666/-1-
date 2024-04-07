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

namespace BF1ServerTools.Views;

/// <summary>
/// RobotView.xaml 的交互逻辑
/// </summary>
public partial class RobotView : UserControl
{  /// <summary>
///   换图播报控制
/// </summary>
    public static bool showflag { get; set; }
    public static bool liveflag { get; set; } = false;//监控存活判断

    public static int jiankongflag = 0; // 1表示启动切换地图
    
    public Queue<PlayerData> excludeList = new Queue<PlayerData>(); // 使用队列来管理排除名单 
   
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
        if (isPlayerInTeam3)
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
            NotifierHelper.Show(NotifierType.Success, "以玩家模式运行");
            flag = await QueryRecordWindow.GetGameCount(Globals.PersonaId);
            await Task.Delay(500);
            flagthen = flag;
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
            if (!isPlayerInTeam3)
            {
                flagthen = await QueryRecordWindow.GetGameCount(Globals.PersonaId);
            }
            else
            {
                flagthen = flag + jiankongflag;
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

            if (votes.Count > 0)
            {
                if (showflag)
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
               
            }
           
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
                    if (isPlayerInTeam3)
                    {
                        // 取消当前的监控任务
                        while (liveflag)
                        {
                            monitoringCts.Cancel();
                            liveflag = false;
                            await Task.Delay(1000);  // 等待一段时间确保监控任务已经完全停止
                        }
                        jiankongflag = 0;
                         flag = 0;
                         flagthen = 0;
                        await Task.Delay(14000);
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
                        await Task.Delay(800);
                        if (jiankongflag == 1)
                        {
                            MessageBox.Show($"监控过程中出错,请关闭程序");
                        }
                    }
                    else
                    {
                        flag = await QueryRecordWindow.GetGameCount(Globals.PersonaId);
                        flagthen = flag;
                        await Task.Delay(2000);
                    }



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
    private  bool autochange;
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
        }

      

        //ShowServerMapList();
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
        if (!Changerun)
        {
            Changerun = true;
            NotifierHelper.Show(NotifierType.Information, "已启动换图");
            await XPFARM();
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

    public  async  Task ShowServerMapList()
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

            foreach (var item in fullServerDetails.result.serverInfo.rotation)
            {
                mapItems.Add(new MapItem
                {
                    MapName = ChsUtil.ToSimplified(item.mapPrettyName),
                    MapMode = ChsUtil.ToSimplified(item.modePrettyName)
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

        var playerList = Player.GetPlayerList()
                                .Where(player => (player.TeamId == 1 || player.TeamId == 2) && player.PersonaId != 0)
                                .ToList();

        // 确保有玩家参与
        if (playerList.Count == 0)
        {
            return; // 直接返回，因为没有玩家
        }

        // 获取前三名玩家进行监控
        var topPlayers = playerList.GroupBy(p => p.TeamId)
                                   .SelectMany(g => g.OrderByDescending(p => p.Score).Take(3))
                                   .ToList();

        var initialGameCounts = new Dictionary<long, int>();
        foreach (var player in topPlayers)
        {
            initialGameCounts[player.PersonaId] = await QueryRecordWindow.GetGameCount(player.PersonaId);
        }

        // 监控循环
        while (!cancellationToken.IsCancellationRequested)
        {
            int increasedCount = 0;

            foreach (var player in topPlayers)
            {
                int currentGameCount = await QueryRecordWindow.GetGameCount(player.PersonaId);

                if (initialGameCounts[player.PersonaId] < currentGameCount)
                {
                    increasedCount++;
                }
            }
            liveflag = true;
            int playerCountThreshold = topPlayers.Count == 1 ? 1 : topPlayers.Count - 1;
            if (increasedCount >= playerCountThreshold)
            {
                if (cancellationToken.IsCancellationRequested)
                { break; }
                jiankongflag = 1;
                break; // 至少比玩家数少一的玩家游戏场数加1，或在一个人时该人场数加1
            }

            await Task.Delay(checkInterval, cancellationToken);
        }
    }


    public async Task ContinuousMonitoring()
    {
        while (!monitoringCts.Token.IsCancellationRequested)
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
            liveflag = true;
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
            Console.WriteLine("获取服务器详情失败。");
        }

        return mapNamesToId;
    }



    public async Task XPFARM()
    {
        var mapNamesToId = await CreateMapNamesToIdMapAsync();

        // MapListView是你的ListView控件的名字
        var selectedMaps = MapListView.Items.Cast<MapItem>().Where(item => item.IsSelected).ToList();

        if (!selectedMaps.Any())
        {
            MessageBox.Show("没有选中任何地图。");
            return;
        }

        int currentIndex = 0;
        string currentMapName = ChsUtil.ToSimplified(ClientHelper.GetMapChsName(MonitView.Mapnameget()));

        // 找到当前地图在列表中的位置
        var currentMap = selectedMaps.FirstOrDefault(item => item.MapName == currentMapName);
        if (currentMap != null)
        {
            currentIndex = selectedMaps.IndexOf(currentMap);
        }

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
                currentIndex = (currentIndex + 1) % selectedMaps.Count;
                var nextMap = selectedMaps[currentIndex];
                int mapIdNext = mapNamesToId[nextMap.MapName];

                var result = await BF1API.RSPChooseLevel(Globals.SessionId, Globals.PersistedGameId, mapIdNext);
                if (result.IsSuccess)
                {
                    NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换服务器 {Globals.GameId} 地图为 {nextMap.MapName} 成功");
                }
                else
                {
                   NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换服务器 {Globals.GameId} 地图为 {nextMap.MapName} 失败");
                }
                    // 取消当前的监控任务
                    while (liveflag)
                    {
                        monitoringCts.Cancel();
                        liveflag = false;
                        await Task.Delay(1000);  // 等待一段时间确保监控任务已经完全停止
                    }
                    jiankongflag = 0;
                    jiankongflag = 0;
                await Task.Delay(20000);  // 等待一段时间确保监控任务已经完全停止

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


