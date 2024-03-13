using BF1ServerTools.API.Resp;

using RestSharp;

namespace BF1ServerTools.API;

public static class EA2API
{
    private const string host1 = "https://accounts.ea.com/connect/auth?response_type=token&locale=zh_CN&client_id=ORIGIN_JS_SDK&redirect_uri=nucleus%3Arest";
    private const string host2 = "https://gateway.ea.com/proxy/identity/personas?namespaceName=cem_ea_id&displayName=";

    private static readonly RestClient client;

    static EA2API()
    {
        if (client == null)
        {
            var options = new RestClientOptions()
            {
                MaxTimeout = 5000,
                ThrowOnAnyError = true
            };
            client = new RestClient(options);
        }
    }

    /// <summary>
    /// 使用Cookies获取access_token
    /// </summary>
    /// <param name="remid"></param>
    /// <param name="sid"></param>
    /// <returns></returns>
    public static async Task<RespContent> GetAccessToken(string remid, string sid)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var request = new RestRequest(host1)
                .AddHeader("Cookie", $"remid={remid};sid={sid};");

            var response = await client.ExecuteGetAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
                respContent.IsSuccess = true;

            respContent.Content = response.Content;
        }
        catch (Exception ex)
        {
            respContent.Content = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 获取玩家数字id
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="playerName"></param>
    /// <returns></returns>
    public static async Task<RespContent> GetPlayerPersonaId(string accessToken, string playerName)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var request = new RestRequest($"{host2}{playerName}")
                .AddHeader("X-Expand-Results", true)
                .AddHeader("Authorization", $"Bearer {accessToken}");

            var response = await client.ExecuteGetAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
                respContent.IsSuccess = true;

            respContent.Content = response.Content;
        }
        catch (Exception ex)
        {
            respContent.Content = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }
}
