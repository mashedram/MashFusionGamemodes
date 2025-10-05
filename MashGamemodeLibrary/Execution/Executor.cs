using LabFusion.Network;
using LabFusion.Player;
using MelonLoader;

namespace MashGamemodeLibrary.Execution;

public static class Executor
{
    public delegate void Runnable();
    
    // TODO: Add option to run an executable after a delay

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
    
    public static void RunIfHost(Runnable runnable)
    {
        if (!NetworkInfo.IsHost)
            return;
        
        Run(runnable);
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
}