namespace BF1ServerTools.Configs;

public class RobotConfig
{
    public long QQGroupID { get; set; }
    public List<long> QQGroupMemberID { get; set; }
    public bool IsSendChangeTeamToQQ { get; set; }
    public bool IsSendGameChatToQQ { get; set; }
}
