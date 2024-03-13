namespace BF1ServerTools.Data;

public class ChangeTeamInfo
{
    /// <summary>
    /// 更换队伍的玩家等级
    /// </summary>
    public int Rank { get; set; }
    /// <summary>
    /// 更换队伍的玩家ID
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 更换队伍的玩家数字ID
    /// </summary>
    public long PersonaId { get; set; }
    /// <summary>
    /// 地图模式
    /// </summary>
    public string GameMode { get; set; }
    /// <summary>
    /// 地图名称
    /// </summary>
    public string MapName { get; set; }
    /// <summary>
    /// 队伍1名称
    /// </summary>
    public string Team1Name { get; set; }
    /// <summary>
    /// 队伍2名称
    /// </summary>
    public string Team2Name { get; set; }
    /// <summary>
    /// 更换队伍的状态
    /// </summary>
    public string State { get; set; }
    /// <summary>
    /// 更换队伍的时间
    /// </summary>
    public DateTime Time { get; set; }
}
