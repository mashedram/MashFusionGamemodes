using LabFusion.Player;

namespace MashGamemodeLibrary.networking.Validation;

public static class NetworkValidatorHelper
{
    public static bool IsClient(byte smallId)
    {
        return smallId != PlayerIDManager.HostSmallID;
    }

    public static bool IsHost(byte smallId)
    {
        return smallId == PlayerIDManager.HostSmallID;
    }
}