﻿using BF1ServerTools.API;
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
using System.Numerics;
using FFmpeg.AutoGen;
using System.Drawing.Imaging;
using System.Xml;
using Path = System.IO.Path;
using Size = System.Drawing.Size;
using Rectangle = System.Drawing.Rectangle;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using System.Collections.Concurrent;
using NLog.MessageTemplates;





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
        return null; 
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
            labelCurrentScore.Content = $"当前启动自动平衡所需分差为{newValue}（0代表忽略该条件）(一方分数大于900失效）"; // 更新标签内容
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
    private void Slider_ValueChangedscore(object sender, RoutedPropertyChangedEventArgs<double> e)
    {

        if (labelCurrentdscore != null) // 检查控件是否为null
        {
            double newValue = e.NewValue; // 获取滑块的当前值
            labelCurrentdscore.Text = $"平衡目标为移动优势前{newValue}名"; // 更新标签内容
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
    private bool stopbalanceflag = false;
    private async void Button_StopWebsocketServer_Click(object sender, RoutedEventArgs e)
    {
        stopbalanceflag = true;
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
            maxscore = 0;
        }
        await Task.Delay(3000);
        stopbalanceflag = false;
        //FFmpegBinariesHelper.RegisterFFmpegBinaries();
        // DynamicallyLoadedBindings.Initialize();

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
    int maxscore = 0;
    private void Button_RunGoCqHttpServer_Click(object sender, RoutedEventArgs e)
    {
         // 检查定时器是否已经在运行，如果是则返回，防止重复执行
        if (isTimerRunning)
        {
            MessageBox.Show("自动平衡已经运行了");
            return;
        }
        int score = (int)(sliderscore != null ? sliderscore.Value : 0);
        // 创建一个 CancellationTokenSource 对象来控制任务的取消
        CancellationTokenSource cts = new CancellationTokenSource();
        if (score != 0)
        {
            isTimerRunning = true;
            // 启动任务
            Task balanceTask = AutoScoreBalance(score, cts.Token);

            maxscore = 900;
        }
        // 创建定时器实例
        timer = new DispatcherTimer();

        // 获取滑块当前的值，如果slider为null，则使用默认的10分钟
        double minutes = slider != null ? slider.Value : 10;
        if (minutes == 0 && score != 0)
        {
            MessageBox.Show($"当前自动平衡仅检测分差");
            return;
        }
        if (minutes == 0 && score == 0)
        {
            MessageBox.Show($"参数错误，请检查设置");
            return;
        }

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
            string lastContent = Chat.GetLastChatContent(out long pContent);
            if (!string.IsNullOrEmpty(lastContent) && lastContent.StartsWith("report "))
            {
                string voteName = lastContent.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1);
                if (voteName != null)
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
                            BEGIN:
                                if (stopRequested)
                                {
                                    break;
                                }
                                List<PlayerData> playerList = Player.GetPlayerList(); // 获取当前所有玩家的列表
                                if (playerList == null)
                                { break; }
                                PlayerData player = playerList.FirstOrDefault(p => p.Name == voteName);
                                if (player == null)
                                { break; }
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
                            // StopRecording();
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
    { while (!cancellationToken.IsCancellationRequested)
        {
            if (Math.Abs(Server.GetTeam1Score() - Server.GetTeam2Score()) >= scoreflag && Server.GetTeam1Score() < 900 && Server.GetTeam2Score() < 900)
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
                    if (minutes > 0)
                    {
                        // 设置定时器的时间间隔为滑块的值
                        timer.Interval = TimeSpan.FromMinutes(minutes);
                        // 设置定时器触发的事件
                        timer.Tick += Timer_Tick;

                        // 启动定时器
                        timer.Start();
                    }

                }
                RunPeriodicTasks();
                int score = Server.GetTeam1FlagScore();
                while (Server.GetTeam1FlagScore() >= score || Server.GetTeam2FlagScore() >= score)
                {
                    try
                    {
                        await Task.Delay(5000, cancellationToken);

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
    public static async Task<int> Autowatch(string name)
    {
        List<PlayerData> playerList = Player.GetPlayerList(); // 获取当前所有玩家的列表
        if (playerList == null)
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
                vertical = vertical + 0.016 * (playerRank - 1);
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
            if (maxscore != 0)
            {
                if (Server.GetTeam1Score() > maxscore || Server.GetTeam2Score() > maxscore)
                {
                    return;
                }
            }
            for (int i = 0; i < 99 && !balanceAchieved && !stopbalanceflag ; i++)
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
                int score = (int)(sliderscorebalance != null ? sliderscorebalance.Value : 0);
                int flagscore = (int)(sliderscore != null ? sliderscore.Value : 0);
                double kdkpmflag = sliderkdkpm != null ? sliderkdkpm.Value : 0;
                double skillflag = sliderskill != null ? sliderskill.Value : 0;
                var team1Players = playerList.Where(p => p.TeamId == 1).ToList();
                var team2Players = playerList.Where(p => p.TeamId == 2).ToList();
                if (count < 15 || team1Players.Count == 0 || team2Players.Count == 0)
                {
                    NotifierHelper.Show(NotifierType.Error, "人数不足,或游戏刚开始");
                    await Task.Delay(1000); // 暂停一秒再继续，避免频繁操作
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
                    if (adminIds.Count == 0) {
                        NotifierHelper.Show(NotifierType.Error, "加载管理名单时出错");
                        break;
                    }
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
                    NotifierHelper.Show(NotifierType.Error, "移动玩家超过5人仍未平衡");
                    return;
                }
                if (scorebalance.IsChecked ?? false)
                {
                    if (Math.Abs(Server.GetTeam1Score() - Server.GetTeam2Score()) >= flagscore)
                    {
                        // 获取要排除的玩家 PersonaId 列表
                        var excludePlayerIds = excludeplayer.Select(p => p.PersonaId).ToHashSet();

                        // 从 playerList 中移除存在于 excludePlayerIds 中的玩家
                        // 现在 updatedPlayerList 是移除后的玩家列表
                        int teamid = 0;
                        var updatedPlayerList = playerList.Where(p => !excludePlayerIds.Contains(p.PersonaId)).ToList();
                        teamid = Server.GetTeam1Score() - Server.GetTeam2Score() >= flagscore ? 1 : 2;// 决定移动方向
                        var Players = updatedPlayerList
                                                .Where(p => p.TeamId == teamid) // 筛选指定 teamId 的玩家
                                                  .OrderByDescending(p => p.Score) // 按 Score 降序排列
                                                 .Take(score) // 取前 score 名玩家
                                                  .ToList();
                        // 移动玩家
                        var tasks = Players.Select(player => new
                        {
                            Task = BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, player.PersonaId, teamid),
                            Player = player
                        }).ToList();

                        var results = await Task.WhenAll(tasks.Select(x => x.Task));
                        teamid = teamid == 1 ? 2 : 1;
                        for (int o = 0; o < results.Length; o++)
                        {

                            var result = results[o];
                            var player = tasks[o].Player;
                            if (result.IsSuccess)
                            {
                                NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换玩家 {player.Name} 到队伍{teamid}成功");
                            }
                            else
                            {
                                NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换玩家 {player.Name} 到队伍{teamid}失败");
                            }
                        }
                        return;
                    }
                    else
                    {
                        NotifierHelper.Show(NotifierType.Notification, "分差过小，取消平衡");
                        return;
                    }







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
                            if (Autobalanceshow.IsChecked ?? false)
                            {
                                ChatInputWindow.SendChsToBF1Chat($"自动平衡:更换玩家 {playerToMove.Name} 到队伍{OriginTeam}成功");
                            }
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
                            if (Autobalanceshow.IsChecked ?? false)
                            {
                                ChatInputWindow.SendChsToBF1Chat($"自动平衡:更换玩家 {playerToMove.Name} 到队伍{OriginTeam}成功");
                            }
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
                                if (Autobalanceshow.IsChecked ?? false)
                                {
                                    ChatInputWindow.SendChsToBF1Chat($"自动平衡:更换玩家 {playerToMove.Name} 到队伍{OriginTeam}成功");
                                }
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
                                if (Autobalanceshow.IsChecked ?? false)
                                {
                                    ChatInputWindow.SendChsToBF1Chat($"自动平衡:更换玩家 {playerToMove.Name} 到队伍{OriginTeam}成功");
                                }
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
            {
                continue; }
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
            {
                continue;
            }
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
            {
                continue;
            }
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
            {
                continue;
            }
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
    private volatile bool isRecording;

    public sealed unsafe class H264VideoStreamEncoder : IDisposable
    {
        private readonly Size _frameSize;
        private readonly AVCodec* _pCodec;
        private readonly AVCodecContext* _pCodecContext;
        private readonly AVFormatContext* _pFormatContext;
        private readonly AVStream* _pStream;
        private readonly int _fps;
        private readonly Stream _stream;

        public H264VideoStreamEncoder(string outputFilePath, int fps, Size frameSize)
        {
            _fps = fps;
            _frameSize = frameSize;

            // 初始化编解码器
            _pCodec = ffmpeg.avcodec_find_encoder(AVCodecID.AV_CODEC_ID_H264);
            if (_pCodec == null)
                throw new InvalidOperationException("Codec not found.");

            _pCodecContext = ffmpeg.avcodec_alloc_context3(_pCodec);
            _pCodecContext->width = frameSize.Width;
            _pCodecContext->height = frameSize.Height;
            _pCodecContext->time_base = new AVRational { num = 1, den = fps };
            _pCodecContext->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
            _pCodecContext->bit_rate = 8000000;
            ffmpeg.av_opt_set(_pCodecContext->priv_data, "preset", "medium", 0);

            int ret = ffmpeg.avcodec_open2(_pCodecContext, _pCodec, null);
            if (ret < 0)
                throw new ApplicationException($"Could not open codec: {ret}");

            // 初始化格式上下文和流
            AVFormatContext* formatContext = null;
            ret = ffmpeg.avformat_alloc_output_context2(&formatContext, null, null, outputFilePath);
            if (ret < 0)
                throw new ApplicationException($"Could not allocate format context: {ret}");
            _pFormatContext = formatContext;

            _pStream = ffmpeg.avformat_new_stream(_pFormatContext, _pCodec);
            if (_pStream == null)
                throw new ApplicationException("Could not create stream.");

            ret = ffmpeg.avcodec_parameters_from_context(_pStream->codecpar, _pCodecContext);
            if (ret < 0)
                throw new ApplicationException($"Could not set codec parameters: {ret}");
            _pStream->time_base = new AVRational { num = 1, den = fps };

            ret = ffmpeg.avio_open(&_pFormatContext->pb, outputFilePath, ffmpeg.AVIO_FLAG_WRITE);
            if (ret < 0)
                throw new ApplicationException($"Could not open output file: {ret}");

            ret = ffmpeg.avformat_write_header(_pFormatContext, null);
            if (ret < 0)
                throw new ApplicationException($"Error writing header: {ret}");
        }

        public void Dispose()
        {
            ffmpeg.avcodec_close(_pCodecContext);
            ffmpeg.av_free(_pCodecContext);
            ffmpeg.av_write_trailer(_pFormatContext);
            ffmpeg.avio_close(_pFormatContext->pb);
            ffmpeg.avformat_free_context(_pFormatContext);
        }

        public void Encode(AVFrame* frame)
        {
            int ret = ffmpeg.avcodec_send_frame(_pCodecContext, frame);
            if (ret < 0)
            {
                Console.WriteLine($"Error sending frame to codec: {ret}");
                return;
            }

            var pPacket = ffmpeg.av_packet_alloc();
            try
            {
                while (true)
                {
                    ret = ffmpeg.avcodec_receive_packet(_pCodecContext, pPacket);
                    if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || ret == ffmpeg.AVERROR_EOF)
                        break;
                    else if (ret < 0)
                    {
                        Console.WriteLine($"Error receiving packet from codec: {ret}");
                        break;
                    }

                    pPacket->stream_index = _pStream->index;
                    ffmpeg.av_packet_rescale_ts(pPacket, _pCodecContext->time_base, _pStream->time_base);
                    ret = ffmpeg.av_interleaved_write_frame(_pFormatContext, pPacket);
                    if (ret < 0)
                    {
                        Console.WriteLine($"Error writing frame: {ret}");
                        break;
                    }

                    ffmpeg.av_packet_unref(pPacket);
                }
            }
            finally
            {
                ffmpeg.av_packet_free(&pPacket);
            }
        }

        public void Drain()
        {
            int ret = ffmpeg.avcodec_send_frame(_pCodecContext, null);
            if (ret < 0)
            {
                Console.WriteLine($"Error sending frame to codec: {ret}");
                return;
            }

            var pPacket = ffmpeg.av_packet_alloc();
            try
            {
                while (true)
                {
                    ret = ffmpeg.avcodec_receive_packet(_pCodecContext, pPacket);
                    if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || ret == ffmpeg.AVERROR_EOF)
                        break;
                    else if (ret < 0)
                    {
                        Console.WriteLine($"Error receiving packet from codec: {ret}");
                        break;
                    }

                    pPacket->stream_index = _pStream->index;
                    ffmpeg.av_packet_rescale_ts(pPacket, _pCodecContext->time_base, _pStream->time_base);
                    ret = ffmpeg.av_interleaved_write_frame(_pFormatContext, pPacket);
                    if (ret < 0)
                    {
                        Console.WriteLine($"Error writing frame: {ret}");
                        break;
                    }

                    ffmpeg.av_packet_unref(pPacket);
                }
            }
            finally
            {
                ffmpeg.av_packet_free(&pPacket);
            }
        }
    }






    private static ConcurrentQueue<Bitmap> bitQueue = new ConcurrentQueue<Bitmap>();
    private static DateTime lastCaptureTime = DateTime.MinValue;
    private H264VideoStreamEncoder _encoder;
        private Thread _recordingThread;
    private Thread _recordingThread2;
   private Thread _recordingThread3;
    private Thread _recordingThread4;
    private Thread _recordingThread5;
    private Thread _recordingThread6;
    private FileStream _outputStream;

    public void StartRecording(string fileName)
    {
        try
        {
            // 注册 FFmpeg 二进制文件
            FFmpegBinariesHelper.RegisterFFmpegBinaries();

            // 初始化动态加载绑定
            FFmpeg.AutoGen.DynamicallyLoadedBindings.Initialize();

           
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            var frameSize = new Size(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height); // 设置屏幕宽度和高度
            _encoder = new H264VideoStreamEncoder(outputPath, 30, frameSize);

            isRecording = true;
            Thread Writebmpthread = new Thread(() => Writebmp(frameSize));
            Writebmpthread.Start();
            Writebmpthread.IsBackground = true;
           _recordingThread = new Thread(() => RecordScreen(frameSize));
            _recordingThread.Start();
            Thread.Sleep(100);
           // _recordingThread2= new Thread(() => RecordScreen(outputPath, 5, frameSize));
            //_recordingThread2.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start recording: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void StopRecording()
    {
        isRecording = false;
        _frameReadyEvent.Set(); // Signal the recording thread to stop
        _recordingThread?.Join(); // Wait for the recording thread to finish
        _encoder?.Drain();
        _encoder?.Dispose();
    }
    private readonly AutoResetEvent _frameReadyEvent = new AutoResetEvent(false);

    private  void Writebmp(Size frameSize)
    {
        while (isRecording)
        {
            
                Thread thread = new Thread(() =>
                {
                    
                       

                       

                        var bmp = ScreenCapture.CaptureScreen(frameSize); 

                       
                        
                        bitQueue.Enqueue(bmp);
                        
                    
                });
                thread.Start();
                thread.IsBackground = true;
                Thread.Sleep(30); // 等待33毫秒后启动下一个线程
            

        }


    }
    private unsafe void RecordScreen(Size frameSize)
    {
        var frameNumber = 0;


        // 创建一个线程用于编码数据
        Thread encodeThread = new Thread(() =>
        {
            while (isRecording)
            {
                var stopwatch = Stopwatch.StartNew();
                _frameReadyEvent.WaitOne(); // 等待新帧准备好

                var startFrameTime = stopwatch.ElapsedMilliseconds;



                // 获取队列中的第一个捕获图像
                Bitmap bmp;
                if(!bitQueue.TryDequeue(out bmp))
                { 
                    // 队列中没有可用的图像，等待一段时间后继续
                    Thread.Sleep(10);
                    continue;
                };
               

                // 创建 AVFrame 并设置属性
                var frame = ffmpeg.av_frame_alloc();
                if (frame == null)
                {
                    throw new ApplicationException("Could not allocate AVFrame.");
                }

                frame->format = (int)AVPixelFormat.AV_PIX_FMT_YUV420P;
                frame->width = frameSize.Width;
                frame->height = frameSize.Height;
                frame->pts = frameNumber++;

                int ret = ffmpeg.av_frame_get_buffer(frame, 32);
                if (ret < 0)
                {
                    ffmpeg.av_frame_free(&frame);
                    throw new ApplicationException($"Could not allocate frame buffer: {ret}");
                }

                // 锁定位图数据并复制到 AVFrame 中
                var bits = bmp.LockBits(new Rectangle(0, 0, frameSize.Width, frameSize.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    var srcData = bits.Scan0;
                    var srcStride = bits.Stride;

                    byte_ptrArray4 dstData = new byte_ptrArray4();
                    int_array4 dstLinesize = new int_array4();
                    ffmpeg.av_image_fill_arrays(ref dstData, ref dstLinesize, (byte*)srcData, AVPixelFormat.AV_PIX_FMT_BGRA, frameSize.Width, frameSize.Height, 1);

                    // 进行颜色空间转换
                    var swsContext = ffmpeg.sws_getContext(
                        frameSize.Width, frameSize.Height, AVPixelFormat.AV_PIX_FMT_BGRA,
                        frameSize.Width, frameSize.Height, AVPixelFormat.AV_PIX_FMT_YUV420P,
                        ffmpeg.SWS_BILINEAR, null, null, null);

                    if (swsContext == null)
                    {
                        throw new ApplicationException("Could not initialize the conversion context.");
                    }

                    // 创建转换后的帧
                    var convertedFrame = ffmpeg.av_frame_alloc();
                    if (convertedFrame == null)
                    {
                        ffmpeg.av_frame_free(&frame);
                        throw new ApplicationException("Could not allocate converted frame.");
                    }

                    convertedFrame->format = (int)AVPixelFormat.AV_PIX_FMT_YUV420P;
                    convertedFrame->width = frameSize.Width;
                    convertedFrame->height = frameSize.Height;
                    convertedFrame->pts = frame->pts;

                    ret = ffmpeg.av_frame_get_buffer(convertedFrame, 32);
                    if (ret < 0)
                    {
                        ffmpeg.av_frame_free(&frame);
                        ffmpeg.av_frame_free(&convertedFrame);
                        throw new ApplicationException($"Could not allocate converted frame buffer: {ret}");
                    }

                    try
                    {
                        // 进行颜色空间转换
                        ffmpeg.sws_scale(swsContext, dstData, dstLinesize, 0, frameSize.Height, convertedFrame->data, convertedFrame->linesize);
                    }
                    finally
                    {
                        ffmpeg.sws_freeContext(swsContext);
                    }

                    // 编码并写入帧
                    _encoder.Encode(convertedFrame);

                    // 释放转换后的帧
                    ffmpeg.av_frame_free(&convertedFrame);
                }
                finally
                {
                    bmp.UnlockBits(bits);
                }

                // 释放 AVFrame
                ffmpeg.av_frame_free(&frame);

                // 计算帧处理时间
                var endFrameTime = stopwatch.ElapsedMilliseconds;
                //MessageBox.Show($"{endFrameTime}");
                var frameProcessingTime = endFrameTime - startFrameTime;

                //计算下一个帧应等待的时间，保证约 5 fps
                var targetFrameInterval = 1000 / 30; // 5 fps 
                var sleepTime = targetFrameInterval - frameProcessingTime;

                if (sleepTime > 0)
                    Thread.Sleep((int)sleepTime);
                else
                {
                    //MessageBox.Show("视频录制遇到性能问题");
                }

            }
        });

        encodeThread.Start();
        encodeThread.IsBackground = true; // 设置为后台线程
        while (isRecording)
        {
            _frameReadyEvent.Set(); // Signal the encoding thread to process a frame
        }

        encodeThread.Join(); // Wait for the encoding thread to finish
    }

    public class ScreenCapture
    {
        // 引入 User32.dll 中的函数
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        private const int SRCCOPY = 0x00CC0020;

        public static Bitmap CaptureScreen(Size frameSize)
        {
            Bitmap bmp = new Bitmap(frameSize.Width, frameSize.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                IntPtr hdcDest = g.GetHdc();
                IntPtr hdcSrc = GetDC(IntPtr.Zero); // 获取整个屏幕的设备上下文

                // 进行屏幕截图
                BitBlt(hdcDest, 0, 0, frameSize.Width, frameSize.Height, hdcSrc, 0, 0, SRCCOPY);

                // 释放设备上下文
                ReleaseDC(IntPtr.Zero, hdcSrc);
                g.ReleaseHdc(hdcDest);
            }

            return bmp;
        }
    }







}




public class FFmpegBinariesHelper
{
    public static void RegisterFFmpegBinaries()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var current = Environment.CurrentDirectory;
            var probe = Path.Combine("FFmpeg", "bin", Environment.Is64BitProcess ? "x64" : "x86");

            while (current != null)
            {
                var ffmpegBinaryPath = Path.Combine(current, probe);

                if (Directory.Exists(ffmpegBinaryPath))
                {
                    Console.WriteLine($"FFmpeg binaries found in: {ffmpegBinaryPath}");
                    FFmpeg.AutoGen.Bindings.DynamicallyLoaded.DynamicallyLoadedBindings.LibrariesPath = ffmpegBinaryPath;
                    ffmpeg.RootPath = ffmpegBinaryPath;
                    return;
                }

                current = Directory.GetParent(current)?.FullName;
            }
        }
        else
        {
            throw new NotSupportedException("This platform is not supported.");
        }

        throw new ApplicationException("FFmpeg binaries not found.");
    }

}