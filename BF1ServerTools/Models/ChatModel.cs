using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1ServerTools.Models;

public partial class ChatModel : ObservableObject
{
    /// <summary>
    /// 按键延迟
    /// </summary>
    [ObservableProperty]
    private int keyPressDelay;

    /// <summary>
    /// 自动发送文本定时器周期
    /// </summary>
    [ObservableProperty]
    private int autoSendMsgInterval;
}
