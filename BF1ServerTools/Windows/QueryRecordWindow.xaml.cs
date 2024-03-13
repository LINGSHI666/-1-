using BF1ServerTools.RES;
using BF1ServerTools.Data;
using BF1ServerTools.Utils;
using BF1ServerTools.Models;
using BF1ServerTools.API;
using BF1ServerTools.API.RespJson;

namespace BF1ServerTools.Windows;

/// <summary>
/// QueryRecordWindow.xaml 的交互逻辑
/// </summary>
public partial class QueryRecordWindow
{
    /// <summary>
    /// 数据模型绑定
    /// </summary>
    public QueryModel QueryModel { get; set; } = new();

    /// <summary>
    /// 玩家综合数据
    /// </summary>
    public ObservableCollection<string> ListBox_PlayerDatas { get; set; } = new();
    /// <summary>
    /// 玩家武器数据
    /// </summary>
    public ObservableCollection<WeaponInfo> ListBox_WeaponInfos { get; set; } = new();
    /// <summary>
    /// 玩家载具数据
    /// </summary>
    public ObservableCollection<VehicleInfo> ListBox_VehicleInfos { get; set; } = new();

    /////////////////////////////////////////////////////

    /// <summary>
    /// 玩家ID
    /// </summary>
    public string PlayerName { get; set; }
    /// <summary>
    /// 玩家数字ID
    /// </summary>
    public long PersonaId { get; set; }
    /// <summary>
    /// 玩家等级
    /// </summary>
    public int Rank { get; set; }

    public QueryRecordWindow(string playerName, long personaId, int rank)
    {
        InitializeComponent();

        PlayerName = playerName;
        PersonaId = personaId;
        Rank = rank;
    }

    private void Window_QueryRecord_Loaded(object sender, RoutedEventArgs e)
    {
        this.DataContext = this;

        Title = $"{this.Title} > 玩家ID : {PlayerName} > 数字ID : {PersonaId}";

        if (PersonaId != 0)
        {
            QueryModel.IsLoading = true;
            QueryPlayerRecord(PersonaId);
        }
    }

    private void Window_QueryRecord_Closing(object sender, CancelEventArgs e)
    {

    }

    /// <summary>
    /// 分段查询玩家数据
    /// </summary>
    /// <param name="personaId"></param>
    private void QueryPlayerRecord(long personaId)
    {
        GetPersonas(personaId);
        DetailedStats(personaId);

        GetWeapons(personaId);
        GetVehicles(personaId);
    }

    /// <summary>
    /// 获取玩家信息
    /// </summary>
    /// <param name="personaId"></param>
    private async void GetPersonas(long personaId)
    {
        var result = await BF1API.GetPersonasByIds(Globals.SessionId, personaId);
        if (result.IsSuccess)
        {
            JsonNode jNode = JsonNode.Parse(result.Content);
            if (jNode["result"]![$"{personaId}"] != null)
            {
                QueryModel.Avatar = jNode["result"]![$"{personaId}"]!["avatar"].GetValue<string>();
                QueryModel.Rank = $"等级 : {Rank}";
            }
            else
            {
                return;
            }
        }
    }

    /// <summary>
    /// 获取玩家详情数据
    /// </summary>
    /// <param name="personaId"></param>
    private async void DetailedStats(long personaId)
    {
        var result = await BF1API.DetailedStatsByPersonaId(Globals.SessionId, personaId);
        if (result.IsSuccess)
        {
            var detailed = JsonHelper.JsonDese<DetailedStats>(result.Content);

            var basic = detailed.result.basicStats;
            QueryModel.PlayTime = $"时长 : {PlayerUtil.GetPlayTime(basic.timePlayed)}";

            await Task.Run(() =>
            {
                AddPlayerInfo($"KD : {PlayerUtil.GetPlayerKD(basic.kills, basic.deaths):0.00}");
                AddPlayerInfo($"KPM : {basic.kpm}");
                AddPlayerInfo($"SPM : {basic.spm}");

                AddPlayerInfo($"命中率 : {detailed.result.accuracyRatio * 100:0.00}%");
                AddPlayerInfo($"爆头率 : {PlayerUtil.GetPlayerPercentage(detailed.result.headShots, basic.kills)}");
                AddPlayerInfo($"爆头数 : {detailed.result.headShots}");

                AddPlayerInfo($"最高连续击杀数 : {detailed.result.highestKillStreak}");
                AddPlayerInfo($"最远爆头距离 : {detailed.result.longestHeadShot}");
                AddPlayerInfo($"最佳兵种 : {ClientHelper.GetClassChs(detailed.result.favoriteClass)}");

                AddPlayerInfo("");

                AddPlayerInfo($"击杀 : {basic.kills}");
                AddPlayerInfo($"死亡 : {basic.deaths}");
                AddPlayerInfo($"协助击杀数 : {detailed.result.killAssists}");

                AddPlayerInfo($"仇敌击杀数 : {detailed.result.avengerKills}");
                AddPlayerInfo($"救星击杀数 : {detailed.result.saviorKills}");
                AddPlayerInfo($"急救数 : {detailed.result.revives}");
                AddPlayerInfo($"治疗分 : {detailed.result.heals}");
                AddPlayerInfo($"修理分 : {detailed.result.repairs}");

                AddPlayerInfo("");

                AddPlayerInfo($"胜利场数 : {basic.wins}");
                AddPlayerInfo($"战败场数 : {basic.losses}");
                AddPlayerInfo($"胜率 : {PlayerUtil.GetPlayerPercentage(basic.wins, detailed.result.roundsPlayed)}");
                AddPlayerInfo($"技巧值 : {basic.skill}");
                AddPlayerInfo($"游戏总场数 : {detailed.result.roundsPlayed}");
                AddPlayerInfo($"取得狗牌数 : {detailed.result.dogtagsTaken}");

                AddPlayerInfo($"小隊分数 : {detailed.result.squadScore}");
                AddPlayerInfo($"奖励分数 : {detailed.result.awardScore}");
                AddPlayerInfo($"加成分数 : {detailed.result.bonusScore}");
            });
        }
    }

    /// <summary>
    /// 获取玩家武器数据
    /// </summary>
    /// <param name="personaId"></param>
    private async void GetWeapons(long personaId)
    {
        var result = await BF1API.GetWeaponsByPersonaId(Globals.SessionId, personaId);
        if (result.IsSuccess)
        {
            var getWeapons = JsonHelper.JsonDese<GetWeapons>(result.Content);

            var weapons = new List<WeaponInfo>();
            foreach (var res in getWeapons.result)
            {
                foreach (var wea in res.weapons)
                {
                    if (wea.stats.values.kills == 0)
                        continue;

                    weapons.Add(new()
                    {
                        name = ChsUtil.ToSimplified(wea.name),
                        imageUrl = ClientHelper.GetTempImagePath(wea.imageUrl, "weapon2"),
                        star = PlayerUtil.GetKillStar((int)wea.stats.values.kills),
                        kills = (int)wea.stats.values.kills,
                        killsPerMinute = PlayerUtil.GetPlayerKPM(wea.stats.values.kills, wea.stats.values.seconds),
                        headshots = (int)wea.stats.values.headshots,
                        headshotsVKills = PlayerUtil.GetPlayerPercentage(wea.stats.values.headshots, wea.stats.values.kills),
                        shots = (int)wea.stats.values.shots,
                        hits = (int)wea.stats.values.hits,
                        hitsVShots = PlayerUtil.GetPlayerPercentage(wea.stats.values.hits, wea.stats.values.shots),
                        hitVKills = $"{wea.stats.values.hits / wea.stats.values.kills:0.00}",
                        time = PlayerUtil.GetPlayTime(wea.stats.values.seconds)
                    });
                }
            }

            weapons.Sort((a, b) => b.kills.CompareTo(a.kills));

            QueryModel.IsLoading = false;

            await Task.Run(() =>
            {
                foreach (var item in weapons)
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                    {
                        ListBox_WeaponInfos.Add(item);
                    }));
                }
            });
        }
        else
        {
            QueryModel.IsLoading = false;
        }
    }

    /// <summary>
    /// 获取玩家载具数据
    /// </summary>
    /// <param name="personaId"></param>
    private async void GetVehicles(long personaId)
    {
        var result = await BF1API.GetVehiclesByPersonaId(Globals.SessionId, personaId);
        if (result.IsSuccess)
        {
            var getVehicles = JsonHelper.JsonDese<GetVehicles>(result.Content);

            var vehicles = new List<VehicleInfo>();
            foreach (var res in getVehicles.result)
            {
                foreach (var veh in res.vehicles)
                {
                    if (veh.stats.values.kills == 0)
                        continue;

                    vehicles.Add(new()
                    {
                        name = ChsUtil.ToSimplified(veh.name),
                        imageUrl = ClientHelper.GetTempImagePath(veh.imageUrl, "weapon2"),
                        star = PlayerUtil.GetKillStar((int)veh.stats.values.kills),
                        kills = (int)veh.stats.values.kills,
                        killsPerMinute = PlayerUtil.GetPlayerKPM(veh.stats.values.kills, veh.stats.values.seconds),
                        destroyed = (int)veh.stats.values.destroyed,
                        time = PlayerUtil.GetPlayTime(veh.stats.values.seconds)
                    });
                }
            }

            vehicles.Sort((a, b) => b.kills.CompareTo(a.kills));

            await Task.Run(() =>
            {
                foreach (var item in vehicles)
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                    {
                        ListBox_VehicleInfos.Add(item);
                    }));
                }
            });
        }
    }

    private void AddPlayerInfo(string str)
    {
        this.Dispatcher.Invoke(DispatcherPriority.Background, () =>
        {
            ListBox_PlayerDatas.Add(str);
        });
    }
}
