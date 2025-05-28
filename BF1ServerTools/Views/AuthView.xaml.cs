using BF1ServerTools.API;
using BF1ServerTools.API.RespJson;
using BF1ServerTools.SDK;
using BF1ServerTools.Utils;
using BF1ServerTools.Models;
using BF1ServerTools.Helper;
using BF1ServerTools.Configs;
using BF1ServerTools.Windows;

using Microsoft.Web.WebView2.Core;
using CommunityToolkit.Mvvm.Messaging;
using System.Xml.Linq;
using System;
using Newtonsoft.Json;
using System.Reflection.Metadata;
using BF1ServerTools.RES;
using BF1ServerTools.SDK.Data;
using Newtonsoft.Json.Serialization;
using System.Net.Http;
using System.ComponentModel;
namespace BF1ServerTools.Views;
public class Config
{
    public string ServerUrl { get; set; }
}

/// <summary>
/// AuthView.xaml 的交互逻辑
/// </summary>
public partial class AuthView : UserControl
{
    /// <summary>
    /// 数据模型绑定
    /// </summary>
    public AuthModel AuthModel { get; set; } = new();

    /// <summary>
    /// 配置文件路径
    /// </summary>
    private readonly string F_Auth_Path = BF1ServerTools.Utils.FileUtil.D_Config_Path + @"\AuthConfig.json";

    /// <summary>
    /// 刷分自动换边
    /// </summary>
    public static bool scorechangesite = false;
    /// <summary>
    /// 配置文件，以json格式保存到本地
    /// </summary>
    private AuthConfig AuthConfig = new();

   public static readonly string F_Auth_Path2 = BF1ServerTools.Utils.FileUtil.D_Config_Path + @"\Scorelist.json";

    public static readonly string F_Auth_Path3 = BF1ServerTools.Utils.FileUtil.D_Config_Path + @"\Loadlist.json";
    // 配置文件路径
    private readonly string F_Auth_Path4 = BF1ServerTools.Utils.FileUtil.D_Config_Path + @"\APIConfig.json";
    // 固定5个槽位，用于保存 URL
    private string[] urls = new string[5];
    // 默认 URL
    private const string defaultUrl = "https://data.bf1robot.com/rec/tool/game/data";
    private void TextBox_MessageContent_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBox textBox = sender as TextBox;
        SaveInput(textBox.Text);
    }
    private string lastInput = ""; 
    public static long gameid233 = 0 ;
    public static string URL = "";
    private void SaveInput(string input)
    {
        if (lastInput != input)
        {
            lastInput = input;
            string url = $"https://api.gametools.network/bf1/players/?gameid={Uri.EscapeDataString(input)}";
            
            long.TryParse(lastInput, out gameid233);
            
            URL = url ;
            
        }
    }

    /// <summary>
    /// 配置文件名称动态集合
    /// </summary>
    public ObservableCollection<string> ConfigNames { get; set; } = new();

    public AuthView()
    {
        InitializeComponent();

        this.DataContext = this;
        MainWindow.WindowClosingEvent += MainWindow_WindowClosingEvent;

        #region 配置文件
        // 如果配置文件不存在就创建
        if (!File.Exists(F_Auth_Path))
        {
            AuthConfig.IsUseMode1 = true;
            AuthConfig.SelectedIndex = 0;
            AuthConfig.AuthInfos = new();
            // 初始化10个配置文件槽
            for (int i = 0; i < 10; i++)
            {
                AuthConfig.AuthInfos.Add(new()
                {
                    Avatar2 = "",
                    DisplayName2 = $"配置槽名称 {i + 1}",
                    PersonaId2 = 0,
                    Sid = "",
                    Remid = "",
                    AccessToken = "",
                    SessionId2 = ""
                });
            }
            // 保存配置文件
            SaveConfig();
        }

        // 如果配置文件存在就读取
        if (File.Exists(F_Auth_Path))
        {
            using StreamReader streamReader = new(F_Auth_Path);
            AuthConfig = JsonHelper.JsonDese<AuthConfig>(streamReader.ReadToEnd());
            // 读取配置文件名称
            foreach (var item in AuthConfig.AuthInfos)
                ConfigNames.Add(item.DisplayName2);
            // 读取选中配置文件索引
            ComboBox_ConfigNames.SelectedIndex = AuthConfig.SelectedIndex;
        }
        #endregion
        // 设置默认选择第一个槽位（确保 ComboBox_Slot 已加载）
        if (ComboBox_Slot != null)
        {
            ComboBox_Slot.SelectedIndex = 0;
        }

        // 加载配置文件（如果不存在则填充为默认 URL）
        LoadConfig();
        /////////////////////////////////////////////////////////////////////

        // 用于接收WebView2传回的数据
        WeakReferenceMessenger.Default.Register<string, string>(this, "SendRemidSid", (s, e) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                AuthModel.Avatar2 = Globals.Avatar2;
                AuthModel.DisplayName2 = Globals.DisplayName2;
                AuthModel.PersonaId2 = Globals.PersonaId2;

                AuthModel.Sid = Globals.Sid;
                AuthModel.Remid = Globals.Remid;
                AuthModel.AccessToken = Globals.AccessToken;
                AuthModel.SessionId2 = Globals.SessionId2;

                SaveConfig();
            });
        });

        // 模式1 定时内存扫描SessionId 周期5分钟
        var AutoRefreshTimerModel1 = new Timer
        {
            AutoReset = true,
            Interval = TimeSpan.FromMinutes(5).TotalMilliseconds
        };
        AutoRefreshTimerModel1.Elapsed += AutoRefreshTimerModel1_Elapsed;
        AutoRefreshTimerModel1.Start();

        // 模式2 定时更新玩家Cookies 周期30分钟
        var AutoRefreshTimerModel2 = new Timer
        {
            AutoReset = true,
            Interval = TimeSpan.FromMinutes(30).TotalMilliseconds
        };
        AutoRefreshTimerModel2.Elapsed += AutoRefreshTimerModel2_Elapsed;
        AutoRefreshTimerModel2.Start();

        ////////////////////////////////////////////

        if (Globals.IsUseMode1)
            AutoRefreshTimerModel1_Elapsed(null, null);
        else
            AutoRefreshTimerModel2_Elapsed(null, null);

    }

    /// <summary>
    /// 加载 JSON 配置文件，如果不存在或格式不正确，则填充为默认配置
    /// </summary>
    private void LoadConfig()
    {
        if (File.Exists(F_Auth_Path4))
        {
            try
            {
                string json = File.ReadAllText(F_Auth_Path4);
                urls = JsonConvert.DeserializeObject<string[]>(json);
                if (urls == null || urls.Length != 5)
                {
                    // 若数组为空或长度不为5，则创建默认配置
                    CreateDefaultConfig();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载配置文件失败: " + ex.Message);
                CreateDefaultConfig();
            }
        }
        else
        {
            CreateDefaultConfig();
        }
        UpdateSlotConfigDisplay();
    }

    /// <summary>
    /// 填充 5 个默认 URL，并保存配置
    /// </summary>
    private void CreateDefaultConfig()
    {
        for (int i = 0; i < 5; i++)
        {
            urls[i] = defaultUrl;
        }
        SaveConfig2();
    }

    /// <summary>
    /// 将当前的 5 个槽位 URL 配置保存到 JSON 文件
    /// </summary>
    private void SaveConfig2()
    {
        try
        {
            string json = JsonConvert.SerializeObject(urls, Formatting.Indented);
            File.WriteAllText(F_Auth_Path4, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show("保存配置文件失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 更新 ListBox 显示当前槽位配置情况
    /// </summary>
    private void UpdateSlotConfigDisplay()
    {
        if (ListBox_SlotConfig != null)
        {
            ListBox_SlotConfig.Items.Clear();
            for (int i = 0; i < 5; i++)
            {
                ListBox_SlotConfig.Items.Add($"槽位{i + 1}: {urls[i]}");
            }
        }
    }

    /// <summary>
    /// 点击“保存槽位URL”按钮后，更新选定槽位的 URL 并保存到文件
    /// </summary>
    private void Button_SaveSlotUrl_Click(object sender, RoutedEventArgs e)
    {
        int slotIndex = ComboBox_Slot.SelectedIndex;
        if (slotIndex < 0 || slotIndex >= 5)
        {
            MessageBox.Show("请选择有效的槽位！");
            return;
        }

        string newUrl = TextBox_SlotUrl.Text.Trim();
        if (string.IsNullOrEmpty(newUrl))
        {
            MessageBox.Show("请输入有效的 URL！");
            return;
        }

        // 弹出确认对话框
        var result = MessageBox.Show(
            $"是否覆盖槽位{slotIndex + 1}的 URL？\n旧 URL: {urls[slotIndex]}\n新 URL: {newUrl}",
            "确认覆盖",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            urls[slotIndex] = newUrl;
            SaveConfig2();
            UpdateSlotConfigDisplay();
            MessageBox.Show("槽位 URL 保存成功！");
        }
    }
    
    /// <summary>
    /// 主窗口关闭事件
    /// </summary>
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
            AuthConfig.IsUseMode1 = Globals.IsUseMode1;
            AuthConfig.SelectedIndex = index;

            var auth = AuthConfig.AuthInfos[index];

            auth.Avatar2 = Globals.Avatar2;
            auth.DisplayName2 = Globals.DisplayName2;
            auth.PersonaId2 = Globals.PersonaId2;
            auth.Sid = Globals.Sid;
            auth.Remid = Globals.Remid;
            auth.AccessToken = Globals.AccessToken;
            auth.SessionId2 = Globals.SessionId2;
        }
        // 写入到Json文件
        File.WriteAllText(F_Auth_Path, JsonHelper.JsonSeri(AuthConfig));
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

        Globals.IsUseMode1 = AuthConfig.IsUseMode1;

        if (Globals.IsUseMode1)
            RadioButton_Mode1.IsChecked = true;
        else
            RadioButton_Mode2.IsChecked = true;

        ///////////////////////////////////////////

        var auth = AuthConfig.AuthInfos[index];

        AuthModel.Avatar2 = auth.Avatar2;
        AuthModel.DisplayName2 = auth.DisplayName2;
        AuthModel.PersonaId2 = auth.PersonaId2;
        AuthModel.Sid = auth.Sid;
        AuthModel.Remid = auth.Remid;
        AuthModel.AccessToken = auth.AccessToken;
        AuthModel.SessionId2 = auth.SessionId2;

        Globals.Avatar2 = auth.Avatar2;
        Globals.DisplayName2 = auth.DisplayName2;
        Globals.PersonaId2 = auth.PersonaId2;
        Globals.Sid = auth.Sid;
        Globals.Remid = auth.Remid;
        Globals.AccessToken = auth.AccessToken;
        Globals.SessionId2 = auth.SessionId2;

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

        var index = ComboBox_ConfigNames.SelectedIndex;
        if (index != -1)
        {
            ConfigNames[index] = Globals.DisplayName2;
            ComboBox_ConfigNames.SelectedIndex = index;
        }

        NotifierHelper.Show(NotifierType.Success, "保存配置文件成功");
    }

    ////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 模式1 定时内存扫描SessionId 周期5分钟
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private async void AutoRefreshTimerModel1_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (!Globals.IsUseMode1)
            return;

        var sessionId = await Scan.GetGatewaySession();
        if (sessionId != string.Empty)
        {
            Globals.SessionId1 = sessionId;
            LoggerHelper.Info($"内存扫描SessionId成功 {Globals.SessionId}");
        }
        else
        {
            if (string.IsNullOrEmpty(Globals.Remid) || string.IsNullOrEmpty(Globals.Sid))
                return;

            var respAuth = await EA1API.GetAuthCode(Globals.Remid, Globals.Sid);
            if (respAuth.IsSuccess)
            {
                if (!string.IsNullOrEmpty(respAuth.Remid))
                    Globals.Remid = respAuth.Remid;
                if (!string.IsNullOrEmpty(respAuth.Sid))
                    Globals.Sid = respAuth.Sid;

                var result = await EA2API.GetAccessToken(Globals.Remid, Globals.Sid);
                if (result.IsSuccess)
                {
                    var jNode = JsonNode.Parse(result.Content);
                    Globals.AccessToken = jNode["access_token"].GetValue<string>();
                    AuthModel.AccessToken = Globals.AccessToken;
                    LoggerHelper.Info("刷新玩家access_token成功");
                }

                result = await BF1API.GetEnvIdViaAuthCode(respAuth.Code);
                if (result.IsSuccess)
                {
                    var content = result.Content;
                    // 使用正则表达式从字符串中提取 sessionId
                    var match = Regex.Match(result.Content, @"sessionId:\s*([a-f0-9-]+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        // 提取并存储到 Globals.SessionId2
                        Globals.SessionId2 = match.Groups[1].Value;
                    }
                    result = await EA2API.GetPlayerPersonaId(Globals.AccessToken, Globals.DisplayName2);
                    if (result.IsSuccess)
                    {
                        var jNode = JsonNode.Parse(result.Content);
                        if (jNode["personas"]!["persona"] != null)
                        {
                            Globals.PersonaId2 = jNode["personas"]!["persona"][0]["personaId"].GetValue<long>();
                        }
                        else
                        {
                            LoggerHelper.Info("personid获取失败");
                        }
                    }
                    else
                    {
                        LoggerHelper.Info("personid获取失败");
                    }
                    //Globals.PersonaId2 = long.Parse(envIdViaAuthCode.result.personaId);

                    result = await BF1API.GetPersonasByIds(Globals.SessionId2, Globals.PersonaId);
                    if (result.IsSuccess)
                    {
                        var jNode = JsonNode.Parse(result.Content);
                        var personas = jNode["result"]![$"{Globals.PersonaId}"];
                        if (personas != null)
                        {
                            Globals.Avatar2 = personas!["avatar"].GetValue<string>();
                            Globals.DisplayName2 = personas!["displayName"].GetValue<string>();

                            AuthModel.Avatar2 = Globals.Avatar2;
                            AuthModel.DisplayName2 = Globals.DisplayName2;
                            AuthModel.PersonaId2 = Globals.PersonaId2;

                            AuthModel.Sid = Globals.Sid;
                            AuthModel.Remid = Globals.Remid;
                            AuthModel.SessionId2 = Globals.SessionId2;

                            Globals.Avatar1 = personas!["avatar"].GetValue<string>();
                            Globals.DisplayName1 = personas!["displayName"].GetValue<string>();

                           

                            Globals.SessionId1 = Globals.SessionId2;
                            LoggerHelper.Info("刷新玩家Cookies数据成功");
                        }
                    }
                }
            }
            //LoggerHelper.Error("内存扫描SessionID失败");
        }
    }

    /// <summary>
    /// 模式2 定时更新玩家Cookies 周期30分钟
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AutoRefreshTimerModel2_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (Globals.IsUseMode1)
            return;

        if (string.IsNullOrEmpty(Globals.Remid) || string.IsNullOrEmpty(Globals.Sid))
            return;

        var respAuth = await EA1API.GetAuthCode(Globals.Remid, Globals.Sid);
        if (respAuth.IsSuccess)
        {
            if (!string.IsNullOrEmpty(respAuth.Remid))
                Globals.Remid = respAuth.Remid;
            if (!string.IsNullOrEmpty(respAuth.Sid))
                Globals.Sid = respAuth.Sid;

            var result = await EA2API.GetAccessToken(Globals.Remid, Globals.Sid);
            if (result.IsSuccess)
            {
                var jNode = JsonNode.Parse(result.Content);
                Globals.AccessToken = jNode["access_token"].GetValue<string>();
                AuthModel.AccessToken = Globals.AccessToken;
                LoggerHelper.Info("刷新玩家access_token成功");
            }
            
            result = await BF1API.GetEnvIdViaAuthCode(respAuth.Code);
            if (result.IsSuccess)
            {
                var content = result.Content;
                // 使用正则表达式从字符串中提取 sessionId
                var match = Regex.Match(result.Content, @"sessionId:\s*([a-f0-9-]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    // 提取并存储到 Globals.SessionId2
                    Globals.SessionId2 = match.Groups[1].Value;
                }
                result = await EA2API.GetPlayerPersonaId(Globals.AccessToken, Globals.DisplayName2);
                if (result.IsSuccess)
                {
                    var jNode = JsonNode.Parse(result.Content);
                    if (jNode["personas"]!["persona"] != null)
                    {
                        Globals.PersonaId2 = jNode["personas"]!["persona"][0]["personaId"].GetValue<long>();
                    }
                    else
                    {
                        LoggerHelper.Info("personid获取失败");
                    }
                }
                else
                {
                    LoggerHelper.Info("personid获取失败");
                }
               //Globals.PersonaId2 = long.Parse(envIdViaAuthCode.result.personaId);

                result = await BF1API.GetPersonasByIds(Globals.SessionId2, Globals.PersonaId);
                if (result.IsSuccess)
                {
                    var jNode = JsonNode.Parse(result.Content);
                    var personas = jNode["result"]![$"{Globals.PersonaId}"];
                    if (personas != null)
                    {
                        Globals.Avatar2 = personas!["avatar"].GetValue<string>();
                        Globals.DisplayName2 = personas!["displayName"].GetValue<string>();

                        AuthModel.Avatar2 = Globals.Avatar2;
                        AuthModel.DisplayName2 = Globals.DisplayName2;
                        AuthModel.PersonaId2 = Globals.PersonaId2;

                        AuthModel.Sid = Globals.Sid;
                        AuthModel.Remid = Globals.Remid;
                        AuthModel.SessionId2 = Globals.SessionId2;

                        LoggerHelper.Info("刷新玩家Cookies数据成功");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 选择工作模式事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RadioButton_Mode12_Click(object sender, RoutedEventArgs e)
    {
        Globals.IsUseMode1 = RadioButton_Mode1.IsChecked == true;
    }

    /// <summary>
    /// 获取玩家Cookies数据
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_GetPlayerCookies_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(CoreWebView2Environment.GetAvailableBrowserVersionString()))
        {
            NotifierHelper.Show(NotifierType.Warning, "未检测到WebView2对应依赖，请安装对应依赖");
            return;
        }

        var webView2Window = new WebView2Window()
        {
            Owner = MainWindow.MainWindowInstance
        };
        webView2Window.ShowDialog();
    }

    /// <summary>
    /// 刷新玩家Cookies数据
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_RefreshAuthInfo_Click(object sender, RoutedEventArgs e)
    {
        if (Globals.IsUseMode1)
        {
            //NotifierHelper.Show(NotifierType.Information, "正在内存扫描中，请稍后...");

            var sessionId = await Scan.GetGatewaySession();
            if (sessionId != string.Empty)
            {
                Globals.SessionId1 = sessionId;
                NotifierHelper.Show(NotifierType.Information, $"内存扫描SessionId成功 {Globals.SessionId1}");
            }
            else
            {
                NotifierHelper.Show(NotifierType.Information, "正在刷新中，请稍后...");

                var respAuth = await EA1API.GetAuthCode(Globals.Remid, Globals.Sid);
                if (respAuth.IsSuccess)
                {
                    if (!string.IsNullOrEmpty(respAuth.Remid))
                        Globals.Remid = respAuth.Remid;
                    if (!string.IsNullOrEmpty(respAuth.Sid))
                        Globals.Sid = respAuth.Sid;

                    var result = await EA2API.GetAccessToken(Globals.Remid, Globals.Sid);
                    if (result.IsSuccess)
                    {
                        var jNode = JsonNode.Parse(result.Content);
                        Globals.AccessToken = jNode["access_token"].GetValue<string>();
                        AuthModel.AccessToken = Globals.AccessToken;
                        NotifierHelper.Show(NotifierType.Success, "刷新玩家access_token成功");
                    }

                    result = await BF1API.GetEnvIdViaAuthCode(respAuth.Code);
                    if (result.IsSuccess)
                    {
                        var content = result.Content;
                        // 使用正则表达式从字符串中提取 sessionId
                        var match = Regex.Match(result.Content, @"sessionId:\s*([a-f0-9-]+)", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            // 提取并存储到 Globals.SessionId2
                            Globals.SessionId2 = match.Groups[1].Value;
                            NotifierHelper.Show(NotifierType.Success, "SessionId获取成功");
                        }
                        result = await EA2API.GetPlayerPersonaId(Globals.AccessToken, Globals.DisplayName2);
                        if (result.IsSuccess)
                        {
                            var jNode = JsonNode.Parse(result.Content);
                            if (jNode["personas"]!["persona"] != null)
                            {
                                Globals.PersonaId2 = jNode["personas"]!["persona"][0]["personaId"].GetValue<long>();
                            }
                            else
                            {
                                NotifierHelper.Show(NotifierType.Error, "personid获取失败");
                            }
                        }
                        else
                        {
                            NotifierHelper.Show(NotifierType.Error, "personid获取失败");
                        }
                        result = await BF1API.GetPersonasByIds(Globals.SessionId2, Globals.PersonaId);
                        if (result.IsSuccess)
                        {
                            var jNode = JsonNode.Parse(result.Content);
                            var personas = jNode["result"]![$"{Globals.PersonaId}"];
                            if (personas != null)
                            {
                                Globals.Avatar2 = personas!["avatar"].GetValue<string>();
                                Globals.DisplayName2 = personas!["displayName"].GetValue<string>();

                                AuthModel.Avatar2 = Globals.Avatar2;
                                AuthModel.DisplayName2 = Globals.DisplayName2;
                                AuthModel.PersonaId2 = Globals.PersonaId2;

                                AuthModel.Sid = Globals.Sid;
                                AuthModel.Remid = Globals.Remid;
                                AuthModel.SessionId2 = Globals.SessionId2;

                                NotifierHelper.Show(NotifierType.Success, "刷新玩家Cookies数据成功");
                            }
                        }
                        else
                        {
                            NotifierHelper.Show(NotifierType.Error, $"刷新失败\n{result.Content}");
                        }
                    }
                    else
                    {
                        NotifierHelper.Show(NotifierType.Error, $"刷新失败\n{result.Content}");
                    }
                }
                else
                {
                    NotifierHelper.Show(NotifierType.Error, "刷新失败，玩家Remid或Sid可能已过期");
                }
                Globals.SessionId1 = Globals.SessionId2;
                NotifierHelper.Show(NotifierType.Information, "改为从网络获取sessionId");
                //NotifierHelper.Show(NotifierType.Information, "内存扫描SessionId失败");
            }
        }
        else
        {
            if (string.IsNullOrEmpty(Globals.Remid) || string.IsNullOrEmpty(Globals.Sid))
            {
                NotifierHelper.Show(NotifierType.Warning, "玩家Remid或Sid为空，请先获取玩家Cookies");
                return;
            }

            NotifierHelper.Show(NotifierType.Information, "正在刷新中，请稍后...");

            var respAuth = await EA1API.GetAuthCode(Globals.Remid, Globals.Sid);
            if (respAuth.IsSuccess)
            {
                if (!string.IsNullOrEmpty(respAuth.Remid))
                    Globals.Remid = respAuth.Remid;
                if (!string.IsNullOrEmpty(respAuth.Sid))
                    Globals.Sid = respAuth.Sid;

                var result = await EA2API.GetAccessToken(Globals.Remid, Globals.Sid);
                if (result.IsSuccess)
                {
                    var jNode = JsonNode.Parse(result.Content);
                    Globals.AccessToken = jNode["access_token"].GetValue<string>();
                    AuthModel.AccessToken = Globals.AccessToken;
                    NotifierHelper.Show(NotifierType.Success, "刷新玩家access_token成功");
                }

                result = await BF1API.GetEnvIdViaAuthCode(respAuth.Code);
                if (result.IsSuccess)
                {
                    var content = result.Content;
                    // 使用正则表达式从字符串中提取 sessionId
                    var match = Regex.Match(result.Content, @"sessionId:\s*([a-f0-9-]+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        // 提取并存储到 Globals.SessionId2
                        Globals.SessionId2 = match.Groups[1].Value;
                        NotifierHelper.Show(NotifierType.Success, "SessionId获取成功");
                    }
                    result = await EA2API.GetPlayerPersonaId(Globals.AccessToken, Globals.DisplayName2);
                    if (result.IsSuccess)
                    {
                        var jNode = JsonNode.Parse(result.Content);
                        if (jNode["personas"]!["persona"] != null)
                        {
                            Globals.PersonaId2 = jNode["personas"]!["persona"][0]["personaId"].GetValue<long>();
                        }
                        else
                        {
                            NotifierHelper.Show(NotifierType.Error,"personid获取失败");
                        }
                    }
                    else
                    {
                        NotifierHelper.Show(NotifierType.Error, "personid获取失败");
                    }
                    result = await BF1API.GetPersonasByIds(Globals.SessionId2, Globals.PersonaId);
                    if (result.IsSuccess)
                    {
                        var jNode = JsonNode.Parse(result.Content);
                        var personas = jNode["result"]![$"{Globals.PersonaId}"];
                        if (personas != null)
                        {
                            Globals.Avatar2 = personas!["avatar"].GetValue<string>();
                            Globals.DisplayName2 = personas!["displayName"].GetValue<string>();

                            AuthModel.Avatar2 = Globals.Avatar2;
                            AuthModel.DisplayName2 = Globals.DisplayName2;
                            AuthModel.PersonaId2 = Globals.PersonaId2;

                            AuthModel.Sid = Globals.Sid;
                            AuthModel.Remid = Globals.Remid;
                            AuthModel.SessionId2 = Globals.SessionId2;

                            NotifierHelper.Show(NotifierType.Success, "刷新玩家Cookies数据成功");
                        }
                    }
                    else
                    {
                        NotifierHelper.Show(NotifierType.Error, $"刷新失败\n{result.Content}");
                    }
                }
                else
                {
                    NotifierHelper.Show(NotifierType.Error, $"刷新失败\n{result.Content}");
                }
            }
            else
            {
                NotifierHelper.Show(NotifierType.Error, "刷新失败，玩家Remid或Sid可能已过期");
            }
        }
    }

    /// <summary>
    /// 验证玩家SessionId有效性
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_VerifySessionId_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Globals.SessionId))
        {
            NotifierHelper.Show(NotifierType.Warning, "玩家SessionId为空，请先获取玩家SessionId");
            return;
        }

        TextBlock_SessionIdState.Text = "正在验证中，请稍后...";
        Border_SessionIdState.Background = Brushes.Gray;
        NotifierHelper.Show(NotifierType.Information, "正在验证中，请稍后...");

        _ = BF1API.SetAPILocale(Globals.SessionId);

        var result = await BF1API.GetWelcomeMessage(Globals.SessionId);
        if (result.IsSuccess)
        {
            var welcomeMsg = JsonHelper.JsonDese<WelcomeMsg>(result.Content);
            var firstMessage = ChsUtil.ToSimplified(welcomeMsg.result.firstMessage);

            TextBlock_SessionIdState.Text = firstMessage;
            Border_SessionIdState.Background = Brushes.Green;
            NotifierHelper.Show(NotifierType.Success, $"[{result.ExecTime:0.00} 秒]  验证成功\n{firstMessage}");
        }
        else
        {
            TextBlock_SessionIdState.Text = "验证失败";
            Border_SessionIdState.Background = Brushes.OrangeRed;
            NotifierHelper.Show(NotifierType.Error, $"[{result.ExecTime:0.00} 秒]  验证失败\n{result.Content}");
        }
    }

    /// <summary>
    /// 网络检测
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_DNS_Click(object sender, RoutedEventArgs e)
    {
        var btnContent = (e.OriginalSource as Button).Content.ToString();

        switch (btnContent)
        {
            case "查询DNS缓存":
                ProcessUtil.RunCMD("ipconfig /displaydns");
                break;
            case "刷新DNS缓存":
                ProcessUtil.RunCMD("ipconfig /flushdns");
                break;
            case "检测EA服务器本地解析":
                ProcessUtil.RunCMD("nslookup accounts.ea.com & nslookup signin.ea.com & nslookup gateway.ea.com & nslookup eaassets-a.akamaihd.net & nslookup sparta-gw.battlelog.com");
                break;
            case "批量Ping检测":
                ProcessUtil.RunCMD("ping accounts.ea.com & ping signin.ea.com & ping gateway.ea.com & ping eaassets-a.akamaihd.net & ping sparta-gw.battlelog.com");
                break;
            case "编辑Host文件":
                ProcessUtil.OpenPath("notepad.exe", @"C:\windows\system32\drivers\etc\hosts");
                break;
        }
    }

    /// <summary>
    /// 打开配置文件夹
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_OpenConfigFolder_Click(object sender, RoutedEventArgs e)
    {
        ProcessUtil.OpenPath(BF1ServerTools.Utils.FileUtil.Default_Path);
    }

    /// <summary>
    /// 通用超链接点击事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        string url = null;
        if (sender is Hyperlink link)
        {
            url = link.NavigateUri.AbsoluteUri;
        }
        else if (sender is Button btn && btn.Tag is string tagUrl)
        {
            url = tagUrl;
        }

        if (!string.IsNullOrEmpty(url))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
    //刷分模式开关
    private void ScoreSwitch_Checked(object sender, RoutedEventArgs e)
    {
        SwitchLabel.Text = "刷分自动换边开启";
        scorechangesite = true;
    }

    private void ScoreSwitch_Unchecked(object sender, RoutedEventArgs e)
    {
        SwitchLabel.Text = "刷分自动换边关闭";
        scorechangesite = false;
    }

    /// <summary>
    /// API上传数据
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private bool apiupdate = false;
    private void Button_WebApi_Click(object sender, RoutedEventArgs e)
    {
        int slotIndex = ComboBox_Slot.SelectedIndex;
        if (slotIndex < 0 || slotIndex >= urls.Length)
        {
            MessageBox.Show("请选择一个有效的槽位！");
            return;
        }
        if (apiupdate)
        {
            NotifierHelper.Show(NotifierType.Error, "已经开始上报了，请勿重复点击"); 
            return; }
        else
        {
            NotifierHelper.Show(NotifierType.Success, "开始上报");
            apiupdate = true;
        }
        // 从 urls 数组中读取选中槽位的 URL
        string url = urls[slotIndex];

        Thread backgroundThread = new Thread(async () =>
        {
            while (true)
            {
                if (Player.IsUseMode1)
                {
                    string servername = Server.GetServerName();
                    long gameid = string.IsNullOrEmpty(servername) ? 0 : Globals.GameId;
                    List<PlayerData> playerlist = Player.GetPlayerList();
                    foreach (var player in playerlist)
                    {

                        if (player.SquadId == 0)
                            player.SquadId = 99;
                        player.SquadId2 = ClientHelper.GetSquadChsName(player.SquadId);

                        player.Kd = PlayerUtil.GetPlayerKD(player.Kill, player.Dead);


                        if (player.LifeKd == 0 || player.LifeKpm == 0)
                        {
                            player.LifeKd = PlayerUtil.GetLifeKD(player.PersonaId);
                            player.LifeKpm = PlayerUtil.GetLifeKPM(player.PersonaId);
                        }
                        player.LifeTime = PlayerUtil.GetLifeTime(player.PersonaId);

                        player.Admin = PlayerUtil.IsAdminVIP(player.PersonaId, Globals.ServerAdmins_PID);
                        player.Vip = PlayerUtil.IsAdminVIP(player.PersonaId, Globals.ServerVIPs_PID);
                        player.White = PlayerUtil.IsWhite(player.Name, Globals.CustomWhites_Name);

                        player.Kit2 = ClientHelper.GetPlayerKitImage(player.Kit);
                        player.Kit3 = ClientHelper.GetPlayerKitName(player.Kit);
                    }
                    // 服务器地图名称
                    var MapName = Server.GetMapName();
                    var Team1Name = ClientHelper.GetTeamChsName(MapName, 1);
                    var Team2Name = ClientHelper.GetTeamChsName(MapName, 2);

                    MapName = string.IsNullOrEmpty(MapName) ? "未知" : MapName;
                    

                    // 服务器游戏模式
                    var GameMode = Server.GetGameMode();
                    GameMode = ClientHelper.GetGameMode3(GameMode);
                    var gameData = new
                    {
                        Servername = servername,
                        MapName,
                        GameMode,
                        GameId = gameid,
                        PlayerList = playerlist                  
                    };

                    // 将对象序列化为 JSON 格式
                    string jsonData = JsonConvert.SerializeObject(gameData,
             new JsonSerializerSettings
             {
                 ContractResolver = new CamelCasePropertyNamesContractResolver(),
                 Formatting = Newtonsoft.Json.Formatting.Indented
             });
                    // 指定文件路径
                    string filePath = "gameData.json";

                    await UploadJsonAsync(url, jsonData);
                    // 将 JSON 数据写入文件
                    File.WriteAllText(filePath, jsonData);
                }
                Thread.Sleep(1000);  // 暂停1秒
            }
        });
        backgroundThread.IsBackground = true;  // 设置为后台线程
        backgroundThread.Start();

        // 上传数据

    }

    static async Task UploadJsonAsync(string url, string json)
    {
        using (HttpClient client = new HttpClient())
        {
            // 创建HttpContent对象，指定内容类型为application/json
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // 发送POST请求
                HttpResponseMessage response = await client.PostAsync(url, content);

                // 检查响应状态
                if (response.IsSuccessStatusCode)
                {
                    Debug.WriteLine("上传成功！");
                }
                else
                {
                    Debug.WriteLine($"上传失败，状态码: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"发生错误: {ex.Message}");
            }
        }
    }
}
