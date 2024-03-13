namespace BF1ServerTools.Configs;

public class RuleConfig
{
    public int SelectedIndex { get; set; }
    public List<RuleInfo> RuleInfos { get; set; }
    public class RuleInfo
    {
        public string RuleName { get; set; }
        public bool WhiteLifeKD { get; set; }
        public bool WhiteLifeKPM { get; set; }
        public bool WhiteLifeWeaponStar { get; set; }
        public bool WhiteLifeVehicleStar { get; set; }
        public bool WhiteKill { get; set; }
        public bool WhiteKD { get; set; }
        public bool WhiteKPM { get; set; }
        public bool WhiteRank { get; set; }
        public bool WhiteWeapon { get; set; }
        public Rule Team1Rule { get; set; }
        public Rule Team2Rule { get; set; }
        public List<string> Team1Weapon { get; set; }
        public List<string> Team2Weapon { get; set; }
        public List<string> BlackList { get; set; }
        public List<string> WhiteList { get; set; }
        public class Rule
        {
            public int MaxKill { get; set; }
            public int FlagKD { get; set; }
            public float MaxKD { get; set; }
            public int FlagKPM { get; set; }
            public float MaxKPM { get; set; }
            public int MinRank { get; set; }
            public int MaxRank { get; set; }
            public float LifeMaxKD { get; set; }
            public float LifeMaxKPM { get; set; }
            public int LifeMaxWeaponStar { get; set; }
            public int LifeMaxVehicleStar { get; set; }
        }
    }
}
