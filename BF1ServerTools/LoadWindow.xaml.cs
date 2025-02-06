using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using BF1ServerTools.SDK;
using BF1ServerTools.SDK.Core;
using BF1ServerTools.Utils;
using BF1ServerTools.Models;
using BF1ServerTools.Helper;

using Chinese;
using CommunityToolkit.Mvvm.Input;
using BF1ServerTools.API;
using BF1ServerTools.Data;
using BF1ServerTools.Views;
using System.Drawing;
using BF1ServerTools.UI.Controls;


namespace BF1ServerTools;


/// <summary>
/// LoadWindow.xaml 的交互逻辑
/// </summary>
public partial class LoadWindow : UiWindow
{
    /// <summary>
    /// Load的数据模型绑定
    /// </summary>
    public LoadModel LoadModel { get; set; } = new();
   
    // 声明DispatcherTimer
   
    public LoadWindow()
    {
        InitializeComponent();
       

        // 配置DispatcherTimer
        
    }
    
    /// <summary>
    /// Load窗口加载完成事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_Load_Loaded(object sender, RoutedEventArgs e)
    {
        this.DataContext = this;

        Task.Run(() =>
        {
            try
            {
                
                LoadModel.LoadState = "正在初始化工具中...";

                LoggerHelper.Info("开始初始化程序...");
                LoggerHelper.Info($"当前程序版本号 {CoreUtil.ClientVersionInfo}");
                LoggerHelper.Info($"当前程序最后编译时间 {CoreUtil.ClientBuildTime}");

                // 客户端程序版本号
                LoadModel.VersionInfo = CoreUtil.ClientVersionInfo;
                // 最后编译时间
                LoadModel.BuildDate = CoreUtil.ClientBuildTime;

                // 关闭第三方进程
                ProcessUtil.CloseThirdProcess();

                /////////////////////////////////////////////////////////////////////

                LoadModel.LoadState = "正在初始化配置文件...";
                LoggerHelper.Info("正在初始化配置文件...");

                // 创建配置目录
                Directory.CreateDirectory(BF1ServerTools.Utils.FileUtil.D_Cache_Path);
                Directory.CreateDirectory(BF1ServerTools.Utils.FileUtil.D_Config_Path);
                Directory.CreateDirectory(BF1ServerTools.Utils.FileUtil.D_Data_Path);
                Directory.CreateDirectory(BF1ServerTools.Utils.FileUtil.D_Log_Path);
                Directory.CreateDirectory(BF1ServerTools.Utils.FileUtil.D_Robot_Path);

                LoadModel.LoadState = "正在检测战地1是否运行...";
                // 检测战地1是否运行，测试离线运行
                if (true)
                {
                    if (false) {
                        if (!ProcessUtil.IsBf1Run())
                        {
                            LoadModel.LoadState = "未发现《战地1》游戏进程！程序即将关闭";
                            LoggerHelper.Error("未发现战地1进程");

                            Task.Delay(2000).Wait();
                            this.Dispatcher.Invoke(() =>
                            {
                                Application.Current.Shutdown();
                            });
                            return;
                        }
                    }
                    

                    LoadModel.LoadState = "正在初始化战地1内存模块...";
                    // 初始化战地1内存模块
                    if (!Memory.Initialize() && false)//test
                    {
                        LoadModel.LoadState = $"战地1内存模块初始化失败！程序即将关闭";
                        LoggerHelper.Error("战地1内存模块初始化失败");

                        Task.Delay(2000).Wait();
                        this.Dispatcher.Invoke(() =>
                        {
                             Application.Current.Shutdown();
                        });
                        return;
                    }

                    LoadModel.LoadState = "正在初始化SQLite数据库...";
                    // 初始化SQLite数据库
                    if (!SQLiteHelper.Initialize())
                    {
                        LoadModel.LoadState = "SQLite数据库初始化失败！程序即将关闭";
                        LoggerHelper.Error("SQLite数据库初始化失败");

                        Task.Delay(2000).Wait();
                        this.Dispatcher.Invoke(() =>
                        {
                            Application.Current.Shutdown();
                        });
                        return;
                    }

                    /////////////////////////////////////////////////////////////////////

                    LoadModel.LoadState = "正在准备最后工作...";

                    // 释放必要文件
                    if (!File.Exists(BF1ServerTools.Utils.FileUtil.D_Robot_Path + "\\config.yml"))
                        BF1ServerTools.Utils.FileUtil.ExtractResFile(BF1ServerTools.Utils.FileUtil.Resource_Path + "config.yml", BF1ServerTools.Utils.FileUtil.D_Robot_Path + "\\config.yml");

                    if (!File.Exists(BF1ServerTools.Utils.FileUtil.D_Robot_Path + "\\go-cqhttp.exe"))
                        BF1ServerTools.Utils.FileUtil.ExtractResFile(BF1ServerTools.Utils.FileUtil.Resource_Path + "go-cqhttp.exe", BF1ServerTools.Utils.FileUtil.D_Robot_Path + "\\go-cqhttp.exe");

                    Chat.AllocateMemory();
                    LoggerHelper.Info($"中文聊天指针分配成功 0x{Chat.AllocateMemAddress:x}");

                    ChineseConverter.ToTraditional("免费，跨平台，开源！");
                    LoggerHelper.Info("简繁翻译库初始化成功");

                    /////////////////////////////////////////////////////////////////////
                    /*
                    this.Dispatcher.Invoke(() =>
                    {
                        var mainWindow = new MainWindow();
                        // 显示主窗口
                        mainWindow.Show();
                        // 转移主程序控制权
                        Application.Current.MainWindow = mainWindow;
                        // 关闭初始化窗口
                        this.Close();
                    });*/
                }
                //Memory.Initialize();
               // SQLiteHelper.Initialize();
                LoadModel.LoadState = "正在准备最后工作...";

                // 释放必要文件
                if (!File.Exists(BF1ServerTools.Utils.FileUtil.D_Robot_Path + "\\config.yml"))
                    BF1ServerTools.Utils.FileUtil.ExtractResFile(BF1ServerTools.Utils.FileUtil.Resource_Path + "config.yml", BF1ServerTools.Utils.FileUtil.D_Robot_Path + "\\config.yml");

                if (!File.Exists(BF1ServerTools.Utils.FileUtil.D_Robot_Path + "\\go-cqhttp.exe"))
                    BF1ServerTools.Utils.FileUtil.ExtractResFile(BF1ServerTools.Utils.FileUtil.Resource_Path + "go-cqhttp.exe", BF1ServerTools.Utils.FileUtil.D_Robot_Path + "\\go-cqhttp.exe");

                Chat.AllocateMemory();
                LoggerHelper.Info($"中文聊天指针分配成功 0x{Chat.AllocateMemAddress:x}");

                ChineseConverter.ToTraditional("免费，跨平台，开源！");
                LoggerHelper.Info("简繁翻译库初始化成功");

                /////////////////////////////////////////////////////////////////////
                

                this.Dispatcher.Invoke(() =>
                {
                    var mainWindow = new MainWindow();
                    // 显示主窗口
                    mainWindow.Show();
                    // 转移主程序控制权
                    Application.Current.MainWindow = mainWindow;
                    // 关闭初始化窗口
                    this.Close();
                });
            }
            catch (Exception ex)
            {
                LoadModel.LoadState = $"初始化错误，发生了未知异常！\n\n{ex.Message}\n\n提示：尝试清空程序配置文件夹全部文件后重试";
                LoggerHelper.Error("初始化错误，发生了未知异常", ex);

                this.Dispatcher.Invoke(() =>
                {
                    WrapPanel_ExceptionState.Visibility = Visibility.Visible;
                });
            } 
        });
    }

    [RelayCommand]
    private void ButtonClick(string name)
    {
        switch (name)
        {
            case "OpenDefaultPath":
                ProcessUtil.OpenPath(BF1ServerTools.Utils.FileUtil.Default_Path);
                break;
            case "ExitMainAPP":
                Application.Current.Shutdown();
                break;
        }
    }
}
