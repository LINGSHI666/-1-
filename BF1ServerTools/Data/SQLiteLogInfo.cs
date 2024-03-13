namespace BF1ServerTools.Data;

public class SQLiteLogInfo
{
    /// <summary>
    /// 索引
    /// </summary>
    public int Index { get; set; }
    /// <summary>
    /// 等级
    /// </summary>
    public int Rank { get; set; }
    /// <summary>
    /// 玩家ID
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 玩家数字ID
    /// </summary>
    public long PersonaId { get; set; }
    /// <summary>
    /// 日志类型
    /// </summary>
    public string Type { get; set; }
    /// <summary>
    /// 日志信息1
    /// </summary>
    public string Message1 { get; set; }
    /// <summary>
    /// 日志信息2
    /// </summary>
    public string Message2 { get; set; }
    /// <summary>
    /// 日志信息3
    /// </summary>
    public string Message3 { get; set; }
    /// <summary>
    /// 记录日志时间
    /// </summary>
    public string Date { get; set; }
}
