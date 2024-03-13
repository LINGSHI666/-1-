using BF1ServerTools.Models;
using BF1ServerTools.SDK;
using BF1ServerTools.SDK.Core;
using BF1ServerTools.Utils;

using CommunityToolkit.Mvvm.Input;

namespace BF1ServerTools.Windows;

/// <summary>
/// ChatInputWindow.xaml 的交互逻辑
/// </summary>
public partial class ChatInputWindow : Window
{
    [DllImport("user32.dll")]
    private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    private enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
        ACCENT_INVALID_STATE = 5,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    private enum WindowCompositionAttribute
    {
        // 省略其他未使用的字段
        WCA_ACCENT_POLICY = 19,
        // 省略其他未使用的字段
    }

    //////////////////////////////////////////////////////

    private bool IsAppRunning = true;

    private bool ToggleChsIME = true;

    private IntPtr ThisWindowHandle = IntPtr.Zero;

    //////////////////////////////////////////////////////

    public ChatInputWindow()
    {
        InitializeComponent();
    }

    private void Window_ChatInput_Loaded(object sender, RoutedEventArgs e)
    {
        this.DataContext = this;
        this.Hide();

        new Thread(UpdateInputStateThread)
        {
            Name = "UpdateInputStateThread",
            IsBackground = true
        }.Start();

        if (Memory.GetBF1WindowData(out WindowData windowData))
        {
            this.Top = windowData.Top + 10;
            this.Left = windowData.Left + 10;
            this.Width = windowData.Width - 20;
        }

        ThisWindowHandle = new WindowInteropHelper(this).Handle;

        //////////////////////////////////////////////////////

        var accent = new AccentPolicy
        {
            AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND
        };

        var accentStructSize = Marshal.SizeOf(accent);

        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
        Marshal.StructureToPtr(accent, accentPtr, false);

        // 设置模糊特效
        var data = new WindowCompositionAttributeData
        {
            Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
            SizeOfData = accentStructSize,
            Data = accentPtr,
        };
        SetWindowCompositionAttribute(ThisWindowHandle, ref data);
    }

    private void Window_ChatInput_Closing(object sender, CancelEventArgs e)
    {
        IsAppRunning = false;
    }

    private void UpdateInputStateThread()
    {
        bool isShow = false;

        while (IsAppRunning)
        {
            if (Chat.GetChatIsOpen())
            {
                if (!isShow)
                {
                    isShow = true;

                    this.Dispatcher.Invoke(() =>
                    {
                        var thisWindowThreadId = Win32.GetWindowThreadProcessId(ThisWindowHandle, IntPtr.Zero);
                        var currentForegroundWindow = Win32.GetForegroundWindow();
                        var currentForegroundWindowThreadId = Win32.GetWindowThreadProcessId(currentForegroundWindow, IntPtr.Zero);

                        Win32.AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, true);

                        this.Show();
                        this.Activate();
                        this.Focus();

                        Win32.AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, false);

                        this.Topmost = true;
                        this.Topmost = false;

                        Win32.SetForegroundWindow(ThisWindowHandle);
                        TextBox_InputMessage.Focus();

                        if (Memory.GetBF1WindowData(out WindowData windowData))
                        {
                            this.Top = windowData.Top + 10;
                            this.Left = windowData.Left + 10;
                            this.Width = windowData.Width - 20;
                        }

                        ChsUtil.SetInputLanguageZHCN();
                    });
                }
            }
            else
            {
                if (isShow)
                {
                    isShow = false;

                    this.Dispatcher.Invoke(HideWindow);
                }
            }

            Thread.Sleep(200);
        }
    }

    /// <summary>
    /// 输入法切换
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBlock_InputMethod_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ToggleChsIME = !ToggleChsIME;

        if (ToggleChsIME)
            ChsUtil.SetInputLanguageZHCN();
        else
            ChsUtil.SetInputLanguageENUS();
    }

    /// <summary>
    /// 发送中文消息命令
    /// </summary>
    [RelayCommand]
    private void SendChsMessage()
    {
        var message = TextBox_InputMessage.Text.Trim();

        TextBox_InputMessage.Clear();

        ChsUtil.SetInputLanguageENUS();
        Memory.SetBF1WindowForeground();

        //////////////////////////////////////////////////////

        Chat.SendChsToBF1Chat(ChsUtil.ToTraditional(message));
    }
    public void SendMessage(string message)
    {
        TextBox_InputMessage.Clear();

        ChsUtil.SetInputLanguageENUS();
        Memory.SetBF1WindowForeground();

        //////////////////////////////////////////////////////

        Chat.SendChsToBF1Chat(ChsUtil.ToTraditional(message));
    }
    public static void SendChsToBF1Chat(string message)
    {
       ChsUtil.SetInputLanguageENUS();
        Memory.SetBF1WindowForeground();

        //////////////////////////////////////////////////////
        Thread.Sleep(500);
        Memory.KeyPress(WinVK.J, 50);
        Thread.Sleep(500);

        Chat.SendChsToBF1Chat(ChsUtil.ToTraditional(message));
    }
    /// <summary>
    /// 取消发送消息命令
    /// </summary>
    [RelayCommand]
    private void CancelMessage()
    {
        HideWindow();
        Memory.KeyPress(WinVK.ESCAPE);
    }

    /// <summary>
    /// 隐藏输入窗口
    /// </summary>
    private void HideWindow()
    {
        TextBox_InputMessage.Clear();
        ChsUtil.SetInputLanguageENUS();
        this.Hide();
    }
}
