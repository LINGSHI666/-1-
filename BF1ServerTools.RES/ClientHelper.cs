using BF1ServerTools.RES.Img;
using BF1ServerTools.RES.Data;

namespace BF1ServerTools.RES;

public static class ClientHelper
{
    /// <summary>
    /// 获取地图对应中文名称
    /// </summary>
    /// <param name="originMapName"></param>
    /// <returns></returns>
    public static string GetMapChsName(string originMapName)
    {
        var index = MapData.AllMapInfo.FindIndex(var => var.English == originMapName);
        if (index != -1)
            return MapData.AllMapInfo[index].Chinese;
        else
            return originMapName;
    }

    /// <summary>
    /// 获取地图对应预览图
    /// </summary>
    /// <param name="originMapName"></param>
    /// <returns></returns>
    public static string GetMapPrevImage(string originMapName)
    {
        var index = MapData.AllMapInfo.FindIndex(var => var.English == originMapName);
        if (index != -1)
            return MapData.AllMapInfo[index].Image;
        else
            return string.Empty;
    }

    /// <summary>
    /// 获取武器对应中文名称
    /// </summary>
    /// <param name="originWeaponName"></param>
    /// <returns></returns>
    public static string GetWeaponChsName(string originWeaponName)
    {
        if (string.IsNullOrEmpty(originWeaponName))
            return string.Empty;

        if (originWeaponName.Contains("_KBullet"))
            return "K 弹";

        if (originWeaponName.Contains("_RGL_Frag"))
            return "步枪手榴弹（破片）";

        if (originWeaponName.Contains("_RGL_Smoke"))
            return "步枪手榴弹（烟雾）";

        if (originWeaponName.Contains("_RGL_HE"))
            return "步枪手榴弹（高爆）";

        int index = WeaponData.AllWeaponInfo.FindIndex(var => var.English == originWeaponName);
        if (index != -1)
            return WeaponData.AllWeaponInfo[index].Chinese;
        else
            return originWeaponName;
    }

    /// <summary>
    /// 获取武器对应本地图片路径
    /// </summary>
    /// <param name="originWeaponName"></param>
    /// <returns></returns>
    public static string GetWeaponImagePath(string originWeaponName)
    {
        if (string.IsNullOrEmpty(originWeaponName))
            return string.Empty;

        var imageName = string.Empty;

        if (originWeaponName.Contains("_KBullet"))
            imageName = "GadgetKBullets-0ec1f92a.png";

        if (originWeaponName.Contains("_RGL_Frag"))
            imageName = "MedicRifleLauncher_B-a712e224.png";

        if (originWeaponName.Contains("_RGL_Smoke"))
            imageName = "MedicRifleLauncher_A-438b725e.png";

        if (originWeaponName.Contains("_RGL_HE"))
            imageName = "MedicRifleLauncher_B-a712e224.png";

        var index = WeaponData.AllWeaponInfo.FindIndex(var => var.English == originWeaponName);
        if (index != -1)
            imageName = WeaponData.AllWeaponInfo[index].ImageName;
        else
            return string.Empty;

        return WeaponImg.Weapon2Dict.ContainsKey(imageName) ? WeaponImg.Weapon2Dict[imageName] : string.Empty;
    }

    /// <summary>
    /// 获取本地图片路径，如果未找到会返回空字符串
    /// </summary>
    /// <param name="url"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetTempImagePath(string url, string type)
    {
        var extension = Path.GetFileName(url);
        switch (type)
        {
            case "map":
                return MapImg.MapDict.ContainsKey(extension) ? MapImg.MapDict[extension] : string.Empty;
            case "weapon":
                return WeaponImg.WeaponDict.ContainsKey(extension) ? WeaponImg.WeaponDict[extension] : string.Empty;
            case "weapon2":
                return WeaponImg.Weapon2Dict.ContainsKey(extension) ? WeaponImg.Weapon2Dict[extension] : string.Empty;
            case "kit":
                return KitImg.KitDict.ContainsKey(extension) ? KitImg.KitDict[extension] : string.Empty;
            case "kit2":
                return KitImg.Kit2Dict.ContainsKey(extension) ? KitImg.Kit2Dict[extension] : string.Empty;
            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// 获取武器简短名称，用于踢人理由
    /// </summary>
    /// <param name="weaponName"></param>
    /// <returns></returns>
    public static string GetWeaponShortTxt(string weaponName)
    {
        var index = WeaponData.AllWeaponInfo.FindIndex(var => var.English.Equals(weaponName));
        if (index != -1)
            return WeaponData.AllWeaponInfo[index].ShortName;

        return weaponName;
    }

    /// <summary>
    /// 获取小队的中文名称
    /// </summary>
    /// <param name="squadID"></param>
    /// <returns></returns>
    public static string GetSquadChsName(int squadID)
    {
        switch (squadID)
        {
            case 0:
            case 99:
                return "无";
            case 1:
                return "苹果";
            case 2:
                return "奶油";
            case 3:
                return "查理";
            case 4:
                return "达夫";
            case 5:
                return "爱德华";
            case 6:
                return "弗莱迪";
            case 7:
                return "乔治";
            case 8:
                return "哈利";
            case 9:
                return "墨水";
            case 10:
                return "强尼";
            case 11:
                return "国王";
            case 12:
                return "伦敦";
            case 13:
                return "猿猴";
            case 14:
                return "疯子";
            case 15:
                return "橘子";
            default:
                return squadID.ToString();
        }
    }

    /// <summary>
    /// 获取队伍1阵营图片路径
    /// </summary>
    /// <param name="mapName"></param>
    public static string GetTeam1Image(string mapName)
    {
        var index = MapData.AllMapInfo.FindIndex(var => var.English.Equals(mapName));
        if (index != -1 && mapName != "ID_M_LEVEL_MENU")
            return $"\\BF1ServerTools.RES;component\\Assets\\Images\\Teams\\{MapData.AllMapInfo[index].Team1Image}.png";

        return "\\BF1ServerTools.RES;component\\Assets\\Images\\Teams\\_DEF.png";
    }

    /// <summary>
    /// 获取队伍2阵营图片路径
    /// </summary>
    /// <param name="mapName"></param>
    public static string GetTeam2Image(string mapName)
    {
        var index = MapData.AllMapInfo.FindIndex(var => var.English.Equals(mapName));
        if (index != -1 && mapName != "ID_M_LEVEL_MENU")
            return $"\\BF1ServerTools.RES;component\\Assets\\Images\\Teams\\{MapData.AllMapInfo[index].Team2Image}.png";

        return "\\BF1ServerTools.RES;component\\Assets\\Images\\Teams\\_DEF.png";
    }

    /// <summary>
    /// 获取队伍阵营中文名称
    /// </summary>
    /// <param name="mapName"></param>
    /// <param name="teamId"></param>
    /// <returns></returns>
    public static string GetTeamChsName(string mapName, int teamId)
    {
        var index = MapData.AllMapInfo.FindIndex(var => var.English.Equals(mapName));
        if (index != -1 && mapName != "ID_M_LEVEL_MENU")
        {
            if (teamId == 1)
                return MapData.AllMapInfo[index].Team1;

            return MapData.AllMapInfo[index].Team2;
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取当前地图游戏模式
    /// </summary>
    /// <param name="modeName"></param>
    /// <returns></returns>
    public static string GetGameMode(string modeName)
    {
        var index = ModeData.AllModeInfo.FindIndex(var => var.Mark.Equals(modeName));
        if (index != -1)
            return ModeData.AllModeInfo[index].Chinese;
        else
            return string.Empty;
    }

    /// <summary>
    /// 获取兵种中文名称
    /// </summary>
    /// <returns></returns>
    public static string GetClassChs(string className)
    {
        switch (className)
        {
            case "Medic":
                return "医疗兵";
            case "Support":
                return "支援兵";
            case "Scout":
                return "侦察兵";
            case "Assault":
                return "突击兵";
            case "Pilot":
                return "驾驶员";
            case "Cavalry":
                return "骑兵";
            case "Tanker":
                return "坦克";
            default:
                return className;
        }
    }

    /// <summary>
    /// 获取玩家当前兵种图片
    /// </summary>
    /// <param name="kit"></param>
    /// <returns></returns>
    public static string GetPlayerKitImage(string kit)
    {
        switch (kit)
        {
            // 坦克
            case "ID_M_TANKER":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconTankerLarge.png";
            // 飞机
            case "ID_M_PILOT":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconPilotLarge.png";
            // 骑兵
            case "ID_M_CAVALRY":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconRiderLarge.png";
            // 哨兵
            case "ID_M_SENTRY":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconSentryLarge.png";
            // 喷火兵
            case "ID_M_FLAMETHROWER":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconFlamethrowerLarge.png";
            // 入侵者
            case "ID_M_INFILTRATOR":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconInfiltratorLarge.png";
            // 战壕奇兵
            case "ID_M_TRENCHRAIDER":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconTrenchRaiderLarge.png";
            // 坦克猎手
            case "ID_M_ANTITANK":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconAntiTankLarge.png";
            // 突击兵
            case "ID_M_ASSAULT":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconAssaultLarge.png";
            // 医疗兵
            case "ID_M_MEDIC":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconMedicLarge.png";
            // 支援兵
            case "ID_M_SUPPORT":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconSupportLarge.png";
            // 侦察兵
            case "ID_M_SCOUT":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconScoutLarge.png";
            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// 获取玩家当前兵种名称
    /// </summary>
    /// <param name="kit"></param>
    /// <returns></returns>
    public static string GetPlayerKitName(string kit)
    {
        switch (kit)
        {
            // 坦克
            case "ID_M_TANKER":
                return "12 坦克";
            // 飞机
            case "ID_M_PILOT":
                return "11 飞机";
            // 骑兵
            case "ID_M_CAVALRY":
                return "10 骑兵";
            // 哨兵
            case "ID_M_SENTRY":
                return "09 哨兵";
            // 喷火兵
            case "ID_M_FLAMETHROWER":
                return "08 喷火兵";
            // 入侵者
            case "ID_M_INFILTRATOR":
                return "07 入侵者";
            // 战壕奇兵
            case "ID_M_TRENCHRAIDER":
                return "06 战壕奇兵";
            // 坦克猎手
            case "ID_M_ANTITANK":
                return "05 坦克猎手";
            // 突击兵
            case "ID_M_ASSAULT":
                return "04 突击兵";
            // 医疗兵
            case "ID_M_MEDIC":
                return "03 医疗兵";
            // 支援兵
            case "ID_M_SUPPORT":
                return "02 支援兵";
            // 侦察兵
            case "ID_M_SCOUT":
                return "01 侦察兵";
            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// 获取玩家当前兵种图片
    /// </summary>
    /// <param name="weaponS0">主要武器</param>
    /// <param name="weaponS2">配备一</param>
    /// <param name="weaponS5">配备二</param>
    /// <returns></returns>
    public static string GetPlayerKitImage(string weaponS0, string weaponS2, string weaponS5)
    {
        switch (weaponS0)
        {
            // 哨兵
            case "U_MaximMG0815":
            case "U_VillarPerosa":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconSentryLarge.png";
            // 喷火兵
            case "U_FlameThrower":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconFlamethrowerLarge.png";
            // 战壕奇兵
            case "U_RoyalClub":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconTrenchRaiderLarge.png";
            // 入侵者
            case "U_MartiniGrenadeLauncher":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconInfiltratorLarge.png";
            // 坦克猎手
            case "U_TankGewehr":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconAntiTankLarge.png";
            // 骑兵
            case "ID_P_VNAME_HORSE":
            case "U_WinchesterM1895_Horse":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconRiderLarge.png";
            // 机械巨兽 飞船 l30
            case "ID_P_VNAME_ZEPPELIN":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\ZEPPELIN.png";
            // 机械巨兽 装甲列车
            case "ID_P_VNAME_ARMOREDTRAIN":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\ARMOREDTRAIN.png";
            // 机械巨兽 无畏舰
            case "ID_P_VNAME_IRONDUKE":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\IRONDUKE.png";
            // 机械巨兽 Char 2C
            case "ID_P_VNAME_CHAR":
                return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\CHAR.png";
        }

        // 坦克
        if (KitData.TankeKit.Contains(weaponS0) || (weaponS2 == "U_Wrench" && weaponS5 == "U_ATGrenade"))
        {
            return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconTankerLarge.png";
        }

        // 飞机
        if (KitData.PilotKit.Contains(weaponS0) || (weaponS2 == "U_Wrench" && weaponS5 == "U_FlareGun"))
        {
            return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconPilotLarge.png";
        }

        // 定点武器
        if (KitData.DingDianKit.Contains(weaponS0))
        {
            return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\DINGDIAN.png";
        }

        // 运输载具
        if (KitData.YunShuKit.Contains(weaponS0))
        {
            return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\YUNSHU.png";
        }

        // 突击兵
        if (KitData.AssaultKit.Contains(weaponS0) && (KitData.AssaultKit.Contains(weaponS2) || KitData.AssaultKit.Contains(weaponS5)))
        {
            return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconAssaultLarge.png";
        }

        // 医疗兵
        if (KitData.MedicKit.Contains(weaponS0) && (KitData.MedicKit.Contains(weaponS2) || KitData.MedicKit.Contains(weaponS5)))
        {
            return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconMedicLarge.png";
        }

        // 支援兵
        if (KitData.SupportKit.Contains(weaponS0) && (KitData.SupportKit.Contains(weaponS2) || KitData.SupportKit.Contains(weaponS5)))
        {
            return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconSupportLarge.png";
        }

        // 侦察兵
        if (KitData.ScoutKit.Contains(weaponS0) && (KitData.ScoutKit.Contains(weaponS2) || KitData.ScoutKit.Contains(weaponS5)))
        {
            return "\\BF1ServerTools.RES;component\\Assets\\Images\\Kits2\\KitIconScoutLarge.png";
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取玩家当前兵种名称
    /// </summary>
    /// <param name="weaponS0">主要武器</param>
    /// <param name="weaponS2">配备一</param>
    /// <param name="weaponS5">配备二</param>
    /// <returns></returns>
    public static string GetPlayerKitName(string weaponS0, string weaponS2, string weaponS5)
    {
        switch (weaponS0)
        {
            // 哨兵
            case "U_MaximMG0815":
            case "U_VillarPerosa":
            // 喷火兵
            case "U_FlameThrower":
            // 战壕奇兵
            case "U_RoyalClub":
            // 入侵者
            case "U_MartiniGrenadeLauncher":
            // 坦克猎手
            case "U_TankGewehr":
                return "15 精英兵";
            // 骑兵
            case "ID_P_VNAME_HORSE":
            case "U_WinchesterM1895_Horse":
                return "16 骑兵";
            // 机械巨兽 飞船 l30
            case "ID_P_VNAME_ZEPPELIN":
            // 机械巨兽 装甲列车
            case "ID_P_VNAME_ARMOREDTRAIN":
            // 机械巨兽 无畏舰
            case "ID_P_VNAME_IRONDUKE":
            // 机械巨兽 Char 2C
            case "ID_P_VNAME_CHAR":
                return "19 机械巨兽";
        }

        // 坦克
        if (KitData.TankeKit.Contains(weaponS0) || (weaponS2 == "U_Wrench" && weaponS5 == "U_ATGrenade"))
        {
            return "18 坦克";
        }

        // 飞机
        if (KitData.PilotKit.Contains(weaponS0) || (weaponS2 == "U_Wrench" && weaponS5 == "U_FlareGun"))
        {
            return "17 飞机";
        }

        // 定点武器
        if (KitData.DingDianKit.Contains(weaponS0))
        {
            return "13 定点武器";
        }

        // 运输载具
        if (KitData.YunShuKit.Contains(weaponS0))
        {
            return "14 运输载具";
        }

        // 突击兵
        if (KitData.AssaultKit.Contains(weaponS0) && (KitData.AssaultKit.Contains(weaponS2) || KitData.AssaultKit.Contains(weaponS5)))
        {
            return "09 突击兵";
        }

        // 医疗兵
        if (KitData.MedicKit.Contains(weaponS0) && (KitData.MedicKit.Contains(weaponS2) || KitData.MedicKit.Contains(weaponS5)))
        {
            return "08 医疗兵";
        }

        // 支援兵
        if (KitData.SupportKit.Contains(weaponS0) && (KitData.SupportKit.Contains(weaponS2) || KitData.SupportKit.Contains(weaponS5)))
        {
            return "07 支援兵";
        }

        // 侦察兵
        if (KitData.ScoutKit.Contains(weaponS0) && (KitData.ScoutKit.Contains(weaponS2) || KitData.ScoutKit.Contains(weaponS5)))
        {
            return "06 侦察兵";
        }

        return string.Empty;
    }
}
