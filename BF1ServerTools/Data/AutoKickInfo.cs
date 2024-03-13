namespace BF1ServerTools.Data;

public class AutoKickInfo
{
    /// <summary>
    /// 被踢出的玩家等级
    /// </summary>
    public int Rank { get; set; }
    /// <summary>
    /// 被踢出的玩家ID
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 被踢出的玩家数字ID
    /// </summary>
    public long PersonaId { get; set; }
    /// <summary>
    /// 被踢出的原因
    /// </summary>
    public string Reason { get; set; }
    /// <summary>
    /// 踢人标志枚举
    /// </summary>
    public KickFlag Flag { get; set; }
    /// <summary>
    /// 执行踢人操作的状态
    /// </summary>
    public string State { get; set; }
    /// <summary>
    /// 记录踢人请求响应时间
    /// </summary>
    public DateTime Time { get; set; }
}

public enum KickFlag
{
    Default,
    Kicking,
    Success,
    Faild
}
