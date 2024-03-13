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

namespace BF1ServerTools.Views;

/// <summary>
/// RobotView.xaml 的交互逻辑
/// </summary>
public partial class RobotView : UserControl
{
    /// <summary>
    /// Robot配置文件集，以json格式保存到本地
    /// </summary>
    private RobotConfig RobotConfig { get; set; } = new();

    /// <summary>
    /// Robot配置文件路径
    /// </summary>
    private readonly string F_Robot_Path = FileUtil.D_Config_Path + @"\RobotConfig.json";

    ////////////////////////////////////////////////////////

    private readonly Uri url = new("ws://127.0.0.1:65502");

    private static WebsocketClient websocketClient = null;

    /// <summary>
    /// 获取到的QQ群列表
    /// </summary>
    private List<long> QQGroupList = new();

    /// <summary>
    /// 换边通知转发到QQ群委托
    /// </summary>
    public static Action<ChangeTeamInfo> ActionSendChangeTeamLogToQQ;
    /// <summary>
    /// 局内聊天转发到QQ群委托
    /// </summary>
    public static Action<string, string> ActionSendGameChatsMsgToQQ;

    public RobotView()
    {
        InitializeComponent();
        this.DataContext = this;
        MainWindow.WindowClosingEvent += MainWindow_WindowClosingEvent;

        //////////////////////////////////////////////////////////////////

        // 如果配置文件不存在就创建
        if (!File.Exists(F_Robot_Path))
        {
            RobotConfig.QQGroupID = default;
            RobotConfig.QQGroupMemberID = new List<long>();
            RobotConfig.IsSendChangeTeamToQQ = false;
            // 保存配置文件
            SaveConfig();
        }

        // 如果配置文件存在就读取
        if (File.Exists(F_Robot_Path))
        {
            using var streamReader = new StreamReader(F_Robot_Path);
            RobotConfig = JsonHelper.JsonDese<RobotConfig>(streamReader.ReadToEnd());

            TextBox_QQGroupID.Text = RobotConfig.QQGroupID.ToString();

            CheckBox_IsSendChangeTeam.IsChecked = RobotConfig.IsSendChangeTeamToQQ;
            CheckBox_IsSendGameChat.IsChecked = RobotConfig.IsSendGameChatToQQ;

            foreach (var item in RobotConfig.QQGroupMemberID)
            {
                ListBox_QQGroupMemberIDs.Items.Add(item);
            }
        }

        //////////////////////////////////////////////////////////////////

        ActionSendChangeTeamLogToQQ = SendChangeTeamLogToQQ;
        ActionSendGameChatsMsgToQQ = SendGameChatsMsgToQQ;
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
        RobotConfig.QQGroupMemberID.Clear();
        if (ListBox_QQGroupMemberIDs.Items.Count != 0)
        {
            foreach (var item in ListBox_QQGroupMemberIDs.Items)
            {
                RobotConfig.QQGroupMemberID.Add(long.Parse(item.ToString()));
            }
        }

        var qqGroup = TextBox_QQGroupID.Text.Trim();
        RobotConfig.QQGroupID = string.IsNullOrEmpty(qqGroup) ? 0 : long.Parse(qqGroup);

        RobotConfig.IsSendChangeTeamToQQ = CheckBox_IsSendChangeTeam.IsChecked == true;
        RobotConfig.IsSendGameChatToQQ = CheckBox_IsSendGameChat.IsChecked == true;

        File.WriteAllText(F_Robot_Path, JsonHelper.JsonSeri(RobotConfig));
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

    /// <summary>
    /// 启动QQ机器人服务
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_RunGoCqHttpServer_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("program is active");
        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromMinutes(5); // 设置为5分钟
        timer.Tick += Timer_Tick;
        timer.Start();

        // 可以选择在这里立即调用RunPeriodicTasks来立刻执行任务
        RunPeriodicTasks();


    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        // 定时器触发时执行的任务
        RunPeriodicTasks();
    }
    private async void RunPeriodicTasks()
    {
        bool balanceAchieved = false;
        for (int i = 0; i < 99 && !balanceAchieved; i++)
        {
            List<PlayerData> playerList = Player.GetPlayerList(); // 假设这个方法可以获取当前所有玩家的列表

            // 更新玩家的生涯KD和KPM
            foreach (var item in playerList)
            {
                item.LifeKd = PlayerUtil.GetLifeKD(item.PersonaId);
                item.LifeKpm = PlayerUtil.GetLifeKPM(item.PersonaId);
            }

            var team1Players = playerList.Where(p => p.TeamId == 1).ToList();
            var team2Players = playerList.Where(p => p.TeamId == 2).ToList();

            var avgLifeKdTeam1 = team1Players.Average(p => p.LifeKd);
            var avgLifeKpmTeam1 = team1Players.Average(p => p.LifeKpm);

            var avgLifeKdTeam2 = team2Players.Average(p => p.LifeKd);
            var avgLifeKpmTeam2 = team2Players.Average(p => p.LifeKpm);

            // 判断是否需要调整玩家队伍
            if (avgLifeKdTeam1 < avgLifeKdTeam2 + 0.1 && avgLifeKpmTeam1 < avgLifeKpmTeam2 + 0.1)
            {
                // 示例：移动生涯KD最高的玩家从队伍1到队伍2
                var playerToMove = team1Players.OrderByDescending(p => p.LifeKd).FirstOrDefault();
                if (playerToMove != null)
                {
                    var result = await BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, playerToMove.PersonaId, 2);
                    if (result.IsSuccess)
                    {
                        NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍1成功");
                    }
                    else
                    {
                        NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍1失败\n{result.Content}");
                    }
                }
            }
            else if (avgLifeKdTeam1 > avgLifeKdTeam2 + 0.2 || avgLifeKpmTeam1 > avgLifeKpmTeam2 + 0.2)
            {
                // 示例：移动生涯KPM最低的玩家从队伍1到队伍2
                var playerToMove = team1Players.OrderBy(p => p.LifeKpm).FirstOrDefault();
                if (playerToMove != null)
                {
                    var result = await BF1API.RSPMovePlayer(Globals.SessionId, Globals.GameId, playerToMove.PersonaId, 2);
                    if (result.IsSuccess)
                    {
                        NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍1成功");
                    }
                    else
                    {
                        NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒] 更换玩家 {playerToMove.Name} 到队伍1失败\n{result.Content}");
                    }
                }
            }
            else
            {
                balanceAchieved = true;
                NotifierHelper.Show(NotifierType.Information, "队伍已平衡，无需进一步操作。");
            }

            if (!balanceAchieved)
            {
                await Task.Delay(6000);
            }
        }
    }
    

private DispatcherTimer timer;
    /// <summary>
    /// 启动Websocket服务
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_RunWebsocketServer_Click(object sender, RoutedEventArgs e)
    {
        SaveConfig();

        if (!ProcessUtil.IsAppRun("go-cqhttp"))
        {
            AppendLogger("请先启动 go-cqhttp 程序");
            NotifierHelper.Show(NotifierType.Warning, "请先启动 go-cqhttp 程序");
            return;
        }

        if (websocketClient != null)
        {
            AppendLogger("请不要重复打开，Websocket 程序已经在运行了");
            NotifierHelper.Show(NotifierType.Warning, "请不要重复打开，Websocket 程序已经在运行了");
            return;
        }
        else
        {
            // 初始化客户端
            websocketClient = new(url)
            {
                ReconnectTimeout = TimeSpan.FromMinutes(5)
            };

            // 重连事件
            websocketClient.ReconnectionHappened.Subscribe(async info =>
            {
                AppendLogger($"客户端重新连接, 类型: {info.Type}");

                // 获取QQ群列表消息
                QQGroupList.Clear();
                var result = await QQAPI.GetGroupList();
                if (result.IsSuccess)
                {
                    var getGroupList = JsonHelper.JsonDese<GetGroupList>(result.Message);
                    AppendLogger($"检测到QQ群数量：{getGroupList.data.Count}");
                    for (int i = 0; i < getGroupList.data.Count; i++)
                    {
                        var item = getGroupList.data[i];

                        QQGroupList.Add(item.group_id);
                        AppendLogger($"QQ群 {i + 1}：{item.group_id} {item.group_name}");
                    }
                }
            });

            // 连接断开事件
            websocketClient.DisconnectionHappened.Subscribe(info =>
            {
                AppendLogger($"客户端连接断开, 类型: {info.Type}");
            });

            // 消息接收事件
            websocketClient.MessageReceived.Where(msg => msg.Text != null).Subscribe(MessageHandling);
            // 开始客户端
            websocketClient.Start();
        }
    }

    /// <summary>
    /// 关闭Websocket服务
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_StopWebsocketServer_Click(object sender, RoutedEventArgs e)
    {
        if (websocketClient != null)
        {
            websocketClient.Dispose();
            websocketClient = null;
            AppendLogger("客户端 WebsocketServer 连接关闭");
            NotifierHelper.Show(NotifierType.Notification, "客户端 WebsocketServer 连接关闭");
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "未发现客户端 WebsocketServer 连接");
        }
    }

    /// <summary>
    /// 接收的消息处理
    /// </summary>
    /// <param name="msg"></param>
    private void MessageHandling(ResponseMessage msg)
    {
        var jNode = JsonNode.Parse(msg.Text);

        // 过滤心跳消息
        if (jNode["post_type"].GetValue<string>() == "meta_event")
            return;

        if (jNode["post_type"].GetValue<string>() == "message")
        {
            if (jNode["message_type"].GetValue<string>() == "group")
            {
                var user_id = jNode["user_id"].GetValue<long>();
                var group_id = jNode["group_id"].GetValue<long>();

                // 判断当前QQ群是否和授权QQ群一致
                if (group_id != RobotConfig.QQGroupID)
                    return;

                // 判断当前QQ群成员权限
                if (!RobotConfig.QQGroupMemberID.Contains(user_id))
                    return;

                // 原始聊天消息
                var raw_message = jNode["raw_message"].GetValue<string>().Trim();

                if (raw_message.StartsWith("#中文聊天"))
                {
                    PrintAdminLog(user_id, raw_message);

                    raw_message = raw_message.Replace("#中文聊天", "").Trim();
                    if (!string.IsNullOrEmpty(raw_message))
                    {
                        SendChatChsRetrunImg(group_id, $"[来自QQ群] {raw_message}");
                    }

                    return;
                }

                if (raw_message.Equals("#屏幕截图"))
                {
                    PrintAdminLog(user_id, raw_message);

                    GetPrintScreen(group_id);

                    return;
                }

                if (raw_message.Equals("#得分板截图"))
                {
                    PrintAdminLog(user_id, raw_message);

                    GetScorePrintScreen(group_id);

                    return;
                }
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////

    private void PrintAdminLog(long user_id, string raw_message)
    {
        AppendLogger($"收到群指令: {user_id} {raw_message}");
    }

    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 发送换边通知到QQ群
    /// </summary>
    /// <param name="info"></param>
    private void SendChangeTeamLogToQQ(ChangeTeamInfo info)
    {
        if (!RobotConfig.IsSendChangeTeamToQQ)
            return;

        if (RobotConfig.QQGroupID == 0)
            return;

        if (!QQGroupList.Contains(RobotConfig.QQGroupID))
            return;

        var sb = new StringBuilder();
        sb.AppendLine("=== 换边通知 ===");
        sb.AppendLine($"操作时间: {DateTime.Now}");
        sb.AppendLine($"等级: {info.Rank}");
        sb.AppendLine($"玩家ID: {info.Name}");
        sb.AppendLine($"玩家数字ID: {info.PersonaId}");
        sb.AppendLine($"服务器地图: {info.GameMode} - {info.MapName}");
        sb.Append($"状态: {info.State}");

        _ = QQAPI.SendGroupMsg(RobotConfig.QQGroupID, sb.ToString());
    }

    /// <summary>
    /// 发送局内聊天到QQ群
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="content"></param>
    private void SendGameChatsMsgToQQ(string sender, string content)
    {
        if (!RobotConfig.IsSendGameChatToQQ)
            return;

        if (RobotConfig.QQGroupID == 0)
            return;

        if (!QQGroupList.Contains(RobotConfig.QQGroupID))
            return;

        sender = sender.Replace(":", "");
        var localData = Player.GetLocalPlayer();

        if (!string.IsNullOrEmpty(sender) && sender != localData.FullName)
            _ = QQAPI.SendGroupMsg(RobotConfig.QQGroupID, $"收到游戏内聊天\n{sender} 说: {content}");
    }

    /// <summary>
    /// 发送战地1中文聊天并返回聊天截图
    /// </summary>
    /// <param name="group_id"></param>
    /// <param name="message"></param>
    private void SendChatChsRetrunImg(long group_id, string message)
    {
        Task.Run(async () =>
        {
            _ = QQAPI.SendGroupMsg(group_id, "正在执行中...");

            ChatView.ActionSendTextToBf1Game(message);
            await Task.Delay(1000);

            if (Memory.GetBF1WindowData(out WindowData windowData))
            {
                windowData.Width /= 3;

                var bitmap = new Bitmap(windowData.Width, windowData.Height);
                var graphics = Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(new Point(windowData.Left, windowData.Top), new Point(0, 0), new Size(windowData.Width, windowData.Height));

                var file = $"BF1#{DateTime.Now:yyyyMMdd_HH-mm-ss-ffff}.png";
                var path = $"{FileUtil.D_Robot_Path}\\data\\images\\{file}";
                bitmap.Save(path, ImageFormat.Png);
                graphics.Dispose();

                _ = QQAPI.SendGroupMsg(group_id, $"[CQ:image,file={file}]");
            }
            else
            {
                _ = QQAPI.SendGroupMsg(group_id, "战地1窗口最小化时无法截图");
            }
        });
    }

    /// <summary>
    /// 获取战地1屏幕截图
    /// </summary>
    private void GetPrintScreen(long group_id)
    {
        Task.Run(async () =>
        {
            _ = QQAPI.SendGroupMsg(group_id, "正在执行中...");

            Memory.SetBF1WindowForeground();
            await Task.Delay(100);

            if (Memory.GetBF1WindowData(out WindowData windowData))
            {
                var bitmap = new Bitmap(windowData.Width, windowData.Height);
                var graphics = Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(new Point(windowData.Left, windowData.Top), new Point(0, 0), new Size(windowData.Width, windowData.Height));

                var file = $"BF1#{DateTime.Now:yyyyMMdd_HH-mm-ss-ffff}.png";
                var path = $"{FileUtil.D_Robot_Path}\\data\\images\\{file}";
                bitmap.Save(path, ImageFormat.Png);
                graphics.Dispose();

                _ = QQAPI.SendGroupMsg(group_id, $"[CQ:image,file={file}]");
            }
            else
            {
                _ = QQAPI.SendGroupMsg(group_id, "战地1窗口最小化时无法截图");
            }
        });
    }

    /// <summary>
    /// 获取战地1得分板屏幕截图
    /// </summary>
    private void GetScorePrintScreen(long group_id)
    {
        Task.Run(async () =>
        {
            _ = QQAPI.SendGroupMsg(group_id, "正在执行中...");

            Memory.SetBF1WindowForeground();
            await Task.Delay(100);

            ChsUtil.SetInputLanguageENUS();
            await Task.Delay(100);

            if (Memory.GetBF1WindowData(out WindowData windowData))
            {
                var bitmap = new Bitmap(windowData.Width, windowData.Height);
                var graphics = Graphics.FromImage(bitmap);

                Win32.Keybd_Event(WinVK.TAB, Win32.MapVirtualKey(WinVK.TAB, 0), 0, 0);
                await Task.Delay(2000);
                graphics.CopyFromScreen(new Point(windowData.Left, windowData.Top), new Point(0, 0), new Size(windowData.Width, windowData.Height));
                await Task.Delay(100);
                Win32.Keybd_Event(WinVK.TAB, Win32.MapVirtualKey(WinVK.TAB, 0), 2, 0);

                var file = $"BF1#{DateTime.Now:yyyyMMdd_HH-mm-ss-ffff}.png";
                var path = $"{FileUtil.D_Robot_Path}\\data\\images\\{file}";
                bitmap.Save(path, ImageFormat.Png);
                graphics.Dispose();

                _ = QQAPI.SendGroupMsg(group_id, $"[CQ:image,file={file}]");
            }
            else
            {
                _ = QQAPI.SendGroupMsg(group_id, "战地1窗口最小化时无法截图");
            }
        });
    }

    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 增加QQ群授权成员QQ号
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_AddQQGMID_Click(object sender, RoutedEventArgs e)
    {
        var qq = TextBox_AddQQGroupMemberID.Text.Trim();
        if (MiscUtil.IsNumber(qq))
        {
            ListBox_QQGroupMemberIDs.Items.Add(qq);
            TextBox_AddQQGroupMemberID.Clear();

            NotifierHelper.Show(NotifierType.Success, $"增加QQ群授权成员QQ号 {qq} 成功");
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "请输入正确的QQ群授权成员QQ号");
        }
    }

    /// <summary>
    /// 删除选中QQ群授权成员
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_RemoveQQGMID_Click(object sender, RoutedEventArgs e)
    {
        var index = ListBox_QQGroupMemberIDs.SelectedIndex;
        if (index != -1)
        {
            ListBox_QQGroupMemberIDs.Items.RemoveAt(index);
            NotifierHelper.Show(NotifierType.Success, "删除选中QQ群授权成员成功");
        }
        else
        {
            NotifierHelper.Show(NotifierType.Warning, "请正确选中QQ群授权成员");
        }
    }

    /// <summary>
    /// 清空QQ群授权成员
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_ClearQQGMID_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("你确认要清空QQ群授权成员列表吗？该操作不可恢复", "清空QQ群授权成员", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            ListBox_QQGroupMemberIDs.Items.Clear();
            NotifierHelper.Show(NotifierType.Success, "清空QQ群授权成员成功");
        }
    }

    /// <summary>
    /// 手动保存配置文件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_SaveRobotConfig_Click(object sender, RoutedEventArgs e)
    {
        SaveConfig();
        NotifierHelper.Show(NotifierType.Success, "手动保存配置文件成功");
    }

    /// <summary>
    /// 更换QQ机器人账号
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_ChangeGoCqHttpQQID_Click(object sender, RoutedEventArgs e)
    {
        ProcessUtil.CloseProcess("go-cqhttp");

        if (websocketClient != null)
        {
            websocketClient.Dispose();
            websocketClient = null;
            AppendLogger("客户端 WebsocketServer 连接关闭");
        }

        FileUtil.DelectDir(FileUtil.D_Robot_Path);

        FileUtil.ExtractResFile(FileUtil.Resource_Path + "config.yml", FileUtil.D_Robot_Path + "\\config.yml");
        FileUtil.ExtractResFile(FileUtil.Resource_Path + "go-cqhttp.exe", FileUtil.D_Robot_Path + "\\go-cqhttp.exe");

        NotifierHelper.Show(NotifierType.Success, "操作成功，请重新启动QQ机器人服务");
    }

    /// <summary>
    /// 清理图片缓存
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_ClearImageCache_Click(object sender, RoutedEventArgs e)
    {
        var imageCache = FileUtil.D_Robot_Path + @"\data\images";
        if (Directory.Exists(imageCache))
        {
            FileUtil.DelectDir(imageCache);
            NotifierHelper.Show(NotifierType.Success, "清理图片缓存成功");
        }
        else
        {
            NotifierHelper.Show(NotifierType.Notification, "未发现需要清理的图片缓存");
        }
    }

    /// <summary>
    /// 打开配置文件夹
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_OpenGoCqHttpPath_Click(object sender, RoutedEventArgs e)
    {
        ProcessUtil.OpenPath(FileUtil.D_Robot_Path);
    }
}
