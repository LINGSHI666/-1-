using BF1ServerTools.Helper;

namespace BF1ServerTools.Utils;

public static class PlayerUtil
{
    /// <summary>
    /// 线程锁
    /// </summary>
    private static readonly object Obj = new();

    /// <summary>
    /// 获取玩家游玩时间，返回分钟数或小时数
    /// </summary>
    /// <param name="second"></param>
    /// <returns></returns>
    public static string GetPlayTime(double second)
    {
        var ts = TimeSpan.FromSeconds(second);

        if (ts.TotalHours < 1)
            return ts.TotalMinutes.ToString("0") + " 分钟";

        return ts.TotalHours.ToString("0") + " 小时";
    }

    /// <summary>
    /// 获取游玩小时数
    /// </summary>
    /// <param name="second"></param>
    /// <returns></returns>
    public static int GetPlayHours(double second)
    {
        var ts = TimeSpan.FromSeconds(second);
        return (int)ts.TotalHours;
    }

    /// <summary>
    /// 计算玩家KD
    /// </summary>
    /// <param name="kill">玩家击杀数</param>
    /// <param name="dead">玩家死亡数</param>
    /// <returns>返回玩家KD（小数float）<returns>
    public static float GetPlayerKD(int kill, int dead)
    {
        if (kill == 0 && dead >= 0)
            return 0.0f;
        else if (kill > 0 && dead == 0)
            return kill;
        else if (kill > 0 && dead > 0)
            return (float)kill / dead;
        else
            return (float)kill / dead;
    }

    /// <summary>
    /// 计算玩家KPM
    /// </summary>
    /// <param name="kill"></param>
    /// <param name="minute"></param>
    /// <returns></returns>
    public static float GetPlayerKPM(int kill, float minute)
    {
        if (minute != 0.0f)
            return kill / minute;
        else
            return 0.0f;
    }

    /// <summary>
    /// 获取玩家KPM
    /// </summary>
    /// <param name="kill"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static string GetPlayerKPM(float kill, float time)
    {
        if (time < 60)
        {
            return "0.00";
        }
        else
        {
            var minute = (int)(time / 60);
            return $"{kill / minute:0.00}";
        }
    }

    /// <summary>
    /// 计算百分比
    /// </summary>
    /// <param name="num1"></param>
    /// <param name="num2"></param>
    /// <returns></returns>
    public static string GetPlayerPercentage(float num1, float num2)
    {
        if (num2 != 0)
            return $"{num1 / num2 * 100:0.00}%";
        else
            return "0%";
    }

    /// <summary>
    /// 获取击杀星数
    /// </summary>
    /// <param name="kills"></param>
    /// <returns></returns>
    public static string GetKillStar(int kills)
    {
        if (kills < 100)
            return "";
        else
            return $"{kills / 100:0}";
    }

    /// <summary>
    /// 小数类型的时间秒，转为mm:ss格式
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static string SecondsToMMSS(float time)
    {
        var mm = time / 60;
        var ss = time % 60;

        return $"{mm:00}:{ss:00}";
    }

    /// <summary>
    /// 修正服务器得分数据
    /// </summary>
    /// <param name="score"></param>
    /// <returns></returns>
    public static int FixedServerScore(int score)
    {
        return score < 0 || score > 2000 ? 0 : score;
    }

    /// <summary>
    /// 修正服务器得分数据
    /// </summary>
    /// <param name="score"></param>
    /// <returns></returns>
    public static double FixedServerScore(double score)
    {
        return score < 0 || score > 125 ? 0 : score;
    }

    /// <summary>
    /// 小数类型的时间秒，转为分钟
    /// </summary>
    /// <param name="second"></param>
    /// <returns></returns>
    public static int SecondsToMinute(float second)
    {
        if (second >= 0 && second <= 36000)
            return (int)(second / 60);
        else
            return 0;
    }

    /// <summary>
    /// 判断玩家是不是管理员、VIP
    /// </summary>
    /// <param name="personaId"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public static bool IsAdminVIP(long personaId, List<long> list)
    {
        return list.IndexOf(personaId) != -1;
    }

    /// <summary>
    /// 判断玩家是不是白名单
    /// </summary>
    /// <param name="personaId"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public static bool IsWhite(string name, List<string> list)
    {
        return list.IndexOf(name) != -1;
    }

    /// <summary>
    /// 获取生涯KD
    /// </summary>
    /// <param name="personaId"></param>
    /// <returns></returns>
    public static float GetLifeKD(long personaId)
    {
        lock (Obj)
        {
            if (Globals.LifePlayerCacheDatas != null)
            {
                var index = Globals.LifePlayerCacheDatas.FindIndex(item => item.PersonaId == personaId);
                if (index != -1)
                    return Globals.LifePlayerCacheDatas[index].KD;
            }

            return 0;
        }
    }

    /// <summary>
    /// 获取生涯KPM
    /// </summary>
    /// <param name="personaId"></param>
    /// <returns></returns>
    public static float GetLifeKPM(long personaId)
    {
        lock (Obj)
        {
            if (Globals.LifePlayerCacheDatas != null)
            {
                var index = Globals.LifePlayerCacheDatas.FindIndex(item => item.PersonaId == personaId);
                if (index != -1)
                    return Globals.LifePlayerCacheDatas[index].KPM;
            }

            return 0;
        }
    }

    /// <summary>
    /// 获取游玩时长
    /// </summary>
    /// <param name="personaId"></param>
    /// <returns></returns>
    public static int GetLifeTime(long personaId)
    {
        lock (Obj)
        {
            if (Globals.LifePlayerCacheDatas != null)
            {
                var index = Globals.LifePlayerCacheDatas.FindIndex(item => item.PersonaId == personaId);
                if (index != -1)
                    return Globals.LifePlayerCacheDatas[index].Time;
            }

            return 0;
        }
    }

    /// <summary>
    /// 检查SessionId
    /// </summary>
    /// <returns></returns>
    public static bool CheckSId()
    {
        if (Globals.GameId == 0)
        {
            NotifierHelper.Show(NotifierType.Warning, "GameId为空，请先进入服务器");
            return false;
        }

        if (string.IsNullOrEmpty(Globals.SessionId))
        {
            NotifierHelper.Show(NotifierType.Warning, "请先获取玩家SessionId后，再执行本操作");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 检查SessionId和管理员授权
    /// </summary>
    /// <returns></returns>
    public static bool CheckAuth()
    {
        if (Globals.GameId == 0)
        {
            NotifierHelper.Show(NotifierType.Warning, "GameId为空，请先进入服务器");
            return false;
        }

        if (string.IsNullOrEmpty(Globals.SessionId))
        {
            NotifierHelper.Show(NotifierType.Warning, "请先获取玩家SessionId后，再执行本操作");
            return false;
        }

        if (!Globals.LoginPlayerIsAdmin)
        {
            NotifierHelper.Show(NotifierType.Warning, $"玩家 {Globals.DisplayName} 不是当前服务器的管理员");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 检查SessionId、管理员和ServerId
    /// </summary>
    /// <returns></returns>
    public static bool CheckAuth2()
    {
        if (Globals.GameId == 0)
        {
            NotifierHelper.Show(NotifierType.Warning, "GameId为空，请先进入服务器");
            return false;
        }

        if (string.IsNullOrEmpty(Globals.SessionId))
        {
            NotifierHelper.Show(NotifierType.Warning, "请先获取玩家SessionId后，再执行本操作");
            return false;
        }

        if (!Globals.LoginPlayerIsAdmin)
        {
            NotifierHelper.Show(NotifierType.Warning, $"玩家 {Globals.DisplayName} 不是当前服务器的管理员");
            return false;
        }

        if (Globals.ServerId == 0)
        {
            NotifierHelper.Show(NotifierType.Warning, "ServerId为空，请重新获取服务器详细信息");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 获取默认踢人中文原因
    /// </summary>
    /// <param name="reason"></param>
    /// <returns></returns>
    public static string GetDefaultChsReason(string reason)
    {
        return reason switch
        {
            "OFFENSIVEBEHAVIOR" => "DEFAULT 攻击性行为",
            "LATENCY" => "DEFAULT 延迟",
            "RULEVIOLATION" => "DEFAULT 违反规则",
            "GENERAL" => "DEFAULT 其他",
            _ => reason,
        };
    }
}
