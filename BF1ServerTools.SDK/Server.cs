using BF1ServerTools.SDK.Core;

namespace BF1ServerTools.SDK;

public static class Server
{
    /// <summary>
    /// 获取服务器名称
    /// </summary>
    /// <returns></returns>
    public static string GetServerName()
    {
        long pointer = Memory.Read<long>(0x143A20898);
        pointer = Memory.Read<long>(pointer + 0x30);
        return Memory.ReadString(pointer, 64);
    }

    /// <summary>
    /// 获取服务器数字Id
    /// </summary>
    /// <returns></returns>
    public static long GetGameId()
    {
        long pointer = Memory.Read<long>(0x143A20898);
        return Memory.Read<long>(pointer + 0x100);
    }

    /// <summary>
    /// 获取服务器地图名称
    /// </summary>
    /// <returns></returns>
    public static string GetMapName()
    {
        long pointer = Memory.Read<long>(Offsets.OFFSET_CLIENTGAMECONTEXT);
        pointer = Memory.Read<long>(pointer + 0x30);
        pointer = Memory.Read<long>(pointer + 0x18);
        pointer = Memory.Read<long>(pointer + 0xB0);
        return Memory.ReadString(pointer, 64);
    }

    /// <summary>
    /// 获取服务器游戏模式
    /// </summary>
    /// <returns></returns>
    public static string GetGameMode()
    {
        long pointer = Memory.Read<long>(5427429792);
        pointer = Memory.Read<long>(pointer + 0x648);
        return Memory.ReadString(pointer, 64);
    }

    /// <summary>
    /// 获取服务器时间
    /// </summary>
    /// <returns></returns>
    public static float GetServerTime()
    {
        long pointer = Memory.Read<long>(Memory.Bf1ProBaseAddress + Offsets.Offset_ServerScore);
        pointer = Memory.Read<long>(pointer + 0x10);
        pointer = Memory.Read<long>(pointer + 0x88);
        pointer = Memory.Read<long>(pointer + 0x20);
    
        return Memory.Read<float>(pointer + 0x50);
    }

    /// <summary>
    /// 获取服务器分数指针
    /// </summary>
    /// <returns></returns>
    public static long GetServerScorePtr()
    {
        //Memory.Bf1ProBaseAddress + Offsets.Offset_ServerScore);
        long pointer = Memory.Read<long>(Memory.Bf1ProBaseAddress + Offsets.Offset_ServerScore);
        
        pointer = Memory.Read<long>(pointer + 0x10);
        pointer = Memory.Read<long>(pointer + 0x88);

        long a = Memory.Read<long>(pointer + 0x20);
        return a;
    }

    /// <summary>
    /// 获取服务器最大比分
    /// </summary>
    /// <returns></returns>
    public static int GetServerMaxScore()
    {
        return Memory.Read<int>(GetServerScorePtr() + 0x1E0);
    }

    /// <summary>
    /// 获取服务器队伍1分数
    /// </summary>
    /// <returns></returns>
    public static int GetTeam1Score()
    {
        int a=Memory.Read<int>(GetServerScorePtr() + 0x28);
        if (a < 2001 && a >= 0)
        { return a; }
        else { return 0; }
    }

    /// <summary>
    /// 获取服务器队伍2分数
    /// </summary>
    /// <returns></returns>
    public static int GetTeam2Score()
    {
        int a=Memory.Read<int>(GetServerScorePtr() + 0x88);
        if (a < 2001 && a >= 0)
        { return a; }
        else { return 0; }
    }

    /// <summary>
    /// 获取服务器队伍1从击杀获取得分
    /// </summary>
    /// <returns></returns>
    public static int GetTeam1KillScore()
    {
        int a = Memory.Read<int>(GetServerScorePtr() + 0x230);
        if (a < 2001 && a >= 0)
        { return a; }
        else { return 0; }
    }

    /// <summary>
    /// 获取服务器队伍2从击杀获取得分
    /// </summary>
    /// <returns></returns>
    public static int GetTeam2KillScore()
    {
        int a= Memory.Read<int>(GetServerScorePtr() + 0x238);
        if (a < 2001 && a >= 0)
        { return a; }
        else { return 0; }
    }

    /// <summary>
    /// 获取服务器队伍1从旗帜获取得分
    /// </summary>
    /// <returns></returns>
    public static int GetTeam1FlagScore()
    {
        int a = Memory.Read<int>(GetServerScorePtr() + 0x250);
       if(a<2001&&a>=0)
        { return a; }
       else { return 0; }
    }

    /// <summary>
    /// 获取服务器队伍2从旗帜获取得分
    /// </summary>
    /// <returns></returns>
    public static int GetTeam2FlagScore()
    {
        int a = Memory.Read<int>(GetServerScorePtr() + 0x258);
        if (a < 2001 && a >= 0)
        { return a; }
        else { return 0; }
    }
}
