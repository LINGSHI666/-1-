using BF1ServerTools.SDK.Core;

namespace BF1ServerTools.SDK;

public static class Chat
{
    /// <summary>
    /// 聊天框起始偏移
    /// </summary>
    public const int OFFSET_CHAT_MESSAGE_START = 0x180;
    /// <summary>
    /// 聊天框结束偏移
    /// </summary>
    public const int OFFSET_CHAT_MESSAGE_END = 0x188;

    /// <summary>
    /// 最后聊天发送者偏移
    /// </summary>
    public const int OFFSET_CHAT_LAST_SENDER = 0x138;
    /// <summary>
    /// 最后聊天发送内容偏移
    /// </summary>
    public const int OFFSET_CHAT_LAST_CONTENT = 0x140;

    /// <summary>
    /// 申请的内存地址
    /// </summary>
    public static IntPtr AllocateMemAddress { get; private set; } = IntPtr.Zero;

    /// <summary>
    /// 用于线程加锁
    /// </summary>
    private static readonly object Obj = new();

    /// <summary>
    /// 判断战地1聊天框是否开启，开启返回true，关闭或其他返回false
    /// </summary>
    /// <returns></returns>
    public static bool GetChatIsOpen()
    {
        var address = Memory.Read<long>(Memory.Bf1ProBaseAddress + 0x39F1E50);
        if (!Memory.IsValid(address))
            return false;
        address = Memory.Read<long>(address + 0x08);
        if (!Memory.IsValid(address))
            return false;
        address = Memory.Read<long>(address + 0x28);
        if (!Memory.IsValid(address))
            return false;
        address = Memory.Read<long>(address + 0x00);
        if (!Memory.IsValid(address))
            return false;
        address = Memory.Read<long>(address + 0x20);
        if (!Memory.IsValid(address))
            return false;
        address = Memory.Read<long>(address + 0x18);
        if (!Memory.IsValid(address))
            return false;
        address = Memory.Read<long>(address + 0x28);
        if (!Memory.IsValid(address))
            return false;
        address = Memory.Read<long>(address + 0x38);
        if (!Memory.IsValid(address))
            return false;
        address = Memory.Read<long>(address + 0x40);
        if (!Memory.IsValid(address))
            return false;

        return Memory.Read<byte>(address + 0x30) == 0x01;
    }

    /// <summary>
    /// 获取聊天框指针，成功返回指针，失败返回0
    /// </summary>
    /// <returns></returns>
    public static long ChatMessagePointer()
    {
        var address = Memory.Read<long>(Memory.Bf1ProBaseAddress + 0x3A327E0);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0x20);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0x18);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0x38);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0x08);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0x68);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0xB8);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0x10);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0x10);
        if (!Memory.IsValid(address))
            return 0;
        else
            return address;
    }

    /// <summary>
    /// 获取聊天框列表，成功返回指针，失败返回0
    /// </summary>
    /// <returns></returns>
    public static long ChatListPointer()
    {
        var address = Memory.Read<long>(Memory.Bf1ProBaseAddress + 0x39F1E50);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0x70);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0x20);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0x18);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0x28);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0x28);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0x38);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0xD8);
        if (!Memory.IsValid(address))
            return 0;
        address = Memory.Read<long>(address + 0x50);
        if (!Memory.IsValid(address))
            return 0;
        else
            return address;
    }

    /// <summary>
    /// 获取最后聊天发送者
    /// </summary>
    /// <returns></returns>
    public static string GetLastChatSender(out long pSender)
    {
        pSender = 0;
        if (ChatListPointer() != 0)
        {
            pSender = Memory.Read<long>(ChatListPointer() + OFFSET_CHAT_LAST_SENDER);
            return Memory.ReadString(Memory.Read<long>(pSender), 32);
        }
        return string.Empty;
    }

    /// <summary>
    /// 获取最后聊天发送内容
    /// </summary>
    /// <returns></returns>
    public static string GetLastChatContent(out long pContent)
    {
        pContent = 0;
        if (ChatListPointer() != 0)
        {
            pContent = Memory.Read<long>(ChatListPointer() + OFFSET_CHAT_LAST_CONTENT);
            return Memory.ReadString(Memory.Read<long>(pContent), 256);
        }
        return string.Empty;
    }

    //////////////////////////////////////////////////////////////////
    
    /// <summary>
    /// 发送中文聊天消息到战地1
    /// </summary>
    /// <param name="message"></param>
    public static void SendChsToBF1Chat(string message)
    {
        lock (Obj)
        {
            if (GetChatIsOpen())
            {
                // 挂起战地1进程
                Memory.SuspendBF1Process();

                var length = GetStrLength(message.Trim());
                Memory.WriteString(AllocateMemAddress.ToInt64(), message);

                var startPtr = ChatMessagePointer() + OFFSET_CHAT_MESSAGE_START;
                var endPtr = ChatMessagePointer() + OFFSET_CHAT_MESSAGE_END;

                var oldStartPtr = Memory.Read<long>(startPtr);
                var oldEndPtr = Memory.Read<long>(endPtr);

                Memory.Write(startPtr, AllocateMemAddress.ToInt64());
                Memory.Write(endPtr, AllocateMemAddress.ToInt64() + length);

                // 恢复战地1进程
                Memory.ResumeBF1Process();

                //////////////////////////////////////////////////////

                Memory.KeyPress(WinVK.RETURN);

                //////////////////////////////////////////////////////

                // 挂起战地1进程
                Memory.SuspendBF1Process();
                Memory.Write(startPtr, oldStartPtr);
                Memory.Write(endPtr, oldEndPtr);
                // 恢复战地1进程
                Memory.ResumeBF1Process();
            }
        }
    }

    /// <summary>
    /// 战地1窗口是否在最前
    /// </summary>
    /// <returns></returns>
    public static bool GetWindowIsTop()
    {
        var address = Memory.Read<long>(Offsets.OFFSET_DXRENDERER);
        if (!Memory.IsValid(address))
            return false;
        address = Memory.Read<long>(address + 0x820);
        if (!Memory.IsValid(address))
            return false;

        return Memory.Read<byte>(address + 0x5F) == 0x01;
    }

    /// <summary>
    /// 判断战地1输入框字符串长度，中文3，英文1
    /// </summary>
    /// <param name="str">需要判断的字符串</param>
    /// <returns></returns>
    public static int GetStrLength(string str)
    {
        str = str.Trim();
        if (string.IsNullOrEmpty(str))
            return 0;

        int tempLen = 0;
        var bytes = new ASCIIEncoding().GetBytes(str);
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] == 63)
                tempLen += 3;
            else
                tempLen += 1;
        }

        return tempLen;
    }

    //////////////////////////////////////////////////////////////////

    /// <summary>
    /// 申请内存空间
    /// </summary>
    /// <returns></returns>
    public static bool AllocateMemory()
    {
        if (AllocateMemAddress == IntPtr.Zero)
            AllocateMemAddress = Win32.VirtualAllocEx(Memory.Bf1ProHandle, IntPtr.Zero, (IntPtr)0x300, AllocationType.Commit, MemoryProtection.ReadWrite);

        return AllocateMemAddress != IntPtr.Zero;
    }

    /// <summary>
    /// 释放申请的内存空间
    /// </summary>
    public static void FreeMemory()
    {
        if (AllocateMemAddress != IntPtr.Zero)
            Win32.VirtualFreeEx(Memory.Bf1ProHandle, AllocateMemAddress, 0, AllocationType.Reset);
    }
}
