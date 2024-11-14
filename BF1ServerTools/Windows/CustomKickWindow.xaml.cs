using BF1ServerTools.API;
using BF1ServerTools.Data;
using BF1ServerTools.Utils;
using BF1ServerTools.Views;
using BF1ServerTools.Helper;

namespace BF1ServerTools.Windows;

/// <summary>
/// CustomKickWindow.xaml 的交互逻辑
/// </summary>
public partial class CustomKickWindow
{
    public int Rank { get; }
    public string PlayerName { get; }
    public long PersonaId { get; }

    public CustomKickWindow(int rank, string playerName, long personaId)
    {
        InitializeComponent();
        this.DataContext = this;

        Rank = rank;
        PlayerName = playerName;
        PersonaId = personaId;
    }

    private async void Button_KickPlayer_Click(object sender, RoutedEventArgs e)
    {
        this.Hide();

        string reason = string.Empty;
        if (RadioButton_Reson0.IsChecked == true)
        {
            reason = TextBox_CustomReason.Text.Trim();
            if (!string.IsNullOrEmpty(reason))
                reason = ChsUtil.ToTraditional(reason);
        }
        

        NotifierHelper.Show(NotifierType.Information, $"正在踢出玩家 {PlayerName} 中...");

        var result = await BF1API.RSPKickPlayer(Globals.SessionId, Globals.GameId, PersonaId, reason);
        if (result.IsSuccess)
        {
            var info = new AutoKickInfo()
            {
                Rank = Rank,
                Name = PlayerName,
                PersonaId = PersonaId,
                Reason = reason,
                State = "踢出成功"
            };
            LogView.ActionScoreKickLog(info);
            NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  踢出玩家 {PlayerName} 成功");
        }
        else
        {
            var info = new AutoKickInfo()
            {
                Rank = Rank,
                Name = PlayerName,
                PersonaId = PersonaId,
                Reason = reason,
                State = $"踢出失败  {result.Content}"
            };
            LogView.ActionScoreKickLog(info);
            NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  踢出玩家 {PlayerName} 失败\n{result.Content}");
        }

        this.Close();
    }
}
