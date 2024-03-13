using BF1ServerTools.Data;
using BF1ServerTools.Utils;

namespace BF1ServerTools.Views.More;

/// <summary>
/// CacheView.xaml 的交互逻辑
/// </summary>
public partial class CacheView : UserControl
{
    /// <summary>
    /// 绑定UI动态数据集合
    /// </summary>
    public ObservableCollection<LifeDataInfo> DataGrid_LifeDataInfos { get; set; } = new();

    public CacheView()
    {
        InitializeComponent();
        this.DataContext = this;
        MainWindow.WindowClosingEvent += MainWindow_WindowClosingEvent;
    }

    private void MainWindow_WindowClosingEvent()
    {

    }

    ///////////////////////////////////////////////////////

    /// <summary>
    /// 刷新已缓存玩家生涯数据
    /// </summary>
    private void RefreshCacheLifeData()
    {
        lock (this)
        {
            DataGrid_LifeDataInfos.Clear();

            Task.Run(() =>
            {
                for (int i = 0; i < Globals.LifePlayerCacheDatas.Count; i++)
                {
                    var item = Globals.LifePlayerCacheDatas[i];

                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
                    {
                        DataGrid_LifeDataInfos.Add(new()
                        {
                            Index = DataGrid_LifeDataInfos.Count + 1,
                            Name = item.Name,
                            PersonaId = item.PersonaId,
                            KD = item.KD,
                            KPM = item.KPM,
                            Time = item.Time,
                            IsWeaponOK = item.IsWeaponOK,
                            IsVehicleOK = item.IsVehicleOK,
                            WeaponCount = item.WeaponInfos.Count,
                            VehicleCount = item.VehicleInfos.Count,
                            Date = $"{MiscUtil.DiffMinutes(item.Date, DateTime.Now):0.00} 分钟"
                        });
                    });
                }
            });
        }
    }

    /// <summary>
    /// 刷新生涯缓存数据
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItem_RefushLifeDataCache_Click(object sender, RoutedEventArgs e)
    {
        RefreshCacheLifeData();
    }
}
