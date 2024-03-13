using BF1ServerTools.API.Requ;
using BF1ServerTools.API.Resp;

using RestSharp;

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
    /// 根据AuthCode获取玩家SessionId
    /// </summary>
    /// <param name="authCode">通过本地重定向获取</param>
    /// <returns></returns>
    public static async Task<RespContent> GetEnvIdViaAuthCode(string authCode)
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
    /// 设置战地1 API语言为 繁体中文
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
    /// 获取战地1欢迎语
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
    /// 踢出目标玩家
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
    /// 更换玩家到指定队伍
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
    /// 更换服务器地图
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
    /// 添加服务器管理员
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
    /// 移除服务器管理员
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
    /// 添加服务器VIP
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
    /// 移除服务器VIP
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
    /// 添加服务器BAN
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
    /// 移除服务器BAN
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
    /// 获取服务器完整详情信息
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
    /// 获取服务器RSP详情信息
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
    /// 更新服务器信息
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
    /// 搜索服务器
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
    /// 离开服务器
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
    /// 通过玩家数字Id获取玩家相关信息
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
    /// 获取玩家基础数据
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
    /// 获取玩家武器数据
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
    /// 获取玩家载具数据
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
    /// 获取玩家正在游玩服务器
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
    /// 获取玩家佩戴的图章
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
