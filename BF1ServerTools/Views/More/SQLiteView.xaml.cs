using BF1ServerTools.Data;
using BF1ServerTools.Helper;
using BF1ServerTools.Utils;
using System.Reflection.PortableExecutable;
using System;

namespace BF1ServerTools.Views.More;

/// <summary>
/// SQLiteView.xaml 的交互逻辑
/// </summary>
public partial class SQLiteView : UserControl
{
    /// <summary>
    /// 绑定UI动态数据集合
    /// </summary>
    public ObservableCollection<SQLiteLogInfo> DataGrid_SQLiteLogInfos { get; set; } = new();

    public SQLiteView()
    {
        InitializeComponent();
        this.DataContext = this;
        MainWindow.WindowClosingEvent += MainWindow_WindowClosingEvent;
    }

    private void MainWindow_WindowClosingEvent()
    {

    }

    /////////////////////////////////////////////////////

    /// <summary>
    /// 数据库日志查询
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItem_QueryLog_Table_Click(object sender, RoutedEventArgs e)
    {
        var name = (e.OriginalSource as MenuItem).Header.ToString();

        string table_name;
        switch (name)
        {
            case "表1 手动操作":
                table_name = "score_kick";
                break;
            case "表2 踢出成功":
                table_name = "kick_ok";
                break;
            case "表3 踢出失败":
                table_name = "kick_no";
                break;
            case "表4 换边记录":
                table_name = "change_team";
                break;
            default:
                return;
        }

        lock (this)
        {
            DataGrid_SQLiteLogInfos.Clear();

            Task.Run(() =>
            {
                int index = 1;

                foreach (var item in SQLiteHelper.QueryLog(table_name))
                {
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                    {
                        DataGrid_SQLiteLogInfos.Add(new()
                        {
                            Index = index++,
                            Rank = item.Rank,
                            Name = item.Name,
                            PersonaId = item.PersonaId,
                            Type = item.Type,
                            Message1 = item.Message1,
                            Message2 = item.Message2,
                            Message3 = item.Message3,
                            Date = item.Date
                        });
                    });
                }
            });
        }
    }
}
