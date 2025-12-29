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
}