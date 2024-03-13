namespace BF1ServerTools.RES.Data;

public static class WeaponData
{
    public class WeaponName
    {
        public string Kind;
        public string English;
        public string Chinese;
        public string ShortName;
        public string ImageName;
    }

    /// <summary>
    /// 全部武器信息，ShortName不超过16个字符
    /// </summary>
    public readonly static List<WeaponName> AllWeaponInfo = new()
    {
        // 配枪
        new WeaponName(){ Kind="公用配枪", English="U_M1911", Chinese="M1911", ShortName="M1911", ImageName="Colt1911-ed324bf1.png" },
        new WeaponName(){ Kind="公用配枪", English="U_LugerP08", Chinese="P08 手枪", ShortName="P08", ImageName="LugerP08-7f07aa2d.png" },
        new WeaponName(){ Kind="公用配枪", English="U_FN1903", Chinese="Mle 1903", ShortName="M1903", ImageName="Mle1903-a0fe1ec3.png" },
        new WeaponName(){ Kind="公用配枪", English="U_BorchardtC93", Chinese="C93", ShortName="C93", ImageName="Mle1903-a0fe1ec3.png" },
        new WeaponName(){ Kind="公用配枪", English="U_SmithWesson", Chinese="3 号左轮手枪", ShortName="No3 Rev", ImageName="SmithWesson-e26b4f24.png" },
        new WeaponName(){ Kind="公用配枪", English="U_Kolibri", Chinese="Kolibri", ShortName="Kolibri", ImageName="KolibriPistol-ec20b160.png" },
        new WeaponName(){ Kind="公用配枪", English="U_NagantM1895", Chinese="纳甘左轮手枪", ShortName="Nagant Rev", ImageName="NagantM1895-05035f4a.png" },
        new WeaponName(){ Kind="公用配枪", English="U_Obrez", Chinese="Obrez 手枪", ShortName="Obrez", ImageName="ObrezPistol-0c86b6ed.png" },
        new WeaponName(){ Kind="公用配枪", English="U_Webley_Mk6", Chinese="Mk VI 左轮手枪", ShortName="Mk VI", ImageName="Webley_MK6-da81b474.png" },
        new WeaponName(){ Kind="公用配枪", English="U_M1911_Preorder_Hellfighter", Chinese="地狱战士 M1911", ShortName="M1911 HF", ImageName="Colt1911-ed324bf1.png" },
        new WeaponName(){ Kind="公用配枪", English="U_LugerP08_Wep_Preorder", Chinese="红男爵的 P08", ShortName="P08 HNJ", ImageName="LugerP08-7f07aa2d.png" },
        new WeaponName(){ Kind="公用配枪", English="U_M1911_Suppressed", Chinese="M1911（消音器）", ShortName="M1911 XYQ", ImageName="M1911Silencer-d6c0e687.png" },
        new WeaponName(){ Kind="公用配枪", English="U_SingleActionArmy", Chinese="维和左轮 Peacekeeper", ShortName="Peacekeeper", ImageName="Colt_SAA-ef15294c.png" },
        new WeaponName(){ Kind="公用配枪", English="U_M1911_Preorder_Triforce", Chinese="步兵小子 M1911", ShortName="M1911 BBXZ", ImageName="Colt1911-ed324bf1.png" },

        // 手榴弹
        new WeaponName(){ Kind="手榴弹", English="U_GermanStick", Chinese="破片手榴弹", ShortName="German Stick", ImageName="GadgetFragmented-8c15152e.png" },
        new WeaponName(){ Kind="手榴弹", English="U_FragGrenade", Chinese="棒式手榴弹", ShortName="Frag Grenade", ImageName="GadgetFragmented-8c15152e.png" },
        new WeaponName(){ Kind="手榴弹", English="U_GasGrenade", Chinese="毒气手榴弹", ShortName="Gas Grenade", ImageName="GadgetGas-2bee4386.png" },
        new WeaponName(){ Kind="手榴弹", English="U_ImpactGrenade", Chinese="冲击手榴弹", ShortName="Impact Grenade", ImageName="GadgetImpact-f0c7f39e.png" },
        new WeaponName(){ Kind="手榴弹", English="U_Incendiary", Chinese="燃烧手榴弹", ShortName="Incendiary", ImageName="GadgetIncindiary-68d49a3a.png" },
        new WeaponName(){ Kind="手榴弹", English="U_MiniGrenade", Chinese="小型手榴弹", ShortName="Mini Grenade", ImageName="GadgetMiniOffensive-2d19e08a.png" },
        new WeaponName(){ Kind="手榴弹", English="U_SmokeGrenade", Chinese="烟雾手榴弹", ShortName="Smoke Grenade", ImageName="GadgetSmoke-af84f434.png" },
        new WeaponName(){ Kind="手榴弹", English="U_Grenade_AT", Chinese="轻型反坦克手榴弹", ShortName="Grenade AT", ImageName="GadgetTrooperATGrenade-a6575030.png" },
        new WeaponName(){ Kind="手榴弹", English="U_ImprovisedGrenade", Chinese="土制手榴弹", ShortName="Imsp Grenade", ImageName="ImprovisedGrenade-fea87071.png" },
        new WeaponName(){ Kind="手榴弹", English="U_RussianBox", Chinese="俄罗斯标准手榴弹", ShortName="Russian Box", ImageName="RU_Grenade-a7e29a54.png" },

        ////////////////////////////////// 突击兵 Assault //////////////////////////////////

        // 突击兵 主要武器
        new WeaponName(){ Kind="突击兵 主要武器", English="U_RemingtonM10_Wep_Slug", Chinese="Model 10-A（霰弹块）", ShortName="10A XDK", ImageName="RemingtonM10-08ab3f5b.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_RemingtonM10_Wep_Choke", Chinese="Model 10-A（猎人）", ShortName="10A LR", ImageName="RemingtonM10-08ab3f5b.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_RemingtonM10", Chinese="Model 10-A（原厂）", ShortName="10A YC", ImageName="RemingtonM10-08ab3f5b.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_Winchester1897_Wep_Sweeper", Chinese="M97 战壕枪（扫荡）", ShortName="M97 SD", ImageName="WinchesterM1897-bb453195.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_Winchester1897_Wep_LowRecoil", Chinese="M97 战壕枪（Back-Bored）", ShortName="M97 BB", ImageName="WinchesterM1897-bb453195.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_Winchester1897_Wep_Choke", Chinese="M97 战壕枪（猎人）", ShortName="M97 LR", ImageName="WinchesterM1897-bb453195.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_MP18_Wep_Trench", Chinese="MP 18（壕沟战）", ShortName="MP18 HGZ", ImageName="BergmannSchmeisserMP18-761af430.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_MP18_Wep_Burst", Chinese="MP 18（实验）", ShortName="MP18 SY", ImageName="BergmannSchmeisserMP18-761af430.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_MP18_Wep_Accuracy", Chinese="MP 18（瞄准镜）", ShortName="MP18 MZJ", ImageName="BergmannSchmeisserMP18-761af430.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_BerettaM1918_Wep_Trench", Chinese="M1918 自动冲锋枪（壕沟战）", ShortName="MP1918 HGZ", ImageName="Beretta1918-3daab991.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_BerettaM1918_Wep_Stability", Chinese="M1918 自动冲锋枪（冲锋）", ShortName="MP1918 CF", ImageName="Beretta1918-3daab991.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_BerettaM1918", Chinese="M1918 自动冲锋枪（原厂）", ShortName="MP1918 YC", ImageName="Beretta1918-3daab991.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_BrowningA5_Wep_LowRecoil", Chinese="12g 自动霰弹枪（Back-Bored）", ShortName="12g BB", ImageName="BrowingA5-95b260b4.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_BrowningA5_Wep_Choke", Chinese="12g 自动霰弹枪（猎人）", ShortName="12g LR", ImageName="BrowingA5-95b260b4.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_BrowningA5_Wep_ExtensionTube", Chinese="12g 自动霰弹枪（加长）", ShortName="12g JC", ImageName="BrowingA5-95b260b4.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_Hellriegel1915", Chinese="Hellriegel 1915（原厂）", ShortName="H1915 YC", ImageName="Hellriegel1915-e2513c1e.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_Hellriegel1915_Wep_Accuracy", Chinese="Hellriegel 1915（防御）", ShortName="H1915 FY", ImageName="Hellriegel1915-e2513c1e.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_Winchester1897_Wep_Preorder", Chinese="地狱战士战壕霰弹枪", ShortName="M97 DYZS", ImageName="WinchesterM1897-bb453195.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_SjogrenShotgun", Chinese="Sjögren Inertial（原厂）", ShortName="RDP YC", ImageName="SjogrenShotgun-e95b3db0.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_SjogrenShotgun_Wep_Slug", Chinese="Sjögren Inertial（霰弹块）", ShortName="RDP XDK", ImageName="SjogrenShotgun-e95b3db0.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_Ribeyrolles", Chinese="利贝罗勒 1918（原厂）", ShortName="L1918 YC", ImageName="Ribeyrolles-0e43197c.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_Ribeyrolles_Wep_Optical", Chinese="Ribeyrolles 1918（瞄准镜）", ShortName="L1918 MZJ", ImageName="Ribeyrolles-0e43197c.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_RemingtonModel1900", Chinese="Model 1900（原厂）", ShortName="M1900 YC", ImageName="RemingtonModel1900-e80b885b.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_RemingtonModel1900_Wep_Slug", Chinese="Model 1900（霰弹块）", ShortName="M1900 XDK", ImageName="RemingtonModel1900-e80b885b.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_MaximSMG", Chinese="SMG 08/18（原厂）", ShortName="SMG0818 YC", ImageName="MaximSMG-c3563db7.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_MaximSMG_Wep_Accuracy", Chinese="SMG 08/18（瞄准镜）", ShortName="SMG0818 MZJ", ImageName="MaximSMG-c3563db7.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_SteyrM1912_P16", Chinese="M1912/P.16（冲锋）", ShortName="M1912 P.16 CF", ImageName="SteyrM1912Stock-a1ad884f.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_SteyrM1912_P16_Wep_Burst", Chinese="Maschinenpistole M1912/P.16（实验）", ShortName="M1912 P.16 SY", ImageName="SteyrM1912Stock-a1ad884f.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_Mauser1917Trench", Chinese="M1917 战壕卡宾枪", ShortName="M1917 KBQ ZH", ImageName="MauserM1917TrenchCarbine-9a4158a1.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_Mauser1917Trench_Wep_Scope", Chinese="M1917 卡宾枪（巡逻）", ShortName="M1917 KBQ XL", ImageName="MauserM1917TrenchCarbine-9a4158a1.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_ChauchatSMG", Chinese="RSC 冲锋枪（原厂）", ShortName="RSC YC", ImageName="Chauchat-Ribeyrolles-4af8a912.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_ChauchatSMG_Wep_Optical", Chinese="RSC 冲锋枪（瞄准镜）", ShortName="RSC MZJ", ImageName="Chauchat-Ribeyrolles-4af8a912.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_M1919Thompson_Wep_Trench", Chinese="Annihilator（壕沟）", ShortName="Annihilator HG", ImageName="ThompsonAnnihilatorTr-1a660e74.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_M1919Thompson_Wep_Stability", Chinese="Annihilator（冲锋）", ShortName="Annihilator CF", ImageName="ThompsonAnnihilatorTr-1a660e74.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_M1919Thompson", Chinese="M1919 冲锋枪", ShortName="M1919", ImageName="M1919Thompson-1cf7343d.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_FrommerStopAuto", Chinese="费罗梅尔停止手枪（自动）", ShortName="FrommerStopAuto", ImageName="FrommerStop-506df97e.png" },
        new WeaponName(){ Kind="突击兵 主要武器", English="U_SawnOffShotgun", Chinese="短管霰弹枪", ShortName="SawnOffShotgun", ImageName="SawedOfShotgun-d31e0dd8.png" },

        // 突击兵 配枪
        new WeaponName(){ Kind="突击兵 配枪", English="U_GasserM1870", Chinese="加塞 M1870", ShortName="M1870", ImageName="GasserM1870-00471df4.png" },
        new WeaponName(){ Kind="突击兵 配枪", English="U_LancasterHowdah", Chinese="Howdah 手枪", ShortName="Howdah", ImageName="LancasterHowdah-9100578c.png" },
        new WeaponName(){ Kind="突击兵 配枪", English="U_Hammerless", Chinese="1903 Hammerless", ShortName="1903", ImageName="Hammerless-e61505d4.png" },

        // 突击兵 配备一二
        new WeaponName(){ Kind="突击兵 配备一二", English="U_Dynamite", Chinese="炸药", ShortName="Dynamite", ImageName="GadgetDynamite-b6283212.png" },
        new WeaponName(){ Kind="突击兵 配备一二", English="U_ATGrenade", Chinese="反坦克手榴弹", ShortName="ATGrenade", ImageName="GadgetATGrenade-4b135d46.png" },
        new WeaponName(){ Kind="突击兵 配备一二", English="U_ATMine", Chinese="反坦克地雷", ShortName="ATMine", ImageName="GadgetMine-527cef72.png" },
        new WeaponName(){ Kind="突击兵 配备一二", English="U_BreechGun", Chinese="反坦克火箭炮", ShortName="AT", ImageName="GadgetBreechgun-f2188c3f.png" },
        new WeaponName(){ Kind="突击兵 配备一二", English="U_BreechGun_Flak", Chinese="防空火箭炮", ShortName="AAT", ImageName="AA-Rocket-Gun-49a4e8d1.png" },

        ////////////////////////////////// 医疗兵 Medic //////////////////////////////////

        // 医疗兵 主要武器
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_CeiRigottiM1895_Wep_Trench", Chinese="Cei-Rigotti（壕沟战）", ShortName="M1895 HGZ", ImageName="CeiRigotti-8ae129e0.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_CeiRigottiM1895_Wep_Range", Chinese="Cei-Rigotti（瞄准镜）", ShortName="M1895 MZJ", ImageName="CeiRigotti-8ae129e0.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_CeiRigottiM1895", Chinese="Cei-Rigotti（原厂）", ShortName="M1895 YC", ImageName="CeiRigotti-8ae129e0.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_MauserSL1916_Wep_Scope", Chinese="Selbstlader M1916（神射手）", ShortName="M1916 SSS", ImageName="MauserSelbstladerM1916-c86e8775.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_MauserSL1916_Wep_Range", Chinese="Selbstlader M1916（瞄准镜）", ShortName="M1916 MZJ", ImageName="MauserSelbstladerM1916-c86e8775.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_MauserSL1916", Chinese="Selbstlader M1916（原厂）", ShortName="M1916 YC", ImageName="MauserSelbstladerM1916-c86e8775.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_WinchesterM1907_Wep_Trench", Chinese="M1907 半自动步枪（壕沟战）", ShortName="M1907 JGZ", ImageName="WinchesterM1907-3e99346c.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_WinchesterM1907_Wep_Auto", Chinese="M1907 半自动步枪（扫荡）", ShortName="M1907 SD", ImageName="WinchesterM1907-3e99346c.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_WinchesterM1907", Chinese="M1907 半自动步枪（原厂）", ShortName="M1907 YC", ImageName="WinchesterM1907-3e99346c.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_Mondragon_Wep_Range", Chinese="蒙德拉贡步枪（瞄准镜）", ShortName="Mondragon MZJ", ImageName="Mondragon-a3950be7.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_Mondragon_Wep_Stability", Chinese="蒙德拉贡步枪（冲锋）", ShortName="Mondragon CF", ImageName="Mondragon-a3950be7.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_Mondragon_Wep_Bipod", Chinese="蒙德拉贡步枪（狙击手）", ShortName="Mondragon JJS", ImageName="Mondragon-a3950be7.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_RemingtonModel8", Chinese="自动装填步枪 8.35（原厂）", ShortName="8.35 YC", ImageName="RemingtonM8_Special-398391d9.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_RemingtonModel8_Wep_Scope", Chinese="自动装填步枪 8.35（神射手）", ShortName="8.35 SSS", ImageName="RemingtonM8_Special-398391d9.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_RemingtonModel8_Wep_ExtendedMag", Chinese="自动装填步枪 8.25（加长）", ShortName="8.25 JC", ImageName="RemingtonM8_Special-398391d9.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_Luger1906", Chinese="Selbstlader 1906（原厂）", ShortName="1906 YC", ImageName="Luger1906-3238a6b3.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_Luger1906_Wep_Scope", Chinese="Selbstlader 1906（狙击手）", ShortName="1906 JJS", ImageName="Luger1906-3238a6b3.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_RSC1917_Wep_Range", Chinese="RSC 1917（瞄准镜）", ShortName="RSC 1917 MZJ", ImageName="RSC1917-35904a91.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_RSC1917", Chinese="RSC 1917（原厂）", ShortName="RSC 1917 YC", ImageName="RSC1917-35904a91.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_FedorovAvtomat_Wep_Trench", Chinese="费德洛夫自动步枪（壕沟战）", ShortName="Fedorov HGZ", ImageName="FederovAvtomat-aa228b15.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_FedorovAvtomat_Wep_Range", Chinese="费德洛夫自动步枪（瞄准镜）", ShortName="Fedorov MZJ", ImageName="FederovAvtomat-aa228b15.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_GeneralLiuRifle", Chinese="刘将军步枪（原厂）", ShortName="GeneralLiu YC", ImageName="GeneralLiu-f926d015.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_GeneralLiuRifle_Wep_Stability", Chinese="刘将军步枪（冲锋）", ShortName="GeneralLiu CF", ImageName="GeneralLiu-f926d015.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_FarquharHill_Wep_Range", Chinese="Farquhar-Hill 步枪（瞄准镜）", ShortName="Farquhar MZJ", ImageName="FarquharHill-11f5925b.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_FarquharHill_Wep_Stability", Chinese="Farquhar-Hill 步枪（冲锋）", ShortName="Farquhar CF", ImageName="FarquharHill-11f5925b.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_BSAHowellM1916", Chinese="Howell 自动步枪（原厂）", ShortName="Howell YC", ImageName="BSA_Howell-c3f2e18b.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_BSAHowellM1916_Wep_Scope", Chinese="Howell 自动步枪（狙击手）", ShortName="Howell JJS", ImageName="BSA_Howell-c3f2e18b.png" },
        new WeaponName(){ Kind="医疗兵 主要武器", English="U_FedorovDegtyarev", Chinese="费德洛夫 Degtyarev", ShortName="Fedorov SL", ImageName="FederovDegtyarev-ed497b9d.png" },

        // 医疗兵 配枪
        new WeaponName(){ Kind="医疗兵 配枪", English="U_WebFosAutoRev_455Webley", Chinese="自动左轮手枪", ShortName="Auto Rev", ImageName="WebleyFosberyAutoRevolver-a57ea28c.png" },
        new WeaponName(){ Kind="医疗兵 配枪", English="U_MauserC96", Chinese="C96", ShortName="C96", ImageName="MauserC96-52835b08.png" },
        new WeaponName(){ Kind="医疗兵 配枪", English="U_Mauser1914", Chinese="Taschenpistole M1914", ShortName="M1914", ImageName="Mauser1914-53a1954e.png" },

        // 医疗兵 配备一二
        new WeaponName(){ Kind="医疗兵 配备一二", English="U_Syringe", Chinese="医疗用针筒", ShortName="Syringe", ImageName="GadgetSyringe-e6c764c2.png" },
        new WeaponName(){ Kind="医疗兵 配备一二", English="U_MedicBag", Chinese="医护箱", ShortName="MedicBag", ImageName="GadgetMedicBag-159f240b.png" },
        new WeaponName(){ Kind="医疗兵 配备一二", English="U_Bandages", Chinese="绷带包", ShortName="Bandages", ImageName="GadgetBandages-1d1fc900.png" },
        new WeaponName(){ Kind="医疗兵 配备一二", English="_RGL_Frag", Chinese="步枪手榴弹（破片）", ShortName="RGL Frag", ImageName="MedicRifleLauncher_B-a712e224.png" },
        new WeaponName(){ Kind="医疗兵 配备一二", English="_RGL_Smoke", Chinese="步枪手榴弹（烟雾）", ShortName="RGL Smoke", ImageName="MedicRifleLauncher_A-438b725e.png" },
        new WeaponName(){ Kind="医疗兵 配备一二", English="_RGL_HE", Chinese="步枪手榴弹（高爆）", ShortName="RGL HE", ImageName="MedicRifleLauncher_B-a712e224.png" },

        ////////////////////////////////// 支援兵 Support //////////////////////////////////

        // 支援兵 主要武器
        new WeaponName(){ Kind="支援兵 主要武器", English="U_LewisMG_Wep_Suppression", Chinese="路易士机枪（压制）", ShortName="LewisMG YZ", ImageName="LewisLMG-832c29e8.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_LewisMG_Wep_Range", Chinese="路易士机枪（瞄准镜）", ShortName="LewisMG MZJ", ImageName="LewisLMG-832c29e8.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_LewisMG", Chinese="路易士机枪（轻量化）", ShortName="LewisMG QLH", ImageName="LewisLMG-832c29e8.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_HotchkissM1909_Wep_Stability", Chinese="M1909 贝内特·梅西耶机枪（冲锋）", ShortName="M1909 CF", ImageName="HotchkissLMG-06defda3.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_HotchkissM1909_Wep_Range", Chinese="M1909 贝内特·梅西耶机枪（瞄准镜）", ShortName="M1909 MZJ", ImageName="HotchkissLMG-06defda3.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_HotchkissM1909_Wep_Bipod", Chinese="M1909 贝内特·梅西耶机枪（望远瞄具）", ShortName="M1909 WYMJ", ImageName="HotchkissLMG-06defda3.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_MadsenMG_Wep_Trench", Chinese="麦德森机枪（壕沟战）", ShortName="MadsenMG HGZ", ImageName="MadsenMG-51e41523.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_MadsenMG_Wep_Stability", Chinese="麦德森机枪（冲锋）", ShortName="MadsenMG CF", ImageName="MadsenMG-51e41523.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_MadsenMG", Chinese="麦德森机枪（轻量化）", ShortName="MadsenMG QLH", ImageName="MadsenMG-51e41523.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_Bergmann1915MG_Wep_Suppression", Chinese="MG15 n.A.（压制）", ShortName="MG15 YZ", ImageName="Bergmann1915MG-891af31f.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_Bergmann1915MG_Wep_Stability", Chinese="MG15 n.A.（冲锋）", ShortName="MG15 CF", ImageName="Bergmann1915MG-891af31f.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_Bergmann1915MG", Chinese="MG15 n.A.（轻量化）", ShortName="MG15 QLH", ImageName="Bergmann1915MG-891af31f.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_BARM1918_Wep_Trench", Chinese="M1918 白朗宁自动步枪（壕沟战）", ShortName="M1918 HGZ", ImageName="Barm1918-3c14511c.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_BARM1918_Wep_Stability", Chinese="M1918 白朗宁自动步枪（冲锋）", ShortName="M1918 CF", ImageName="Barm1918-3c14511c.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_BARM1918_Wep_Bipod", Chinese="M1918 白朗宁自动步枪（望远瞄具）", ShortName="M1918 WYMJ", ImageName="Barm1918-3c14511c.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_BARM1918A2", Chinese="M1918A2 白朗宁自动步枪", ShortName="M1918A2", ImageName="BARM1918A2-48c755b2.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_HuotAutoRifle", Chinese="Huot 自动步枪（轻量化）", ShortName="Huot QLH", ImageName="HuotAutoRifle-4ab70c1a.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_HuotAutoRifle_Wep_Range", Chinese="Huot 自动步枪（瞄准镜）", ShortName="Huot HGZ", ImageName="HuotAutoRifle-4ab70c1a.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_Chauchat", Chinese="绍沙轻机枪（轻量化）", ShortName="Chauchat QLH", ImageName="Chauchat-787ad478.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_Chauchat_Wep_Bipod", Chinese="绍沙轻机枪（望远瞄具）", ShortName="Chauchat WYMJ", ImageName="Chauchat-787ad478.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_ParabellumLMG", Chinese="Parabellum MG14/17（轻量化）", ShortName="MG1417 QLH", ImageName="ParabellumMG1417-09dccd5b.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_ParabellumLMG_Wep_Suppression", Chinese="Parabellum MG14/17（压制）", ShortName="MG1417 YZ", ImageName="ParabellumMG1417-09dccd5b.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_PerinoM1908", Chinese="Perino Model 1908（轻量化）", ShortName="M1908 QLH", ImageName="Perino1908-e97144b1.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_PerinoM1908_Wep_Defensive", Chinese="Perino Model 1908（防御）", ShortName="M1908 FY", ImageName="Perino1908-e97144b1.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_BrowningM1917", Chinese="M1917 机枪（轻量化）", ShortName="M1917 QLH", ImageName="Browning1917-61290bc9.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_BrowningM1917_Wep_Suppression", Chinese="M1917 机枪（望远瞄具）", ShortName="M1917 WYMJ", ImageName="Browning1917-61290bc9.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_MG0818", Chinese="轻机枪 08/18（轻量化）", ShortName="MG0818 QLH", ImageName="LMG_08-18-743c1aa8.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_MG0818_Wep_Defensive", Chinese="轻机枪 08/18（压制）", ShortName="MG0818 YZ", ImageName="LMG_08-18-743c1aa8.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_WinchesterBurton_Wep_Trench", Chinese="波顿 LMR（战壕）", ShortName="Burton LMR ZH", ImageName="WinchesterBurton-ce3988cc.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_WinchesterBurton_Wep_Optical", Chinese="波顿 LMR（瞄准镜）", ShortName="Burton LMR HZJ", ImageName="WinchesterBurton-ce3988cc.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_MauserC96AutoPistol", Chinese="C96（卡宾枪）", ShortName="C96 KBQ", ImageName="MauserC96CCarbine-741ab77d.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_LugerArtillery", Chinese="P08 Artillerie", ShortName="P08 Artillerie", ImageName="LugerArtillery-1fbfb83c.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_PieperCarbine", Chinese="皮珀 M1893", ShortName="M1893", ImageName="PieperCarbine-31e63cfb.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_M1911_Stock", Chinese="M1911（加长）", ShortName="M1911 JC", ImageName="M1911ExtendedMag-eb019f60.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_FN1903stock", Chinese="Mle 1903（加长）", ShortName="Mle 1903 JC", ImageName="FN1903stock-d8904447.png" },
        new WeaponName(){ Kind="支援兵 主要武器", English="U_C93Carbine", Chinese="C93（卡宾枪）", ShortName="C93 KBQ", ImageName="C93CarbineSup-120665d1.png" },

        // 支援兵 配枪
        new WeaponName(){ Kind="支援兵 配枪", English="U_SteyrM1912", Chinese="Repetierpistole M1912", ShortName="M1912", ImageName="SteyrM1912-a49c97dd.png" },
        new WeaponName(){ Kind="支援兵 配枪", English="U_Bulldog", Chinese="斗牛犬左轮手枪", ShortName="Bulldog", ImageName="Bulldog-d95cfd90.png" },
        new WeaponName(){ Kind="支援兵 配枪", English="U_BerettaM1915", Chinese="Modello 1915", ShortName="Modello 1915", ImageName="Beretta1915-e2c3c8d8.png" },
        new WeaponName(){ Kind="支援兵 配枪", English="U_M1911_A1", Chinese="M1911A1", ShortName="M1911A1", ImageName="Colt1911-ed324bf1.png" },

        // 支援兵 配备一二
        new WeaponName(){ Kind="支援兵 配备一二", English="U_AmmoCrate", Chinese="弹药箱", ShortName="Ammo Crate", ImageName="GadgetAmmoCrate-61f48e78.png" },
        new WeaponName(){ Kind="支援兵 配备一二", English="U_AmmoPouch", Chinese="弹药包", ShortName="Ammo Pouch", ImageName="GadgetSmallAmmoPack-5837fde5.png" },
        new WeaponName(){ Kind="支援兵 配备一二", English="U_Mortar", Chinese="迫击炮（空爆）", ShortName="Mortar KB", ImageName="MortarAirburst-77c9647f.png" },
        new WeaponName(){ Kind="支援兵 配备一二", English="U_Mortar_HE", Chinese="迫击炮（高爆）", ShortName="Mortar GB", ImageName="GadgetMortar-84e30045.png" },
        new WeaponName(){ Kind="支援兵 配备一二", English="U_Wrench", Chinese="维修工具", ShortName="Wrench", ImageName="GadgetWrench-07e2c76d.png" },
        new WeaponName(){ Kind="支援兵 配备一二", English="U_LimpetMine", Chinese="磁吸地雷", ShortName="Limpet Mine", ImageName="GadgetLimpetMine-a6d78b8f.png" },
        new WeaponName(){ Kind="支援兵 配备一二", English="U_Crossbow", Chinese="十字弓发射器（破片）", ShortName="Crossbow PP", ImageName="crossbow-5f3dc5e6.png" },
        new WeaponName(){ Kind="支援兵 配备一二", English="U_Crossbow_HE", Chinese="十字弓发射器（高爆）", ShortName="Crossbow GB", ImageName="crossbow-5f3dc5e6.png" },

        ////////////////////////////////// 侦察兵 Scout   //////////////////////////////////

        // 侦察兵 主要武器
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_WinchesterM1895_Wep_Trench", Chinese="Russian 1895（壕沟战）", ShortName="1895 HGZ", ImageName="Winchester1895-69d56c0b.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_WinchesterM1895_Wep_Long", Chinese="Russian 1895（狙击手）", ShortName="1895 JJS", ImageName="Winchester1895-69d56c0b.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_WinchesterM1895", Chinese="Russian 1895（步兵）", ShortName="1895 BB", ImageName="Winchester1895-69d56c0b.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_Gewehr98_Wep_Scope", Chinese="Gewehr 98（神射手）", ShortName="G98 SSS", ImageName="MauserGewehr98-f159616f.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_Gewehr98_Wep_LongRange", Chinese="Gewehr 98（狙击手）", ShortName="G98 JJS", ImageName="MauserGewehr98-f159616f.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_Gewehr98", Chinese="Gewehr 98（步兵）", ShortName="G98 BB", ImageName="MauserGewehr98-f159616f.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_LeeEnfieldSMLE_Wep_Scope", Chinese="SMLE MKIII（神射手）", ShortName="MKIII SSS", ImageName="LeeEnfield-52626131.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_LeeEnfieldSMLE_Wep_Med", Chinese="SMLE MKIII（卡宾枪）", ShortName="MKIII KBQ", ImageName="LeeEnfield-52626131.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_LeeEnfieldSMLE", Chinese="SMLE MKIII（步兵）", ShortName="MKIII BB", ImageName="LeeEnfield-52626131.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_SteyrManM1895_Wep_Scope", Chinese="Gewehr M.95（神射手）", ShortName="G95 SSS", ImageName="Mannlicher1895-7850a8ec.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_SteyrManM1895_Wep_Med", Chinese="Gewehr M.95（卡宾枪）", ShortName="G95 KBQ", ImageName="Mannlicher1895-7850a8ec.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_SteyrManM1895", Chinese="Gewehr M.95（步兵）", ShortName="G95 BB", ImageName="Mannlicher1895-7850a8ec.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_SpringfieldM1903_Wep_Scope", Chinese="M1903（神射手）", ShortName="M1903 SSS", ImageName="SpringfieldM1903-c8ae5988.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_SpringfieldM1903_Wep_LongRange", Chinese="M1903（狙击手）", ShortName="M1903 JJS", ImageName="SpringfieldM1903-c8ae5988.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_SpringfieldM1903_Wep_Pedersen", Chinese="M1903（实验）", ShortName="M1903 SY", ImageName="SpringfieldM1903-c8ae5988.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_MartiniHenry", Chinese="马提尼·亨利步枪（步兵）", ShortName="MartiniHenry BB", ImageName="MartinHenry-c8477a11.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_MartiniHenry_Wep_LongRange", Chinese="马提尼·亨利步枪（狙击手）", ShortName="MartiniHenry JJS", ImageName="MartinHenry-c8477a11.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_LeeEnfieldSMLE_Wep_Preorder", Chinese="阿拉伯的劳伦斯的 SMLE", ShortName="SMLE LLS", ImageName="LeeEnfield-52626131.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_Lebel1886_Wep_LongRange", Chinese="勒贝尔 M1886（狙击手）", ShortName="M1886 JJS", ImageName="Lebel1886-31bf07f8.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_Lebel1886", Chinese="勒贝尔 M1886（步兵）", ShortName="M1886 BB", ImageName="Lebel1886-31bf07f8.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_MosinNagant1891", Chinese="莫辛-纳甘 M91（步兵）", ShortName="M91 BB", ImageName="MosinNagantM1891-fac2efac.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_MosinNagant1891_Wep_Scope", Chinese="莫辛-纳甘 M91（神射手）", ShortName="M91 SSS", ImageName="MosinNagantM1891-fac2efac.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_MosinNagantM38", Chinese="莫辛-纳甘 M38 卡宾枪", ShortName="M38 KBQ", ImageName="MosinNagantM38-dd529587.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_VetterliVitaliM1870", Chinese="Vetterli-Vitali M1870/87（步兵）", ShortName="M1870 BB", ImageName="Vetterli-VitaliM1870-87-faadf520.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_VetterliVitaliM1870_Wep_Med", Chinese="Vetterli-Vitali M1870/87（卡宾枪）", ShortName="M1870 KBQ", ImageName="Vetterli-VitaliM1870-87-faadf520.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_Type38Arisaka", Chinese="三八式步枪（步兵）", ShortName="Type38 BB", ImageName="Type38Arisaka-a1c192e3.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_Type38Arisaka_Wep_Scope", Chinese="三八式步枪（巡逻）", ShortName="Type38 XL", ImageName="Type38Arisaka-a1c192e3.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_CarcanoCarbine", Chinese="卡尔卡诺 M91 卡宾枪", ShortName="M91 KBQ", ImageName="M1891CarcanoCarbine-cc7d34a1.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_CarcanoCarbine_Wep_Scope", Chinese="卡尔卡诺 M91 卡宾枪（巡逻）", ShortName="M91 KBQ XL", ImageName="M1891CarcanoCarbine-cc7d34a1.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_RossMkIII", Chinese="罗斯 MKIII（步兵）", ShortName="RossMkIII BB", ImageName="Ross_Mk3-f8900bf5.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_RossMkIII_Wep_Scope", Chinese="罗斯 MKIII（神射手）", ShortName="RossMkIII SSS", ImageName="Ross_Mk3-f8900bf5.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_Enfield1917", Chinese="M1917 Enfield（步兵）", ShortName="M1917 BB", ImageName="Enfield1917-d33fc14d.png" },
        new WeaponName(){ Kind="侦察兵 主要武器", English="U_Enfield1917_Wep_LongRange", Chinese="M1917 Enfield（消音器）", ShortName="M1917 XYQ", ImageName="Enfield1917-d33fc14d.png" },

        // 侦察兵 配枪
        new WeaponName(){ Kind="侦察兵 配枪", English="U_MarsAutoPistol", Chinese="Mars 自动手枪", ShortName="MarsAutoPistol", ImageName="MarsAutoPistol-7f2606e9.png" },
        new WeaponName(){ Kind="侦察兵 配枪", English="U_Bodeo1889", Chinese="Bodeo 1889", ShortName="Bodeo 1889", ImageName="Bodeo1889-a62282b6.png" },
        new WeaponName(){ Kind="侦察兵 配枪", English="U_FrommerStop", Chinese="费罗梅尔停止手枪", ShortName="Frommer Stop", ImageName="FrommerStopAuto-ea5b918e.png" },

        // 侦察兵 配备一二
        new WeaponName(){ Kind="侦察兵 配备一二", English="U_FlareGun", Chinese="信号枪（侦察）", ShortName="Flare Gun ZC", ImageName="GadgetWebleyScottFlaregun-4438a413.png" },
        new WeaponName(){ Kind="侦察兵 配备一二", English="U_FlareGun_Flash", Chinese="信号枪（闪光）", ShortName="Flare Gun SG", ImageName="GadgetWebleyScottFlaregunFlash-40b27cca.png" },
        new WeaponName(){ Kind="侦察兵 配备一二", English="U_TrPeriscope", Chinese="战壕潜望镜", ShortName="Tr Periscope", ImageName="GadgetTrenchPeriscope-d916e58e.png" },
        new WeaponName(){ Kind="侦察兵 配备一二", English="U_Shield", Chinese="狙击手护盾", ShortName="Shield", ImageName="GadgetShield-9a6f10a4.png" },
        new WeaponName(){ Kind="侦察兵 配备一二", English="U_HelmetDecoy", Chinese="狙击手诱饵", ShortName="Helmet Decoy", ImageName="GadgetHelmetDecoy-182ae8c4.png" },
        new WeaponName(){ Kind="侦察兵 配备一二", English="U_TripWireBomb", Chinese="绊索炸弹（高爆）", ShortName="Trip Wire Bomb", ImageName="GadgetTripWireGrenade-1618bbc3.png" },
        new WeaponName(){ Kind="侦察兵 配备一二", English="U_TripWireGas", Chinese="绊索炸弹（毒气）", ShortName="Trip Wire Gas", ImageName="GadgetTripWireBombGas-f1eabac0.png" },
        new WeaponName(){ Kind="侦察兵 配备一二", English="U_TripWireBurn", Chinese="绊索炸弹（燃烧）", ShortName="Trip Wire Burn", ImageName="TripWireBombINC-6a9a41fb.png" },
        new WeaponName(){ Kind="侦察兵 配备一二", English="_KBullet", Chinese="K 弹", ShortName="K Bullet", ImageName="GadgetKBullets-0ec1f92a.png" },

        /////////////////////////////////////////////////////////////////////////////

        // 精英兵
        new WeaponName(){ Kind="精英兵", English="U_MaximMG0815", Chinese="哨兵 MG 08/15", ShortName="Maxim MG0815", ImageName="Maxim0815-0879ffaa.png" },
        new WeaponName(){ Kind="精英兵", English="U_VillarPerosa", Chinese="哨兵 维拉·佩罗萨冲锋枪", ShortName="Villar Perosa", ImageName="VillarPerosa-4ba7d141.png" },
        new WeaponName(){ Kind="精英兵", English="U_FlameThrower", Chinese="喷火兵 Wex", ShortName="Wex", ImageName="WEXFlammenwerfer-13f2b3af.png" },
        new WeaponName(){ Kind="精英兵", English="U_Incendiary_Hero", Chinese="燃烧手榴弹", ShortName="Incendiary Hero", ImageName="GadgetIncindiary-68d49a3a.png" },
        new WeaponName(){ Kind="精英兵", English="U_RoyalClub", Chinese="战壕奇兵 奇兵棒", ShortName="Royal Club", ImageName="Steelclub-83b053cf.png" },
        new WeaponName(){ Kind="精英兵", English="U_MartiniGrenadeLauncher", Chinese="入侵者 马提尼·亨利步枪榴弹发射器", ShortName="Martini GL", ImageName="MartiniHenryGrenadeLauncher-65e27bf0.png" },
        new WeaponName(){ Kind="精英兵", English="U_SawnOffShotgun_FK", Chinese="短管霰弹枪", ShortName="SawnOffShotgun", ImageName="SawedOfShotgun-d31e0dd8.png" },
        new WeaponName(){ Kind="精英兵", English="U_FlareGun_Elite", Chinese="信号枪 — 信号", ShortName="FlareGun Elite", ImageName="GadgetWebleyScottFlaregunFlash-40b27cca.png" },
        new WeaponName(){ Kind="精英兵", English="U_SpawnBeacon", Chinese="重生信标", ShortName="Spawn Beacon", ImageName="GadgetHeliograph-66004cd6.png" },
        new WeaponName(){ Kind="精英兵", English="U_TankGewehr", Chinese="坦克猎手 Tankgewehr M1918", ShortName="Tank Gewehr", ImageName="MauserTankgewehr1918-aedf4c56.png" },
        new WeaponName(){ Kind="精英兵", English="U_TrPeriscope_Elite", Chinese="战壕潜望镜", ShortName="Tr Periscope", ImageName="GadgetTrenchPeriscope-d916e58e.png" },
        new WeaponName(){ Kind="精英兵", English="U_ATGrenade_VhKit", Chinese="反坦克手榴弹", ShortName="AT Grenade", ImageName="GadgetATGrenade-4b135d46.png" },

        ///////////////////////////////////////////////////////////////////////////////////

        // 载具
        new WeaponName(){ Kind="坦克", English="ID_P_VNAME_MARKV", Chinese="巡航坦克 Mark V 巡航坦克", ShortName="Mark V", ImageName="GBRMarkV-bf3b1d1a.png" },
        new WeaponName(){ Kind="巡航坦克1", English="U_GBR_MarkV_Package_Mortar", Chinese="迫击炮巡航坦克", ShortName="Mark V PJP", ImageName="GBRMarkV-bf3b1d1a.png" },
        new WeaponName(){ Kind="巡航坦克2", English="U_GBR_MarkV_Package_AntiTank", Chinese="坦克猎手巡航坦克", ShortName="Mark V TKLS", ImageName="GBRMarkV-bf3b1d1a.png" },
        new WeaponName(){ Kind="巡航坦克3", English="U_GBR_MarkV_Package_SquadSupport", Chinese="小队支援巡航坦克", ShortName="Mark V XDZY", ImageName="GBRMarkV-bf3b1d1a.png" },

        new WeaponName(){ Kind="坦克", English="ID_P_VNAME_A7V", Chinese="重型坦克 AV7 重型坦克", ShortName="AV7", ImageName="GERA7V-bfc09237.png" },
        new WeaponName(){ Kind="重型坦克1", English="U_GER_A7V_Package_Assault", Chinese="重型突击坦克", ShortName="AV7 TJ", ImageName="GERA7V-bfc09237.png" },
        new WeaponName(){ Kind="重型坦克2", English="U_GER_A7V_Package_Breakthrough", Chinese="重型突破坦克", ShortName="AV7 TP", ImageName="GERA7V-bfc09237.png" },
        new WeaponName(){ Kind="重型坦克3", English="U_GER_A7V_Package_Flamethrower", Chinese="重型火焰喷射器坦克", ShortName="AV7 HYPSQ", ImageName="GERA7V-bfc09237.png" },

        new WeaponName(){ Kind="坦克", English="ID_P_VNAME_FT17", Chinese="轻型坦克 FT-17 轻型坦克", ShortName="FT-17", ImageName="FRARenaultFt-17-aea9e5e7.png" },
        new WeaponName(){ Kind="轻型坦克1", English="U_FRA_FT_Package_37mm", Chinese="轻型近距离支援坦克", ShortName="FT-17 JJLZY", ImageName="FRARenaultFt-17-aea9e5e7.png" },
        new WeaponName(){ Kind="轻型坦克2", English="U_FRA_FT_Package_20mm", Chinese="轻型侧翼攻击坦克", ShortName="FT-17 CYGJ", ImageName="FRARenaultFt-17-aea9e5e7.png" },
        new WeaponName(){ Kind="轻型坦克3", English="U_FRA_FT_Package_75mm", Chinese="轻型榴弹炮坦克", ShortName="FT-17 LDP", ImageName="FRARenaultFt-17-aea9e5e7.png" },

        new WeaponName(){ Kind="坦克", English="ID_P_VNAME_ARTILLERYTRUCK", Chinese="装甲车 火炮装甲车", ShortName="ARTILLERYTRUCK", ImageName="GBRPierceArrowAALorry-6e6d8d9f.png" },
        new WeaponName(){ Kind="火炮装甲车1", English="U_GBR_PierceArrow_Package_Artillery", Chinese="火炮装甲车", ShortName="ATruck HP", ImageName="GBRPierceArrowAALorry-6e6d8d9f.png" },
        new WeaponName(){ Kind="火炮装甲车2", English="U_GBR_PierceArrow_Package_AntiAircraft", Chinese="防空装甲车", ShortName="ATruck AA", ImageName="GBRPierceArrowAALorry-6e6d8d9f.png" },
        new WeaponName(){ Kind="火炮装甲车3", English="U_GBR_PierceArrow_Package_Mortar", Chinese="迫击炮装甲车", ShortName="ATruck Mortar", ImageName="GBRPierceArrowAALorry-6e6d8d9f.png" },

        new WeaponName(){ Kind="坦克", English="ID_P_VNAME_STCHAMOND", Chinese="攻击坦克 圣沙蒙", ShortName="STCHAMOND", ImageName="FRAStChamond-3123e0cd.png" },
        new WeaponName(){ Kind="攻击坦克1", English="U_FRA_StChamond_Package_Assault", Chinese="战地攻击坦克", ShortName="STCHAMOND ZD", ImageName="FRAStChamond-3123e0cd.png" },
        new WeaponName(){ Kind="攻击坦克2", English="U_FRA_StChamond_Package_Gas", Chinese="毒气攻击坦克", ShortName="STCHAMOND DQ", ImageName="FRAStChamond-3123e0cd.png" },
        new WeaponName(){ Kind="攻击坦克3", English="U_FRA_StChamond_Package_Standoff", Chinese="对峙攻击坦克", ShortName="STCHAMOND DZ", ImageName="FRAStChamond-3123e0cd.png" },

        new WeaponName(){ Kind="坦克", English="ID_P_VNAME_ASSAULTTRUCK", Chinese="突袭装甲车 朴帝洛夫·加福德", ShortName="ASSAULTTRUCK", ImageName="PutilovGarford-20a4fd91.png" },
        new WeaponName(){ Kind="突袭装甲车1", English="U_RU_PutilovGarford_Package_AssaultGun", Chinese="突袭装甲车", ShortName="ATTruck TX", ImageName="PutilovGarford-20a4fd91.png" },
        new WeaponName(){ Kind="突袭装甲车2", English="U_RU_PutilovGarford_Package_AntiVehicle", Chinese="反坦克装甲车", ShortName="ATTruck AT", ImageName="PutilovGarford-20a4fd91.png" },
        new WeaponName(){ Kind="突袭装甲车3", English="U_RU_PutilovGarford_Package_Recon", Chinese="侦察装甲车", ShortName="ATTruck ZC", ImageName="PutilovGarford-20a4fd91.png" },

        ////////////////
        
        new WeaponName(){ Kind="飞机", English="ID_P_VNAME_HALBERSTADT", Chinese="攻击机 哈尔伯施塔特 CL.II 攻击机", ShortName="HALBERSTADT", ImageName="GERHalberstadtCLII-c1cb8257.png" },
        new WeaponName(){ Kind="飞机", English="ID_P_VNAME_BRISTOL", Chinese="攻击机 布里斯托 F2.B 攻击机", ShortName="BRISTOL", ImageName="GBRBristolF2B-141b8daa.png" },
        new WeaponName(){ Kind="飞机", English="ID_P_VNAME_SALMSON", Chinese="攻击机 A.E.F 2-A2 攻击机", ShortName="SALMSON", ImageName="FRA_Salmson_2-05f47b5c.png" },
        new WeaponName(){ Kind="飞机", English="ID_P_VNAME_RUMPLER", Chinese="攻击机 Rumpler C.I 攻击机", ShortName="RUMPLER", ImageName="AHU_Rumpler_CI-eb45a6be.png" },

        new WeaponName(){ Kind="攻击机1", English="U_2Seater_Package_GroundSupport", Chinese="地面支援攻击机", ShortName="2Seater DMZY", ImageName="AHU_Rumpler_CI-eb45a6be.png" },
        new WeaponName(){ Kind="攻击机2", English="U_2Seater_Package_TankHunter", Chinese="坦克猎手攻击机", ShortName="2Seater TKLS", ImageName="AHU_Rumpler_CI-eb45a6be.png" },
        new WeaponName(){ Kind="攻击机3", English="U_2Seater_Package_AirshipBuster", Chinese="飞船毁灭者攻击机", ShortName="2Seater FCHMZ", ImageName="AHU_Rumpler_CI-eb45a6be.png" },

        new WeaponName(){ Kind="飞机", English="ID_P_VNAME_GOTHA", Chinese="轰炸机 戈塔 G 轰炸机", ShortName="GOTHA", ImageName="GERGothaGIV-54bfb0bf.png" },
        new WeaponName(){ Kind="飞机", English="ID_P_VNAME_CAPRONI", Chinese="轰炸机 卡普罗尼 CA.5 轰炸机", ShortName="CAPRONI", ImageName="ITACaproniCa5-31fc77c8.png" },
        new WeaponName(){ Kind="飞机", English="ID_P_VNAME_DH10", Chinese="轰炸机 Airco DH.10 轰炸机", ShortName="DH10", ImageName="GBR_Airco_DH10-05e772e8.png" },
        new WeaponName(){ Kind="飞机", English="ID_P_VNAME_HBG1", Chinese="轰炸机 汉莎·布兰登堡 G.I 轰炸机", ShortName="HBG1", ImageName="AHU_Hansa_Brandenburg_GI-042fc3dc.png" },

        new WeaponName(){ Kind="轰炸机1", English="U_Bomber_Package_Barrage", Chinese="弹幕轰炸机", ShortName="Bomber DM", ImageName="AHU_Hansa_Brandenburg_GI-042fc3dc.png" },
        new WeaponName(){ Kind="轰炸机2", English="U_Bomber_Package_Firestorm", Chinese="火焰风暴轰炸机", ShortName="Bomber YYFB", ImageName="AHU_Hansa_Brandenburg_GI-042fc3dc.png" },
        new WeaponName(){ Kind="轰炸机3", English="U_Bomber_Package_Torpedo", Chinese="鱼雷轰炸机", ShortName="Bomber YL", ImageName="AHU_Hansa_Brandenburg_GI-042fc3dc.png" },

        new WeaponName(){ Kind="飞机", English="ID_P_VNAME_SPAD", Chinese="战斗机 SPAD S XIII 战斗机", ShortName="SPAD S XIII", ImageName="FRA_SPAD_X_XIII-8f60a194.png" },
        new WeaponName(){ Kind="飞机", English="ID_P_VNAME_SOPWITH", Chinese="战斗机 索普维斯骆驼式战斗机", ShortName="SOPWITH", ImageName="GBRSopwithCamel-39d664a3.png" },
        new WeaponName(){ Kind="飞机", English="ID_P_VNAME_DR1", Chinese="战斗机 DR.1 战斗机", ShortName="DR1", ImageName="GERFokkerDR1-14f95745.png" },
        new WeaponName(){ Kind="飞机", English="ID_P_VNAME_ALBATROS", Chinese="战斗机 信天翁 D-III 战斗机", ShortName="ALBATROS", ImageName="GER_Albatros_DIII-5ca9e1d3.png" },
         
        new WeaponName(){ Kind="战斗机1", English="U_Scout_Package_Dogfighter", Chinese="空战机", ShortName="Scout KZJ", ImageName="GER_Albatros_DIII-5ca9e1d3.png" },
        new WeaponName(){ Kind="战斗机2", English="U_Scout_Package_BomberKiller", Chinese="轰炸机杀手", ShortName="Scout HZJSS", ImageName="GER_Albatros_DIII-5ca9e1d3.png" },
        new WeaponName(){ Kind="战斗机3", English="U_Scout_Package_TrenchFighter", Chinese="战壕战斗机", ShortName="Scout ZHZDJ", ImageName="GER_Albatros_DIII-5ca9e1d3.png" },

        new WeaponName(){ Kind="飞机", English="ID_P_VNAME_ILYAMUROMETS", Chinese="重型轰炸机 伊利亚·穆罗梅茨", ShortName="ILYAMUROMETS", ImageName="IlyaMurometsHeavyBomber-74779164.png" },

        new WeaponName(){ Kind="重型轰炸机1", English="U_HeavyBomber_Package_Strategic", Chinese="重型战略轰炸机", ShortName="HeavyBomber ZL", ImageName="IlyaMurometsHeavyBomber-74779164.png" },
        new WeaponName(){ Kind="重型轰炸机2", English="U_HeavyBomber_Package_Demolition", Chinese="重型爆破轰炸机", ShortName="HeavyBomber BP", ImageName="IlyaMurometsHeavyBomber-74779164.png" },
        new WeaponName(){ Kind="重型轰炸机3", English="U_HeavyBomber_Package_Support", Chinese="重型支援轰炸机", ShortName="HeavyBomber ZY", ImageName="IlyaMurometsHeavyBomber-74779164.png" },

        new WeaponName(){ Kind="飞船", English="ID_P_VNAME_ASTRATORRES", Chinese="飞船 C 级飞船", ShortName="ASTRATORRES", ImageName="AstraTorresAirship-e2148807.png" },

        new WeaponName(){ Kind="飞船1", English="U_CoastalAirship_Package_Observation", Chinese="观察者", ShortName="Airship GCZ", ImageName="AstraTorresAirship-e2148807.png" },
        new WeaponName(){ Kind="飞船2", English="U_CoastalAirship_Package_Raider", Chinese="掠夺者", ShortName="Airship NDZ", ImageName="AstraTorresAirship-e2148807.png" },

        ////////////////

        new WeaponName(){ Kind="驱逐舰", English="ID_P_VNAME_HMS_LANCE", Chinese="驱逐舰 L 级驱逐舰", ShortName="HMS LANCE", ImageName="HMSLancerDestroyer-65317e44.png" },

        new WeaponName(){ Kind="驱逐舰1", English="U_HMS_Lance_Package_Destroyer", Chinese="鱼雷艇驱逐舰", ShortName="HMS LANCE YL", ImageName="HMSLancerDestroyer-65317e44.png" },
        new WeaponName(){ Kind="驱逐舰2", English="U_HMS_Lance_Package_Minelayer", Chinese="水雷布设艇", ShortName="HMS LANCE SL", ImageName="HMSLancerDestroyer-65317e44.png" },

        ////////////////
        
        new WeaponName(){ Kind="骑兵", English="ID_P_VNAME_HORSE", Chinese="骑兵 战马", ShortName="HORSE", ImageName="Horse-c07830d0.png" },

        ////////////////
        
        new WeaponName(){ Kind="驾驶员下车", English="U_WinchesterM1895_Horse", Chinese="Russian 1895（骑兵）", ShortName="M1895 Horse", ImageName="Winchester1895-69d56c0b.png" },
        new WeaponName(){ Kind="驾驶员下车", English="U_AmmoPouch_Cav", Chinese="弹药包", ShortName="Ammo Pouch", ImageName="GadgetSmallAmmoPack-5837fde5.png" },
        new WeaponName(){ Kind="驾驶员下车", English="U_Bandages_Cav", Chinese="绷带包", ShortName="Bandages", ImageName="GadgetBandages-1d1fc900.png" },
        new WeaponName(){ Kind="驾驶员下车", English="U_Grenade_AT_Cavalry", Chinese="轻型反坦克手榴弹", ShortName="Grenade AT", ImageName="GadgetTrooperATGrenade-a6575030.png" },
        new WeaponName(){ Kind="驾驶员下车", English="U_LugerP08_VhKit", Chinese="P08 手枪", ShortName="LugerP08 VhKit", ImageName="LugerP08-7f07aa2d.png" },

        ////////////////
        
        new WeaponName(){ Kind="特殊载具", English="ID_P_INAME_U_MORTAR", Chinese="特殊载具 空爆迫击炮", ShortName="MORTAR", ImageName="MortarAirburst-77c9647f.png" },
        new WeaponName(){ Kind="特殊载具", English="ID_P_INAME_MORTAR_HE", Chinese="特殊载具 高爆迫击炮", ShortName="MORTAR HE", ImageName="GadgetMortar-84e30045.png" },

        /////////////////////////////////////////////////////////////////////////////

        // 运输载具
        new WeaponName(){ Kind="运输载具", English="ID_P_VNAME_MODEL30", Chinese="运输载具 M30 侦察车", ShortName="MODEL30", ImageName="USADodgeScoutCar-843c9c16.png" },
        new WeaponName(){ Kind="运输载具", English="ID_P_VNAME_MERCEDES_37", Chinese="运输载具 37/95 侦察车", ShortName="MERCEDES 37", ImageName="AHU_Mercedes_37_95-69b407d2.png" },
        new WeaponName(){ Kind="运输载具", English="ID_P_VNAME_BENZ_MG", Chinese="运输载具 KFT 侦察车", ShortName="BENZ MG", ImageName="GER_Benz_MGCarrier-474daf7b.png" },
        new WeaponName(){ Kind="运输载具", English="ID_P_VNAME_MOTORCYCLE", Chinese="运输载具 MC 18J 附边车摩托车", ShortName="MOTORCYCLE", ImageName="USAHarleyDavidsson18J-27b0d7ef.png" },
        new WeaponName(){ Kind="运输载具", English="ID_P_VNAME_NSU", Chinese="运输载具 MC 3.5HP 附边车摩托车", ShortName="NSU", ImageName="GER_NSU_1914-e1a63515.png" },

        new WeaponName(){ Kind="运输载具", English="ID_P_VNAME_ROLLS", Chinese="运输载具 RNAS 装甲车", ShortName="ROLLS", ImageName="GBRRollsRoyceArmoredCar-4c6ccdf0.png" },
        new WeaponName(){ Kind="运输载具", English="ID_P_VNAME_ROMFELL", Chinese="运输载具 Romfell 装甲车", ShortName="ROMFELL", ImageName="GER_Romfell-79d5be52.png" },
        new WeaponName(){ Kind="运输载具", English="ID_P_VNAME_EHRHARDT", Chinese="运输载具 EV4 装甲车", ShortName="EHRHARDT", ImageName="GER_Ehrhardt_EV4-1e718572.png" },
        new WeaponName(){ Kind="运输载具", English="ID_P_VNAME_TERNI", Chinese="运输载具 F.T. 装甲车", ShortName="TERNI", ImageName="ITA_Fiat_Terni-3d8076d6.png" },

        new WeaponName(){ Kind="运输载具", English="ID_P_VNAME_MAS15", Chinese="运输载具 M.A.S 鱼雷快艇", ShortName="MAS15", ImageName="ITAMAS-51e28b0e.png" },
        new WeaponName(){ Kind="运输载具", English="ID_P_VNAME_YLIGHTER", Chinese="运输载具 Y-Lighter 登陆艇", ShortName="MAS15", ImageName="GBR_Y_Lighter-468f2eaa.png" },

        /////////////////////////////////////////////////////////////////////////////

        // 定点武器
        new WeaponName(){ Kind="定点武器", English="ID_P_VNAME_FIELDGUN", Chinese="定点武器 FK 96 野战炮", ShortName="FIELDGUN", ImageName="GERFk96nA-760d0461.png" },
        new WeaponName(){ Kind="定点武器", English="ID_P_VNAME_TURRET", Chinese="定点武器 堡垒火炮", ShortName="TURRET", ImageName="FRAFortressTurret-9fb165ad.png" },
        new WeaponName(){ Kind="定点武器", English="ID_P_VNAME_AASTATION", Chinese="定点武器 QF 1 防空炮", ShortName="AASTATION", ImageName="GBRQF1-63882f78.png" },
        new WeaponName(){ Kind="定点武器", English="ID_P_INAME_MAXIM", Chinese="定点武器 重型机枪", ShortName="MAXIM", ImageName="GBRVickers-ea4826ae.png" },
        // 定点武器 高爆机砲 GER_BeckerM_Stationary-c741a373.png
        new WeaponName(){ Kind="定点武器", English="ID_P_VNAME_BL9", Chinese="定点武器 BL 9.2 攻城炮", ShortName="BL9", ImageName="GBRBL9-0a10176d.png" },
        new WeaponName(){ Kind="定点武器", English="ID_P_VNAME_COASTALBATTERY", Chinese="定点武器 350/52 o 岸防炮", ShortName="COASTALBATTERY", ImageName="DagoCoastalArtilleryGun-b4b737b1.png" },
        new WeaponName(){ Kind="定点武器", English="ID_P_VNAME_SK45GUN", Chinese="定点武器 SK45 岸防炮", ShortName="SK45GUN", ImageName="" },

        /////////////////////////////////////////////////////////////////////////////

        // 战争巨兽
        new WeaponName(){ Kind="机械巨兽", English="ID_P_VNAME_ZEPPELIN", Chinese="机械巨兽 飞船 l30", ShortName="ZEPPELIN", ImageName="GERZeppelinL30-62618731.png" },
        new WeaponName(){ Kind="机械巨兽", English="ID_P_VNAME_ARMOREDTRAIN", Chinese="机械巨兽 装甲列车", ShortName="ARMOREDTRAIN", ImageName="RUSArmoredTrain-564a4e48.png" },
        new WeaponName(){ Kind="机械巨兽", English="ID_P_VNAME_IRONDUKE", Chinese="机械巨兽 无畏舰", ShortName="IRONDUKE", ImageName="GBRHMSIronDuke-3b82016f.png" },
        new WeaponName(){ Kind="机械巨兽", English="ID_P_VNAME_CHAR", Chinese="机械巨兽 Char 2C", ShortName="CHAR", ImageName="FRAChar2C-b8f3c0e2.png" },

        /////////////////////////////////////////////////////////////////////////////
        
        // 近战
        new WeaponName(){ Kind="近战武器", English="U_GrenadeClub", Chinese="哑弹棒", ShortName="Grenade Club", ImageName="" },
        new WeaponName(){ Kind="近战武器", English="U_Club", Chinese="棍棒", ShortName="Club", ImageName="" },

        // 其他
        new WeaponName(){ Kind="其他", English="U_GasMask", Chinese="防毒面具", ShortName="Gas Mask", ImageName="" },
    };
}