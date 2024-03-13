using BF1ServerTools.Data;
using BF1ServerTools.Views.More;

using CommunityToolkit.Mvvm.Input;

namespace BF1ServerTools.Views;

/// <summary>
/// MoreView.xaml 的交互逻辑
/// </summary>
public partial class MoreView : UserControl
{
    /// <summary>
    /// 导航菜单
    /// </summary>
    public List<MenuBar> MenuBars { get; set; } = new();

    private readonly ServerView ServerView = new();
    private readonly QueryView QueryView = new();
    private readonly CacheView CacheView = new();
    private readonly SQLiteView SQLiteView = new();

    private readonly AboutView AboutView = new();

    public MoreView()
    {
        InitializeComponent();
        this.DataContext = this;
        MainWindow.WindowClosingEvent += MainWindow_WindowClosingEvent;

        // 创建菜单
        CreateMenuBar();
        // 设置主页
        ContentControl_Main.Content = ServerView;
    }

    private void MainWindow_WindowClosingEvent()
    {

    }

    /// <summary>
    /// 创建导航菜单
    /// </summary>
    private void CreateMenuBar()
    {
        MenuBars.Add(new MenuBar() { Icon = "\xe968", Title = "服务器", NameSpace = "ServerView" });
        MenuBars.Add(new MenuBar() { Icon = "\xe968", Title = "玩家数据", NameSpace = "QueryView" });
        MenuBars.Add(new MenuBar() { Icon = "\xe968", Title = "生涯缓存", NameSpace = "CacheView" });
        MenuBars.Add(new MenuBar() { Icon = "\xe968", Title = "历史日志", NameSpace = "SQLiteView" });

        MenuBars.Add(new MenuBar() { Icon = "\xe684", Title = "关于", NameSpace = "AboutView" });
    }

    /// <summary>
    /// 页面导航
    /// </summary>
    /// <param name="menu"></param>
    [RelayCommand]
    private void Navigate(MenuBar menu)
    {
        if (menu == null || string.IsNullOrEmpty(menu.NameSpace))
            return;

        switch (menu.NameSpace)
        {
            case "ServerView":
                ContentControl_Main.Content = ServerView;
                break;
            case "QueryView":
                ContentControl_Main.Content = QueryView;
                break;
            case "CacheView":
                ContentControl_Main.Content = CacheView;
                break;
            case "SQLiteView":
                ContentControl_Main.Content = SQLiteView;
                break;
            case "AboutView":
                ContentControl_Main.Content = AboutView;
                break;
        }
    }
}
