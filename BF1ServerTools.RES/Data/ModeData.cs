namespace BF1ServerTools.RES.Data;

public static class ModeData
{
    public struct ModeName
    {
        public string English;
        public string Chinese;
        public string Mark;
    }

    /// <summary>
    /// 地图模式数据
    /// </summary>
    public readonly static List<ModeName> AllModeInfo = new()
    {
        new() { English="ID_M_GAMEMODE_ZONECONTROL", Chinese="空降补给", Mark="ZoneControl0" },
        new() { English="ID_M_GAMEMODE_AIRASSAULT", Chinese="空中突袭", Mark="AirAssault0" },
        new() { English="ID_M_GAMEMODE_TUGOFWAR", Chinese="前线", Mark="TugOfWar0" },
        new() { English="ID_M_GAMEMODE_DOMINATION", Chinese="抢攻", Mark="Domination0" },
        new() { English="ID_M_GAMEMODE_BREAKTHROUGH", Chinese="闪击行动", Mark="Breakthrough0" },
        new() { English="ID_M_GAMEMODE_RUSH", Chinese="突袭", Mark="Rush0" },
        new() { English="ID_M_GAMEMODE_TEAMDEATHMATCH", Chinese="团队死斗", Mark="TeamDeathMatch0" },
        new() { English="ID_M_GAMEMODE_BREAKTHROUGHLARGE", Chinese="行动模式", Mark="BreakthroughLarge0" },
        new() { English="ID_M_GAMEMODE_POSSESSION", Chinese="战争信鸽", Mark="Possession0" },
        new() { English="ID_M_GAMEMODE_CONQUEST", Chinese="征服", Mark="Conquest0" }
    };
    public readonly static List<ModeName> AllModeInfo2 = new()
{
    new() { English="ZoneControl", Chinese="空降补给", Mark="ZoneControl0" },
    new() { English="AirAssault", Chinese="空中突袭", Mark="AirAssault0" },
    new() { English="TugOfWar", Chinese="前线", Mark="TugOfWar0" },
    new() { English="Domination", Chinese="抢攻", Mark="Domination0" },
    new() { English="Breakthrough", Chinese="闪击行动", Mark="Breakthrough0" },
    new() { English="Rush", Chinese="突袭", Mark="Rush0" },
    new() { English="TeamDeathMatch", Chinese="团队死斗", Mark="TeamDeathMatch0" },
    new() { English="BreakthroughLarge", Chinese="行动模式", Mark="BreakthroughLarge0" },
    new() { English="Possession", Chinese="战争信鸽", Mark="Possession0" },
    new() { English="Conquest", Chinese="征服", Mark="Conquest0" }
};
}
