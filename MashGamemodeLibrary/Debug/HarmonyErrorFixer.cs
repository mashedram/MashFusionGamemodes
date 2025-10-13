#if DEBUG
using System.Reflection;

using HarmonyLib;
using Il2CppSystem.Diagnostics;
using MelonLoader;

namespace MashGamemodeLibrary.Debug;

[HarmonyPatch]
public static class Il2CppDetourMethodPatcherPatches
{
    public static MethodBase TargetMethod()
    {
        var type = Type.GetType("Il2CppInterop.HarmonySupport.Il2CppDetourMethodPatcher, Il2CppInterop.HarmonySupport", true);
        var method = AccessTools.FirstMethod(type, (method) => method.Name.Contains("ReportException"));
        return method;
    }

    public static bool Prefix(Exception ex)
    {
        var trace = new StackTrace();
        
        MelonLogger.Error("During invoking native->managed trampoline", ex);
        MelonLogger.Error("Found at trace", trace);
        return false;
    }
}
#endif