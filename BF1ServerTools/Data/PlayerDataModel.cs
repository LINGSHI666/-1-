using CommunityToolkit.Mvvm.ComponentModel;

namespace BF1ServerTools.Data;

public partial class PlayerDataModel : ObservableObject, IComparable<PlayerDataModel>
{
    /// <summary>
    /// 序号
    /// </summary>
    [ObservableProperty]
    private int index;

    ///////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 玩家战队
    /// </summary>
    [ObservableProperty]
    private string clan;

    /// <summary>
    /// 玩家ID
    /// </summary>
    [ObservableProperty]
    private string name;

    /// <summary>
    /// 玩家数字ID
    /// </summary>
    [ObservableProperty]
    private long personaId;

    /// <summary>
    /// 玩家小队Id
    /// </summary>
    [ObservableProperty]
    private int squadId;

    /// <summary>
    /// 玩家小队Id
    /// </summary>
    [ObservableProperty]
    private string squadId2;

    ///////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 管理员
    /// </summary>
    [ObservableProperty]
    private bool admin;

    /// <summary>
    /// VIP
    /// </summary>
    [ObservableProperty]
    private bool vip;

    /// <summary>
    /// 白名单
    /// </summary>
    [ObservableProperty]
    private bool white;

    ///////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 等级
    /// </summary>
    [ObservableProperty]
    private int rank;

    /// <summary>
    /// 击杀
    /// </summary>
    [ObservableProperty]
    private int kill;

    /// <summary>
    /// 死亡
    /// </summary>
    [ObservableProperty]
    private int dead;

    /// <summary>
    /// 得分
    /// </summary>
    [ObservableProperty]
    private int score;

    /// <summary>
    /// KD
    /// </summary>
    [ObservableProperty]
    private float kd;

    /// <summary>
    /// KPM
    /// </summary>
    [ObservableProperty]
    private float kpm;

    /// <summary>
    /// 生涯KD
    /// </summary>
    [ObservableProperty]
    private float lifeKd;

    /// <summary>
    /// 生涯KPM
    /// </summary>
    [ObservableProperty]
    private float lifeKpm;

    /// <summary>
    /// 生涯时长
    /// </summary>
    [ObservableProperty]
    private int lifeTime;

    ///////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 兵种
    /// </summary>
    [ObservableProperty]
    private string kit;

    /// <summary>
    /// 兵种2
    /// </summary>
    [ObservableProperty]
    private string kit2;

    /// <summary>
    /// 兵种3
    /// </summary>
    [ObservableProperty]
    private string kit3;

    /// <summary>
    /// 武器槽0
    /// </summary>
    [ObservableProperty]
    private string weaponS0;

    /// <summary>
    /// 武器槽1
    /// </summary>
    [ObservableProperty]
    private string weaponS1;

    /// <summary>
    /// 武器槽2
    /// </summary>
    [ObservableProperty]
    private string weaponS2;

    /// <summary>
    /// 武器槽3
    /// </summary>
    [ObservableProperty]
    private string weaponS3;

    /// <summary>
    /// 武器槽4
    /// </summary>
    [ObservableProperty]
    private string weaponS4;

    /// <summary>
    /// 武器槽5
    /// </summary>
    [ObservableProperty]
    private string weaponS5;

    /// <summary>
    /// 武器槽6
    /// </summary>
    [ObservableProperty]
    private string weaponS6;

    /// <summary>
    /// 武器槽0
    /// </summary>
    [ObservableProperty]
    private string weaponS7;

    ///////////////////////////////////////////////////////////////////////

    public int CompareTo(PlayerDataModel other)
    {
        switch (Globals.OrderBy)
        {
            case OrderBy.Score:
                return other.score.CompareTo(this.score);
            case OrderBy.Rank:
                return other.rank.CompareTo(this.rank);
            case OrderBy.Clan:
                return other.clan.CompareTo(this.clan);
            case OrderBy.Name:
                return this.name.CompareTo(other.name);
            case OrderBy.SquadId:
                return this.squadId.CompareTo(other.squadId);
            case OrderBy.Kill:
                return other.kill.CompareTo(this.kill);
            case OrderBy.Dead:
                return other.dead.CompareTo(this.dead);
            case OrderBy.KD:
                return other.kd.CompareTo(this.kd);
            case OrderBy.KPM:
                return other.kpm.CompareTo(this.kpm);
            case OrderBy.LKD:
                return other.lifeKd.CompareTo(this.lifeKd);
            case OrderBy.LKPM:
                return other.lifeKpm.CompareTo(this.lifeKpm);
            case OrderBy.LTime:
                return other.lifeTime.CompareTo(this.lifeTime);
            case OrderBy.Kit3:
                return other.kit3.CompareTo(this.kit3);
            case OrderBy.Weapon:
                return other.weaponS0.CompareTo(this.weaponS0);
            default:
                return other.score.CompareTo(this.score);
        }
    }
}
