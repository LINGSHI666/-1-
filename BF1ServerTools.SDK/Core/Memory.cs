using System.IO;
using System.Windows.Media.Media3D;
using static BF1ServerTools.SDK.Core.Memory;
using BF1ServerTools;
using System.Windows;
using System.Windows.Markup;
using System.Drawing;
namespace BF1ServerTools.SDK.Core;

public static class Memory
{
    public static DriverCommunication driverCommunication;
    /// <summary>
    /// 战地1进程类
    /// </summary>
    private static Process Bf1Process { get; set; } = null;
    /// <summary>
    /// 战地1窗口句柄
    /// </summary>
    public static IntPtr Bf1WinHandle { get; private set; } = IntPtr.Zero;
    /// <summary>
    /// 战地1进程Id
    /// </summary>
    public static int Bf1ProId { get; private set; } = 0;
    /// <summary>
    /// 战地1主模块基址
    /// </summary>
    public static long Bf1ProBaseAddress { get; private set; } = 0;
    public static long Bf1ProBaseAddress2 { get; private set; } = 0;
    /// <summary>
    /// 战地1进程句柄
    /// </summary>
    public static IntPtr Bf1ProHandle { get; private set; } = IntPtr.Zero;

    /// <summary>
    /// 初始化内存模块
    /// </summary>
    /// <returns></returns>
    public static bool Initialize()
    {
        try
        {
            driverCommunication = new DriverCommunication();
            //MessageBox.Show("驱动成功打开");
        }
        catch (InvalidOperationException ex)
        {

        }
        try
        {
            var pArray = Process.GetProcessesByName("bf1");
            if (pArray.Length > 0)
            {
                foreach (var item in pArray)
                {
                    if (item.MainWindowTitle.Equals("Battlefield™ 1"))
                    {
                        Bf1Process = item;
                        break;
                    }
                }

                if (Bf1Process == null)
                    return false;

                Bf1WinHandle = Bf1Process.MainWindowHandle;
                Bf1ProId = Bf1Process.Id;

                

                if (Bf1ProId != null)
                {
                    string baseAddressString = driverCommunication.ReadBaseAddress(Bf1ProId);

                    // 去除尾部的 \0 字符
                    //baseAddressString = baseAddressString.TrimEnd('\0');

                    // 去除 0x 前缀
                    //if (baseAddressString.StartsWith("0x"))
                    // {
                    //baseAddressString = baseAddressString.Substring(2); // 去掉 '0x' 前缀
                    // }
                    //Bf1ProHandle = Win32.OpenProcess(ProcessAccessFlags.All, false, Bf1ProId);//马恩用
                    // 将基地址字符串转换为 long 类型
                    Bf1ProBaseAddress = 0x140000000;
                    Bf1ProBaseAddress2 = Bf1ProBaseAddress + 0x1000;
                    byte a = Read<byte>(Bf1ProBaseAddress);
                    return true;
                }
            }

            return false;
        }
        catch { return false; }
    }

    /// <summary>
    /// 释放内存模块
    /// </summary>
    public static void UnInitialize()
    {
        if (Bf1ProHandle != IntPtr.Zero)
        {
            Win32.CloseHandle(Bf1ProHandle);
            Bf1ProHandle = IntPtr.Zero;
        }

        if (Bf1Process != null)
            Bf1Process = null;

        if (Bf1WinHandle != IntPtr.Zero)
            Bf1WinHandle = IntPtr.Zero;

        if (Bf1ProId != 0)
            Bf1ProId = 0;

        if (Bf1ProBaseAddress != 0)
            Bf1ProBaseAddress = 0;
    }

    /// <summary>
    /// 将战地1窗口置于前面
    /// </summary>
    public static void SetBF1WindowForeground()
    {
        Win32.SetForegroundWindow(Bf1WinHandle);
    }

    /// <summary>
    /// 按键模拟
    /// </summary>
    /// <param name="winVK">虚拟按键</param>
    /// <param name="delay">延迟，单位毫秒</param>
    public static void KeyPress(WinVK winVK, int delay = 50)
    {
        Win32.Keybd_Event(winVK, Win32.MapVirtualKey(winVK, 0), 0, 0);
        Thread.Sleep(delay);
        Win32.Keybd_Event(winVK, Win32.MapVirtualKey(winVK, 0), 2, 0);
        Thread.Sleep(delay);
    }

    /// <summary>
    /// 暂停战地1进程
    /// </summary>
    public static void SuspendBF1Process()
    {
        Win32.NtSuspendProcess(Bf1ProHandle);
    }

    /// <summary>
    /// 恢复进程
    /// </summary>
    public static void ResumeBF1Process()
    {
        Win32.NtResumeProcess(Bf1ProHandle);
    }

    /// <summary>
    /// 获取战地1窗口数据
    /// </summary>
    /// <returns></returns>
    public static bool GetBF1WindowData(out WindowData windowData)
    {
        // 获取指定窗口句柄的窗口矩形数据和客户区矩形数据
        Win32.GetWindowRect(Bf1WinHandle, out W32RECT windowRect);
        Win32.GetClientRect(Bf1WinHandle, out W32RECT clientRect);

        // 计算窗口区的宽和高
        var windowWidth = windowRect.Right - windowRect.Left;
        var windowHeight = windowRect.Bottom - windowRect.Top;

        // 处理窗口最小化
        if (windowRect.Left == -32000)
        {
            windowData.Left = 0;
            windowData.Top = 0;
            windowData.Width = 1;
            windowData.Height = 1;
            return false;
        }

        // 计算客户区的宽和高
        var clientWidth = clientRect.Right - clientRect.Left;
        var clientHeight = clientRect.Bottom - clientRect.Top;

        // 计算边框
        var borderWidth = (windowWidth - clientWidth) / 2;
        var borderHeight = windowHeight - clientHeight - borderWidth;

        windowData.Left = windowRect.Left += borderWidth;
        windowData.Top = windowRect.Top += borderHeight;
        windowData.Width = clientWidth;
        windowData.Height = clientHeight;
        return true;
    }

    //////////////////////////////////////////////////////////////////
    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryRequest
    {
        public ulong Address;  // 目标地址
        public uint Size;     // 读取的字节数
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MemoryData
    {
        public byte[] Data;   // 存储读取的内存数据

        public MemoryData(int size)
        {
            Data = new byte[size];
        }
    }
    public static void ShowBuffer(byte[] buffer)
    {
        return;
        Debug.Write("Buffer: ");
        foreach (byte b in buffer)
        {
            Debug.Write($"{b:X2} ");
        }
        Debug.WriteLine("");
    }
    /// <summary>
    /// 泛型读取内存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <returns></returns>
    public static T Read<T>(long address) where T : struct
    {
        
        // 计算结构体 T 的大小
        var buffer = new byte[Marshal.SizeOf(typeof(T))];

        // 使用 IOCTL 向驱动发送内存读取请求
        MemoryRequest request = new MemoryRequest
        {
            Address = (ulong)address, // 传递要读取的地址
            Size = (uint)buffer.Length // 传递要读取的大小
        };

        try
        {
            // 使用 IOCTL 向驱动发送请求
            MemoryData memoryData = driverCommunication.ReadMemory(address, (uint)buffer.Length);

            // 将返回的内存数据转换为结构体 T
            buffer = memoryData.Data;

            // 将字节数组转换为结构体 T
            return ByteArrayToStructure<T>(buffer);
        }
        catch (Exception ex)
        {
           
        }
        return default(T);
    }

    /// <summary>
    /// 泛型写入内存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <param name="value"></param>
    public static void Write<T>(long address, T value) where T : struct
    {
        // 将结构体转换为字节数组
        var buffer = StructureToByteArray(value);
        try
        {
            // 调用 WriteMemory 执行内存写入操作
            driverCommunication.WriteMemory(address, buffer);  // 使用 WriteMemory 进行内存写入
        }
        catch (Exception ex)
        {

        }
        //Win32.WriteProcessMemory(Bf1ProHandle, address, buffer, buffer.Length, out _);
    }

    /// <summary>
    /// 读取字符串
    /// </summary>
    /// <param name="address"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static string ReadString(long address, int size)
    {
        var buffer = new byte[size];
       
        try
        {
            // 使用 IOCTL 向驱动发送请求
            MemoryData memoryData = driverCommunication.ReadMemory(address, (uint)size);

            // 将返回的内存数据转换为结构体 T
            buffer = memoryData.Data;
            //Array.Reverse(buffer);

            ShowBuffer(buffer);

        }
        catch (Exception ex)
        {
            
        }


        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] == 0)
            {
                byte[] _buffer = new byte[i];
                Buffer.BlockCopy(buffer, 0, _buffer, 0, i);
                return Encoding.UTF8.GetString(_buffer);
            }
        }

        return Encoding.UTF8.GetString(buffer);
    }

    /// <summary>
    /// 写入字符串
    /// </summary>
    /// <param name="address"></param>
    /// <param name="vaule"></param>
    public static void WriteString(long address, string vaule)
    {
        var buffer = new UTF8Encoding().GetBytes(vaule);

        try
        {
            // 使用 IOCTL 向驱动发送请求
            driverCommunication.WriteMemory(address, buffer);

           

            ShowBuffer(buffer);

        }
        catch (Exception ex)
        {

        }


      
      //  Win32.WriteProcessMemory(Bf1ProHandle, address, buffer, buffer.Length, out _);
    }

    //////////////////////////////////////////////////////////////////

    /// <summary>
    /// 判断内存地址是否有效
    /// </summary>
    /// <param name="Address"></param>
    /// <returns></returns>
    public static bool IsValid(long Address)
    {
        return Address >= 0x10000 && Address < 0x000F000000000000;
    }

    /// <summary>
    /// 字符数组转结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="bytes"></param>
    /// <returns></returns>
    private static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
    {
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            var obj = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            if (obj != null)
                return (T)obj;
            else
                return default;
        }
        finally
        {
            handle.Free();
        }
    }

    /// <summary>
    /// 结构转字节数组
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static byte[] StructureToByteArray(object obj)
    {
        var length = Marshal.SizeOf(obj);
        var array = new byte[length];
        IntPtr pointer = Marshal.AllocHGlobal(length);
        Marshal.StructureToPtr(obj, pointer, true);
        Marshal.Copy(pointer, array, 0, length);
        Marshal.FreeHGlobal(pointer);
        return array;
    }
}

public struct WindowData
{
    public int Left;
    public int Top;
    public int Width;
    public int Height;
}
public class DriverCommunication
{
    
    private const string DriverDevicePath = @"\\.\battledriver";

    // 驱动设备的文件句柄
    private  IntPtr _deviceHandle;

    // 构造函数，打开驱动设备
    public DriverCommunication()
    {
        _deviceHandle = CreateFile(DriverDevicePath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

        if (_deviceHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Unable to open driver device.");
        }
    }

    // 关闭驱动设备
    public void Close()
    {
        if (_deviceHandle != IntPtr.Zero)
        {
            CloseHandle(_deviceHandle);
            _deviceHandle = IntPtr.Zero;
        }
    }

    // 通过 IOCTL 向驱动发送读取内存请求
    public MemoryData ReadMemory(long address, uint size)
    {

        // **创建 12 字节的 `RequestData`**
        byte[] RequestData = new byte[16];
        //Debug.WriteLine("");
        // **手动填充 `MemoryRequest` 结构体**
        RequestData[0] = (byte)(address & 0xFF);
        RequestData[1] = (byte)((address >> 8) & 0xFF);
        RequestData[2] = (byte)((address >> 16) & 0xFF);
        RequestData[3] = (byte)((address >> 24) & 0xFF);
        RequestData[4] = (byte)((address >> 32) & 0xFF);
        RequestData[5] = (byte)((address >> 40) & 0xFF);
        RequestData[6] = (byte)((address >> 48) & 0xFF);
        RequestData[7] = (byte)((address >> 56) & 0xFF);

        RequestData[8] = (byte)(size & 0xFF);
        RequestData[9] = (byte)((size >> 8) & 0xFF);
        RequestData[10] = (byte)((size >> 16) & 0xFF);
        RequestData[11] = (byte)((size >> 24) & 0xFF);
        // **填充 4 个额外字节，使用 0xCC**
        RequestData[12] = 0xCC;
        RequestData[13] = 0xCC;
        RequestData[14] = 0xCC;
        RequestData[15] = 0xCC;

        // **打印 `RequestData` 以确保正确**
        //Debug.Write("RequestData: ");
       // foreach (byte b in RequestData)
        //{
            // Debug.Write($"{b:X2} ");
        //}
        //Debug.WriteLine("");


        // 创建输出缓冲区
        var outputBuffer = new byte[size];

        // 调用 DeviceIoControl
        bool success = DeviceIoControl2(
            _deviceHandle,
            CTL_CODE(FILE_DEVICE_UNKNOWN, 0x802, METHOD_BUFFERED, FILE_ANY_ACCESS),
            RequestData,
            (uint)Marshal.SizeOf(typeof(MemoryRequest)),
            outputBuffer,         // 直接传递字节数组
            (uint)outputBuffer.Length,
            out uint bytesReturned,
            IntPtr.Zero);
        
        if (!success)
        {
            //throw new InvalidOperationException($"Failed to send IOCTL request. Error: {Marshal.GetLastWin32Error()}");
        }

        // 检查返回的字节数是否与预期一致
        if (bytesReturned != size)
        {
            //throw new InvalidOperationException($"Unexpected number of bytes returned: {bytesReturned}");
        }


        // 将数据存储到 MemoryData 结构体
        var memoryData = new MemoryData((int)size);
        Array.Copy(outputBuffer, memoryData.Data, size);

        return memoryData;


    }
    // MemoryRequest 结构体 (目标地址、数据长度、数据内容)
    [StructLayout(LayoutKind.Sequential, Pack = 8)] // 手动调整对齐
    public struct WriteMemoryRequest
    {
        public ulong Address;     // 目标地址
        public uint DataLength;  // 数据长度
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)] // 固定大小
        public byte[] Data;      // 数据内容
    }
    public bool WriteMemory(long targetAddress, byte[] data)
    {
        // 创建 MemoryRequest 结构体
        WriteMemoryRequest writeRequest = new WriteMemoryRequest
        {
            Address = (ulong)targetAddress,
            DataLength = (uint)data.Length,
            Data = new byte[4096]  // 创建一个足够大的数组
        };

        // 将数据填充到 writeRequest 结构体
        Array.Copy(data, writeRequest.Data, data.Length);

        // 将 MemoryRequest 结构体转换为字节数组
        byte[] byteArray = StructureToByteArray(writeRequest);

      

        uint bytesReturned = 0;

        // 调用 DeviceIoControl 执行写操作
        bool result = DeviceIoControlWrite(
            _deviceHandle,                              // 驱动设备句柄
            CTL_CODE(FILE_DEVICE_UNKNOWN, 0x803, METHOD_BUFFERED, FILE_ANY_ACCESS), // IOCTL 写内存操作码
            byteArray,                                   // 输入缓冲区
            (uint)byteArray.Length,                     // 输入缓冲区大小
            IntPtr.Zero,                                // 无输出缓冲区
            0,                                          // 无输出缓冲区大小
            out bytesReturned,                          // 返回的字节数
            IntPtr.Zero                                 // 没有重叠结构
        );

       

        // 检查返回结果
        if (!result)
        {
            Debug.WriteLine($"WriteMemory failed. Error: {Marshal.GetLastWin32Error()}");
            return false;
        }

        return true;
    }

    //  MemoryRequest 结构体，用于传递 PID
    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryRequest2
    {
        public int PID;  // 进程 ID
    }

    //  MemoryData 结构体，用于接收基地址信息（这里接收 wchar_t 字符串）
    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryData2
    {
        public IntPtr BaseAddress;  // 存储基地址的指针
    }

    // IOCTL 操作代码（与驱动中定义的一致）
    public const uint IOCTL_MY_DRIVER_OPERATION2 = 0x801; // IOCTL 操作代码

    // 定义 CTL_CODE
    public static uint CTL_CODE(uint deviceType, uint function, uint method, uint access)
    {
        return (deviceType << 16) | (access << 14) | (function << 2) | method;
    }

    // 定义文件设备类型和权限
    public const uint FILE_DEVICE_UNKNOWN = 0x00000022;
    public const uint METHOD_BUFFERED = 0;
    public const uint FILE_ANY_ACCESS = 0;

    [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl")]
    private static extern bool DeviceIoControl2(
     IntPtr hDevice,
     uint dwIoControlCode,
     byte[] lpInBuffer,        // 修改为 byte
     uint nInBufferSize,
     byte[] lpOutBuffer,       
     uint nOutBufferSize,
     out uint lpBytesReturned,
     IntPtr lpOverlapped);
    [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl")]
    private static extern bool DeviceIoControlWrite(
    IntPtr hDevice,
    uint dwIoControlCode,
    byte[] lpInBuffer,        // 输入缓冲区，修改为 byte[]
    uint nInBufferSize,       // 输入缓冲区大小
    IntPtr lpOutBuffer,       // 输出缓冲区（不需要输出数据时可以传递 IntPtr.Zero）
    uint nOutBufferSize,      // 输出缓冲区大小（不需要输出数据时为 0）
    out uint lpBytesReturned, // 返回的字节数
    IntPtr lpOverlapped);     // 重叠结构（通常传递 IntPtr.Zero）


    // 专门的读取基地址的函数
    public string ReadBaseAddress(int pid)
    {
        // 创建一个字节数组来传递 PID 数据
        byte[] pidData = new byte[4]; // 假设 PID 是 4 字节整数
        BitConverter.GetBytes(pid).CopyTo(pidData, 0);

        // 创建一个字节数组用于接收驱动返回的数据
        byte[] buffer = new byte[256]; // 够大

        uint bytesReturned;

        // 调用 DeviceIoControl2 发送读取基地址的请求
        bool success = DeviceIoControl2(
            _deviceHandle,  // 驱动设备句柄
           CTL_CODE(FILE_DEVICE_UNKNOWN, 0x801, METHOD_BUFFERED, FILE_ANY_ACCESS),
             pidData,  // 输入数据（PID）
            (uint)pidData.Length,  // 输入数据大小
             buffer,  // 输出数据（基地址）
            (uint)buffer.Length,  // 输出数据大小
            out bytesReturned,  // 返回的字节数
            IntPtr.Zero);  // 可选的重叠结构体

        if (!success)
        {
            throw new InvalidOperationException("Failed to send IOCTL request to driver.");
        }

        // 返回接收到的字节数组
        return Encoding.Unicode.GetString(buffer);
    }


    // Win32 API 声明
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr CreateFile(
        string lpFileName,
        FileAccess dwDesiredAccess,
        FileShare dwShareMode,
        IntPtr lpSecurityAttributes,
        FileMode dwCreationDisposition,
        int dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool DeviceIoControl(
        IntPtr hDevice,
        uint dwIoControlCode,
        ref MemoryRequest lpInBuffer,
        uint nInBufferSize,
       byte[] lpOutBuffer,
        uint nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped);
}