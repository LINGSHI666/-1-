namespace BF1ServerTools.Windows;

/// <summary>
/// ChangeMapWindow.xaml 的交互逻辑
/// </summary>
public partial class ChangeMapWindow
{
    public string MapName { get; set; }
    public string MapImage { get; set; }

    public ChangeMapWindow(string mapName, string mapImage)
    {
        InitializeComponent();
        this.DataContext = this;

        MapName = mapName;
        MapImage = mapImage;
    }

    private void Button_OK_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = true;
        this.Close();
    }
}
