using BF1ServerTools.SDK.Core;
using BF1ServerTools.SDK.Data;
using Newtonsoft.Json;
using System.IO.Pipes;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using static BF1ServerTools.SDK.Player;
using System.Numerics;


namespace BF1ServerTools.SDK;

public static class Player
{
    /// <summary>
    /// 服务器最大玩家数量
    /// </summary>
    private const int MaxPlayer = 74;

    /// <summary>
    /// 获取自己信息
    /// </summary>
    /// <returns></returns>
    public static LocalData GetLocalPlayer()
    {
        var _baseAddress = Obfuscation.GetLocalPlayer();
        return new LocalData()
        {
            DisplayName = Memory.ReadString(_baseAddress + 0x40, 64),
            PersonaId = Memory.Read<long>(_baseAddress + 0x38),
            FullName = Memory.ReadString(_baseAddress + 0x2156, 64)
        };
    }

    /// <summary>
    /// 获取玩家列表缓存（生涯数据）
    /// </summary>
    /// <returns></returns>
    public static List<PlayerData> GetPlayerCache()
    {
        List<PlayerData> _playerList = new List<PlayerData>();

        if (!IsUseMode1) // 测试时为 false，使用网络请求
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    // 发起同步网络请求
                    var responseTask = httpClient.GetAsync(retrievedString);
                    responseTask.Wait(); // 等待请求完成

                    var response = responseTask.Result; // 获取请求结果

                    // 如果请求成功，处理响应数据
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = response.Content.ReadAsStringAsync().Result; // 同步获取响应内容
                        var serverInfo = JsonConvert.DeserializeObject<ServerInfoRoot>(responseString);

                        // 解析服务器中的队伍和玩家信息
                        foreach (var team in serverInfo.Teams)
                        {
                            foreach (var playerJson in team.Players)
                            {
                                var playerData = new PlayerData
                                {
                                    Clan = playerJson.Platoon,
                                    Name = playerJson.Name,
                                    PersonaId = playerJson.PlayerId,
                                    Rank = playerJson.Rank,
                                    // 根据队伍设置 TeamId
                                    TeamId = team.TeamId == "teamOne" ? 1 : 2,
                                };

                                // 添加玩家数据到列表
                                _playerList.Add(playerData);
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                // 处理HTTP请求异常（网络问题等）
                Console.WriteLine($"HTTP 请求错误: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                // 处理其他异常
                Console.WriteLine($"发生错误: {ex.Message}");
            }
        }
        else
        {
            List<PlayerData> _playerCache = new();
            for (int i = 0; i < MaxPlayer; i++)
            {
                var _baseAddress = Obfuscation.GetPlayerById(i);
                if (!Memory.IsValid(_baseAddress))
                    continue;

                var _personaId = Memory.Read<long>(_baseAddress + 0x38);
                if (_personaId == 0)
                    continue;

                var _name = Memory.ReadString(_baseAddress + 0x40, 64);
                var _teamId = Memory.Read<int>(_baseAddress + 0x1C34);
                var _spectator = Memory.Read<byte>(_baseAddress + 0x1C31);

                _playerCache.Add(new()
                {
                    TeamId = _teamId,
                    Name = _name,
                    Spectator = _spectator,
                    PersonaId = _personaId
                });
            }
            return _playerCache;
        }

        return _playerList;
    }

    /// <summary>
    /// 判断战地1程序是否运行
    /// </summary>
    /// <returns></returns>
    public static bool IsBf1Run()
    {
        var pArray = Process.GetProcessesByName("bf1");
        if (pArray.Length > 0)
        {
            foreach (var item in pArray)
            {
                if (item.MainWindowTitle.Equals("Battlefield™ 1"))
                    return true;
            }
        }

        return false;
    }
    //zhang api解析
    public class ServerResponse2
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public List<PlayerJson2> Data { get; set; }
    }
    public class PlayerJson2
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("mark")]
        public int Mark { get; set; }

        [JsonProperty("teamId")]
        public int TeamId { get; set; }

        [JsonProperty("isSpectator")]
        public bool IsSpectator { get; set; }

        [JsonProperty("clan")]
        public string Clan { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("personaId")]
        public long PersonaId { get; set; }

        [JsonProperty("squadId")]
        public int SquadId { get; set; }

        [JsonProperty("squadName")]
        public string SquadName { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("kill")]
        public int Kill { get; set; }

        [JsonProperty("dead")]
        public int Dead { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("kit")]
        public string Kit { get; set; }

        [JsonProperty("kitName")]
        public string KitName { get; set; }

        [JsonProperty("poseType")]
        public int PoseType { get; set; }

        [JsonProperty("poseName")]
        public string PoseName { get; set; }

        [JsonProperty("isAlive")]
        public bool IsAlive { get; set; }

        [JsonProperty("isInVehicle")]
        public bool IsInVehicle { get; set; }

        [JsonProperty("authorativeYaw")]
        public float AuthorativeYaw { get; set; }

        [JsonProperty("authorativePitch")]
        public float AuthorativePitch { get; set; }

        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("transform")]
        public Transform Transform { get; set; }

        [JsonProperty("vehicleHealth")]
        public float VehicleHealth { get; set; }

        [JsonProperty("health")]
        public float Health { get; set; }

        [JsonProperty("weaponS0")]
        public Weapon WeaponS0 { get; set; }

        [JsonProperty("weaponS1")]
        public Weapon WeaponS1 { get; set; }

        [JsonProperty("weaponS2")]
        public Weapon WeaponS2 { get; set; }

        [JsonProperty("weaponS3")]
        public Weapon WeaponS3 { get; set; }

        [JsonProperty("weaponS4")]
        public Weapon WeaponS4 { get; set; }

        [JsonProperty("weaponS5")]
        public Weapon WeaponS5 { get; set; }

        [JsonProperty("weaponS6")]
        public Weapon WeaponS6 { get; set; }

        [JsonProperty("weaponS7")]
        public Weapon WeaponS7 { get; set; }
    }

    public class Transform
    {
        [JsonProperty("m11")]
        public float M11 { get; set; }

        [JsonProperty("m12")]
        public float M12 { get; set; }

        [JsonProperty("m13")]
        public float M13 { get; set; }

        [JsonProperty("m14")]
        public float M14 { get; set; }

        [JsonProperty("m21")]
        public float M21 { get; set; }

        [JsonProperty("m22")]
        public float M22 { get; set; }

        [JsonProperty("m23")]
        public float M23 { get; set; }

        [JsonProperty("m24")]
        public float M24 { get; set; }

        [JsonProperty("m31")]
        public float M31 { get; set; }

        [JsonProperty("m32")]
        public float M32 { get; set; }

        [JsonProperty("m33")]
        public float M33 { get; set; }

        [JsonProperty("m34")]
        public float M34 { get; set; }

        [JsonProperty("m41")]
        public float M41 { get; set; }

        [JsonProperty("m42")]
        public float M42 { get; set; }

        [JsonProperty("m43")]
        public float M43 { get; set; }

        [JsonProperty("m44")]
        public float M44 { get; set; }
    }
    public class Location
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }
    }
    public class Weapon
    {
        [JsonProperty("guid")]
        public string Guid { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    // ghs api的json解析
    public class ServerResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public List<PlayerJson> Data { get; set; }
    }

    public class PlayerJson
    {
        [JsonProperty("platoonName")]
        public string PlatoonName { get; set; }

        [JsonProperty("personaName")]
        public string PersonaName { get; set; }

        [JsonProperty("personaId")]
        public long PersonaId { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("teamId")]
        public int TeamId { get; set; }

        [JsonProperty("detailed")]
        public PlayerDetailed Detailed { get; set; }
    }

    public class PlayerDetailed
    {
        [JsonProperty("hoursPlayed")]
        public int? HoursPlayed { get; set; }

        [JsonProperty("kills")]
        public int? Kills { get; set; }

        [JsonProperty("deaths")]
        public int? Deaths { get; set; }

        [JsonProperty("kd")]
        public double? Kd { get; set; }

        [JsonProperty("kpm")]
        public double? Kpm { get; set; }

        [JsonProperty("roundAvgKills")]
        public double? RoundAvgKills { get; set; }

        [JsonProperty("accuracyRatio")]
        public double? AccuracyRatio { get; set; }

        [JsonProperty("headShotsRatio")]
        public double? HeadShotsRatio { get; set; }

        [JsonProperty("winRatio")]
        public double? WinRatio { get; set; }

        [JsonProperty("roundsPlayed")]
        public int? RoundsPlayed { get; set; }

        [JsonProperty("skill")]
        public double? Skill { get; set; }
    }

    //json解析
    public class ServerInfoRoot
    {
        [JsonProperty("serverinfo")]
        public ServerInfo ServerInfo { get; set; }

        [JsonProperty("teams")]
        public List<TeamInfo> Teams { get; set; }
    }

    public class ServerInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("maps")]
        public List<string> Maps { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("settings")]
        public List<string> Settings { get; set; }

        [JsonProperty("servertype")]
        public string ServerType { get; set; }
    }

    public class TeamInfo
    {
        [JsonProperty("teamid")]
        public string TeamId { get; set; }

        [JsonProperty("players")]
        public List<PlayerInfo> Players { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("faction")]
        public string Faction { get; set; }
    }

    public class PlayerInfo
    {
        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("latency")]
        public int Latency { get; set; }

        [JsonProperty("slot")]
        public int Slot { get; set; }

        [JsonProperty("join_time")]
        public long JoinTime { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("player_id")]
        public long PlayerId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("platoon")]
        public string Platoon { get; set; }
    }
    // 计算两个字符串的相似度，使用 Levenshtein 距离的简单实现
    static double CalculateStringSimilarity(string str1, string str2)
    {
        // 预处理：移除非字母和数字，统一转换为小写
        str1 = PreprocessString(str1);
        str2 = PreprocessString(str2);

        int len1 = str1.Length;
        int len2 = str2.Length;

        // 定义形状相似字符的映射表
        var similarGroups = new List<HashSet<char>>
        {
            new HashSet<char> { 'i', 'l', '1' },
            new HashSet<char> { 'o', '0' },
            new HashSet<char> { 'm', 'n' },
            new HashSet<char> { 'c', 'e' },
            new HashSet<char> { 'u', 'v' }
        };

        // 判断两个字符是否形状相似
        bool AreSimilar(char c1, char c2)
        {
            foreach (var group in similarGroups)
            {
                if (group.Contains(c1) && group.Contains(c2))
                    return true;
            }
            return false;
        }

        // 创建二维数组来存储计算结果
        var matrix = new int[len1 + 1, len2 + 1];

        for (int i = 0; i <= len1; i++)
            matrix[i, 0] = i;
        for (int j = 0; j <= len2; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= len1; i++)
        {
            for (int j = 1; j <= len2; j++)
            {
                int cost = (str1[i - 1] == str2[j - 1] || AreSimilar(str1[i - 1], str2[j - 1])) ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost
                );
            }
        }

        // 返回相似度
        int editDistance = matrix[len1, len2];
        double maxLength = Math.Max(len1, len2);
        return 1.0 - (editDistance / maxLength);
    }

    // 预处理字符串：移除非字母和数字，统一为小写
    static string PreprocessString(string input)
    {
        // 使用正则表达式保留字母和数字
        string processed = Regex.Replace(input, @"[^a-zA-Z0-9]", "");
        // 转换为小写
        return processed.ToLower();
    }
    public static string retrievedString = "";
    public static long gameId = 0;
    public static bool IsUseMode1 = true;
    public static List<PlayerData> playerlistocr1 = new List<PlayerData>();
    public static List<PlayerData> playerlistocr2 = new List<PlayerData>();
    public static bool ocrflag = true;//真读取1
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Vector
    {
        public float X;
        public float Y;
        public float Z;
    }

    /// <summary>
    /// 获取玩家列表信息
    /// </summary>
    /// <returns></returns>
    public static List<PlayerData> GetPlayerList()

    {
        
        // 初始化 HttpClient 实例
        using (var httpClient = new HttpClient())
            if (!IsUseMode1)//测试时为false
                {   

                    List<PlayerData> _playerList = new List<PlayerData>();
                var playerListOcr = new List<PlayerData>();
                /*
                if (ocrflag)
                {
                    playerListOcr = playerlistocr1;

                }
                else
                {
                    playerListOcr = playerlistocr2;
                }*/
                string urlzhang = $"http://127.0.0.1:10086/Player/GetAllPlayerList";
                string url = $"https://battlefield.tools/api/server/player/query/batch?game_id={gameId}&detailed=true";
                try
                {
                    HttpResponseMessage response;

                    try
                    {
                        var responseTask = httpClient.GetAsync(urlzhang);
                        responseTask.Wait(); // 等待任务完成
                        response = responseTask.Result; // 获取结果
                    }
                    catch (HttpRequestException ex)  // 处理网络异常
                    {
                       
                        response = new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable) // 503 服务不可用
                        {
                            ReasonPhrase = "网络请求失败"
                        };
                    }
                    catch (Exception ex)  // 处理其他未知异常
                    {
                        
                        response = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError) // 500 服务器错误
                        {
                            ReasonPhrase = "未知错误"
                        };
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = response.Content.ReadAsStringAsync().Result; // 同步获取响应内容
                        var serverInfo = JsonConvert.DeserializeObject<ServerResponse2>(responseString);

                        foreach (var playerJson in serverInfo.Data)
                        {
                            var playerData = new PlayerData
                            {
                                Mark = (byte)playerJson.Mark,
                                TeamId = playerJson.TeamId,
                                Spectator = (byte)(playerJson.IsSpectator ? 1 : 0),
                                Clan = playerJson.Clan,
                                Name = playerJson.Name,
                                PersonaId = playerJson.PersonaId,
                                SquadId = playerJson.SquadId,
                                SquadId2 = playerJson.SquadName,
                                Rank = playerJson.Rank,
                                Kill = playerJson.Kill,
                                Dead = playerJson.Dead,
                                Score = playerJson.Score,
                                Kit = playerJson.Kit,
                                Kit2 = playerJson.KitName,
                                X = playerJson.Location?.X ?? 0,
                                Y = playerJson.Location?.Y ?? 0,
                                Z = playerJson.Location?.Z ?? 0,
                                WeaponS0 = playerJson.WeaponS0?.Id ?? "",
                                WeaponS1 = playerJson.WeaponS1?.Id ?? "",
                                WeaponS2 = playerJson.WeaponS2?.Id ?? "",
                                WeaponS3 = playerJson.WeaponS3?.Id ?? "",
                                WeaponS4 = playerJson.WeaponS4?.Id ?? "",
                                WeaponS5 = playerJson.WeaponS5?.Id ?? "",
                                WeaponS6 = playerJson.WeaponS6?.Id ?? "",
                                WeaponS7 = playerJson.WeaponS7?.Id ?? "",
                                Kd = playerJson.Kill == 0 ? 0 : (float)playerJson.Kill / (playerJson.Dead == 0 ? 1 : playerJson.Dead),

                            };

                            _playerList.Add(playerData);
                        }
                        int a = 1;

                    }
                    else

                    {





                        var responseTask = httpClient.GetAsync(url);
                        responseTask.Wait(); // 等待任务完成

                        response = responseTask.Result; // 获取结果

                        if (response.IsSuccessStatusCode)
                        {
                            var responseString = response.Content.ReadAsStringAsync().Result; // 同步获取响应内容
                            var serverInfo = JsonConvert.DeserializeObject<ServerResponse>(responseString);

                            if (serverInfo != null && serverInfo.Code == 200 && serverInfo.Data != null)
                            {
                                foreach (var playerJson in serverInfo.Data)
                                {
                                    var playerData = new PlayerData
                                    {
                                        Clan = playerJson.PlatoonName,
                                        Name = playerJson.PersonaName,
                                        PersonaId = playerJson.PersonaId,
                                        Rank = playerJson.Rank,
                                        TeamId = playerJson.TeamId,

                                        // playerJson.Detailed 不为空
                                        LifeKd = playerJson.Detailed != null ? (float)playerJson.Detailed.Kd : 0,
                                        LifeKpm = playerJson.Detailed != null ? (float)playerJson.Detailed.Kpm : 0



                                    };

                                    _playerList.Add(playerData);
                                }
                            }
                        }
                        else
                        {
                            responseTask = httpClient.GetAsync(retrievedString);
                            responseTask.Wait(); // 等待任务完成

                            response = responseTask.Result; // 获取结果
                            if (response.IsSuccessStatusCode)
                            {
                                var responseString = response.Content.ReadAsStringAsync().Result; // 同步获取响应内容
                                var serverInfo2 = JsonConvert.DeserializeObject<ServerInfoRoot>(responseString);

                                foreach (var team in serverInfo2.Teams)
                                {
                                    foreach (var playerJson in team.Players)
                                    {
                                        var playerData = new PlayerData
                                        {
                                            Clan = playerJson.Platoon,
                                            Name = playerJson.Name,
                                            PersonaId = playerJson.PlayerId,
                                            Rank = playerJson.Rank,
                                            // 初始化其他字段...
                                            TeamId = team.TeamId == "teamOne" ? 1 : 2,
                                        };

                                        _playerList.Add(playerData);
                                    }
                                }
                            }
                        }
                    }
                }

                catch (Exception ex)
                {

                }
                if(playerListOcr.Count != 0)
                { 
                foreach (var playerOcr in playerListOcr)
                {
                    foreach (var player in _playerList)
                    {
                        // 拼接 _playerList 中的 clan 和 name
                        string _playerString = player.Clan + player.Name;

                        // 计算相似度
                        double similarity = CalculateStringSimilarity(playerOcr.Name, _playerString);

                        // 如果相似度超过50%
                        if (similarity >= 0.7)
                        {
                            // 将 playerListOcr 中的 Kill, Death, Score 赋值给 _playerList 中相应的元素
                            player.Kill = playerOcr.Kill;
                            player.Dead = playerOcr.Dead;
                            player.Score = playerOcr.Score;

                            break;
                        }
                    }
                }
                }
                return _playerList;
                
    }
        else
        {
            List<PlayerData> _playerList = new();
            var _weaponSlot = new string[8] { "", "", "", "", "", "", "", "" };

            //////////////////////////////// 玩家数据 ////////////////////////////////

            for (int i = 0; i < MaxPlayer; i++)
            {
                var _baseAddress = Obfuscation.GetPlayerById(i);
                if (!Memory.IsValid(_baseAddress))
                    continue;

                var _personaId = Memory.Read<long>(_baseAddress + 0x38);
                if (_personaId == 0)
                    continue;

                var _mark = Memory.Read<byte>(_baseAddress + 0x1D7C);
                var _teamId = Memory.Read<int>(_baseAddress + 0x1C34);
                var _spectator = Memory.Read<byte>(_baseAddress + 0x1C31);
                var _squadId = Memory.Read<int>(_baseAddress + 0x1E50);
                var _clan = Memory.ReadString(_baseAddress + 0x2151, 64);
                var _name = Memory.ReadString(_baseAddress + 0x40, 64);
                    // 载具实体指针
                    var pClientVehicleEntity = Memory.Read<long>(_baseAddress + 0x1D38);
                    var pClientSoldierEntity = Memory.Read<long>(_baseAddress + 0x1D48);
                    // 当玩家进入载具后，pClientVehicleEntity与pClientSoldierEntity相同
                    var isInVehicle = Memory.IsValid(pClientVehicleEntity) && Memory.IsValid(pClientSoldierEntity);
                    Vector xyz;
                    xyz.X = 0;
                    xyz.Y = 0;
                    xyz.Z = 0;
                    // 玩家进入载具
                    if (isInVehicle) {
                        // 玩家载具坐标
                        var m_collection = Memory.Read<long>(pClientVehicleEntity + 0x38);
                        var _9 = Memory.Read<byte>(m_collection + 0x09);
                        var _10 = Memory.Read<byte>(m_collection + 0x0A);
                        var componentCollectionOffset = 0x20 * (_10 + (0x02 * _9));

                        var matrix16 = Memory.Read<Matrix4x4>(m_collection + componentCollectionOffset + 0x10);
                        if (Math.Abs(matrix16.M11 + matrix16.M12 + matrix16.M13) > 3.0f)
                        {
                            xyz.X = matrix16.M11;
                            xyz.Y = matrix16.M12;
                            xyz.Z = matrix16.M13;

                        }
                        else if (Math.Abs(matrix16.M21 + matrix16.M22 + matrix16.M23) > 3.0f)
                        {
                            xyz.X = matrix16.M21;
                            xyz.Y = matrix16.M22;
                            xyz.Z = matrix16.M23;
                        }
                        else if (Math.Abs(matrix16.M31 + matrix16.M32 + matrix16.M33) > 3.0f)
                        {
                            xyz.X = matrix16.M31;
                            xyz.Y = matrix16.M32;
                            xyz.Z = matrix16.M33;
                        }
                        else if (Math.Abs(matrix16.M41 + matrix16.M42 + matrix16.M43) > 3.0f)
                        {
                            xyz.X = matrix16.M41;
                            xyz.Y = matrix16.M42;
                            xyz.Z = matrix16.M43;
                        }
                    }
                    else { xyz = Memory.Read<Vector>(pClientSoldierEntity + (long)0x0990); }
                       
                    
                    var offset = Memory.Read<long>(_baseAddress + 0x11A8);
                offset = Memory.Read<long>(offset + 0x28);
                var _kit = Memory.ReadString(offset, 64);

                for (int j = 0; j < 8; j++)
                    _weaponSlot[j] = string.Empty;

                var _pClientVehicleEntity = Memory.Read<long>(_baseAddress + 0x1D38);
                if (Memory.IsValid(_pClientVehicleEntity))//测试时改为true
                    {
                    var _pVehicleHealthComponent = Memory.Read<long>(_pClientVehicleEntity + 0x1D0);
                    if (!Memory.IsValid(_pVehicleHealthComponent))
                        goto NOWEAPON;
                    var _health = Memory.Read<float>(_pVehicleHealthComponent + 0x40);
                    if (_health <= 0)
                        goto NOWEAPON;

                    var _pVehicleEntityData = Memory.Read<long>(_pClientVehicleEntity + 0x30);
                    _weaponSlot[0] = Memory.ReadString(Memory.Read<long>(_pVehicleEntityData + 0x2F8), 64);

                    for (int j = 0; j < 100; j++)
                    {
                        var tempMultiUnlockAsset = Memory.Read<long>(_baseAddress + j * 0x8 + 0x13A8);
                        if (!Memory.IsValid(tempMultiUnlockAsset))
                            continue;

                        var vtable = Memory.Read<long>(tempMultiUnlockAsset);
                        if (vtable == 0x142B8E188)
                        {
                            var tempVehicleName = Memory.ReadString(Memory.Read<long>(tempMultiUnlockAsset + 0x20), 64);
                            if (FixVehicleKits(_weaponSlot[0], tempVehicleName))
                            {
                                _weaponSlot[1] = tempVehicleName;
                                break;
                            }
                        }
                    }
                        // 载具武器组件数量
                        var componentCount = Memory.Read<byte>(pClientVehicleEntity + 0x570 + 0x09);
                        // 临时存放载具武器名称
                        var componentNames = new List<string>();
                        for (var count = 0; count < componentCount; count++)
                        {
                            var testActive = Memory.Read<short>(pClientVehicleEntity + 0x570 + count * 0x20 - 0x08);
                            if (testActive != 2056)
                                continue;

                            var pointer = Memory.Read<long>(pClientVehicleEntity + 0x570 + count * 0x20);
                            if (!Memory.IsValid(pointer))
                                continue;
                            var weaponComponentData = Memory.Read<long>(pointer + 0x10);
                            if (!Memory.IsValid(weaponComponentData))
                                continue;
                            var weaponPointer = Memory.Read<long>(weaponComponentData + 0x120);
                            if (!Memory.IsValid(weaponPointer))
                                continue;

                            var weaponName = Memory.ReadString(weaponPointer, 64);
                            if (!string.IsNullOrWhiteSpace(weaponName))
                                componentNames.Add(weaponName);
                        }

                        _weaponSlot[2] = string.Join(",", componentNames);
                    }
                else
                {
                    var _pClientSoldierEntity = Memory.Read<long>(_baseAddress + 0x1D48);
                    if (!Memory.IsValid(_pClientSoldierEntity))
                        _kit = "";

                    var _pSoldierHealthComponent = Memory.Read<long>(_pClientSoldierEntity + 0x1D0);
                    if (!Memory.IsValid(_pSoldierHealthComponent))
                        goto NOWEAPON;
                    var _health = Memory.Read<float>(_pSoldierHealthComponent + 0x40);
                    if (_health <= 0)
                        goto NOWEAPON;

                    var _pClientSoldierWeaponComponent = Memory.Read<long>(_pClientSoldierEntity + 0x698);
                    var _m_handler = Memory.Read<long>(_pClientSoldierWeaponComponent + 0x8A8);

                    for (int j = 0; j < 8; j++)
                    {
                        var offset0 = Memory.Read<long>(_m_handler + j * 0x8);
                        offset0 = Memory.Read<long>(offset0 + 0x4A30);
                        offset0 = Memory.Read<long>(offset0 + 0x20);
                        offset0 = Memory.Read<long>(offset0 + 0x38);
                        offset0 = Memory.Read<long>(offset0 + 0x20);
                        _weaponSlot[j] = Memory.ReadString(offset0, 64);
                    }
                }

            NOWEAPON:
                var index = _playerList.FindIndex(val => val.PersonaId == _personaId);
                if (index == -1)
                {
                        _playerList.Add(new()
                        {
                            Mark = _mark,
                            TeamId = _teamId,
                            Spectator = _spectator,
                            Clan = _clan,
                            Name = _name,
                            PersonaId = _personaId,
                            SquadId = _squadId,
                            Kit = _kit,
                            X = xyz.X,
                            Y = xyz.Y,
                            Z = xyz.Z,
                            Rank = 0,
                            Kill = 0,
                            Dead = 0,
                            Score = 0,

                            WeaponS0 = _weaponSlot[0],
                            WeaponS1 = _weaponSlot[1],
                            WeaponS2 = _weaponSlot[2],
                            WeaponS3 = _weaponSlot[3],
                            WeaponS4 = _weaponSlot[4],
                            WeaponS5 = _weaponSlot[5],
                            WeaponS6 = _weaponSlot[6],
                            WeaponS7 = _weaponSlot[7],
                        });
                }
            }

            //////////////////////////////// 得分板数据 ////////////////////////////////

            var _pClientScoreBA = Memory.Read<long>(Memory.Bf1ProBaseAddress2 + 0x39EB8D8);
            _pClientScoreBA = Memory.Read<long>(_pClientScoreBA + 0x68);

            for (int i = 0; i < MaxPlayer; i++)
            {
                _pClientScoreBA = Memory.Read<long>(_pClientScoreBA);
                var _pClientScoreOffset = Memory.Read<long>(_pClientScoreBA + 0x10);
                if (!Memory.IsValid(_pClientScoreOffset))
                    continue;

                var _mark = Memory.Read<byte>(_pClientScoreOffset + 0x300);
                var _rank = Memory.Read<int>(_pClientScoreOffset + 0x304);
                var _kill = Memory.Read<int>(_pClientScoreOffset + 0x308);
                var _dead = Memory.Read<int>(_pClientScoreOffset + 0x30C);
                var _score = Memory.Read<int>(_pClientScoreOffset + 0x314);

                var index = _playerList.FindIndex(val => val.Mark == _mark);
                if (index != -1)
                {
                    _playerList[index].Rank = _rank;
                    _playerList[index].Kill = _kill;
                    _playerList[index].Dead = _dead;
                    _playerList[index].Score = _score;
                }
            }


            return _playerList;
        }
    }

    /// <summary>
    /// 修复载具分类
    /// </summary>
    /// <param name="name1"></param>
    /// <param name="name2"></param>
    /// <returns></returns>
    private static bool FixVehicleKits(string name1, string name2)
    {
        switch (name1)
        {
            // 巡航坦克
            case "ID_P_VNAME_MARKV":
                if (name2 == "U_GBR_MarkV_Package_Mortar" || name2 == "U_GBR_MarkV_Package_AntiTank" || name2 == "U_GBR_MarkV_Package_SquadSupport")
                    return true;
                else
                    return false;
            // 重型坦克
            case "ID_P_VNAME_A7V":
                if (name2 == "U_GER_A7V_Package_Assault" || name2 == "U_GER_A7V_Package_Breakthrough" || name2 == "U_GER_A7V_Package_Flamethrower")
                    return true;
                else
                    return false;
            // 轻型坦克
            case "ID_P_VNAME_FT17":
                if (name2 == "U_FRA_FT_Package_37mm" || name2 == "U_FRA_FT_Package_20mm" || name2 == "U_FRA_FT_Package_75mm")
                    return true;
                else
                    return false;
            // 火炮装甲车
            case "ID_P_VNAME_ARTILLERYTRUCK":
                if (name2 == "U_GBR_PierceArrow_Package_Artillery" || name2 == "U_GBR_PierceArrow_Package_AntiAircraft" || name2 == "U_GBR_PierceArrow_Package_Mortar")
                    return true;
                else
                    return false;
            // 攻击坦克
            case "ID_P_VNAME_STCHAMOND":
                if (name2 == "U_FRA_StChamond_Package_Assault" || name2 == "U_FRA_StChamond_Package_Gas" || name2 == "U_FRA_StChamond_Package_Standoff")
                    return true;
                else
                    return false;
            // 突袭装甲车
            case "ID_P_VNAME_ASSAULTTRUCK":
                if (name2 == "U_RU_PutilovGarford_Package_AssaultGun" || name2 == "U_RU_PutilovGarford_Package_AntiVehicle" || name2 == "U_RU_PutilovGarford_Package_Recon")
                    return true;
                else
                    return false;
            // 攻击机
            case "ID_P_VNAME_HALBERSTADT":
            case "ID_P_VNAME_BRISTOL":
            case "ID_P_VNAME_SALMSON":
            case "ID_P_VNAME_RUMPLER":
                if (name2 == "U_2Seater_Package_GroundSupport" || name2 == "U_2Seater_Package_TankHunter" || name2 == "U_2Seater_Package_AirshipBuster")
                    return true;
                else
                    return false;
            // 轰炸机
            case "ID_P_VNAME_GOTHA":
            case "ID_P_VNAME_CAPRONI":
            case "ID_P_VNAME_DH10":
            case "ID_P_VNAME_HBG1":
                if (name2 == "U_Bomber_Package_Barrage" || name2 == "U_Bomber_Package_Firestorm" || name2 == "U_Bomber_Package_Torpedo")
                    return true;
                else
                    return false;
            // 战斗机
            case "ID_P_VNAME_SPAD":
            case "ID_P_VNAME_SOPWITH":
            case "ID_P_VNAME_DR1":
            case "ID_P_VNAME_ALBATROS":
                if (name2 == "U_Scout_Package_Dogfighter" || name2 == "U_Scout_Package_BomberKiller" || name2 == "U_Scout_Package_TrenchFighter")
                    return true;
                else
                    return false;
            // 重型轰炸机
            case "ID_P_VNAME_ILYAMUROMETS":
                if (name2 == "U_HeavyBomber_Package_Strategic" || name2 == "U_HeavyBomber_Package_Demolition" || name2 == "U_HeavyBomber_Package_Support")
                    return true;
                else
                    return false;
            // 飞船
            case "ID_P_VNAME_ASTRATORRES":
                if (name2 == "U_CoastalAirship_Package_Observation" || name2 == "U_CoastalAirship_Package_Raider")
                    return true;
                else
                    return false;
            // 驱逐舰
            case "ID_P_VNAME_HMS_LANCE":
                if (name2 == "U_HMS_Lance_Package_Destroyer" || name2 == "U_HMS_Lance_Package_Minelayer")
                    return true;
                else
                    return false;
            default:
                return false;
        }
    }
}
