using BF1ServerTools.API.Resp;

using RestSharp;

namespace BF1ServerTools.API;

public static class EA1API
{
    private const string host = "https://accounts.ea.com/connect/auth?client_id=sparta-backend-as-user-pc&response_type=code&release_type=none";

    private static readonly RestClient client;

    static EA1API()
    {
        if (client == null)
        {
            var options = new RestClientOptions()
            {
                MaxTimeout = 5000,
                FollowRedirects = false
            };
            client = new RestClient(options);
        }
    }

    /// <summary>
    /// 使用Cookies获取authcode，同时更新Cookies
    /// </summary>
    /// <param name="remid"></param>
    /// <param name="sid"></param>
    /// <returns></returns>
    public static async Task<RespAuth> GetAuthCode(string remid, string sid)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respAuth = new RespAuth();

        try
        {
            var request = new RestRequest(host)
                .AddHeader("Cookie", $"remid={remid};sid={sid}");

            var response = await client.ExecuteGetAsync(request);
            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                string localtion = response.Headers.ToList()
                    .Find(x => x.Name == "Location")
                    .Value.ToString();

                if (localtion.Contains("127.0.0.1/success?code="))
                {
                    if (response.Cookies.Count == 2)
                    {
                        respAuth.Remid = response.Cookies[0].Value;
                        respAuth.Sid = response.Cookies[1].Value;
                    }
                    else
                    {
                        respAuth.Sid = response.Cookies[0].Value;
                    }

                    respAuth.IsSuccess = true;
                    respAuth.Code = localtion.Replace("http://127.0.0.1/success?code=", "").Replace("https://127.0.0.1/success?code=", "");
                }

                respAuth.Content = localtion;
            }
            else
            {
                respAuth.Content = response.Content;
            }
        }
        catch (Exception ex)
        {
            respAuth.Content = ex.Message;
        }

        sw.Stop();
        respAuth.ExecTime = sw.Elapsed.TotalSeconds;

        return respAuth;
    }
}
