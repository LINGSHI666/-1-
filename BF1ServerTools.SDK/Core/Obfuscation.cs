namespace BF1ServerTools.SDK.Core;

public static class Obfuscation
{
    /// <summary>
    /// 解密玩家指针
    /// </summary>
    /// <param name="encryptedPlayerMgr">玩家指针</param>
    /// <param name="id">玩家ID</param>
    /// <returns></returns>
    private static long EncryptedPlayerMgr_GetPlayer(long encryptedPlayerMgr, int id)
    {
        long XorValue1 = Memory.Read<long>(encryptedPlayerMgr + 0x20) ^ Memory.Read<long>(encryptedPlayerMgr + 0x8);
        long XorValue2 = XorValue1 ^ Memory.Read<long>(encryptedPlayerMgr + 0x10);
        if (!Memory.IsValid(XorValue2))
            return 0;

        return XorValue1 ^ Memory.Read<long>(XorValue2 + 0x8 * id);
    }

    /// <summary>
    /// 读其他玩家指针
    /// </summary>
    /// <param name="id">玩家ID</param>
    /// <returns></returns>
    public static long GetPlayerById(int id)
    {
        long pClientGameContext = Memory.Read<long>(Offsets.OFFSET_CLIENTGAMECONTEXT);
        if (!Memory.IsValid(pClientGameContext))
            return 0;

        long pPlayerManager = Memory.Read<long>(pClientGameContext + 0x68);
        if (!Memory.IsValid(pPlayerManager))
            return 0;

        long pObfuscationMgr = Memory.Read<long>(Offsets.OFFSET_OBFUSCATIONMGR);
        if (!Memory.IsValid(pObfuscationMgr))
            return 0;

        long PlayerListXorValue = Memory.Read<long>(pPlayerManager + 0xF8);
        long PlayerListKey = PlayerListXorValue ^ Memory.Read<long>(pObfuscationMgr + 0x70);

        long mpBucketArray = Memory.Read<long>(pObfuscationMgr + 0x10);

        int mnBucketCount = Memory.Read<int>(pObfuscationMgr + 0x18);
        if (mnBucketCount == 0)
            return 0;

        int startCount = (int)PlayerListKey % mnBucketCount;

        long mpBucketArray_startCount = Memory.Read<long>(mpBucketArray + startCount * 8);
        long node_first = Memory.Read<long>(mpBucketArray_startCount);
        long node_second = Memory.Read<long>(mpBucketArray_startCount + 0x8);
        long node_mpNext = Memory.Read<long>(mpBucketArray_startCount + 0x10);

        while (PlayerListKey != node_first)
        {
            mpBucketArray_startCount = node_mpNext;

            node_first = Memory.Read<long>(mpBucketArray_startCount);
            node_second = Memory.Read<long>(mpBucketArray_startCount + 0x8);
            node_mpNext = Memory.Read<long>(mpBucketArray_startCount + 0x10);
        }

        long EncryptedPlayerMgr = node_second;
        return EncryptedPlayerMgr_GetPlayer(EncryptedPlayerMgr, id);
    }

    /// <summary>
    /// 读取自己指针
    /// </summary>
    /// <returns></returns>
    public static long GetLocalPlayer()
    {
        long pClientGameContext = Memory.Read<long>(Offsets.OFFSET_CLIENTGAMECONTEXT);
        if (!Memory.IsValid(pClientGameContext))
            return 0;

        long pPlayerManager = Memory.Read<long>(pClientGameContext + 0x68);
        if (!Memory.IsValid(pPlayerManager))
            return 0;

        long pObfuscationMgr = Memory.Read<long>(Offsets.OFFSET_OBFUSCATIONMGR);
        if (!Memory.IsValid(pObfuscationMgr))
            return 0;

        long LocalPlayerListXorValue = Memory.Read<long>(pPlayerManager + 0xF0);
        long LocalPlayerListKey = LocalPlayerListXorValue ^ Memory.Read<long>(pObfuscationMgr + 0x70);

        long mpBucketArray = Memory.Read<long>(pObfuscationMgr + 0x10);

        int mnBucketCount = Memory.Read<int>(pObfuscationMgr + 0x18);
        if (mnBucketCount == 0)
            return 0;

        int startCount = (int)LocalPlayerListKey % mnBucketCount;

        long mpBucketArray_startCount = Memory.Read<long>(mpBucketArray + startCount * 8);
        long node_first = Memory.Read<long>(mpBucketArray_startCount);
        long node_second = Memory.Read<long>(mpBucketArray_startCount + 0x8);
        long node_mpNext = Memory.Read<long>(mpBucketArray_startCount + 0x10);

        while (LocalPlayerListKey != node_first)
        {
            mpBucketArray_startCount = node_mpNext;

            node_first = Memory.Read<long>(mpBucketArray_startCount);
            node_second = Memory.Read<long>(mpBucketArray_startCount + 0x8);
            node_mpNext = Memory.Read<long>(mpBucketArray_startCount + 0x10);
        }

        long encryptedPlayerMgr = node_second;
        return EncryptedPlayerMgr_GetPlayer(encryptedPlayerMgr, 0);
    }
}