using BF1ServerTools.SDK.Core;

namespace BF1ServerTools.SDK;

public static class Scan
{
    private struct CanReadAddress
    {
        public long Address;
        public int Size;
    }

    /// <summary>
    /// X-GatewaySession: （16进制字符串）
    /// </summary>
    private const string SessionIdMask = "58 2D 47 61 74 65 77 61 79 53 65 73 73 69 6F 6E 3A 20";

    /// <summary>
    /// 获取内存中GatewaySession
    /// </summary>
    /// <returns></returns>
    public static async Task<string> GetGatewaySession()
    {
        return await Task.Run(() =>
        {
            List<CanReadAddress> canReadAddresses = new();

            var mbi = default(MEMORY_BASIC_INFORMATION64);
            var size = Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION64));

            long baseAddress = 0;
            long maxAddress = long.MaxValue;

            while (baseAddress >= 0 && baseAddress <= maxAddress && mbi.RegionSize >= 0)
            {
                // 扫描内存信息
                if (Win32.VirtualQueryEx(Memory.Bf1ProHandle, new IntPtr(baseAddress), out mbi, size) == 0)
                    break;

                // 如果是已物理分配 并且是 可读写内存
                if (mbi.State == (int)AllocationType.Commit && mbi.Protect == (int)AllocationProtect.PAGE_READWRITE)
                {
                    canReadAddresses.Add(new CanReadAddress()
                    {
                        Address = baseAddress,
                        Size = (int)mbi.RegionSize
                    });
                }

                // 设置基地址偏移
                baseAddress += (long)mbi.RegionSize;
            }

            foreach (var item in canReadAddresses)
            {
                var addr = FindPattern(SessionIdMask, item.Address, item.Size);
                if (addr != 0)
                {
                    var str = Memory.ReadString(addr, 54);
                    str = str.Replace("X-GatewaySession: ", "").Trim();
                    if (IsGuidByReg(str))
                        return str;
                }
            }

            return string.Empty;
        });
    }

    /// <summary>
    /// 特征码搜索
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="baseAddress"></param>
    /// <param name="moduleSize"></param>
    /// <returns></returns>
    private static long FindPattern(string pattern, long baseAddress, int moduleSize)
    {
        List<byte> tempArray = new();
        foreach (var each in pattern.Split(' '))
            tempArray.Add(Convert.ToByte(each, 16));

        byte[] patternByteArray = tempArray.ToArray();
        byte[] localModulebytes = new byte[moduleSize];
        Win32.ReadProcessMemory(Memory.Bf1ProHandle, baseAddress, localModulebytes, moduleSize, out _);

        for (int indexAfterBase = 0; indexAfterBase < localModulebytes.Length; indexAfterBase++)
        {
            bool noMatch = false;

            if (localModulebytes[indexAfterBase] != patternByteArray[0])
                continue;

            for (var MatchedIndex = 0; MatchedIndex < patternByteArray.Length && indexAfterBase + MatchedIndex < localModulebytes.Length; MatchedIndex++)
            {
                if (patternByteArray[MatchedIndex] == 0x0)
                    continue;
                if (patternByteArray[MatchedIndex] != localModulebytes[indexAfterBase + MatchedIndex])
                {
                    noMatch = true;
                    break;
                }
            }

            if (!noMatch)
                return baseAddress + indexAfterBase;
        }

        return 0;
    }

    /// <summary>
    /// 判断字符串是否为GUID
    /// </summary>
    /// <param name="strSrc"></param>
    /// <returns></returns>
    private static bool IsGuidByReg(string strSrc)
    {
        strSrc = strSrc.ToLower();
        Regex reg = new("^[a-f0-9]{8}(-[a-f0-9]{4}){3}-[a-f0-9]{12}$", RegexOptions.Compiled);
        return reg.IsMatch(strSrc);
    }
}
