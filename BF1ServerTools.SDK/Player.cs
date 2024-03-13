using BF1ServerTools.SDK.Core;
using BF1ServerTools.SDK.Data;

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
    /// 获取玩家列表缓存
    /// </summary>
    /// <returns></returns>
    public static List<CacheData> GetPlayerCache()
    {
        List<CacheData> _playerCache = new();

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

    /// <summary>
    /// 获取玩家列表信息
    /// </summary>
    /// <returns></returns>
    public static List<PlayerData> GetPlayerList()
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

            var offset = Memory.Read<long>(_baseAddress + 0x11A8);
            offset = Memory.Read<long>(offset + 0x28);
            var _kit = Memory.ReadString(offset, 64);

            for (int j = 0; j < 8; j++)
                _weaponSlot[j] = string.Empty;

            var _pClientVehicleEntity = Memory.Read<long>(_baseAddress + 0x1D38);
            if (Memory.IsValid(_pClientVehicleEntity))
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
                    if (vtable == 0x142B8CFA8)
                    {
                        var tempVehicleName = Memory.ReadString(Memory.Read<long>(tempMultiUnlockAsset + 0x20), 64);
                        if (FixVehicleKits(_weaponSlot[0], tempVehicleName))
                        {
                            _weaponSlot[1] = tempVehicleName;
                            break;
                        }
                    }
                }
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

        var _pClientScoreBA = Memory.Read<long>(Memory.Bf1ProBaseAddress + 0x39EB8D8);
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
