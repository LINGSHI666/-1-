namespace BF1ServerTools.Utils;

public static class CoreUtil
{
    /// <summary>
    /// 程序客户端版本号，如：1.2.3.4
    /// </summary>
    public static Version ClientVersionInfo = Application.ResourceAssembly.GetName().Version;

    /// <summary>
    /// 程序客户端最后编译时间
    /// </summary>
    public static DateTime ClientBuildTime = File.GetLastWriteTime(Environment.ProcessPath);
}
