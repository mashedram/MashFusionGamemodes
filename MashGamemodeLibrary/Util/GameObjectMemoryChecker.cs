using System.Runtime.InteropServices;

namespace MashGamemodeLibrary.Util;

public static class GameObjectMemoryChecker
{
    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryBasicInformation
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualQuery(IntPtr lpAddress, out MemoryBasicInformation lpBuffer, IntPtr dwLength);

    // Memory protection constants
    private const uint PageNoaccess = 0x01;
    private const uint PageGuard = 0x100;
    private const uint MemFree = 0x10000;
    
    /// <summary>
    /// Checks if an IntPtr is pointing to accessible memory
    /// </summary>
    public static bool IsPointerAccessible(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return false;

        var result = VirtualQuery(ptr, out var mbi, new IntPtr(Marshal.SizeOf(typeof(MemoryBasicInformation))));

        if (result == IntPtr.Zero) return false;

        // Check if page is protected or inaccessible
        return (mbi.Protect & PageNoaccess) == 0 && 
               (mbi.Protect & PageGuard) == 0 &&
               mbi.State != MemFree;
    }
}