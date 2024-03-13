using BF1ServerTools.API;
using BF1ServerTools.RES;
using BF1ServerTools.RES.Data;
using BF1ServerTools.Data;
using BF1ServerTools.Utils;
using BF1ServerTools.Helper;
using BF1ServerTools.Configs;

namespace BF1ServerTools.Views;

/// <summary>
/// RuleView.xaml 的交互逻辑
/// </summary>
public partial class RuleView : UserControl
{
    /// <summary>
    /// Auth配置文件路径
    /// </summary>
    private readonly string F_Rule_Path = FileUtil.D_Config_Path + @"\RuleConfig.json";

    /// <summary>
    /// Rule配置文件，以json格式保存到本地
    /// </summary>
    private RuleConfig RuleConfig = new();

    /// <summary>
    /// 绑定UI 配置文件名称动态集合
    /// </summary>
    public ObservableCollection<string> ConfigNames { get; set; } = new();

    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 绑定UI 队伍1规则集
    /// </summary>
    public RuleTeamModel RuleTeam1Model { get; set; } = new();
    /// <summary>
    /// 绑定UI 队伍2规则集
    /// </summary>
    public RuleTeamModel RuleTeam2Model { get; set; } = new();

    ////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 绑定UI 武器数据
    /// </summary>
    public ObservableCollection<RuleWeaponModel> DataGrid_RuleWeaponModels { get; set; } = new();

    /// <summary>
    /// 绑定UI 规则日志
    /// </summary>
    public ObservableCollection<RuleLog> DataGrid_RuleLogs { get; set; } = new();

    public RuleView()
    {
        InitializeComponent();
        this.DataContext = this;
        MainWindow.WindowClosingEvent += MainWindow_WindowClosingEvent;

        // 添加武器数据列表
        foreach (var item in WeaponData.AllWeaponInfo)
        {
            DataGrid_RuleWeaponModels.Add(new()
            {
                Kind = item.Kind,
                Name = item.Chinese,
                English = item.English,
                Image = ClientHelper.GetWeaponImagePath(item.English),
                Team1 = false,
                Team2 = false
            });
        }
        // 如果配置文件不存在就创建
        if (!File.Exists(F_Rule_Path))
        {
            RuleConfig.SelectedIndex = 0;
            RuleConfig.RuleInfos = new();
            // 初始化10个配置文件槽
            for (int i = 0; i < 10; i++)
            {
                RuleConfig.RuleInfos.Add(new()
                {
                    RuleName = $"自定义规则 {i}",
                    WhiteLifeKD = true,
                    WhiteLifeKPM = true,
                    WhiteLifeWeaponStar = true,
                    WhiteLifeVehicleStar = true,
                    WhiteKill = true,
                    WhiteKD = true,
                    WhiteKPM = true,
                    WhiteRank = true,
                    WhiteWeapon = true,
                    Team1Rule = new(),
                    Team2Rule = new(),
                    Team1Weapon = new(),
                    Team2Weapon = new(),
                    BlackList = new(),
                    WhiteList = new()
                });
            }
            // 保存配置文件
            SaveConfig();
        }

        // 如果配置文件存在就读取
        if (File.Exists(F_Rule_Path))
        {
            using var streamReader = new StreamReader(F_Rule_Path);
            RuleConfig = JsonHelper.JsonDese<RuleConfig>(streamReader.ReadToEnd());
            // 读取配置文件名称
            foreach (var item in RuleConfig.RuleInfos)
                ConfigNames.Add(item.RuleName);
            // 读取选中配置文件索引
            ComboBox_ConfigNames.SelectedIndex = RuleConfig.SelectedIndex;
        }
    }

    private void MainWindow_WindowClosingEvent()
    {
        SaveConfig();
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    private void SaveConfig()
    {
        // 更新当前授权信息
        var index = ComboBox_ConfigNames.SelectedIndex;
        if (index != -1)
        {
            RuleConfig.SelectedIndex = index;

            var rule = RuleConfig.RuleInfos[index];

            rule.WhiteLifeKD = CheckBox_WhiteLifeKD.IsChecked == true;
            rule.WhiteLifeKPM = CheckBox_WhiteLifeKPM.IsChecked == true;
            rule.WhiteLifeWeaponStar = CheckBox_WhiteLifeWeaponStar.IsChecked == true;
            rule.WhiteLifeVehicleStar = CheckBox_WhiteLifeVehicleStar.IsChecked == true;
            rule.WhiteKill = CheckBox_WhiteKill.IsChecked == true;
            rule.WhiteKD = CheckBox_WhiteKD.IsChecked == true;
            rule.WhiteKPM = CheckBox_WhiteKPM.IsChecked == true;
            rule.WhiteRank = CheckBox_WhiteRank.IsChecked == true;
            rule.WhiteWeapon = CheckBox_WhiteWeapon.IsChecked == true;

            rule.Team1Rule.MaxKill = RuleTeam1Model.MaxKill;
            rule.Team1Rule.FlagKD = RuleTeam1Model.FlagKD;
            rule.Team1Rule.MaxKD = RuleTeam1Model.MaxKD;
            rule.Team1Rule.FlagKPM = RuleTeam1Model.FlagKPM;
            rule.Team1Rule.MaxKPM = RuleTeam1Model.MaxKPM;
            rule.Team1Rule.MinRank = RuleTeam1Model.MinRank;
            rule.Team1Rule.MaxRank = RuleTeam1Model.MaxRank;
            rule.Team1Rule.LifeMaxKD = RuleTeam1Model.LifeMaxKD;
            rule.Team1Rule.LifeMaxKPM = RuleTeam1Model.LifeMaxKPM;
            rule.Team1Rule.LifeMaxWeaponStar = RuleTeam1Model.LifeMaxWeaponStar;
            rule.Team1Rule.LifeMaxVehicleStar = RuleTeam1Model.LifeMaxVehicleStar;

            rule.Team2Rule.MaxKill = RuleTeam2Model.MaxKill;
            rule.Team2Rule.FlagKD = RuleTeam2Model.FlagKD;
            rule.Team2Rule.MaxKD = RuleTeam2Model.MaxKD;
            rule.Team2Rule.FlagKPM = RuleTeam2Model.FlagKPM;
            rule.Team2Rule.MaxKPM = RuleTeam2Model.MaxKPM;
            rule.Team2Rule.MinRank = RuleTeam2Model.MinRank;
            rule.Team2Rule.MaxRank = RuleTeam2Model.MaxRank;
            rule.Team2Rule.LifeMaxKD = RuleTeam2Model.LifeMaxKD;
            rule.Team2Rule.LifeMaxKPM = RuleTeam2Model.LifeMaxKPM;
            rule.Team2Rule.LifeMaxWeaponStar = RuleTeam2Model.LifeMaxWeaponStar;
            rule.Team2Rule.LifeMaxVehicleStar = RuleTeam2Model.LifeMaxVehicleStar;

            rule.WhiteList.Clear();
            foreach (string name in ListBox_CustomWhites.Items)
            {
                rule.WhiteList.Add(name);
            }

            rule.BlackList.Clear();
            foreach (string name in ListBox_CustomBlacks.Items)
            {
                rule.BlackList.Add(name);
            }

            rule.Team1Weapon.Clear();
            rule.Team2Weapon.Clear();
            for (int i = 0; i < DataGrid_RuleWeaponModels.Count; i++)
            {
                var item = DataGrid_RuleWeaponModels[i];
                if (item.Team1)
                    rule.Team1Weapon.Add(item.English);

                if (item.Team2)
                    rule.Team2Weapon.Add(item.English);
            }
        }

        File.WriteAllText(F_Rule_Path, JsonHelper.JsonSeri(RuleConfig));
    }

    /// <summary>
    /// ComboBox选中项变更事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ComboBox_ConfigNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var index = ComboBox_ConfigNames.SelectedIndex;
        if (index == -1)
            return;

        var rule = RuleConfig.RuleInfos[index];
        // 白名单特权
        CheckBox_WhiteLifeKD.IsChecked = rule.WhiteLifeKD;
        CheckBox_WhiteLifeKPM.IsChecked = rule.WhiteLifeKPM;
        CheckBox_WhiteLifeWeaponStar.IsChecked = rule.WhiteLifeWeaponStar;
        CheckBox_WhiteLifeVehicleStar.IsChecked = rule.WhiteLifeVehicleStar;
        CheckBox_WhiteKill.IsChecked = rule.WhiteKill;
        CheckBox_WhiteKD.IsChecked = rule.WhiteKD;
        CheckBox_WhiteKPM.IsChecked = rule.WhiteKPM;
        CheckBox_WhiteRank.IsChecked = rule.WhiteRank;
        CheckBox_WhiteWeapon.IsChecked = rule.WhiteWeapon;
        // 应用队伍1规则
        RuleTeam1Model.MaxKill = rule.Team1Rule.MaxKill;
        RuleTeam1Model.FlagKD = rule.Team1Rule.FlagKD;
        RuleTeam1Model.MaxKD = rule.Team1Rule.MaxKD;
        RuleTeam1Model.FlagKPM = rule.Team1Rule.FlagKPM;
        RuleTeam1Model.MaxKPM = rule.Team1Rule.MaxKPM;
        RuleTeam1Model.MinRank = rule.Team1Rule.MinRank;
        RuleTeam1Model.MaxRank = rule.Team1Rule.MaxRank;
        RuleTeam1Model.LifeMaxKD = rule.Team1Rule.LifeMaxKD;
        RuleTeam1Model.LifeMaxKPM = rule.Team1Rule.LifeMaxKPM;
        RuleTeam1Model.LifeMaxWeaponStar = rule.Team1Rule.LifeMaxWeaponStar;
        RuleTeam1Model.LifeMaxVehicleStar = rule.Team1Rule.LifeMaxVehicleStar;
        // 应用队伍2规则
        RuleTeam2Model.MaxKill = rule.Team2Rule.MaxKill;
        RuleTeam2Model.FlagKD = rule.Team2Rule.FlagKD;
        RuleTeam2Model.MaxKD = rule.Team2Rule.MaxKD;
        RuleTeam2Model.FlagKPM = rule.Team2Rule.FlagKPM;
        RuleTeam2Model.MaxKPM = rule.Team2Rule.MaxKPM;
        RuleTeam2Model.MinRank = rule.Team2Rule.MinRank;
        RuleTeam2Model.MaxRank = rule.Team2Rule.MaxRank;
        RuleTeam2Model.LifeMaxKD = rule.Team2Rule.LifeMaxKD;
        RuleTeam2Model.LifeMaxKPM = rule.Team2Rule.LifeMaxKPM;
        RuleTeam2Model.LifeMaxWeaponStar = rule.Team2Rule.LifeMaxWeaponStar;
        RuleTeam2Model.LifeMaxVehicleStar = rule.Team2Rule.LifeMaxVehicleStar;

        // 读取白名单列表
        ListBox_CustomWhites.Items.Clear();
        foreach (var item in rule.WhiteList)
        {
            ListBox_CustomWhites.Items.Add(item);
        }

        // 读取黑名单列表
        ListBox_CustomBlacks.Items.Clear();
        foreach (var item in rule.BlackList)
        {
            ListBox_CustomBlacks.Items.Add(item);
        }

        // 读取武器限制信息
        for (int i = 0; i < DataGrid_RuleWeaponModels.Count; i++)
        {
            var item = DataGrid_RuleWeaponModels[i];

            var v1 = rule.Team1Weapon.IndexOf(item.English);
            if (v1 != -1)
                item.Team1 = true;
            else
                item.Team1 = false;

            var v2 = rule.Team2Weapon.IndexOf(item.English);
            if (v2 != -1)
                item.Team2 = true;
            else
                item.Team2 = false;
        }

        SaveConfig();
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_SaveConfig_Click(object sender, RoutedEventArgs e)
    {
        SaveConfig();
        NotifierHelper.Show(NotifierType.Success, "保存配置文件成功");
    }

    /// <summary>
    /// 当前配置文件重命名
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_ReNameCurrentConfig_Click(object sender, RoutedEventArgs e)
    {
        var name = TextBox_CurrentConfigName.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            NotifierHelper.Show(NotifierType.Warning, "配置文件名称不能为空");
            return;
        }

        var index = ComboBox_ConfigNames.SelectedIndex;
        if (index == -1)
        {
            NotifierHelper.Show(NotifierType.Warning, "请选择正确的配置文件");
            return;
        }

        ConfigNames[index] = name;
        RuleConfig.RuleInfos[index].RuleName = name;

        ComboBox_ConfigNames.SelectedIndex = index;
        NotifierHelper.Show(NotifierType.Success, "当前配置文件重命名成功");
    }

    ////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 打印规则日志
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="t1Value"></param>
    /// <param name="t2Value"></param>
    private void AddRuleLog(string Name, string t1Value = "", string t2Value = "")
    {
        DataGrid_RuleLogs.Add(new RuleLog()
        {
            Name = Name,
            T1Value = t1Value,
            T2Value = t2Value
        });
    }

    /// <summary>
    /// 清空规则日志
    /// </summary>
    private void ClearRuleLog()
    {
        DataGrid_RuleLogs.Clear();
    }

    /// <summary>
    /// 应用并查询当前规则
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_ApplyAndQueryCurrentRule_Click(object sender, RoutedEventArgs e)
    {
        ClearRuleLog();

        // 重置状态
        Globals.IsSetRuleOK = false;
        Globals.AutoKickBreakRulePlayer = false;

        #region 应用当前规则
        Globals.WhiteLifeKD = CheckBox_WhiteLifeKD.IsChecked == true;
        Globals.WhiteLifeKPM = CheckBox_WhiteLifeKPM.IsChecked == true;
        Globals.WhiteLifeWeaponStar = CheckBox_WhiteLifeWeaponStar.IsChecked == true;
        Globals.WhiteLifeVehicleStar = CheckBox_WhiteLifeVehicleStar.IsChecked == true;
        Globals.WhiteKill = CheckBox_WhiteKill.IsChecked == true;
        Globals.WhiteKD = CheckBox_WhiteKD.IsChecked == true;
        Globals.WhiteKPM = CheckBox_WhiteKPM.IsChecked == true;
        Globals.WhiteRank = CheckBox_WhiteRank.IsChecked == true;
        Globals.WhiteWeapon = CheckBox_WhiteWeapon.IsChecked == true;

        Globals.ServerRule_Team1.MaxKill = RuleTeam1Model.MaxKill;
        Globals.ServerRule_Team1.FlagKD = RuleTeam1Model.FlagKD;
        Globals.ServerRule_Team1.MaxKD = RuleTeam1Model.MaxKD;
        Globals.ServerRule_Team1.FlagKPM = RuleTeam1Model.FlagKPM;
        Globals.ServerRule_Team1.MaxKPM = RuleTeam1Model.MaxKPM;
        Globals.ServerRule_Team1.MinRank = RuleTeam1Model.MinRank;
        Globals.ServerRule_Team1.MaxRank = RuleTeam1Model.MaxRank;

        Globals.ServerRule_Team1.LifeMaxKD = RuleTeam1Model.LifeMaxKD;
        Globals.ServerRule_Team1.LifeMaxKPM = RuleTeam1Model.LifeMaxKPM;
        Globals.ServerRule_Team1.LifeMaxWeaponStar = RuleTeam1Model.LifeMaxWeaponStar;
        Globals.ServerRule_Team1.LifeMaxVehicleStar = RuleTeam1Model.LifeMaxVehicleStar;

        Globals.ServerRule_Team2.MaxKill = RuleTeam2Model.MaxKill;
        Globals.ServerRule_Team2.FlagKD = RuleTeam2Model.FlagKD;
        Globals.ServerRule_Team2.MaxKD = RuleTeam2Model.MaxKD;
        Globals.ServerRule_Team2.FlagKPM = RuleTeam2Model.FlagKPM;
        Globals.ServerRule_Team2.MaxKPM = RuleTeam2Model.MaxKPM;
        Globals.ServerRule_Team2.MinRank = RuleTeam2Model.MinRank;
        Globals.ServerRule_Team2.MaxRank = RuleTeam2Model.MaxRank;

        Globals.ServerRule_Team2.LifeMaxKD = RuleTeam2Model.LifeMaxKD;
        Globals.ServerRule_Team2.LifeMaxKPM = RuleTeam2Model.LifeMaxKPM;
        Globals.ServerRule_Team2.LifeMaxWeaponStar = RuleTeam2Model.LifeMaxWeaponStar;
        Globals.ServerRule_Team2.LifeMaxVehicleStar = RuleTeam2Model.LifeMaxVehicleStar;

        /////////////////////////////////////////////////////////////////////////////

        // 检查队伍1等级限制
        if (Globals.ServerRule_Team1.MinRank >= Globals.ServerRule_Team1.MaxRank && Globals.ServerRule_Team1.MinRank != 0 && Globals.ServerRule_Team1.MaxRank != 0)
        {
            Globals.IsSetRuleOK = false;

            NotifierHelper.Show(NotifierType.Warning, "队伍1 限制等级规则设置不正确");
            return;
        }
        // 检查队伍2等级限制
        if (Globals.ServerRule_Team2.MinRank >= Globals.ServerRule_Team2.MaxRank && Globals.ServerRule_Team2.MinRank != 0 && Globals.ServerRule_Team2.MaxRank != 0)
        {
            Globals.IsSetRuleOK = false;

            NotifierHelper.Show(NotifierType.Warning, "队伍2 限制等级规则设置不正确");
            return;
        }

        /////////////////////////////////////////////////////////////////////////////

        // 清空限制武器列表
        Globals.CustomWeapons_Team1.Clear();
        Globals.CustomWeapons_Team2.Clear();
        // 添加自定义限制武器
        foreach (var item in DataGrid_RuleWeaponModels)
        {
            if (item.Team1)
                Globals.CustomWeapons_Team1.Add(item.English);

            if (item.Team2)
                Globals.CustomWeapons_Team2.Add(item.English);
        }

        // 清空白名单列表
        Globals.CustomWhites_Name.Clear();
        // 添加自定义白名单列表
        foreach (string name in ListBox_CustomWhites.Items)
        {
            Globals.CustomWhites_Name.Add(name);
        }

        // 清空黑名单列表
        Globals.CustomBlacks_Name.Clear();
        // 添加自定义黑名单列表
        foreach (string name in ListBox_CustomBlacks.Items)
        {
            Globals.CustomBlacks_Name.Add(name);
        }

        Globals.IsSetRuleOK = true;
        #endregion

        AddRuleLog("【当局规则】");
        AddRuleLog("最高击杀", $"{Globals.ServerRule_Team1.MaxKill}", $"{Globals.ServerRule_Team2.MaxKill}");

        AddRuleLog("KD阈值", $"{Globals.ServerRule_Team1.FlagKD}", $"{Globals.ServerRule_Team2.FlagKD}");
        AddRuleLog("最高KD", $"{Globals.ServerRule_Team1.MaxKD}", $"{Globals.ServerRule_Team2.MaxKD}");

        AddRuleLog("KPM阈值", $"{Globals.ServerRule_Team1.FlagKPM}", $"{Globals.ServerRule_Team2.FlagKPM}");
        AddRuleLog("最高KPM", $"{Globals.ServerRule_Team1.MaxKPM}", $"{Globals.ServerRule_Team2.MaxKPM}");

        AddRuleLog("最低等级", $"{Globals.ServerRule_Team1.MinRank}", $"{Globals.ServerRule_Team2.MinRank}");
        AddRuleLog("最高等级", $"{Globals.ServerRule_Team1.MaxRank}", $"{Globals.ServerRule_Team2.MaxRank}");

        AddRuleLog("【生涯规则】");
        AddRuleLog("生涯KD", $"{Globals.ServerRule_Team1.LifeMaxKD}", $"{Globals.ServerRule_Team2.LifeMaxKD}");
        AddRuleLog("生涯KPM", $"{Globals.ServerRule_Team1.LifeMaxKPM}", $"{Globals.ServerRule_Team2.LifeMaxKPM}");

        AddRuleLog("武器星数", $"{Globals.ServerRule_Team1.LifeMaxWeaponStar}", $"{Globals.ServerRule_Team2.LifeMaxWeaponStar}");
        AddRuleLog("载具星数", $"{Globals.ServerRule_Team1.LifeMaxVehicleStar}", $"{Globals.ServerRule_Team2.LifeMaxVehicleStar}");

        AddRuleLog("【禁用武器】");
        int team1 = Globals.CustomWeapons_Team1.Count;
        int team2 = Globals.CustomWeapons_Team2.Count;
        for (int i = 0; i < Math.Max(team1, team2); i++)
        {
            if (i < team1 && i < team2)
            {
                AddRuleLog($"武器名称 {i + 1}", $"{ClientHelper.GetWeaponChsName(Globals.CustomWeapons_Team1[i])}", $"{ClientHelper.GetWeaponChsName(Globals.CustomWeapons_Team2[i])}");
            }
            else if (i < team1)
            {
                AddRuleLog($"武器名称 {i + 1}", $"{ClientHelper.GetWeaponChsName(Globals.CustomWeapons_Team1[i])}");
            }
            else if (i < team2)
            {
                AddRuleLog($"武器名称 {i + 1}", "", $"{ClientHelper.GetWeaponChsName(Globals.CustomWeapons_Team2[i])}");
            }
        }

        AddRuleLog("【白名单特权】");
        if (Globals.WhiteLifeKD)
            AddRuleLog("", "免疫生涯KD限制");
        if (Globals.WhiteLifeKPM)
            AddRuleLog("", "免疫生涯KPM限制");
        if (Globals.WhiteLifeWeaponStar)
            AddRuleLog("", "免疫生涯武器星数限制");
        if (Globals.WhiteLifeVehicleStar)
            AddRuleLog("", "免疫生涯载具星数限制");
        if (Globals.WhiteKill)
            AddRuleLog("", "免疫击杀限制");
        if (Globals.WhiteKD)
            AddRuleLog("", "免疫KD限制");
        if (Globals.WhiteKPM)
            AddRuleLog("", "免疫KPM限制");
        if (Globals.WhiteRank)
            AddRuleLog("", "免疫等级限制");
        if (Globals.WhiteWeapon)
            AddRuleLog("", "免疫武器限制");

        int index = 1;
        AddRuleLog("【白名单列表】");
        foreach (var item in Globals.CustomWhites_Name)
        {
            AddRuleLog($"玩家ID {index++}", $"{item}");
        }

        index = 1;
        AddRuleLog("【黑名单列表】");
        foreach (var item in Globals.CustomBlacks_Name)
        {
            AddRuleLog($"玩家ID {index++}", $"{item}");
        }

        NotifierHelper.Show(NotifierType.Success, "查询当前规则成功");
    }

    /// <summary>
    /// 从白名单列表移除选中玩家
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_RemoveSelectedWhite_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_CustomWhites.SelectedItem is string name)
        {
            ListBox_CustomWhites.Items.Remove(name);

            NotifierHelper.Show(NotifierType.Success, $"从白名单列表移除玩家 {name} 成功");
        }
    }

    /// <summary>
    /// 添加玩家到白名单列表
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_AddNewWhite_Click(object sender, RoutedEventArgs e)
    {
        var name = TextBox_NewWhiteName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            NotifierHelper.Show(NotifierType.Warning, "请输入正确的玩家名称");
            return;
        }

        ListBox_CustomWhites.Items.Add(name);
        TextBox_NewWhiteName.Clear();

        NotifierHelper.Show(NotifierType.Success, $"添加玩家 {name} 到白名单列表成功");
    }

    /// <summary>
    /// 从黑名单列表移除选中玩家
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_RemoveSelectedBlack_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_CustomBlacks.SelectedItem is string name)
        {
            ListBox_CustomBlacks.Items.Remove(name);

            NotifierHelper.Show(NotifierType.Success, $"从黑名单列表移除玩家 {name} 成功");
        }
    }

    /// <summary>
    /// 添加玩家到黑名单列表
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_AddNewBlack_Click(object sender, RoutedEventArgs e)
    {
        var name = TextBox_NewBlackName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            NotifierHelper.Show(NotifierType.Warning, "请输入正确的玩家名称");
            return;
        }

        ListBox_CustomBlacks.Items.Add(name);
        TextBox_NewBlackName.Clear();

        NotifierHelper.Show(NotifierType.Success, $"添加玩家 {name} 到黑名单列表成功");
    }

    ///////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 导入白名单列表
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItem_ImportCustomWhites_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var fileDialog = new OpenFileDialog
            {
                Title = "批量导入白名单列表",
                RestoreDirectory = true,
                Multiselect = false,
                Filter = "文本文档|*.txt"
            };

            if (fileDialog.ShowDialog() == true)
            {
                ListBox_CustomWhites.Items.Clear();
                foreach (var item in File.ReadAllLines(fileDialog.FileName))
                {
                    if (!string.IsNullOrWhiteSpace(item))
                        ListBox_CustomWhites.Items.Add(item);
                }

                NotifierHelper.Show(NotifierType.Success, "批量导入txt文件到白名单列表成功");
            }
        }
        catch (Exception ex)
        {
            NotifierHelper.ShowException(ex);
        }
    }

    /// <summary>
    /// 导出白名单列表
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItem_ExportCustomWhites_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_CustomWhites.Items.IsEmpty)
        {
            NotifierHelper.Show(NotifierType.Warning, "白名单列表为空，导出操作取消");
            return;
        }

        try
        {
            var fileDialog = new SaveFileDialog
            {
                Title = "批量导出白名单列表",
                RestoreDirectory = true,
                Filter = "文本文档|*.txt"
            };

            if (fileDialog.ShowDialog() == true)
            {
                File.WriteAllText(fileDialog.FileName, string.Join(Environment.NewLine, ListBox_CustomWhites.Items.Cast<string>()));

                NotifierHelper.Show(NotifierType.Success, "批量导出白名单列表到txt文件成功");
            }
        }
        catch (Exception ex)
        {
            NotifierHelper.ShowException(ex);
        }
    }

    /// <summary>
    /// 白名单列表去重
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItem_DistinctCustomWhites_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_CustomWhites.Items.IsEmpty)
        {
            NotifierHelper.Show(NotifierType.Warning, "白名单列表为空，去重操作取消");
            return;
        }

        List<string> tempStr = new();
        foreach (string item in ListBox_CustomWhites.Items)
            tempStr.Add(item);
        ListBox_CustomWhites.Items.Clear();
        foreach (var item in tempStr.Distinct().ToList())
            ListBox_CustomWhites.Items.Add(item);

        NotifierHelper.Show(NotifierType.Success, "白名单列表去重成功");
    }

    /// <summary>
    /// 清空白名单列表
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItem_ClearCustomWhites_Click(object sender, RoutedEventArgs e)
    {
        ListBox_CustomWhites.Items.Clear();
        NotifierHelper.Show(NotifierType.Success, "清空白名单列表成功");
    }

    /// <summary>
    /// 导入黑名单列表
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItem_ImportCustomBlacks_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var fileDialog = new OpenFileDialog
            {
                Title = "批量导入黑名单列表",
                RestoreDirectory = true,
                Multiselect = false,
                Filter = "文本文档|*.txt"
            };

            if (fileDialog.ShowDialog() == true)
            {
                ListBox_CustomBlacks.Items.Clear();
                foreach (var item in File.ReadAllLines(fileDialog.FileName))
                {
                    if (!string.IsNullOrWhiteSpace(item))
                        ListBox_CustomBlacks.Items.Add(item);
                }

                NotifierHelper.Show(NotifierType.Success, "批量导入txt文件到黑名单列表成功");
            }
        }
        catch (Exception ex)
        {
            NotifierHelper.ShowException(ex);
        }
    }

    /// <summary>
    /// 导出黑名单列表
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItem_ExportCustomBlacks_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_CustomBlacks.Items.IsEmpty)
        {
            NotifierHelper.Show(NotifierType.Warning, "黑名单列表为空，导出操作取消");
            return;
        }

        try
        {
            var fileDialog = new SaveFileDialog
            {
                Title = "批量导出黑名单列表",
                RestoreDirectory = true,
                Filter = "文本文档|*.txt"
            };

            if (fileDialog.ShowDialog() == true)
            {
                File.WriteAllText(fileDialog.FileName, string.Join(Environment.NewLine, ListBox_CustomBlacks.Items.Cast<string>()));

                NotifierHelper.Show(NotifierType.Success, "批量导出黑名单列表到txt文件成功");
            }
        }
        catch (Exception ex)
        {
            NotifierHelper.ShowException(ex);
        }
    }

    /// <summary>
    /// 黑名单列表去重
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItem_DistinctCustomBlacks_Click(object sender, RoutedEventArgs e)
    {
        if (ListBox_CustomBlacks.Items.IsEmpty)
        {
            NotifierHelper.Show(NotifierType.Warning, "黑名单列表为空，去重操作取消");
            return;
        }

        List<string> tempStr = new();
        foreach (string item in ListBox_CustomBlacks.Items)
            tempStr.Add(item);
        ListBox_CustomBlacks.Items.Clear();
        foreach (var item in tempStr.Distinct().ToList())
            ListBox_CustomBlacks.Items.Add(item);

        NotifierHelper.Show(NotifierType.Success, "黑名单列表去重成功");
    }

    /// <summary>
    /// 清空黑名单列表
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItem_ClearCustomBlacks_Click(object sender, RoutedEventArgs e)
    {
        ListBox_CustomBlacks.Items.Clear();
        NotifierHelper.Show(NotifierType.Success, "清空黑名单列表成功");
    }
}
