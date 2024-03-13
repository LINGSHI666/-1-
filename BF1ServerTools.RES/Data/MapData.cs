namespace BF1ServerTools.RES.Data;

public static class MapData
{
    private const string AHU = "奥匈帝国";
    private const string GER = "德意志帝国";
    private const string OTM = "奥斯曼帝国";
    private const string ITA = "意大利王国";
    private const string UK = "大英帝国";
    private const string RM = "皇家海军陆战队";
    private const string USA = "美国";
    private const string FRA = "法国";
    private const string RUS = "俄罗斯帝国";
    private const string BOL = "苏联红军";

    public class MapName
    {
        public string English;
        public string Chinese;
        public string Team1;
        public string Team2;
        public string Team1Image;
        public string Team2Image;
        public string Image;
    }

    /// <summary>
    /// 地图数据
    /// </summary>
    public readonly static List<MapName> AllMapInfo = new()
    {
        new() { English="ID_M_LEVEL_MENU", Chinese="大厅菜单", Team1="_DEF", Team2="_DEF", Team1Image="_DEF", Team2Image="_DEF" },
        new() { English="ID_M_MP_LEVEL_MOUNTAIN_FORT", Chinese="格拉巴山", Team1=ITA, Team2=AHU, Team1Image="ITA", Team2Image="AHU", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_MountainFort_LandscapeLarge-8a517533.jpg" },
        new() { English="ID_M_MP_LEVEL_FOREST", Chinese="阿尔贡森林", Team1=USA, Team2=GER, Team1Image="USA", Team2Image="GER", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Forest_LandscapeLarge-dfbbe910.jpg" },
        new() { English="ID_M_MP_LEVEL_ITALIAN_COAST", Chinese="帝国边境", Team1=ITA, Team2=AHU, Team1Image="ITA", Team2Image="AHU", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_ItalianCoast_LandscapeLarge-1503eec7.jpg" },
        new() { English="ID_M_MP_LEVEL_CHATEAU", Chinese="流血宴厅", Team1=USA, Team2=GER, Team1Image="USA", Team2Image="GER", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Chateau_LandscapeLarge-244d5987.jpg" },
        new() { English="ID_M_MP_LEVEL_SCAR", Chinese="圣康坦的伤痕", Team1=GER, Team2=UK, Team1Image="GER", Team2Image="UK", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Scar_LandscapeLarge-ee25fbd6.jpg" },
        new() { English="ID_M_MP_LEVEL_DESERT", Chinese="西奈沙漠", Team1=UK, Team2=OTM, Team1Image="UK", Team2Image="OTM", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Desert_LandscapeLarge-d8f749da.jpg" },
        new() { English="ID_M_MP_LEVEL_AMIENS", Chinese="亚眠", Team1=GER, Team2=UK, Team1Image="GER", Team2Image="UK", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Amiens_LandscapeLarge-e195589d.jpg" },
        new() { English="ID_M_MP_LEVEL_SUEZ", Chinese="苏伊士", Team1=UK, Team2=OTM, Team1Image="UK", Team2Image="OTM", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Suez_LandscapeLarge-f630fc76.jpg" },
        new() { English="ID_M_MP_LEVEL_FAO_FORTRESS", Chinese="法欧堡", Team1=UK, Team2=OTM, Team1Image="UK", Team2Image="OTM", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_FaoFortress_LandscapeLarge-cad1748e.jpg" },
        new() { English="ID_M_MP_LEVEL_GIANT", Chinese="庞然暗影", Team1=UK, Team2=GER, Team1Image="UK", Team2Image="GER", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Giant_LandscapeLarge-dd0b93ef.jpg" },
        new() { English="ID_M_MP_LEVEL_FIELDS", Chinese="苏瓦松", Team1=FRA, Team2=GER, Team1Image="FRA", Team2Image="GER", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Fields_LandscapeLarge-5f53ddc4.jpg" },
        new() { English="ID_M_MP_LEVEL_GRAVEYARD", Chinese="决裂", Team1=FRA, Team2=GER, Team1Image="FRA", Team2Image="GER", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Graveyard_LandscapeLarge-bd1012e6.jpg" },
        new() { English="ID_M_MP_LEVEL_UNDERWORLD", Chinese="法乌克斯要塞", Team1=GER, Team2=FRA, Team1Image="GER", Team2Image="FRA", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Underworld_LandscapeLarge-b6c5c7e7.jpg" },
        new() { English="ID_M_MP_LEVEL_VERDUN", Chinese="凡尔登高地", Team1=GER, Team2=FRA, Team1Image="GER", Team2Image="FRA", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Verdun_LandscapeLarge-1a364063.jpg" },
        new() { English="ID_M_MP_LEVEL_TRENCH", Chinese="尼维尔之夜", Team1=GER, Team2=FRA, Team1Image="GER", Team2Image="FRA", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Trench_LandscapeLarge-dbd1248f.jpg" },
        new() { English="ID_M_MP_LEVEL_SHOVELTOWN", Chinese="攻占托尔", Team1=GER, Team2=FRA, Team1Image="GER", Team2Image="FRA", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Shoveltown_LandscapeLarge-d0aa5920.jpg" },
        new() { English="ID_M_MP_LEVEL_BRIDGE", Chinese="勃鲁希洛夫关口", Team1=RUS, Team2=AHU, Team1Image="RUS", Team2Image="AHU", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Bridge_LandscapeLarge-5b7f1b62.jpg" },
        new() { English="ID_M_MP_LEVEL_ISLANDS", Chinese="阿尔比恩", Team1=GER, Team2=RUS, Team1Image="GER", Team2Image="RUS", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Islands_LandscapeLarge-c9d8272b.jpg" },
        new() { English="ID_M_MP_LEVEL_RAVINES", Chinese="武普库夫山口", Team1=AHU, Team2=RUS, Team1Image="AHU", Team2Image="RUS", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Ravines_LandscapeLarge-1fe0d3f6.jpg" },
        new() { English="ID_M_MP_LEVEL_VALLEY", Chinese="加利西亚", Team1=RUS, Team2=AHU, Team1Image="RUS", Team2Image="AHU", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Valley_LandscapeLarge-8dc1c7ca.jpg" },
        new() { English="ID_M_MP_LEVEL_TSARITSYN", Chinese="察里津", Team1=BOL, Team2=RUS, Team1Image="BOL", Team2Image="RUS", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Tsaritsyn_LandscapeLarge-2dbd3bf5.jpg" },
        new() { English="ID_M_MP_LEVEL_VOLGA", Chinese="窝瓦河", Team1=BOL, Team2=RUS, Team1Image="BOL", Team2Image="RUS", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Volga_LandscapeLarge-6ac49c25.jpg" },
        new() { English="ID_M_MP_LEVEL_BEACHHEAD", Chinese="海丽丝岬", Team1=UK, Team2=OTM, Team1Image="UK", Team2Image="OTM", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Beachhead_LandscapeLarge-5a13c655.jpg" },
        new() { English="ID_M_MP_LEVEL_HARBOR", Chinese="泽布吕赫", Team1=RM, Team2=GER, Team1Image="RM", Team2Image="GER", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Harbor_LandscapeLarge-d382c7ea.jpg" },
        new() { English="ID_M_MP_LEVEL_NAVAL", Chinese="黑尔戈兰湾", Team1=RM, Team2=GER, Team1Image="RM", Team2Image="GER", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Naval_LandscapeLarge-dc2e8daf.jpg" },
        new() { English="ID_M_MP_LEVEL_RIDGE", Chinese="阿奇巴巴", Team1=UK, Team2=OTM, Team1Image="UK", Team2Image="OTM", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Ridge_LandscapeLarge-8c057a19.jpg" },
        new() { English="ID_M_MP_LEVEL_OFFENSIVE", Chinese="索姆河", Team1=UK, Team2=GER, Team1Image="UK", Team2Image="GER", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Offensive_LandscapeLarge-6dabdea3.jpg" },
        new() { English="ID_M_MP_LEVEL_HELL", Chinese="帕斯尚尔", Team1=UK, Team2=GER, Team1Image="UK", Team2Image="GER", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Hell_LandscapeLarge-7176911c.jpg" },
        new() { English="ID_M_MP_LEVEL_RIVER", Chinese="卡波雷托", Team1=AHU, Team2=ITA, Team1Image="AHU", Team2Image="ITA", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_River_LandscapeLarge-21443ae9.jpg" },
        new() { English="ID_M_MP_LEVEL_ALPS", Chinese="剃刀边缘", Team1=GER, Team2=UK, Team1Image="GER", Team2Image="UK", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Alps_LandscapeLarge-7ab30e3e.jpg" },
        new() { English="ID_M_MP_LEVEL_BLITZ", Chinese="伦敦的呼唤：夜袭", Team1=GER, Team2=UK, Team1Image="GER", Team2Image="UK", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_Blitz_LandscapeLarge-5e26212f.jpg" },
        new() { English="ID_M_MP_LEVEL_LONDON", Chinese="伦敦的呼唤：灾祸", Team1=GER, Team2=UK, Team1Image="GER", Team2Image="UK", Image=@"\BF1ServerTools.RES;component\Assets\Images\Maps\MP_London_LandscapeLarge-0b51fe46.jpg" }
    };
}
