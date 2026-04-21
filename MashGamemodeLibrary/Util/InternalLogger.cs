using System.Diagnostics;
using MelonLoader;

namespace MashGamemodeLibrary.Util;

internal static class InternalLogger
{
    [Conditional("DEBUG")]
    internal static void Debug(string txt)
    {
        MelonLogger.Msg($"[Mash's Gamemode Library - DEBUG] {txt}");
    }

    public static void Error(string error)
    {
        MelonLogger.Error($"[Mash's Gamemode Library - ERROR] {error}");
    }
    
    public static void Warn(string warning)
    {
        MelonLogger.Warning($"[Mash's Gamemode Library - WARNING] {warning}");
    }
}