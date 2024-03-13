namespace BF1ServerTools.SDK.Core;

public static class Win32
{
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

    [DllImport("kernel32.dll")]
    public static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(IntPtr hProcess, long lpBaseAddress, [In, Out] byte[] lpBuffer, int nsize, out IntPtr lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    public static extern bool WriteProcessMemory(IntPtr hProcess, long lpBaseAddress, [In, Out] byte[] lpBuffer, int nsize, out IntPtr lpNumberOfBytesWritten);

    [DllImport("user32.dll")]
    public static extern int SetForegroundWindow(IntPtr hwnd);

    [DllImport("ntdll.dll")]
    public static extern void NtResumeProcess([In] IntPtr hProcess);

    [DllImport("ntdll.dll")]
    public static extern void NtSuspendProcess([In] IntPtr hProcess);

    [DllImport("kernel32.dll", ExactSpelling = true)]
    public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

    [DllImport("kernel32.dll", ExactSpelling = true)]
    public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, AllocationType dwFreeType);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out W32RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool GetClientRect(IntPtr hWnd, out W32RECT lpRect);

    [DllImport("user32.dll", EntryPoint = "keybd_event")]
    public static extern void Keybd_Event(WinVK bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

    [DllImport("user32.dll")]
    public static extern uint MapVirtualKey(WinVK uCode, uint uMapType);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

    [DllImport("kernel32.dll")]
    public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, int dwLength);
}

[Flags]
public enum ProcessAccessFlags : uint
{
    All = 0x001F0FFF,
    Terminate = 0x00000001,
    CreateThread = 0x00000002,
    VirtualMemoryOperation = 0x00000008,
    VirtualMemoryRead = 0x00000010,
    VirtualMemoryWrite = 0x00000020,
    DuplicateHandle = 0x00000040,
    CreateProcess = 0x000000080,
    SetQuota = 0x00000100,
    SetInformation = 0x00000200,
    QueryInformation = 0x00000400,
    QueryLimitedInformation = 0x00001000,
    Synchronize = 0x00100000
}

[Flags]
public enum AllocationType
{
    Commit = 0x1000,
    Reserve = 0x2000,
    Decommit = 0x4000,
    Release = 0x8000,
    Reset = 0x80000,
    Physical = 0x400000,
    TopDown = 0x100000,
    WriteWatch = 0x200000,
    LargePages = 0x20000000
}

[Flags]
public enum MemoryProtection
{
    Execute = 0x10,
    ExecuteRead = 0x20,
    ExecuteReadWrite = 0x40,
    ExecuteWriteCopy = 0x80,
    NoAccess = 0x01,
    ReadOnly = 0x02,
    ReadWrite = 0x04,
    WriteCopy = 0x08,
    GuardModifierflag = 0x100,
    NoCacheModifierflag = 0x200,
    WriteCombineModifierflag = 0x400
}

[Flags]
public enum AllocationProtect : uint
{
    PAGE_EXECUTE = 0x00000010,
    PAGE_EXECUTE_READ = 0x00000020,
    PAGE_EXECUTE_READWRITE = 0x00000040,
    PAGE_EXECUTE_WRITECOPY = 0x00000080,
    PAGE_NOACCESS = 0x00000001,
    PAGE_READONLY = 0x00000002,
    PAGE_READWRITE = 0x00000004,
    PAGE_WRITECOPY = 0x00000008,
    PAGE_GUARD = 0x00000100,
    PAGE_NOCACHE = 0x00000200,
    PAGE_WRITECOMBINE = 0x00000400
}

[StructLayout(LayoutKind.Sequential)]
public struct MEMORY_BASIC_INFORMATION64
{
    public ulong BaseAddress;
    public ulong AllocationBase;
    public int AllocationProtect;
    public int __alignment1;
    public ulong RegionSize;
    public int State;
    public int Protect;
    public int Type;
    public int __alignment2;
}

[StructLayout(LayoutKind.Sequential)]
public struct W32RECT
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}
