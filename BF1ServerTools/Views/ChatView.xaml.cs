using BF1ServerTools.API;
using BF1ServerTools.SDK;
using BF1ServerTools.SDK.Core;
using BF1ServerTools.Data;
using BF1ServerTools.Utils;
using BF1ServerTools.Helper;
using BF1ServerTools.Models;
using BF1ServerTools.Configs;
using BF1ServerTools.Windows;

using CommunityToolkit.Mvvm.Input;

namespace BF1ServerTools.Views;

/// <summary>
/// ChatView.xaml 的交互逻辑
/// </summary>
public partial class ChatView : UserControl
{
    /// <summary>
    /// Chat配置文件集，以json格式保存到本地
    /// </summary>
    private ChatConfig ChatConfig { get; set; } = new();

    /// <summary>
    /// 数据模型绑定
    /// </summary>
    public ChatModel ChatModel { get; set; } = new();

    /////////////////////////////////////////////////////////

    /// <summary>
    /// 发送战地1中文聊天委托
    /// </summary>
    public static Action<string> ActionSendTextToBf1Game;

    /// <summary>
    /// 自动发送文本定时器
    /// </summary>
    private Timer TimerAutoSendMsg = null;
    /// <summary>
    /// 战地发送文本消息队列
    /// </summary>
    private List<string> QueueMessages = new();

    /// <summary>
    /// 挂机防踢定时器
    /// </summary>
    private Timer TimerNoAFK = null;

    //////////////////////////////////////////////////////

    /// <summary>
    /// 换边通知委托
    /// </summary>
    public static Action<ChangeTeamInfo> ActionChangeTeamNotice;
    /// <summary>
    /// 是否激活换边通知
    /// </summary>
    private bool IsActiveChangeTeamNotice = false;
    /// <summary>
    /// 换边通知最低等级
    /// </summary>
    private int ChangeTeamMinRank = 1;

    //////////////////////////////////////////////////////

    /// <summary>
    /// 配置文件路径
    /// </summary>
    private readonly string F_Chat_Path = FileUtil.D_Config_Path + @"\ChatConfig.json";

    /// <summary>
    /// 独立聊天窗口
    /// </summary>
    private ChatInputWindow ChatInputWindow = null;

    //////////////////////////////////////////////////////

    public ChatView()
    {
        InitializeComponent();
        this.DataContext = this;
        MainWindow.WindowClosingEvent += MainWindow_WindowClosingEvent;

        // 如果配置文件不存在就创建
        if (!File.Exists(F_Chat_Path))
        {
            ChatConfig.ChatContents = new();
            // 初始化10个配置文件槽
            for (int i = 0; i < 10; i++)
            {
                ChatConfig.ChatContents.Add(new()
                {
                    Name = $"自定义聊天 {i}",
                    Content = $"测试文本 {i} 战地1中文输入测试 {Guid.NewGuid()}"
                });
            }
            // 保存配置文件
            File.WriteAllText(F_Chat_Path, JsonHelper.JsonSeri(ChatConfig));
        }

        // 如果配置文件存在就读取
        if (File.Exists(F_Chat_Path))
        {
            using var streamReader = new StreamReader(F_Chat_Path);
            ChatConfig = JsonHelper.JsonDese<ChatConfig>(streamReader.ReadToEnd());

            ChatModel.KeyPressDelay = ChatConfig.KeyPressDelay;
            ChatModel.AutoSendMsgInterval = ChatConfig.AutoSendMsgInterval;

            RadioButton_MsgIndex0.IsChecked = true;
            TextBox_MessageContent.Text = ChatConfig.ChatContents[0].Content;
        }

        //////////////////////////////////////////////

        TimerAutoSendMsg = new()
        {
            AutoReset = true
        };
        TimerAutoSendMsg.Elapsed += TimerAutoSendMsg_Elapsed;

        TimerNoAFK = new()
        {
            AutoReset = true,
            Interval = TimeSpan.FromSeconds(30).TotalMilliseconds
        };
        TimerNoAFK.Elapsed += TimerNoAFK_Elapsed;

        new Thread(GetLastChatInfoThread)
        {
            Name = "GetLastChatInfoThread",
            IsBackground = true
        }.Start();

        //////////////////////////////////////////////

        ActionChangeTeamNotice = ChangeTeamNotice;
        ActionSendTextToBf1Game = SendTextToBf1Game;
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
        ChatConfig.KeyPressDelay = ChatModel.KeyPressDelay;
        ChatConfig.AutoSendMsgInterval = ChatModel.AutoSendMsgInterval;

        File.WriteAllText(F_Chat_Path, JsonHelper.JsonSeri(ChatConfig));
    }

    /// <summary>
    /// 消息框内容改变事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBox_MessageContent_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBlock_MessageLength.Text = $"当前文本长度 : {Chat.GetStrLength(TextBox_MessageContent.Text)} 字符";

        if (ChatConfig.ChatContents != null)
            ChatConfig.ChatContents[GetRadioButtonCheckedIndex()].Content = TextBox_MessageContent.Text;
    }

    /// <summary>
    /// 消息索引 0~9 点击事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RadioButton_MsgIndex09_Click(object sender, RoutedEventArgs e)
    {
        TextBox_MessageContent.Text = ChatConfig.ChatContents[GetRadioButtonCheckedIndex()].Content;
    }

    /// <summary>
    /// 获取被选中的消息索引
    /// </summary>
    /// <returns></returns>
    private int GetRadioButtonCheckedIndex()
    {
        if (RadioButton_MsgIndex0 != null && RadioButton_MsgIndex0.IsChecked == true)
            return 0;

        if (RadioButton_MsgIndex1 != null && RadioButton_MsgIndex1.IsChecked == true)
            return 1;

        if (RadioButton_MsgIndex2 != null && RadioButton_MsgIndex2.IsChecked == true)
            return 2;

        if (RadioButton_MsgIndex3 != null && RadioButton_MsgIndex3.IsChecked == true)
            return 3;

        if (RadioButton_MsgIndex4 != null && RadioButton_MsgIndex4.IsChecked == true)
            return 4;

        if (RadioButton_MsgIndex5 != null && RadioButton_MsgIndex5.IsChecked == true)
            return 5;

        if (RadioButton_MsgIndex6 != null && RadioButton_MsgIndex6.IsChecked == true)
            return 6;

        if (RadioButton_MsgIndex7 != null && RadioButton_MsgIndex7.IsChecked == true)
            return 7;

        if (RadioButton_MsgIndex8 != null && RadioButton_MsgIndex8.IsChecked == true)
            return 8;

        if (RadioButton_MsgIndex9 != null && RadioButton_MsgIndex9.IsChecked == true)
            return 9;

        return 0;
    }

    /// <summary>
    /// 发送文本到战地1聊天框
    /// </summary>
    [RelayCommand]
    private void SendChsMessage()
    {
        var message = TextBox_MessageContent.Text.Trim();
        if (string.IsNullOrEmpty(message))
        {
            NotifierHelper.Show(NotifierType.Warning, "聊天框内容为空，操作取消");
            return;
        }

        if (Globals.GameId == 0)
        {
            NotifierHelper.Show(NotifierType.Warning, "请进入服务器后再执行本操作");
            return;
        }

        SendTextToBf1Game(message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_SetInputLanguageENUS_Click(object sender, RoutedEventArgs e)
    {
        ChsUtil.SetInputLanguageENUS();
        NotifierHelper.Show(NotifierType.Notification, "切换输入法为英文状态成功");
    }

    /// <summary>
    /// 激活定时发送指定文本
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ToggleButton_ActiveAutoSendMsg_Click(object sender, RoutedEventArgs e)
    {
        if (ToggleButton_ActiveAutoSendMsg.IsChecked == true)
        {
            if (Globals.GameId == 0)
            {
                ToggleButton_ActiveAutoSendMsg.IsChecked = false;
                NotifierHelper.Show(NotifierType.Warning, "请进入服务器后再执行本操作");
                return;
            }

            QueueMessages.Clear();

            if (CheckBox_MsgIndex0 != null && CheckBox_MsgIndex0.IsChecked == true)
                QueueMessages.Add(ChatConfig.ChatContents[0].Content);

            if (CheckBox_MsgIndex1 != null && CheckBox_MsgIndex1.IsChecked == true)
                QueueMessages.Add(ChatConfig.ChatContents[1].Content);

            if (CheckBox_MsgIndex2 != null && CheckBox_MsgIndex2.IsChecked == true)
                QueueMessages.Add(ChatConfig.ChatContents[2].Content);

            if (CheckBox_MsgIndex3 != null && CheckBox_MsgIndex3.IsChecked == true)
                QueueMessages.Add(ChatConfig.ChatContents[3].Content);

            if (CheckBox_MsgIndex4 != null && CheckBox_MsgIndex4.IsChecked == true)
                QueueMessages.Add(ChatConfig.ChatContents[4].Content);

            if (CheckBox_MsgIndex5 != null && CheckBox_MsgIndex5.IsChecked == true)
                QueueMessages.Add(ChatConfig.ChatContents[5].Content);

            if (CheckBox_MsgIndex6 != null && CheckBox_MsgIndex6.IsChecked == true)
                QueueMessages.Add(ChatConfig.ChatContents[6].Content);

            if (CheckBox_MsgIndex7 != null && CheckBox_MsgIndex7.IsChecked == true)
                QueueMessages.Add(ChatConfig.ChatContents[7].Content);

            if (CheckBox_MsgIndex8 != null && CheckBox_MsgIndex8.IsChecked == true)
                QueueMessages.Add(ChatConfig.ChatContents[8].Content);

            if (CheckBox_MsgIndex9 != null && CheckBox_MsgIndex9.IsChecked == true)
                QueueMessages.Add(ChatConfig.ChatContents[9].Content);

            TimerAutoSendMsg.Interval = TimeSpan.FromMinutes(ChatModel.AutoSendMsgInterval).TotalMilliseconds;
            TimerAutoSendMsg.Start();
            NotifierHelper.Show(NotifierType.Notification, "已启用定时发送指定文本功能");
        }
        else
        {
            TimerAutoSendMsg.Stop();
            NotifierHelper.Show(NotifierType.Notification, "已关闭定时发送指定文本功能");
        }
    }

    /// <summary>
    /// 定时发送指定文本线程
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void TimerAutoSendMsg_Elapsed(object sender, ElapsedEventArgs e)
    {
        foreach (var item in QueueMessages)
        {
            SendTextToBf1Game(item);
            Thread.Sleep(1000);
        }
    }

    /// <summary>
    /// 激活游戏内挂机防踢
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ToggleButton_ActiveNoAFK_Click(object sender, RoutedEventArgs e)
    {
        if (ToggleButton_ActiveNoAFK.IsChecked == true)
        {
            if (Globals.GameId == 0)
            {
                ToggleButton_ActiveNoAFK.IsChecked = false;
                NotifierHelper.Show(NotifierType.Warning, "请进入服务器后再执行本操作");
                return;
            }

            TimerNoAFK.Start();
            NotifierHelper.Show(NotifierType.Notification, "已启用游戏内挂机防踢功能");
        }
        else
        {
            TimerNoAFK.Stop();
            NotifierHelper.Show(NotifierType.Notification, "已关闭游戏内挂机防踢功能");
        }
    }

    /// <summary>
    /// 游戏内挂机防踢线程
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void TimerNoAFK_Elapsed(object sender, ElapsedEventArgs e)
    {
        Memory.SetBF1WindowForeground();
        Thread.Sleep(ChatModel.KeyPressDelay);

        Win32.Keybd_Event(WinVK.TAB, Win32.MapVirtualKey(WinVK.TAB, 0), 0, 0);
        Thread.Sleep(3000);
        Win32.Keybd_Event(WinVK.TAB, Win32.MapVirtualKey(WinVK.TAB, 0), 2, 0);
        Thread.Sleep(ChatModel.KeyPressDelay);
    }

    /// <summary>
    /// 发送中文到战地1聊天框
    /// </summary>
    /// <param name="message"></param>
    private void SendTextToBf1Game(string message)
    {
        // 如果内容为空，则跳过
        if (string.IsNullOrEmpty(message))
            return;

        // 如果不在服务器，则跳过
        if (Globals.GameId == 0)
            return;

        // 切换输入法到英文状态
        ChsUtil.SetInputLanguageENUS();
        Thread.Sleep(ChatModel.KeyPressDelay);

        // 将战地1窗口置于前面
        Memory.SetBF1WindowForeground();
        Thread.Sleep(ChatModel.KeyPressDelay);

        // 模拟按键，开启聊天框
        Memory.KeyPress(WinVK.J, ChatModel.KeyPressDelay);
        Thread.Sleep(ChatModel.KeyPressDelay);

        Chat.SendChsToBF1Chat(ChsUtil.ToTraditional(message));
    }

    /// <summary>
    /// 激活换边通知
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ToggleButton_ActiveChangeTeamNotice_Click(object sender, RoutedEventArgs e)
    {
        if (ToggleButton_ActiveChangeTeamNotice.IsChecked == true)
        {
            if (Globals.GameId == 0)
            {
                ToggleButton_ActiveChangeTeamNotice.IsChecked = false;
                NotifierHelper.Show(NotifierType.Warning, "请进入服务器后再执行本操作");
                return;
            }

            IsActiveChangeTeamNotice = true;
            ChangeTeamMinRank = (int)Slider_ChangeTeamMinRank.Value;
            NotifierHelper.Show(NotifierType.Notification, "已启用游戏换边通知功能");
        }
        else
        {
            IsActiveChangeTeamNotice = false;
            NotifierHelper.Show(NotifierType.Notification, "已关闭游戏换边通知功能");
        }
    }

    /// <summary>
    /// 换边通知
    /// </summary>
    /// <param name="info"></param>
    private void ChangeTeamNotice(ChangeTeamInfo info)
    {
        lock (this)
        {
            if (IsActiveChangeTeamNotice)
            {
                if (info.Rank >= ChangeTeamMinRank)
                {
                    SendTextToBf1Game($"玩家 {info.Name} 等级 {info.Rank} 更换队伍 {info.State}");
                    Thread.Sleep(ChatModel.KeyPressDelay);
                }
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 启用游戏内覆盖
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ToggleButton_EnabledChatInputWindow_Click(object sender, RoutedEventArgs e)
    {
        if (ToggleButton_EnabledChatInputWindow.IsChecked == true)
        {
            ChatInputWindow ??= new ChatInputWindow();
            ChatInputWindow.Show();
        }
        else
        {
            ChatInputWindow?.Close();
            ChatInputWindow = null;
        }
    }

    /// <summary>
    /// 获取战地1最后聊天信息线程
    /// </summary>
    private void GetLastChatInfoThread()
    {
        bool flag = false;
        long old_pSender = 0, old_pContent = 0;

        while (MainWindow.IsAppRunning)
        {
            if (Globals.GameId == 0)
            {
                if (flag)
                {
                    flag = false;

                    this.Dispatcher.Invoke(() =>
                    {
                        TextBox_GameChats.Clear();
                    });
                }
            }
            else
            {
                flag = true;

                var sender = Chat.GetLastChatSender(out long pSender);
                var content = Chat.GetLastChatContent(out long pContent);

                if (pSender != 0 && pContent != 0)
                {
                    if (pSender != old_pSender && pContent != old_pContent)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            TextBox_GameChats.AppendText($"{sender} {content}\n");
                        });
                        RobotView.ActionSendGameChatsMsgToQQ(sender, content);

                        old_pSender = pSender;
                        old_pContent = pContent;
                    }
                }
            }

            Thread.Sleep(200);
        }
    }
}
