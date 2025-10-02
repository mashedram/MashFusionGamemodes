using MashGamemodeLibrary.Player;

namespace Clockhunt.Config;

public static class ClockhuntConfig
{
    public static int HidePhaseDuration = 60;
    public static int HuntPhaseDuration = 300;
    public static int EscapePhaseDuration = 120;
    
    public static int NightmareCount = 1;
    
    public static int ClocksPerPlayer = 1;
    public static int HuntPhaseClockCount = 3;
    
    public static bool IsSpectatingEnabled = true;
    public static bool IsEscapePhaseEnabled = false;
    
    public static float DeliveryDistance = 10.0f;
    
    public static PlayerStats DefaultStats = new PlayerStats
    {
        Vitality = 1.0f,
        Speed = 1.0f,
        LowerStrength = 1.0f,
        UpperStrength = 1.0f,
        Agility = 1.0f
    };

    public static float EscapeDistance = 10.0f;
    public static float EscapeDuration = 15.0f;
}