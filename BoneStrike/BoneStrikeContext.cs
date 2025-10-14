using LabFusion.SDK.Gamemodes;
using MashGamemodeLibrary.Context;

namespace BoneStrike;

public class BoneStrikeContext : GameModeContext
{
    public static Team Terrorists = new("Terrorists");
    public static Team CounterTerrorists = new("CounterTerrorists");
    public static TeamManager TeamManager = new();
}