using System.Diagnostics;
using LabFusion.Network;
using LabFusion.Player;
using MashGamemodeLibrary.Player.Team;
using MelonLoader;

namespace MashGamemodeLibrary.Execution;

public static class Executor
{
    public delegate void Runnable();

#if DEBUG
    private static int _hostDepth;
    public static bool IsHostContext => _hostDepth == 0;

    private static void StepInHost()
    {
        _hostDepth += 1;
    }

    private static void StepOutHost()
    {
        _hostDepth = Math.Max(0, _hostDepth - 1);
    }
#endif

    private static void Run(Runnable runnable)
    {
#if DEBUG
        // Make sure the debugger catches the error
        runnable();
#else
        try
        {
            runnable();
        }
        catch (Exception e)
        {
            MelonLogger.Error($"[Mash Gamemode Library] An error occurred during execution: {e}");
        }
#endif
    }

    public static void RunIfHost(Runnable runnable, string? error = null)
    {
        if (!NetworkInfo.IsHost)
        {
            if (error == null) return;

            MelonLogger.Error($"This can only be ran from a host: {error}");
            return;
        }
        
#if DEBUG
        StepInHost();
#endif

        Run(runnable);
        
#if DEBUG
        StepOutHost();
#endif
    }
    
    public static void RunIfNotHost(Runnable runnable, string? error = null)
    {
        if (NetworkInfo.IsHost)
        {
            if (error == null) return;

            MelonLogger.Error($"This can only be ran from a host: {error}");
            return;
        }
        
#if DEBUG
        StepInHost();
#endif

        Run(runnable);
        
#if DEBUG
        StepOutHost();
#endif
    }

    public static void RunIfRemote(PlayerID id, Runnable runnable)
    {
        if (id.IsMe)
            return;

        Run(runnable);
    }

    public static void RunIfMe(PlayerID id, Runnable runnable)
    {
        if (!id.IsMe)
            return;

        Run(runnable);
    }
    
    // Validation
    public static void EnsureHost()
    {
#if DEBUG
        if (IsHostContext) return;

        var trace = new StackTrace();
        MelonLogger.Warning("Possibly executing a host only method on the client!", trace);
#endif
    }
}