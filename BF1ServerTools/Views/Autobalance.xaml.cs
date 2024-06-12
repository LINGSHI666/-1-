using BF1ServerTools.API;
using BF1ServerTools.Data;
using BF1ServerTools.Helper;
using BF1ServerTools.RES;
using BF1ServerTools.SDK;
using BF1ServerTools.SDK.Data;
using BF1ServerTools.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using BF1ServerTools.SDK.Core;
using System.Drawing;
using System.Windows.Media.Animation;
using BF1ServerTools.Windows;
using NStandard;
using System.Windows.Threading;
using System.Xml.Linq;
using SharpAvi;
using SharpAvi.Output;
using System.Diagnostics;
using SharpAvi.Codecs;


using System.Threading.Tasks;
using Point = System.Drawing.Point;
using SharpAvi.Codecs;
using static CommunityToolkit.Mvvm.ComponentModel.__Internals.__TaskExtensions.TaskAwaitableWithoutEndValidation;
using static BF1ServerTools.RES.Data.ModeData;
using Microsoft.VisualBasic.ApplicationServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;




namespace BF1ServerTools.Views;

/// <summary>
/// Autobalance.xaml 的交互逻辑
/// </summary>
public partial class Autobalance : UserControl
{
    private Queue<PlayerData> excludeList = new Queue<PlayerData>(); // 使用队列来管理临时排除名单 
    private Queue<PlayerData> excludeList2 = new Queue<PlayerData>();
    [DllImport("user32.dll")]
    static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

    private const int MOUSEEVENTF_LEFTDOWN = 0x02;
    private const int MOUSEEVENTF_LEFTUP = 0x04;
    public ObservableCollection<PlayerData> playerDataQueue { get; private set; }
    public void EnqueuePlayerData(PlayerData playerData)
    {
        playerDataQueue.Add(playerData);
    }

    public PlayerData DequeuePlayerData()
    {
        if (playerDataQueue.Count > 0)
        {
            var item = playerDataQueue[0];
            playerDataQueue.RemoveAt(0);
            return item;
        }
        return null; // 或其他适当的处理
    }
    public Autobalance()
    {
        InitializeComponent();
        InitializeVoteTimer();
        playerDataQueue = new ObservableCollection<PlayerData>();
        this.DataContext = this;  
    }
   
    
    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {

        if (labelCurrentMinutes != null) // 检查控件是否为null
        {
            double newValue = e.NewValue; // 获取滑块的当前值
            labelCurrentMinutes.Content = $"当前为{newValue}分钟"; // 更新标签内容
        }
    }
    private void Slider_ScoreValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {

        if (labelCurrentScore != null) // 检查控件是否为null
        {
            double newValue = e.NewValue; // 获取滑块的当前值
            labelCurrentScore.Content = $"当前启动自动平衡所需分差为{newValue}（0代表忽略该条件）"; // 更新标签内容
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
    private void Slider_ValueChangedwatchreport(object sender, RoutedPropertyChangedEventArgs<double> e)
    {

        if (labelCurrentdwatchreport != null) // 检查控件是否为null
        {
            double newValue = e.NewValue; // 获取滑块的当前值
            labelCurrentdwatchreport.Text = $"播报间隔为 {newValue} 分钟 0=永不自动播报"; // 更新标签内容
            if (newValue > 0)
            {
                voteTimer.Interval = TimeSpan.FromMinutes(newValue);
                voteTimer.Start();
            }
            else { voteTimer.Stop(); }
        }
    }
    private void Slider_ValueChangedwatchtime(object sender, RoutedPropertyChangedEventArgs<double> e)
    {

        if (labelCurrentdwatchtime != null) // 检查控件是否为null
        {
            double newValue = e.NewValue; // 获取滑块的当前值
            labelCurrentdwatchtime.Text = $"开始观战后录制时长为{newValue}分钟"; // 更新标签内容
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


private void VoteTimer_Tick(object sender, EventArgs e)
{
    // 滑块的值不是0，则设置 showflag 为1
    if (sliderwatchreport.Value > 0)
    {
            ChatInputWindow.SendChsToBF1Chat("自动观战已启用，使用report 玩家ID 来使用");
    }
}
/// <summary>
/// 启动定时平衡
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
private DispatcherTimer timer;
    private bool isTimerRunning = false;
    private void Timer_Tick(object sender, EventArgs e)
    {
        // 定时器触发时执行的任务
        RunPeriodicTasks();
    }
    /// <summary>
    /// 停止自动平衡
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private  async void Button_StopWebsocketServer_Click(object sender, RoutedEventArgs e)
    {
        if (timer != null)
        {
            timer.Stop();
            NotifierHelper.Show(NotifierType.Information, "已停止自动平衡");
            isTimerRunning = false;
        }
        if (cts != null)
        {

            cts.Cancel();
            NotifierHelper.Show(NotifierType.Information, "已停止自动平衡");
        }
        StartRecording("1");
        await Task.Delay(5000);
        StopRecording();
        // await Autowatch("ZED234");



    }
    private void Reportmapinfo_Click(object sender, RoutedEventArgs e)
    {

        ChatInputWindow.SendChsToBF1Chat("自动观战已启用，使用report 玩家ID 来使用");


    }
    public static bool IsOrange(System.Drawing.Color color)
    {
        // 设定橙色的 RGB 范围
        bool redInRange = color.R >= 130 && color.R <= 230;
        bool greenInRange = color.G >= 70 && color.G <= 180;
        bool blueInRange = color.B >= 10 && color.B <= 80;

        return redInRange && greenInRange && blueInRange;
    }
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
        int score = (int)(sliderscore != null ? sliderscore.Value : 0);
        // 创建一个 CancellationTokenSource 对象来控制任务的取消
        CancellationTokenSource cts = new CancellationTokenSource();
        if (score != 0)
        {

            // 启动任务
            Task balanceTask = AutoScoreBalance(score, cts.Token);
        }


    }
    private Thread autoRecordThread; // 定义一个线程变量来控制线程的创建
    private bool autorecord;
    private void Button_ActiveAutoWatch_Click(object sender, RoutedEventArgs e)
    {


        // 检查线程是否已经存在并且正在运行，如果不是，则创建并启动线程
        if (!autorecord)
        {
            NotifierHelper.Show(NotifierType.Success, "自动观战已启动");
            autoRecordThread = new Thread(Autorecord)
            {
                Name = "auto record",
                IsBackground = true
            };
            autoRecordThread.Start();
            autorecord = true;

        }
        else { NotifierHelper.Show(NotifierType.Information, "自动观战已经运行了"); }
    }
    //自动观战
    private async void Autorecord() 
    {
        List<PlayerData> playerListbegin = Player.GetPlayerList(); // 获取当前所有玩家的列表
        int minutes = 1;
        Dispatcher.Invoke(() =>
        {
            minutes = (int)sliderwatchtime.Value;
        });
       
        bool isPlayerInTeam0 = playerListbegin.Any(player => player.PersonaId == Globals.PersonaId && player.TeamId == 0);
        if (!isPlayerInTeam0)//测试时忽略
        {
            autorecord = false;
            Dispatcher.Invoke(() =>
            {
                NotifierHelper.Show(NotifierType.Error, "不处于观战状态下，操作取消");
            });
        }
        
        ChatInputWindow.SendChsToBF1Chat("自动观战已启用，使用report 玩家ID 来使用");
        while (true)
        {
            string lastSender = Chat.GetLastChatSender(out _);
            string lastContent = Chat.GetLastChatContent(out _).ToLower();
            if(!string.IsNullOrEmpty(lastContent) && lastContent.StartsWith("report "))
            {
                string voteName = lastContent.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1)?.ToLower();
                if (voteName != null )
                {
                    int a = await Autowatch(voteName);
                    switch (a)
                    {
                        case 0:
                            ChatInputWindow.SendChsToBF1Chat("开始观战");
                            await Task.Delay(500);
                            while (await Autowatch(voteName) == 0) { }
                            goto case 1;
                            
                           
                            case 1:
                            // 设置定时器来停止录制
                            timerrecord = new System.Threading.Timer(TimerCallback, null, minutes * 60 * 1000, Timeout.Infinite);
                            ChatInputWindow.SendChsToBF1Chat("开始观战");
                            await Task.Delay(500);
                            StartRecording(voteName);
                            while (await Autowatch(voteName) != 2)
                            {
                                BEGIN: List<PlayerData> playerList = Player.GetPlayerList(); // 获取当前所有玩家的列表
                                if (playerList == null)
                                { break; }
                                PlayerData player = playerList.FirstOrDefault(p => p.Name == voteName);
                                if (player.Kit != "")
                                {
                                    await Task.Delay(2000);
                                    goto BEGIN;
                                }
                            } // 如果循环结束但定时器尚未停止录制，则手动停止
                            if (!stopRequested)
                            {
                                StopRecording();
                            }
                            StopRecording();
                            break;
                            case 2:
                            ChatInputWindow.SendChsToBF1Chat("玩家ID错误"); 
                            ;
                            break;
                    }
                }
                
            }
        }
        }
    private System.Threading.Timer timerrecord;
    private bool stopRequested = false;
    private void TimerCallback(object state)
    {
        Console.WriteLine("Timer finished.");
        StopRecording();
        stopRequested = true;
        timerrecord.Dispose(); // 清理定时器
    }
    private async void Button_Balance_Click(object sender, RoutedEventArgs e)
    {
        RunPeriodicTasks();

    }
    private CancellationTokenSource cts;
    //分差自动平衡
    private async Task AutoScoreBalance(int scoreflag, CancellationToken cancellationToken)
    {   while (!cancellationToken.IsCancellationRequested)
        {
            if (Math.Abs(Server.GetTeam1Score() - Server.GetTeam2Score()) >= scoreflag)
            {
                if (timer != null)
                {
                    timer.Stop();
                    double minutes = 10; // 默认值

                    // 使用 Dispatcher.Invoke 在 UI 线程上访问 slider
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        minutes = slider != null ? slider.Value : 10;
                    });

                    // 设置定时器的时间间隔为滑块的值
                    timer.Interval = TimeSpan.FromMinutes(minutes);
                    // 设置定时器触发的事件
                    timer.Tick += Timer_Tick;

                    // 启动定时器
                    timer.Start();

                }
                RunPeriodicTasks();
                int score = Server.GetTeam1FlagScore();
                while (Server.GetTeam1FlagScore() >= score)
                {
                    try
                    {
                        await Task.Delay(2000, cancellationToken);
                       
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                }
            }
            try
            {
                await Task.Delay(1000, cancellationToken);

            }
            catch (TaskCanceledException)
            {
                return;
            }
        }
    }
    public static async Task<int> Autowatch(string name )
    {
        List<PlayerData> playerList = Player.GetPlayerList(); // 获取当前所有玩家的列表
        if( playerList == null )
        { return 2; }
        PlayerData player = playerList.FirstOrDefault(p => p.Name == name);
        if (player != null)
        {
            if (player.Kit != "")
            {
                double horizontal = 0;
                double vertical = 0.1329;
                if (player.TeamId == 1)
                {
                    horizontal = 0.3518;
                }
                else if (player.TeamId == 2)
                {
                    horizontal = 0.6815;
                }
                else
                {
                    return 2;//玩家在观战 
                }
                List<PlayerData> teamPlayers = playerList
                .Where(p => p.TeamId == player.TeamId)
                .OrderByDescending(p => p.Score) // 按分数由大到小排序
                .ToList();
                // 查找特定玩家在排序后队伍中的索引
                int playerRank = teamPlayers.FindIndex(p => p.Name == name) + 1;
                vertical = vertical + 0.016*(playerRank-1) ;
                if (!Chat.GetWindowIsTop())
                {
                    Memory.SetBF1WindowForeground();
                }
                await Task.Delay(1000);
                Win32.Keybd_Event(WinVK.TAB, Win32.MapVirtualKey(WinVK.TAB, 0), 0, 0);
                await Task.Delay(500);
                // 在新线程中执行鼠标点击操作，避免干扰按键事件
                await Task.Run(() => SimulateClickAtPosition(horizontal, vertical));
                await Task.Delay(500);
                Win32.Keybd_Event(WinVK.TAB, Win32.MapVirtualKey(WinVK.TAB, 0), 2, 0);
                await Task.Delay(200);
                Win32.Keybd_Event(WinVK.TAB, Win32.MapVirtualKey(WinVK.TAB, 0), 0, 0);
                await Task.Delay(500);
                System.Drawing.Color color = GetColorAt(horizontal, vertical);
                Win32.Keybd_Event(WinVK.TAB, Win32.MapVirtualKey(WinVK.TAB, 0), 2, 0);
                await Task.Delay(200);
                if (IsOrange(color))
                {
                    Dispatcher dispatcher = Application.Current.Dispatcher;
                    dispatcher.Invoke(() =>
                    {
                        NotifierHelper.Show(NotifierType.Success, "成功观战");
                    });
                   
                    return 1;
                }
                else
                {
                    Dispatcher dispatcher = Application.Current.Dispatcher;
                    dispatcher.Invoke(() =>
                    {
                        NotifierHelper.Show(NotifierType.Error, $"玩家可能处于死亡状态，颜色 RGB: ({color.R}, {color.G}, {color.B})");
                    });
                    
                    { return 0; } //观战失败,玩家处于死亡状态或其他
                }
                

            }
            else if (player.TeamId == 0)
            {
                return 2;//玩家在观战
            }
            
            else
            { return 0; } //观战失败,玩家处于死亡状态或其他
        }
        else
        {
            return 2;//未找到玩家    
        }
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
                bool isPlayerInTeam3 = playerListbegin.Any(player => player.PersonaId == Globals.PersonaId && player.TeamId == 0);
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

                if (ExcludeAdminsCheckBox.IsChecked ?? false)//排除管理员
                {
                    List<long> adminIds = Globals.ServerAdmins_PID.ToList();//list中admin属性不可信
                    excludeplayer.AddRange(playerList.Where(player => PlayerUtil.IsAdminVIP(player.PersonaId, adminIds)).ToList());
                }
                if (ExcludeVIPsCheckBox.IsChecked ?? false)//排除vip
                {
                    List<long> VipIds = Globals.ServerVIPs_PID.ToList();//list中admin属性不可信
                    excludeplayer.AddRange(playerList.Where(player => PlayerUtil.IsAdminVIP(player.PersonaId, VipIds)).ToList());
                }
                if (ExcludeWhitelistCheckBox.IsChecked ?? false)//排除白名单
                {
                  excludeplayer.AddRange(playerList.Where(player => PlayerUtil.IsWhite(player.Name, Globals.CustomWhites_Name)).ToList());
                }
                excludeplayer.AddRange(playerList.Where(player => excludeList.Select(p => p.PersonaId).Contains(player.PersonaId)).ToList());//移除已经换过边的玩家
                                                                                                                                             // 使用 Dispatcher.Invoke 在 UI 线程上访问 slider
                Application.Current.Dispatcher.Invoke(() =>
                {
                    playerDataQueue.Clear();

                    // 遍历 excludeList 并添加每个元素到 playerDataQueue
                    foreach (var player in excludeplayer)
                    {
                        playerDataQueue.Add(player);
                    }
                  
                });                                                                                                                           // 清空现有数据
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
                        if (isPlayerInTeam3)
                        { await Task.Delay(15500); }
                        else
                        {
                            await Task.Delay(1000);
                        }
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
                        var playerToMove = FindBestPlayerToMoveForSkill(team1Players, team2Players, avgSkillTeam1, avgSkillTeam2, skillflag, excludeplayer);

                        // 根据playerToMove所在队伍决定移动方向
                        int OriginTeam = playerToMove.TeamId == 1 ? 1 : 2;

                        // 执行移动
                        var result = await BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, playerToMove.PersonaId, OriginTeam);
                        // 重新获取玩家列表以验证换边是否成功
                        if (isPlayerInTeam3) 
                        { await Task.Delay(15500); }
                        else
                        {
                            await Task.Delay(1000);
                        }
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







                            if (isPlayerInTeam3)
                            { await Task.Delay(15500); }
                            else
                            {
                                await Task.Delay(1000);
                            }
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







                            if (isPlayerInTeam3)
                            { await Task.Delay(15500); }
                            else
                            {
                                await Task.Delay(1000);
                            }
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
    static void SimulateClickAtPosition(double relativeX, double relativeY)
    {
        // 获取屏幕分辨率
        int screenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
        int screenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

        // 计算绝对坐标
        int absoluteX = (int)(relativeX * screenWidth);
        int absoluteY = (int)(relativeY * screenHeight);

        // 设置鼠标位置
        System.Windows.Forms.Cursor.Position = new System.Drawing.Point(absoluteX, absoluteY);
        Task.Delay(500);
        // 模拟鼠标按下和释放
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        Task.Delay(100);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
    }
    static System.Drawing.Color GetColorAt(double relX, double relY)
    {
        // 获取屏幕分辨率
        int screenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
        int screenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

        // 计算绝对坐标
        int x = (int)(relX * screenWidth);
        int y = (int)(relY * screenHeight);

        using (Bitmap bmp = new Bitmap(1, 1))
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                // 捕获屏幕上的一个像素
                g.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(1, 1));
            }
            // 返回该像素的颜色
            return bmp.GetPixel(0, 0);
        }
    }
    private AviWriter aviWriter;
    private IAviVideoStream videoStream;
    private int screenWidth;
    private int screenHeight;
    private Task recordingTask;
    private bool isRecording = false;
    private Process _ffmpegProcess;
    public void StartRecording(string fileName)
    {
        try
        {
            fileName = fileName + ".mp4";
            // 获取程序的当前工作目录
            string outputPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), fileName);

            // 构建 FFmpeg 命令行
            string arguments = $"-f gdigrab -framerate 30 -i desktop -c:v libx264 -preset ultrafast -pix_fmt yuv420p \"{outputPath}\"";

            // 创建并配置 ProcessStartInfo
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = false
            };

            // 创建并启动进程
            _ffmpegProcess = new Process();
            _ffmpegProcess.StartInfo = processStartInfo;
            _ffmpegProcess.Start();
            _ffmpegProcess.BeginErrorReadLine();
        }  // 开始异步读取标准错误输出
        catch (Exception ex)
        {
            // 使用 MessageBox 显示异常信息
            MessageBox.Show("Failed to start recording: ");
        }
    }

    public void StopRecording()
    {
        if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
        {
            // 发送 'q' 字符来结束 FFmpeg 录制
            _ffmpegProcess.StandardInput.WriteLine("q");
            _ffmpegProcess.WaitForExit();
            _ffmpegProcess.Close();
        }
    }

    private void CaptureScreen(int width, int height)
    {
        var buffer = new byte[width * height * 4];

        while (isRecording)
        {
            using (var bmp = new Bitmap(width, height))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, new System.Drawing.Size(width, height));
                }
                var bits = bmp.LockBits(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                System.Runtime.InteropServices.Marshal.Copy(bits.Scan0, buffer, 0, buffer.Length);
                bmp.UnlockBits(bits);
            }

           
                videoStream.WriteFrame(true, buffer, 0, buffer.Length);
            
            System.Threading.Thread.Sleep(16); // 根据希望的帧率调整
        }
    }
}
