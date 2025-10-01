using LabFusion.Player;
using LabFusion.Utilities;
using MelonLoader;

namespace MashGamemodeLibrary.Player;

public static class PlayerStatManager
{
    internal static PlayerStats? LocalStatOverride;
    
    public static void SetStats(PlayerStats stats)
    {
        LocalStatOverride = stats;
        LocalAvatar.RefreshAvatar();
    }

    public static void ResetStats()
    {
        LocalStatOverride = null;
    }
}