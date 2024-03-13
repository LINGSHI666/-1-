using BF1ServerTools.API;
using BF1ServerTools.API.RespJson;
using BF1ServerTools.Utils;

using Microsoft.Web.WebView2.Core;
using CommunityToolkit.Mvvm.Messaging;

namespace BF1ServerTools.Windows;

/// <summary>
/// WebView2Window.xaml 的交互逻辑
/// </summary>
public partial class WebView2Window
{
    private const string host = "https://accounts.ea.com/connect/auth?client_id=sparta-backend-as-user-pc&response_type=code&release_type=none";

    public WebView2Window()
    {
        InitializeComponent();
    }

    private async void Window_WebView2_Loaded(object sender, RoutedEventArgs e)
    {
        // 初始化WebView2环境
        var env = await CoreWebView2Environment.CreateAsync(null, FileUtil.D_Cache_Path, null);
        await WebView2.EnsureCoreWebView2Async(env);

        // 禁止Dev开发工具
        WebView2.CoreWebView2.Settings.AreDevToolsEnabled = false;
        // 禁止右键菜单
        WebView2.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        // 禁止浏览器缩放
        WebView2.CoreWebView2.Settings.IsZoomControlEnabled = false;
        // 禁止显示状态栏（鼠标悬浮在链接上时右下角没有url地址显示）
        WebView2.CoreWebView2.Settings.IsStatusBarEnabled = false;

        // 新窗口打开页面的处理
        WebView2.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        // Url变化的处理
        WebView2.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;
        // 导航到指定Url
        WebView2.CoreWebView2.Navigate(host);
    }

    private void Window_WebView2_Closing(object sender, CancelEventArgs e)
    {
        WebView2.Dispose();
    }

    private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        var deferral = e.GetDeferral();
        e.NewWindow = WebView2.CoreWebView2;
        deferral.Complete();
    }

    private async void CoreWebView2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
    {
        TextBox_Remid.Clear();
        TextBox_Sid.Clear();
        TextBox_SessionId2.Clear();
        TextBox_AccessToken.Clear();

        var source = WebView2.Source.ToString();
        TextBlock_Source.Text = source;

        if (!source.Contains("127.0.0.1/success?code="))
            return;

        var cookies = await WebView2.CoreWebView2.CookieManager.GetCookiesAsync(null);
        if (cookies == null)
        {
            TextBlock_Log.Text = "登录成功，获取Cookie失败，请尝试清除缓存";
            return;
        }

        foreach (var item in cookies)
        {
            if (item.Name == "remid")
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    TextBox_Remid.Text = item.Value;
                    Globals.Remid = item.Value;
                }
                continue;
            }

            if (item.Name == "sid")
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    TextBox_Sid.Text = item.Value;
                    Globals.Sid = item.Value;
                }
                continue;
            }
        }

        var code = source.Replace("http://127.0.0.1/success?code=", "").Replace("https://127.0.0.1/success?code=", "");

        TextBlock_Log.Text = "登录成功，正在获取SessionId";

        // 获取access_token
        var result = await EA2API.GetAccessToken(Globals.Remid, Globals.Sid);
        if (result.IsSuccess)
        {
            var jNode = JsonNode.Parse(result.Content);
            Globals.AccessToken = jNode["access_token"].GetValue<string>();

            TextBox_AccessToken.Text = jNode["access_token"].GetValue<string>();
            TextBlock_Log.Text = "获取access_token成功";
        }

        // 获取SessionId
        result = await BF1API.GetEnvIdViaAuthCode(code);
        if (result.IsSuccess)
        {
            var envIdViaAuthCode = JsonHelper.JsonDese<EnvIdViaAuthCode>(result.Content);

            Globals.SessionId2 = envIdViaAuthCode.result.sessionId;
            Globals.PersonaId2 = long.Parse(envIdViaAuthCode.result.personaId);

            TextBox_SessionId2.Text = envIdViaAuthCode.result.sessionId;
            TextBlock_Log.Text = "获取SessionId成功";

            _ = BF1API.SetAPILocale(Globals.SessionId2);

            result = await BF1API.GetPersonasByIds(Globals.SessionId2, Globals.PersonaId);
            if (result.IsSuccess)
            {
                JsonNode jNode = JsonNode.Parse(result.Content);
                var personas = jNode["result"]![$"{Globals.PersonaId}"];
                if (personas != null)
                {
                    Globals.Avatar2 = personas!["avatar"].GetValue<string>();
                    Globals.DisplayName2 = personas!["displayName"].GetValue<string>();
                }
            }

            // 获取成功，通知主窗口更新数据（相关数据已刷新全局变量）
            WeakReferenceMessenger.Default.Send("", "SendRemidSid");
        }
        else
        {
            TextBlock_Log.Text = $"获取SessionId失败  {result.Content}";
        }
    }

    /// <summary>
    /// 重新加载登录页面
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_ReloadWebView2_Click(object sender, RoutedEventArgs e)
    {
        WebView2.CoreWebView2.Navigate(host);

        TextBlock_Log.Text = "重新加载登录页面成功";
    }

    /// <summary>
    /// 清空WebView2缓存（仅更换账号使用）
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_ClearWebView2Cache_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("你确认要清空本地缓存吗，这一般会在需要更换当前登录账号的情况下使用", "清空本地缓存提示",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            await WebView2.CoreWebView2.ExecuteScriptAsync("localStorage.clear()");
            WebView2.CoreWebView2.CookieManager.DeleteAllCookies();

            WebView2.Reload();

            TextBlock_Log.Text = "清空WebView2缓存成功";
        }
    }
}
