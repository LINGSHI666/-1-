﻿using BF1ServerTools.Utils;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BF1ServerTools;

/// <summary>
/// App.xaml 的交互逻辑
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// 主程序互斥体
    /// </summary>
    public static Mutex AppMainMutex;

    /// <summary>
    /// 应用程序名称
    /// </summary>
    private string AppName = ResourceAssembly.GetName().Name;

    /// <summary>
    /// 保证程序只能同时启动一个
    /// </summary>
    /// <param name="e"></param>
    protected override void OnStartup(StartupEventArgs e)
    {
        AppMainMutex = new Mutex(true, AppName, out var createdNew);
        if (createdNew)
        {
            RegisterEvents();
            base.OnStartup(e);
        }
        else
        {
            MsgBoxUtil.Warning($"请不要重复打开，程序已经运行\n如果一直提示，请到\"任务管理器-详细信息（win7为进程）\"里\n强制结束 \"{AppName}.exe\" 程序",
                "重复运行警告");
            Current.Shutdown();
        }
    }

    /// <summary>
    /// 注册异常捕获事件
    /// </summary>
    private void RegisterEvents()
    {
        // UI线程未捕获异常处理事件（UI主线程）
        this.DispatcherUnhandledException += App_DispatcherUnhandledException;

        // 非UI线程未捕获异常处理事件（例如自己创建的一个子线程）
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        // Task线程内未捕获异常处理事件
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    /// <summary>
    /// UI线程未捕获异常处理事件（UI主线程）
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var msg = GetExceptionInfo(e.Exception, e.ToString());
        BF1ServerTools.Utils.FileUtil.SaveCrashLog(msg);

        // 显示错误消息提示用户
        MessageBox.Show("程序发生了未处理的异常。\n" +
                        "请联系开发者并提供错误日志信息。\n\n" +
                        $"错误信息：{e.Exception.Message}",
                        "程序异常", MessageBoxButton.OK, MessageBoxImage.Error);

        // 防止程序退出
        e.Handled = true;
    }

    /// <summary>
    /// 非UI线程未捕获异常处理事件（例如自己创建的一个子线程）
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var msg = GetExceptionInfo(e.ExceptionObject as Exception, e.ToString());
        BF1ServerTools.Utils.FileUtil.SaveCrashLog(msg);

        // 显示错误消息提示用户
        MessageBox.Show("程序发生了严重的未处理异常，可能会退出。\n" +
                        "请联系开发者并提供错误日志信息。\n\n" +
                        $"错误信息：{((Exception)e.ExceptionObject)?.Message}",
                        "程序异常", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// Task线程内未捕获异常处理事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        var msg = GetExceptionInfo(e.Exception, e.ToString());
        BF1ServerTools.Utils.FileUtil.SaveCrashLog(msg);

        // 标记异常已观察，防止程序退出
        e.SetObserved();
    }

    /// <summary>
    /// 生成自定义异常消息
    /// </summary>
    /// <param name="ex">异常对象</param>
    /// <param name="backStr">备用异常消息：当ex为null时有效</param>
    /// <returns>异常字符串文本</returns>
    private string GetExceptionInfo(Exception ex, string backStr)
    {
        var builder = new StringBuilder();
        builder.AppendLine("===================================");
        builder.AppendLine($"【当前版本】：{CoreUtil.ClientVersionInfo}");
        builder.AppendLine($"【编译时间】：{CoreUtil.ClientBuildTime}");
        builder.AppendLine($"【操作建议】：请将此日志上传到战地1服管工具QQ群");
        builder.AppendLine("===================================");
        builder.AppendLine($"【出现时间】：{DateTime.Now}");

        if (ex != null)
        {
            builder.AppendLine($"【异常类型】：{ex.GetType().Name}");
            builder.AppendLine($"【异常信息】：{ex.Message}");
            builder.AppendLine($"【堆栈调用】：\n{ex.StackTrace}");
        }
        else
        {
            builder.AppendLine($"【未处理异常】：{backStr}");
        }

        return builder.ToString();
    }
}

/// <summary>
/// FileUtil 类，用于保存日志
/// </summary>
public static class FileUtil
{
    public static void SaveCrashLog(string msg)
    {
        try
        {
            string logPath = "CrashLogs";
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            string logFile = Path.Combine(logPath, $"CrashLog_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            File.WriteAllText(logFile, msg);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"日志保存失败：{ex.Message}", "日志错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
