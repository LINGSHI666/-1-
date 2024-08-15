using BF1ServerTools.API.Resp;

using RestSharp;
using System.Text.RegularExpressions;

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
    /// 使用Cookiesq'a'Z获取authcode，同时更新Cookies
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
                // 配置 RestClientOptions 以禁用自动重定向
                var options = new RestClientOptions
                {
                    BaseUrl = new Uri("https://accounts.ea.com"),
                    FollowRedirects = false // 禁用自动重定向
                };
                var client = new RestClient(options);

                // 设置获取 authCode 的请求 URL
                var url = "/connect/auth?client_id=sparta-companion-web&response_type=code&display=web2/login&locale=zh_TW&redirect_uri=https%3A%2F%2Fcompanion.battlefield.com%2Fcompanion%2Fsso%3Fprotocol%3Dhttps";
                var request = new RestRequest(url, Method.Get);

                // 添加 Cookie 到请求头
                request.AddHeader("Cookie", $"remid={remid};sid={sid}");

                // 执行 GET 请求以获取 authCode
                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.Redirect)
                {
                    // 检查是否有 Location 头
                    string location = response.Headers
                        .FirstOrDefault(x => x.Name == "Location")?.Value?.ToString();

                    if (!string.IsNullOrEmpty(location) && location.Contains("code="))
                    {
                        // 提取 authCode
                        respAuth.Code = ExtractAuthCodeFromLocation(location);

                        // 更新 Cookies（如果有新的值）
                        if (response.Cookies.Count > 0)
                        {
                            respAuth.Remid = response.Cookies.FirstOrDefault(c => c.Name == "remid")?.Value ?? remid;
                            respAuth.Sid = response.Cookies.FirstOrDefault(c => c.Name == "sid")?.Value ?? sid;
                        }

                        respAuth.IsSuccess = true;
                        respAuth.Content = location; // 保留原始重定向的 URL
                    }
                    else
                    {
                        respAuth.Content = "重定向中未包含 authCode";
                    }
                }
                else if (response.StatusCode == HttpStatusCode.OK)
                {
                    respAuth.Content = "请求成功但无重定向。";
                }
                else
                {
                    respAuth.Content = $"请求失败，状态码: {(int)response.StatusCode} - {response.StatusDescription}";
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

        // 辅助方法：从 Location URL 中提取 authCode
        private static string ExtractAuthCodeFromLocation(string locationUrl)
        {
            var match = Regex.Match(locationUrl, @"code=([^&]+)");
            return match.Success ? match.Groups[1].Value : null;
        }


    }
