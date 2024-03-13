using BF1ServerTools.Data;

namespace BF1ServerTools;

public static class Globals
{
    /// <summary>
    /// 玩家列表排序规则
    /// </summary>
    public static OrderBy OrderBy = OrderBy.Score;

    /// <summary>
    /// 是否使用模式1
    /// </summary>
    public static bool IsUseMode1 = true;

    ///////////////////////////////////////////////////////

    /// <summary>
    /// 模式1 玩家Avatar
    /// </summary>
    public static string Avatar1 = string.Empty;
    /// <summary>
    /// 模式2 玩家Avatar
    /// </summary>
    public static string Avatar2 = string.Empty;
    /// <summary>
    /// 玩家Avatar
    /// </summary>
    public static string Avatar
    {
        get
        {
            return IsUseMode1 ? Avatar1 : Avatar2;
        }
    }

    /// <summary>
    /// 模式1 玩家DisplayName
    /// </summary>
    public static string DisplayName1 = string.Empty;
    /// <summary>
    /// 模式2 玩家DisplayName
    /// </summary>
    public static string DisplayName2 = string.Empty;
    /// <summary>
    /// 全局玩家DisplayName
    /// </summary>
    public static string DisplayName
    {
        get
        {
            return IsUseMode1 ? DisplayName1 : DisplayName2;
        }
    }

    /// <summary>
    /// 模式1 玩家PersonaId
    /// </summary>
    public static long PersonaId1 = 0;
    /// <summary>
    /// 模式2 玩家PersonaId
    /// </summary>
    public static long PersonaId2 = 0;
    /// <summary>
    /// 全局PersonaId
    /// </summary>
    public static long PersonaId
    {
        get
        {
            return IsUseMode1 ? PersonaId1 : PersonaId2;
        }
    }

    ///////////////////////////////////////////////////////

    /// <summary>
    /// 玩家Remid
    /// </summary>
    public static string Remid = string.Empty;
    /// <summary>
    /// 玩家Sid
    /// </summary>
    public static string Sid = string.Empty;
    /// <summary>
    /// 玩家登录令牌，有效期4小时
    /// </summary>
    public static string AccessToken = string.Empty;

    /// <summary>
    /// 模式1 玩家SessionId
    /// </summary>
    public static string SessionId1 = string.Empty;
    /// <summary>
    /// 模式2 玩家SessionId
    /// </summary>
    public static string SessionId2 = string.Empty;
    /// <summary>
    /// 全局玩家SessionId
    /// </summary>
    public static string SessionId
    {
        get
        {
            return IsUseMode1 ? SessionId1 : SessionId2;
        }
    }

    /// <summary>
    /// 当前服务器游戏Id
    /// </summary>
    public static long GameId = 0;
    /// <summary>
    /// 当前服务器Id
    /// </summary>
    public static int ServerId = 0;
    /// <summary>
    /// 当前服务器游戏Guid
    /// </summary>
    public static string PersistedGameId = string.Empty;

    ///////////////////////////////////////////////////////

    /// <summary>
    /// 判断当前玩家是否为管理员
    /// </summary>
    /// <returns></returns>
    public static bool LoginPlayerIsAdmin
    {
        get
        {
            if (IsUseMode1)
                return ServerAdmins_PID.Contains(PersonaId1);
            else
                return ServerAdmins_PID.Contains(PersonaId2);
        }
    }

    /// <summary>
    /// 服务器管理员，PID
    /// </summary>
    public static List<long> ServerAdmins_PID = new();
    /// <summary>
    /// 服务器VIP
    /// </summary>
    public static List<long> ServerVIPs_PID = new();

    ///////////////////////////////////////////////////////

    /// <summary>
    /// 保存违规玩家列表信息
    /// </summary>
    public static List<BreakRuleInfo> BreakRuleInfo_PlayerList = new();

    /// <summary>
    /// 缓存玩家生涯数据
    /// </summary>
    public static List<LifePlayerData> LifePlayerCacheDatas = new();

    /// <summary>
    /// 踢出玩家CD缓存
    /// </summary>
    public static List<KickCoolDownInfo> KickCoolDownInfos = new();

    ///////////////////////////////////////////////////////

    /// <summary>
    /// 服务器规则 队伍1
    /// </summary>
    public static ServerRule ServerRule_Team1 = new();
    /// <summary>
    /// 服务器规则 队伍2
    /// </summary>
    public static ServerRule ServerRule_Team2 = new();

    /// <summary>
    /// 保存队伍1限制武器名称列表
    /// </summary>
    public static List<string> CustomWeapons_Team1 = new();
    /// <summary>
    /// 保存队伍2限制武器名称列表
    /// </summary>
    public static List<string> CustomWeapons_Team2 = new();

    /// <summary>
    /// 自定义白名单玩家列表
    /// </summary>
    public static List<string> CustomWhites_Name = new();
    /// <summary>
    /// 自定义黑名单玩家列表
    /// </summary>
    public static List<string> CustomBlacks_Name = new();

    ///////////////////////////////////////////////////////

    /// <summary>
    /// 是否设置规则正确
    /// </summary>
    public static bool IsSetRuleOK = false;

    /// <summary>
    /// 是否自动踢出违规玩家
    /// </summary>
    public static bool AutoKickBreakRulePlayer = false;

    /// <summary>
    /// 是否自动踢出观战玩家
    /// </summary>
    public static bool IsAutoKickSpectator = false;

    /// <summary>
    /// 是否启用踢人冷却
    /// </summary>
    public static bool IsEnableKickCoolDown = false;

    /// <summary>
    /// 是否启用踢出非白名单玩家
    /// </summary>
    public static bool IsEnableKickNoWhites = false;

    ///////////////////////////////////////////////////////

    public static bool WhiteLifeKD = true;
    public static bool WhiteLifeKPM = true;
    public static bool WhiteLifeWeaponStar = true;
    public static bool WhiteLifeVehicleStar = true;
    public static bool WhiteKill = true;
    public static bool WhiteKD = true;
    public static bool WhiteKPM = true;
    public static bool WhiteRank = true;
    public static bool WhiteWeapon = true;
}

public enum OrderBy
{
    Score,
    Rank,
    Clan,
    Name,
    SquadId,
    Kill,
    Dead,
    KD,
    KPM,
    LKD,
    LKPM,
    LTime,
    Kit3,
    Weapon
}
