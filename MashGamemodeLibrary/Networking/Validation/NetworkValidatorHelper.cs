using LabFusion.Player;

namespace MashGamemodeLibrary.networking.Validation;

public static class NetworkValidatorHelper
{
    public static bool IsClient(byte playerID)
    {
        return playerID != PlayerIDManager.HostSmallID;
    }

    public static bool IsHost(byte playerID)
    {
        return playerID == PlayerIDManager.HostSmallID;
    }
}