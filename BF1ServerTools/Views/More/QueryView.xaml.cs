using BF1ServerTools.API;
using BF1ServerTools.API.RespJson;
using BF1ServerTools.RES;
using BF1ServerTools.Data;
using BF1ServerTools.Utils;
using BF1ServerTools.Models;
using BF1ServerTools.Helper;

using CommunityToolkit.Mvvm.Input;

namespace BF1ServerTools.Views.More;

/// <summary>
/// QueryView.xaml 的交互逻辑
/// </summary>
public partial class QueryView : UserControl
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

    public QueryView()
    {
        InitializeComponent();
        this.DataContext = this;
        MainWindow.WindowClosingEvent += MainWindow_WindowClosingEvent;

        QueryModel.IsLoading = false;
    }

    private void MainWindow_WindowClosingEvent()
    {
        
    }

    /// <summary>
    /// 查询玩家数据
    /// </summary>
    [RelayCommand]
    private async void QueryPlayer()
    {
        if (Globals.IsUseMode1)
        {
            NotifierHelper.Show(NotifierType.Warning, "查询功能仅模式2可用，操作取消");
            return;
        }

        if (string.IsNullOrEmpty(QueryModel.PlayerName))
        {
            NotifierHelper.Show(NotifierType.Warning, "请输入正确的玩家ID，操作取消");
            return;
        }

        if (string.IsNullOrEmpty(Globals.AccessToken))
        {
            NotifierHelper.Show(NotifierType.Warning, "玩家AccessToken为空，操作取消");
            return;
        }

        ClearUiData();
        QueryModel.IsLoading = true;
        QueryModel.PlayerName = QueryModel.PlayerName.Trim();

        NotifierHelper.Show(NotifierType.Information, $"正在查询玩家 {QueryModel.PlayerName} 数据中...");

        var result = await EA2API.GetPlayerPersonaId(Globals.AccessToken, QueryModel.PlayerName);
        if (result.IsSuccess)
        {
            var jNode = JsonNode.Parse(result.Content);
            if (jNode["personas"]!["persona"] != null)
            {
                var personaId = jNode["personas"]!["persona"][0]["personaId"].GetValue<long>();
                QueryPlayerRecord(personaId);
            }
            else
            {
                QueryModel.IsLoading = false;
                NotifierHelper.Show(NotifierType.Warning, $"玩家 {QueryModel.PlayerName} 不存在");
            }
        }
        else
        {
            QueryModel.IsLoading = false;
            NotifierHelper.Show(NotifierType.Error, $"获取玩家PersonaId失败\n{result.Content}");
        }
    }

    /// <summary>
    /// 查询前清理面板数据
    /// </summary>
    private void ClearUiData()
    {
        QueryModel.Avatar = string.Empty;

        QueryModel.Rank = string.Empty;
        QueryModel.PlayTime = string.Empty;

        QueryModel.PlayingServer = string.Empty;

        QueryModel.DisplayName = string.Empty;
        QueryModel.PersonaId = string.Empty;

        ListBox_PlayerDatas.Clear();
        ListBox_WeaponInfos.Clear();
        ListBox_VehicleInfos.Clear();
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
                QueryModel.DisplayName = "玩家ID : " + jNode["result"]![$"{personaId}"]!["displayName"].GetValue<string>();
                QueryModel.PersonaId = "数字ID : " + jNode["result"]![$"{personaId}"]!["personaId"].GetValue<string>();

                QueryModel.Rank = "等级 : NULL";
            }
            else
            {
                return;
            }
        }

        result = await BF1API.GetServersByPersonaIds(Globals.SessionId, personaId);
        if (result.IsSuccess)
        {
            JsonNode jNode = JsonNode.Parse(result.Content);

            var obj = jNode["result"]![$"{personaId}"];
            if (obj != null)
            {
                var name = obj["name"].GetValue<string>();
                QueryModel.PlayingServer = $"正在游玩 : {name}";
            }
            else
            {
                QueryModel.PlayingServer = $"正在游玩 : 无";
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
        this.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
        {
            ListBox_PlayerDatas.Add(str);
        }));
    }
}
