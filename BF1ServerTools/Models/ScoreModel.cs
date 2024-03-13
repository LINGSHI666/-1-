using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1ServerTools.Models;

public partial class ScoreModel : ObservableObject
{
    /// <summary>
    /// 服务器名称
    /// </summary>
    [ObservableProperty]
    private string serverName;

    /// <summary>
    /// 服务器GameId
    /// </summary>
    [ObservableProperty]
    private long serverGameId;

    /// <summary>
    /// 服务器地图游戏模式
    /// </summary>
    [ObservableProperty]
    private string serverGameMode;

    /// <summary>
    /// 服务器地图名称
    /// </summary>
    [ObservableProperty]
    private string serverMapName;

    /// <summary>
    /// 服务器地图预览图
    /// </summary>
    [ObservableProperty]
    private string serverMapImg;

    /// <summary>
    /// 服务器时间
    /// </summary>
    [ObservableProperty]
    private string serverTime;

    ///////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 队伍1比分
    /// </summary>
    [ObservableProperty]
    private int team1Score;

    /// <summary>
    /// 队伍1比分，图形宽度
    /// </summary>
    [ObservableProperty]
    private double team1ScoreWidth;

    /// <summary>
    /// 队伍1从旗帜获取的得分
    /// </summary>
    [ObservableProperty]
    private int team1Flag;

    /// <summary>
    /// 队伍1从击杀获取的得分
    /// </summary>
    [ObservableProperty]
    private int team1Kill;

    /// <summary>
    /// 队伍1图片
    /// </summary>
    [ObservableProperty]
    private string team1Img;

    /// <summary>
    /// 队伍1名称
    /// </summary>
    [ObservableProperty]
    private string team1Name;

    /// <summary>
    /// 队伍1已部署玩家数量
    /// </summary>
    [ObservableProperty]
    private int team1PlayerCount;

    /// <summary>
    /// 队伍1玩家数量
    /// </summary>
    [ObservableProperty]
    private int team1MaxPlayerCount;

    /// <summary>
    /// 队伍1 150等级玩家数量
    /// </summary>
    [ObservableProperty]
    private int team1Rank150PlayerCount;

    /// <summary>
    /// 队伍1总击杀数
    /// </summary>
    [ObservableProperty]
    private int team1AllKillCount;

    /// <summary>
    /// 队伍1总死亡数
    /// </summary>
    [ObservableProperty]
    private int team1AllDeadCount;

    ///////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 队伍2比分
    /// </summary>
    [ObservableProperty]
    private int team2Score;

    /// <summary>
    /// 队伍2比分，图形宽度
    /// </summary>
    [ObservableProperty]
    private double team2ScoreWidth;

    /// <summary>
    /// 队伍2从旗帜获取的得分
    /// </summary>
    [ObservableProperty]
    private int team2Flag;

    /// <summary>
    /// 队伍2从击杀获取的得分
    /// </summary>
    [ObservableProperty]
    private int team2Kill;

    /// <summary>
    /// 队伍2图片
    /// </summary>
    [ObservableProperty]
    private string team2Img;

    /// <summary>
    /// 队伍2名称
    /// </summary>
    [ObservableProperty]
    private string team2Name;

    /// <summary>
    /// 队伍2已部署玩家数量
    /// </summary>
    [ObservableProperty]
    private int team2PlayerCount;

    /// <summary>
    /// 队伍2玩家数量
    /// </summary>
    [ObservableProperty]
    private int team2MaxPlayerCount;

    /// <summary>
    /// 队伍2 150等级玩家数量
    /// </summary>
    [ObservableProperty]
    private int team2Rank150PlayerCount;

    /// <summary>
    /// 队伍2总击杀数
    /// </summary>
    [ObservableProperty]
    private int team2AllKillCount;

    /// <summary>
    /// 队伍2总死亡数
    /// </summary>
    [ObservableProperty]
    private int team2AllDeadCount;

    ///////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 服务器总人数
    /// </summary>
    [ObservableProperty]
    private int allPlayerCount;
}
