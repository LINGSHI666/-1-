namespace BF1ServerTools.Utils;

public static class FileUtil
{
    /// <summary>
    /// 我的文档目录路径
    /// </summary>
    public static readonly string MyDocuments_Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    /// <summary>
    /// 默认配置文件路径
    /// </summary>
    public static readonly string Default_Path = MyDocuments_Path + @"\BF1ServerTools2";

    public static readonly string D_Cache_Path = Default_Path + @"\Cache";
    public static readonly string D_Config_Path = Default_Path + @"\Config";
    public static readonly string D_Data_Path = Default_Path + @"\Data";
    public static readonly string D_Log_Path = Default_Path + @"\Log";
    public static readonly string D_Robot_Path = Default_Path + @"\Robot";

    /// <summary>
    /// 主程序资源路径
    /// </summary>
    public const string Resource_Path = "BF1ServerTools.Files.";

    /// <summary>
    /// 保存崩溃日志
    /// </summary>
    /// <param name="log">日志内容</param>
    public static void SaveCrashLog(string log)
    {
        var path = D_Log_Path + @"\Crash";
        Directory.CreateDirectory(path);
        path += $@"\#Crash#{DateTime.Now:yyyyMMdd_HH-mm-ss_ffff}.log";
        File.WriteAllText(path, log);
    }

    /// <summary>
    /// 清空指定文件夹下的文件及文件夹
    /// </summary>
    /// <param name="path">文件夹路径</param>
    public static void DelectDir(string path)
    {
        try
        {
            var dir = new DirectoryInfo(path);
            var fileinfo = dir.GetFileSystemInfos();
            foreach (var file in fileinfo)
            {
                if (file is DirectoryInfo)
                {
                    var subdir = new DirectoryInfo(file.FullName);
                    subdir.Delete(true);
                }
                else
                {
                    File.Delete(file.FullName);
                }
            }
        }
        catch { }
    }

    /// <summary>
    /// 从资源文件中抽取资源文件
    /// </summary>
    /// <param name="resFileName">资源文件名称（资源文件名称必须包含目录，目录间用“.”隔开,最外层是项目默认命名空间）</param>
    /// <param name="outputFile">输出文件</param>
    public static void ExtractResFile(string resFileName, string outputFile)
    {
        BufferedStream inStream = null;
        FileStream outStream = null;

        try
        {
            // 读取嵌入式资源
            Assembly asm = Assembly.GetExecutingAssembly();
            inStream = new BufferedStream(asm.GetManifestResourceStream(resFileName));
            outStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);

            byte[] buffer = new byte[1024];
            int length;

            while ((length = inStream.Read(buffer, 0, buffer.Length)) > 0)
                outStream.Write(buffer, 0, length);

            outStream.Flush();
        }
        finally
        {
            outStream?.Close();
            inStream?.Close();
        }
    }
}
