namespace BF1ServerTools.Utils;

public static class ProcessUtil
{
    /// <summary>
    /// 判断程序是否运行
    /// </summary>
    /// <param name="appName">程序名称</param>
    /// <returns>正在运行返回true，未运行返回false</returns>
    public static bool IsAppRun(string appName)
    {
        return Process.GetProcessesByName(appName).Length > 0;
    }

    /// <summary>
    /// 判断战地1程序是否运行，测试时关闭
    /// </summary>
    /// <returns></returns>
    public static bool IsBf1Run()
    {
        var pArray = Process.GetProcessesByName("bf1");
        if (pArray.Length > 0)
        {
            foreach (var item in pArray)
            {
                if (item.MainWindowTitle.Equals("Battlefield™ 1"))
                    return true;
            }
        }

        return true;
    }

    /// <summary>
    /// 打开指定链接或程序
    /// </summary>
    /// <param name="path"></param>
    public static void OpenPath(string path)
    {
        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
    }

    /// <summary>
    /// 打开指定链接或程序
    /// </summary>
    /// <param name="proName"></param>
    /// <param name="path"></param>
    public static void OpenPath(string proName, string path)
    {
        Process.Start(new ProcessStartInfo(proName, path) { UseShellExecute = true });
    }

    /// <summary>
    /// 根据名字关闭指定程序
    /// </summary>
    /// <param name="processName">程序名字，不需要加.exe</param>
    public static void CloseProcess(string processName)
    {
        var appProcess = Process.GetProcesses();
        foreach (var targetPro in appProcess)
        {
            if (targetPro.ProcessName.Equals(processName))
                targetPro.Kill();
        }
    }

    /// <summary>
    /// 运行CMD命令
    /// </summary>
    /// <param name="cmd"></param>
    public static void RunCMD(string cmd)
    {
        Process process = new();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = "/k" + cmd;
        process.Start();
    }

    /// <summary>
    /// 关闭全部第三方exe进程
    /// </summary> 
    public static void CloseThirdProcess()
    {
        CloseProcess("go-cqhttp");
    }
}
