using BF1ServerTools.API.Requ;
using BF1ServerTools.API.Resp;

using RestSharp;
using System.IO;
using System.Text.RegularExpressions;

namespace BF1ServerTools.API;

public static class BF1API
{
    private const string host = "https://sparta-gw.battlelog.com/jsonrpc/pc/api";

    private static readonly RestClient client;

    static BF1API()
    {
        if (client == null)
        {
            var options = new RestClientOptions(host)
            {
                MaxTimeout = 5000,
                ThrowOnAnyError = true
            };
            client = new RestClient(options);
        }
    }

    /// <summary>
    /// ����AuthCode��ȡ���SessionId
    /// </summary>
    /// <param name="authCode">ͨ�������ض����ȡ</param>
    /// <returns></returns>
 

public static async Task<RespContent> GetEnvIdViaAuthCode(string authCode)
{
    var sw = new Stopwatch();
    sw.Start();
    var respContent = new RespContent();

    try
    {
        // ʹ�� RestClientOptions �������Զ��ض���
        var options = new RestClientOptions
        {
            BaseUrl = new Uri("https://companion.battlefield.com"),
            FollowRedirects = true
        };
        var client = new RestClient(options);

        // ��һ���������ʼ GET ���� URL
        var url = $"/companion/sso?protocol=https&code={authCode}";
        var request = new RestRequest(url, Method.Get);

        // ִ�� GET ���󲢼�� Set-Cookie ͷ
        var response = await client.ExecuteAsync(request);
        SaveResponseToFile("Initial_Get_Response.txt", response.Content);  // ������Ӧ

        var setCookieHeader = response.Headers.FirstOrDefault(h => h.Name.Equals("Set-Cookie"));
        string sessionId = null;

        if (setCookieHeader != null)
        {
            sessionId = ExtractSessionIdFromCookie(setCookieHeader.Value.ToString());
        }

        if (string.IsNullOrEmpty(sessionId))
        {
            respContent.Content = "δ�ҵ� sessionId";
            return respContent;
        }

        // �ڶ��������� POST ������֤��¼״̬
        var postRequest = new RestRequest("/jsonrpc/web/api?Companion.isLoggedIn", Method.Post);
        postRequest.AddHeader("X-GatewaySession", sessionId);
        postRequest.AddHeader("Content-Type", "application/json");

        var reqBody = new
        {
            jsonrpc = "2.0",
            method = "Companion.isLoggedIn",
            @params = new { },
            id = Guid.NewGuid().ToString()
        };
        postRequest.AddJsonBody(reqBody);

        // ִ�� POST ����
        var postResponse = await client.ExecuteAsync(postRequest);
        SaveResponseToFile("Post_LoginCheck_Response.txt", postResponse.Content);  // ������Ӧ

        if (postResponse.StatusCode == HttpStatusCode.OK)
        {
            respContent.IsSuccess = true;
            respContent.Content = "��¼��֤�ɹ���sessionId: " + sessionId;
        }
        else
        {
            respContent.Content = $"��¼��֤ʧ�ܣ�״̬��: {(int)postResponse.StatusCode} - {postResponse.StatusDescription}";
        }
    }
    catch (Exception ex)
    {
        respContent.Content = ex.Message;
    }

    sw.Stop();
    respContent.ExecTime = sw.Elapsed.TotalSeconds;

    return respContent;
}

// ������������ Set-Cookie ͷ��ȡ sessionId
private static string ExtractSessionIdFromCookie(string cookie)
{
    var match = Regex.Match(cookie, @"gatewaySessionId=([^;]+)");
    return match.Success ? match.Groups[1].Value : null;
}

// ��������������Ӧ���浽�ı��ļ�
private static void SaveResponseToFile(string fileName, string content)
{
    //var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
    //File.WriteAllText(filePath, content);
}
    public static async Task<RespContent> GetEnvIdViaAuthCode2(string authCode)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "Authentication.getEnvIdViaAuthCode",
                @params = new
                {
                    authCode,
                    locale = "zh-tw"
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ����ս��1 API����Ϊ ��������
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public static async Task<RespContent> SetAPILocale(string sessionId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "CompanionSettings.setLocale",
                @params = new
                {
                    locale = "zh_TW"
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ��ȡս��1��ӭ��
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public static async Task<RespContent> GetWelcomeMessage(string sessionId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "Onboarding.welcomeMessage",
                @params = new
                {
                    game = "tunguska",
                    minutesToUTC = "-480"
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// �߳�Ŀ�����
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="gameId"></param>
    /// <param name="personaId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public static async Task<RespContent> RSPKickPlayer(string sessionId, long gameId, long personaId, string reason)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.kickPlayer",
                @params = new
                {
                    game = "tunguska",
                    gameId,
                    personaId,
                    reason
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ������ҵ�ָ������
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="gameId"></param>
    /// <param name="personaId"></param>
    /// <param name="teamId"></param>
    /// <returns></returns>
    public static async Task<RespContent> RSPMovePlayer(string sessionId, long gameId, long personaId, int teamId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.movePlayer",
                @params = new
                {
                    game = "tunguska",
                    gameId,
                    personaId,
                    teamId,
                    forceKill = true,
                    moveParty = false
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                try
                {
                    var respError = JsonHelper.JsonDese<RespError>(response.Content);
                    respContent.Content = $"{respError.error.code} {respError.error.message}";
                }
                catch (Exception jsonEx)
                {
                    respContent.Content = $"JSON ����ʧ�ܣ�{jsonEx.Message}\n��������: {response.Content}";
                }

            }
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
    /// ������������ͼ
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="persistedGameId"></param>
    /// <param name="levelIndex"></param>
    /// <returns></returns>
    public static async Task<RespContent> RSPChooseLevel(string sessionId, string persistedGameId, int levelIndex)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.chooseLevel",
                @params = new
                {
                    game = "tunguska",
                    persistedGameId,
                    levelIndex
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ��ӷ���������Ա
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="serverId"></param>
    /// <param name="personaName"></param>
    /// <returns></returns>
    public static async Task<RespContent> AddServerAdmin(string sessionId, int serverId, string personaName)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.addServerAdmin",
                @params = new
                {
                    game = "tunguska",
                    serverId,
                    personaName
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// �Ƴ�����������Ա
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="serverId"></param>
    /// <param name="personaId"></param>
    /// <returns></returns>
    public static async Task<RespContent> RemoveServerAdmin(string sessionId, int serverId, long personaId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.removeServerAdmin",
                @params = new
                {
                    game = "tunguska",
                    serverId,
                    personaId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ��ӷ�����VIP
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="serverId"></param>
    /// <param name="personaName"></param>
    /// <returns></returns>
    public static async Task<RespContent> AddServerVip(string sessionId, int serverId, string personaName)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.addServerVip",
                @params = new
                {
                    game = "tunguska",
                    serverId,
                    personaName
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// �Ƴ�������VIP
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="serverId"></param>
    /// <param name="personaId"></param>
    /// <returns></returns>
    public static async Task<RespContent> RemoveServerVip(string sessionId, int serverId, long personaId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.removeServerVip",
                @params = new
                {
                    game = "tunguska",
                    serverId,
                    personaId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ��ӷ�����BAN
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="serverId"></param>
    /// <param name="personaName"></param>
    /// <returns></returns>
    public static async Task<RespContent> AddServerBan(string sessionId, int serverId, string personaName)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.addServerBan",
                @params = new
                {
                    game = "tunguska",
                    serverId,
                    personaName
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// �Ƴ�������BAN
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="serverId"></param>
    /// <param name="personaId"></param>
    /// <returns></returns>
    public static async Task<RespContent> RemoveServerBan(string sessionId, int serverId, long personaId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.removeServerBan",
                @params = new
                {
                    game = "tunguska",
                    serverId,
                    personaId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ��ȡ����������������Ϣ
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="gameId"></param>
    /// <returns></returns>
    public static async Task<RespContent> GetFullServerDetails(string sessionId, long gameId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "GameServer.getFullServerDetails",
                @params = new
                {
                    game = "tunguska",
                    gameId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ��ȡ������RSP������Ϣ
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="serverId"></param>
    /// <returns></returns>
    public static async Task<RespContent> GetServerDetails(string sessionId, int serverId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.getServerDetails",
                @params = new
                {
                    game = "tunguska",
                    serverId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ���·�������Ϣ
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="reqBody"></param>
    /// <returns></returns>
    public static async Task<RespContent> UpdateServer(string sessionId, UpdateServer reqBody)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ����������
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="serverName"></param>
    /// <returns></returns>
    public static async Task<RespContent> SearchServers(string sessionId, string serverName)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "GameServer.searchServers",
                @params = new
                {
                    filterJson = "{\"version\":6,\"name\":\"" + serverName + "\"}",
                    game = "tunguska",
                    limit = 30,
                    protocolVersion = "3779779"
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// �뿪������
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="gameId"></param>
    /// <returns></returns>
    public static async Task<RespContent> LeaveGame(string sessionId, long gameId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "Game.leaveGame",
                @params = new
                {
                    game = "tunguska",
                    gameId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ͨ���������Id��ȡ��������Ϣ
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="personaId"></param>
    /// <returns></returns>
    public static async Task<RespContent> GetPersonasByIds(string sessionId, long personaId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.getPersonasByIds",
                @params = new
                {
                    game = "tunguska",
                    personaIds = new[] { personaId }
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ��ȡ��һ�������
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="personaId"></param>
    /// <returns></returns>
    public static async Task<RespContent> DetailedStatsByPersonaId(string sessionId, long personaId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "Stats.detailedStatsByPersonaId",
                @params = new
                {
                    game = "tunguska",
                    personaId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ��ȡ�����������
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="personaId"></param>
    /// <returns></returns>
    public static async Task<RespContent> GetWeaponsByPersonaId(string sessionId, long personaId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "Progression.getWeaponsByPersonaId",
                @params = new
                {
                    game = "tunguska",
                    personaId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ��ȡ����ؾ�����
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="personaId"></param>
    /// <returns></returns>
    public static async Task<RespContent> GetVehiclesByPersonaId(string sessionId, long personaId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "Progression.getVehiclesByPersonaId",
                @params = new
                {
                    game = "tunguska",
                    personaId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ��ȡ����������������
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="personaId"></param>
    /// <returns></returns>
    public static async Task<RespContent> GetServersByPersonaIds(string sessionId, long personaId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "GameServer.getServersByPersonaIds",
                @params = new
                {
                    game = "tunguska",
                    personaIds = new[] { personaId }
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
    /// ��ȡ��������ͼ��
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="personaId"></param>
    /// <returns></returns>
    public static async Task<RespContent> GetEquippedEmblem(string sessionId, long personaId)
    {
        var sw = new Stopwatch();
        sw.Start();
        var respContent = new RespContent();

        try
        {
            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "Emblems.getEquippedEmblem",
                @params = new
                {
                    platform = "pc",
                    personaId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeader("X-GatewaySession", sessionId)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Content = response.Content;
            }
            else
            {
                var respError = JsonHelper.JsonDese<RespError>(response.Content);
                respContent.Content = $"{respError.error.code} {respError.error.message}";
            }
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
